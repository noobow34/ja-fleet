using AngleSharp.Html.Parser;
using jafleet.Commons.Constants;
using jafleet.Commons.EF;
using jafleet.Manager;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Noobow.Commons.Utils;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace jafleet
{
    public class WorkingCheck
    {
        private IEnumerable<Aircraft> _targetRegistrationNumber;
        private int _interval;
        private const string FR24_DATA_URL = @"https://www.flightradar24.com/data/aircraft/";
        private readonly static TimeSpan CompareTargetTimeSpan = new TimeSpan(2,0,0,0);
        private static readonly string[] MAINTE_PLACE = new string[] {"TPE","MNL","XSP","QPG","XMN","SIN","TNA","HKG","OKA" };
        public static DbContextOptionsBuilder<jafleetContext> Options { get; set; }
        public static bool Processing { get; set; } = false;

        public WorkingCheck(IEnumerable<Aircraft> targetRegistrationNumber,int interval)
        {
            _targetRegistrationNumber = targetRegistrationNumber;
            _interval = interval;
            if(Options == null)
            {
                Options = new DbContextOptionsBuilder<jafleetContext>();
                var config = new ConfigurationBuilder().SetBasePath(Environment.CurrentDirectory).AddJsonFile("appsettings.json").Build();
                var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
                Options.UseLoggerFactory(loggerFactory).UseMySql(config.GetConnectionString("DefaultConnection"),
                        mySqlOptions =>
                        {
                            mySqlOptions.ServerVersion(new Version(10, 4), ServerType.MariaDb);
                        }
                );
            }
        }

        public async Task ExecuteCheckAsync()
        {
            Processing = true;
            var parser = new HtmlParser();

            using var context = new jafleetContext(Options.Options);
            var toWorking0 = new SortedDictionary<string, string>(); //予約登録かつ非稼働が稼働した（テスト飛行した）
            var toWorking1 = new SortedDictionary<string, string>(); //製造中かつ非稼働が稼働した（テスト飛行継続）
            var toWorking2 = new SortedDictionary<string,string>(); //デリバリーかつ非稼働が稼働した（営業運航投入）
            var toWorking3 = new SortedDictionary<string, string>(); //運用中で非稼働が稼働した
            var toWorking7 = new SortedDictionary<string, string>(); //退役で非稼働が稼働した（退役フェリーされた）
            var toNotWorking = new SortedDictionary<string, string>(); //非稼働になった
            var mainteStart = new SortedDictionary<string, string>(); //整備開始の疑い
            var mainteEnd = new SortedDictionary<string, string>(); //整備終了の疑い
            var mainteing = new SortedDictionary<string, string>(); //整備中の疑い
            var allLog = new StringBuilder();

            foreach (Aircraft a in _targetRegistrationNumber)
            {
                bool success = false;
                int failCount = 0;
                Exception exBack = null;
                while(!success && failCount <= 5)
                {
                    try
                    {
                        var htmlDocument = parser.ParseDocument(await HttpClientManager.GetInstance().GetStringAsync(FR24_DATA_URL + a.RegistrationNumber));
                        var row = htmlDocument.GetElementsByClassName("data-row");
                        var status = context.WorkingStatus.Where(s => s.RegistrationNumber == a.RegistrationNumber).FirstOrDefault();
                        var r = new Random();
                        if (row!.Length != 0)
                        {

                            //rowがもつ日付
                            string timestamp = row[0].GetAttribute("data-timestamp");
                            DateTime latestDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(timestamp)).LocalDateTime;
                            var currentInfo = new StringBuilder();
                            currentInfo.Append($"{a.RegistrationNumber}:{latestDate:yyyy/MM/dd HH:mm} ");

                            //tdの各値
                            var td = row[0].GetElementsByTagName("td");
                            if (td!.Length != 0)
                            {
                                currentInfo.Append($"{td[3].TextContent!.Trim()} {td[4].TextContent!.Trim()} {td[5].TextContent!.Trim()} {td[11].TextContent!.Trim()}");
                            }

                            bool? previousWorking;
                            DateTime? previousDate;
                            if (status == null)
                            {
                                status = new WorkingStatus()
                                {
                                    RegistrationNumber = a.RegistrationNumber
                                };
                                context.WorkingStatus.Add(status);
                            }
                            previousWorking = status.Working;
                            previousDate = status.FlightDate;
                            status.FlightDate = latestDate;
                            status.FromAp = td[3].TextContent!.Trim();
                            status.ToAp = td[4].TextContent!.Trim();
                            status.FlightNumber = td[5].TextContent!.Trim();
                            status.Status = td[11].TextContent!.Trim();
                            status.Working = (DateTime.Now.Date < latestDate.Date) || ((DateTime.Now.Date - latestDate.Date) <= CompareTargetTimeSpan);

                            if ((!previousWorking.HasValue || !previousWorking.Value) && status.Working!.Value)
                            {
                                string infoString = $"{currentInfo} ← {previousDate:yyyy/MM/dd HH:mm}";
                                //非稼働から稼働になった
                                switch (a.OperationCode)
                                {
                                    case OperationCode.RESERVED:
                                        toWorking0.Add(a.RegistrationNumber, infoString);
                                        break;

                                    case OperationCode.MAKING:
                                        toWorking1.Add(a.RegistrationNumber, infoString);
                                        break;

                                    case OperationCode.DELIVERY:
                                        toWorking2.Add(a.RegistrationNumber, infoString);
                                        break;

                                    case OperationCode.INTERNATIONAL:
                                    case OperationCode.DOMESTIC:
                                    case OperationCode.BOTH:
                                    case OperationCode.CARGO:
                                        toWorking3.Add(a.RegistrationNumber, infoString);
                                        break;

                                    case OperationCode.RETIRE_REGISTERED:
                                        toWorking7.Add(a.RegistrationNumber, infoString);
                                        break;

                                }
                                //整備終了の疑い
                                if (status.Maintenancing.HasValue && status.Maintenancing.Value)
                                {
                                    status.Maintenancing = false;
                                    mainteEnd.Add(a.RegistrationNumber, currentInfo.ToString());
                                }
                            }
                            else if (previousWorking.HasValue && previousWorking.Value && !status.Working.Value)
                            {
                                //稼働から非稼働になった
                                toNotWorking.Add(a.RegistrationNumber, currentInfo.ToString());
                                //整備開始の疑い
                                if (MAINTE_PLACE.Any(m => status.ToAp.Contains(m)))
                                {
                                    status.Maintenancing = true;
                                    mainteStart.Add(a.RegistrationNumber, currentInfo.ToString());
                                }
                            }
                            else if (status.Maintenancing.HasValue && status.Maintenancing.Value)
                            {
                                mainteing.Add(a.RegistrationNumber, currentInfo.ToString());
                            }
                            Console.WriteLine(currentInfo);
                        }
                        else
                        {
                            if (status != null)
                            {
                                if (status.Working.HasValue && status.Working.Value)
                                {
                                    toNotWorking.Add(a.RegistrationNumber, $"{status.RegistrationNumber}:{status.FlightDate} {status.FromAp} {status.ToAp} {status.FlightNumber} {status.Status}");
                                }
                                status.Working = false;
                            }
                            else
                            {
                                status = new WorkingStatus()
                                {
                                    RegistrationNumber = a.RegistrationNumber,
                                    Working = false
                                };
                                context.WorkingStatus.Add(status);
                            }
                            Console.WriteLine($"{a.RegistrationNumber}:データなし");
                        }
                        int interval = Convert.ToInt32(r.NextDouble() * _interval * 1000);
                        Console.WriteLine($"{interval}ミリ秒待機");
                        Thread.Sleep(interval);
                        success = true;
                    }
                    catch (Exception ex)
                    {
                        failCount++;
                        exBack = ex;
                        Thread.Sleep(60 * 1000); //Exceptionになったら1分待機
                    }
                }
                if(failCount > 5)
                {
                    Console.WriteLine(exBack?.ToString());
                    LineUtil.PushMe($"WorkingCheck異常終了:{DateTime.Now}\n", HttpClientManager.GetInstance());
                    LineUtil.PushMe(exBack?.ToString(), HttpClientManager.GetInstance());
                    Processing = false;
                    return;
                }
            }

            allLog.Append($"WorkingCheck正常終了:{DateTime.Now}\n");
            if (toWorking0.Count > 0)
            {
                allLog.Append("--------予約登録が稼働--------\n");
                allLog.AppendJoin("\n", toWorking0.Values);
                allLog.Append("\n");
            }
            if (toWorking1.Count > 0)
            {
                allLog.Append("--------製造中が稼働--------\n");
                allLog.AppendJoin("\n", toWorking1.Values);
                allLog.Append("\n");
            }
            if (toWorking2.Count > 0)
            {
                allLog.Append("--------デリバリーが稼働--------\n");
                allLog.AppendJoin("\n", toWorking2.Values);
                allLog.Append("\n");
            }
            if (toWorking3.Count > 0)
            {
                allLog.Append("--------運用中非稼働が稼働--------\n");
                allLog.AppendJoin("\n", toWorking3.Values);
                allLog.Append("\n");
            }
            if (toWorking7.Count > 0)
            {
                allLog.Append("--------退役未抹消が稼働--------\n");
                allLog.AppendJoin("\n", toWorking7.Values);
                allLog.Append("\n");
            }
            if (toNotWorking.Count > 0)
            {
                allLog.Append("--------稼働が非稼働--------\n");
                allLog.AppendJoin("\n", toNotWorking.Values);
                allLog.Append("\n");
            }
            if(mainteStart.Count > 0)
            {
                allLog.Append("--------整備入り--------\n");
                allLog.AppendJoin("\n", mainteStart.Values);
                allLog.Append("\n");
            }
            if (mainteEnd.Count > 0)
            {
                allLog.Append("--------整備終了--------\n");
                allLog.AppendJoin("\n", mainteEnd.Values);
                allLog.Append("\n");
            }
            if (mainteing.Count > 0)
            {
                allLog.Append("--------整備中--------\n");
                allLog.AppendJoin("\n", mainteing.Values);
                allLog.Append("\n");
            }

            var workingCheckLog = new Log
            {
                LogDate = DateTime.Now,
                LogType = LogType.WORKING_INFO,
                LogDetail = allLog.ToString(),
            };
            context.Log.Add(workingCheckLog);

            context.SaveChanges();

            LineUtil.PushMe($"WorkingCheck正常終了:{DateTime.Now:yyyy/MM/dd HH:mm:ss}\n" +
                            $"予約登録が稼働:{toWorking0.Count}件\n" +
                            $"製造中が稼働:{toWorking1.Count}件\n" +
                            $"デリバリーが稼働:{toWorking2.Count}件\n" +
                            $"運用中非稼働が稼働:{toWorking3.Count}件\n" +
                            $"退役未抹消が稼働:{toWorking7.Count}件\n" +
                            $"稼働が非稼働:{toNotWorking.Count}件\n" +
                            $"整備入り:{mainteStart.Count}件\n" +
                            $"整備終了:{mainteEnd.Count}件\n" +
                            $"整備中:{mainteing.Count}件\n" +
                            $@"https://ja-fleet.noobow.me/WorkingCheckLog/Index/{DateTime.Now:yyyyMMdd}", HttpClientManager.GetInstance());

            Processing = false;
        }
    }
}
