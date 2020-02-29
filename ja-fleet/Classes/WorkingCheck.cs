using AngleSharp.Html.Parser;
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
        private IEnumerable<string> _targetRegistrationNumber;
        private int _interval;
        private const string FR24_DATA_URL = @"https://www.flightradar24.com/data/aircraft/";
        private readonly TimeSpan CompareTargetTimeSpan = new TimeSpan(2,0,0,0);
        public static DbContextOptionsBuilder<jafleetContext> Options { get; set; }

        public WorkingCheck(IEnumerable<string> targetRegistrationNumber,int interval)
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
            var parser = new HtmlParser();

            using var context = new jafleetContext(Options.Options);
            var toWorking = new StringBuilder();
            var toNotWorking = new StringBuilder();

            foreach (string reg in _targetRegistrationNumber)
            {
                try
                {
                    var htmlDocument = parser.ParseDocument(await HttpClientManager.GetInstance().GetStringAsync(FR24_DATA_URL + reg));
                    var row = htmlDocument.GetElementsByClassName("data-row");
                    var status = context.WorkingStatus.Where(s => s.RegistrationNumber == reg).FirstOrDefault();
                    var r = new Random();
                    if (row!.Length != 0)
                    {

                        //rowがもつ日付
                        string timestamp = row[0].GetAttribute("data-timestamp");
                        DateTime latestDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(timestamp)).LocalDateTime;
                        var currentInfo = new StringBuilder();
                        currentInfo.Append($"{reg}:{latestDate} ");

                        //tdの各値
                        var td = row[0].GetElementsByTagName("td");
                        if(td!.Length != 0)
                        {
                            currentInfo.Append($"{td[3].TextContent!.Trim()} {td[4].TextContent!.Trim()} {td[5].TextContent!.Trim()} {td[11].TextContent!.Trim()}");
                        }

                        bool? previousWorking;
                        DateTime? previousDate;
                        if(status == null)
                        {
                            status = new WorkingStatus()
                            {
                                RegistrationNumber = reg
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

                        if((!previousWorking.HasValue || !previousWorking.Value) && status.Working!.Value)
                        {
                            //非稼働から稼働になった
                            toWorking.Append($"{currentInfo} ← {previousDate}");
                            toWorking.Append("\n");
                        }
                        else if(previousWorking.HasValue && previousWorking.Value && !status.Working.Value)
                        {
                            //稼働から非稼働になった
                            toNotWorking.Append(currentInfo);
                            toNotWorking.Append("\n");
                        }
                        Console.WriteLine(currentInfo);
                    }
                    else
                    {
                        if(status != null)
                        {
                            if (status.Working.HasValue && status.Working.Value)
                            {
                                toNotWorking.Append($"{status.RegistrationNumber}:{status.FlightDate} {status.FromAp} {status.ToAp} {status.FlightNumber} {status.Status}");
                                toNotWorking.Append("\n");
                            }
                            status.Working = false;
                        }
                        else
                        {
                            status = new WorkingStatus()
                            {
                                RegistrationNumber = reg,
                                Working = false
                            };
                            context.WorkingStatus.Add(status);
                        }
                        Console.WriteLine($"{reg}:データなし");
                    }
                    int interval = Convert.ToInt32(r.NextDouble() * _interval * 1000);
                    Console.WriteLine($"{interval}ミリ秒待機");
                    Thread.Sleep(interval);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    LineUtil.PushMe($"WorkingCheck異常終了:{DateTime.Now.ToString()}\n",HttpClientManager.GetInstance());
                    LineUtil.PushMe($"{ex.ToString()}", HttpClientManager.GetInstance());
                    return;
                }
            }
            context.SaveChanges();

            LineUtil.PushMe($"WorkingCheck正常終了:{DateTime.Now.ToString()}\n", HttpClientManager.GetInstance());
            if(toWorking.Length != 0)
            {
                Console.WriteLine("--------非稼働から稼働--------");
                Console.WriteLine(toWorking);
                LineUtil.PushMe($"--------非稼働から稼働--------\n{toWorking}", HttpClientManager.GetInstance());
            }
            if(toNotWorking.Length != 0)
            {
                Console.WriteLine("--------稼働から非稼働--------");
                Console.WriteLine(toNotWorking);
                LineUtil.PushMe($"--------稼働から非稼働--------\n{toNotWorking}", HttpClientManager.GetInstance());
            }

        }
    }
}
