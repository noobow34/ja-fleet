using AngleSharp;
using AngleSharp.Html.Parser;
using EnumStringValues;
using jafleet.Commons.Aircraft;
using jafleet.Commons.Constants;
using jafleet.Commons.EF;
using jafleet.Manager;
using Microsoft.EntityFrameworkCore;
using Noobow.Commons.Constants;
using Noobow.Commons.Extensions;
using Noobow.Commons.Utils;
using System.Diagnostics;
using System.Text;

namespace jafleet
{
    public class RefreshWorkingStatusAndPhoto
    {
        private IEnumerable<AircraftView> _targetRegistrationNumber;
        private int _interval;
        private readonly static TimeSpan CompareTargetTimeSpan = new(2, 0, 0, 0);
        private static readonly string[] MAINTE_PLACE = ["TPE", "MNL", "XSP", "QPG", "XMN", "SIN", "TNA", "HKG", "OKA", "TNN"];
        public static DbContextOptionsBuilder<JafleetContext>? Options { get; set; }
        public static bool Processing { get; set; } = false;

        public RefreshWorkingStatusAndPhoto(IEnumerable<AircraftView> targetRegistrationNumber, int interval)
        {
            _targetRegistrationNumber = targetRegistrationNumber;
            _interval = interval;
            if (Options == null)
            {
                Options = new DbContextOptionsBuilder<JafleetContext>();
                Options.UseNpgsql(Environment.GetEnvironmentVariable("JAFLEET_CONNECTION_STRING") ?? "");
            }
        }

        public async Task ExecuteCheckAsync(bool needsNotify = false)
        {
            int intervalSum = 0;
            var sw = new Stopwatch();
            sw.Start();
            Processing = true;
            var parser = new HtmlParser();

            using var context = new JafleetContext(Options!.Options);
            var toWorkingTest = new SortedDictionary<string, string>();  //テストレジが飛行した（テスト飛行した）
            var toWorking0 = new SortedDictionary<string, string>(); //予約登録かつ非稼働が稼働した（テスト飛行した）
            var toWorking1 = new SortedDictionary<string, string>(); //製造中かつ非稼働が稼働した（テスト飛行継続）
            var toWorking2 = new SortedDictionary<string, string>(); //デリバリーかつ非稼働が稼働した（営業運航投入）
            var toWorking3 = new SortedDictionary<string, string>(); //運用中で非稼働が稼働した
            var toWorking7 = new SortedDictionary<string, string>(); //退役で非稼働が稼働した（退役フェリーされた）
            var toNotWorking = new SortedDictionary<string, string>(); //非稼働になった
            var mainteStart = new SortedDictionary<string, string>(); //整備開始の疑い
            var mainteEnd = new SortedDictionary<string, string>(); //整備終了の疑い
            var mainteing = new SortedDictionary<string, string>(); //整備中の疑い
            var allLog = new StringBuilder();

            AngleSharp.IConfiguration? _config = Configuration.Default.WithDefaultLoader().WithDefaultCookies().WithXPath();
            IBrowsingContext _context = BrowsingContext.New(_config);
            var r = new Random();
            foreach (AircraftView a in _targetRegistrationNumber)
            {
                bool success = false;
                int failCount = 0;
                Exception? exBack = null;
                bool existPhoto = false;
                bool existPage = false;
                bool existOperation = false;
                while (!success && failCount <= 5)
                {
                    try
                    {
                        string url = $"{FlightradarConstant.FR24_DATA_URL}{a.RegistrationNumber}";
                        var doc = await _context.OpenAsync(url);
                        var row = doc?.Body?.GetElementsByClassName("data-row");
                        var ap = AircraftDataExtractor.ExtractPhotoDataFromJetphotos(doc);

                        var currentInfo = new StringBuilder();
                        existPage = doc?.BaseUri.Contains(a.RegistrationNumber, StringComparison.CurrentCultureIgnoreCase) ?? false;
                        string notifyMark = string.Empty;
                        if (!string.IsNullOrEmpty(a.SpecialLivery))
                        {
                            notifyMark = "◎";
                        }
                        else if (a.MaintenanceNotify.HasValue && a.MaintenanceNotify.Value)
                        {
                            notifyMark = "☆";
                        }
                        currentInfo.Append($"{a.RegistrationNumber}{notifyMark}({a.TypeDetailName})");

                        AircraftPhoto? photo = context.AircraftPhotos.Where(p => p.RegistrationNumber == a.RegistrationNumber).FirstOrDefault();
                        if (ap != null) {
                            existPhoto = true;
                            if (photo != null)
                            {
                                photo.PhotoUrl = ap.PhotoUrl;
                                photo.PhotoDirectLarge = ap.PhotoDirectLarge;
                                photo.PhotoDirectSmall = ap.PhotoDirectSmall;
                                photo.LastAccess = DateTime.Now;
                            }
                            else
                            {
                                photo = new AircraftPhoto()
                                {
                                    RegistrationNumber = a.RegistrationNumber,
                                    PhotoUrl = ap.PhotoUrl,
                                    PhotoDirectLarge = ap.PhotoDirectLarge,
                                    PhotoDirectSmall = ap.PhotoDirectSmall,
                                    LastAccess = DateTime.Now
                                };
                                context.AircraftPhotos.Add(photo);
                            }
                        }

                        var status = context.WorkingStatuses.Where(s => s.RegistrationNumber == a.RegistrationNumber).FirstOrDefault();
                        if (row!.Length != 0)
                        {
                            existOperation = true;
                            //rowがもつ日付
                            string? timestamp = row[0].GetAttribute("data-timestamp");
                            DateTime latestDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(timestamp!)).LocalDateTime;
                            currentInfo.Append($":{latestDate:yyyy/MM/dd HH:mm}");

                            //tdの各値
                            var td = row[0].GetElementsByTagName("td");
                            if (td!.Length != 0)
                            {
                                currentInfo.Append($":{td[3].TextContent!.Trim()} {td[4].TextContent!.Trim()} {td[5].TextContent!.Trim()} {td[11].TextContent!.Trim()}");
                            }

                            bool? previousWorking;
                            DateTime? previousDate;
                            if (status == null)
                            {
                                status = new WorkingStatus()
                                {
                                    RegistrationNumber = a.RegistrationNumber
                                };
                                context.WorkingStatuses.Add(status);
                            }
                            previousWorking = status.Working;
                            previousDate = status.FlightDate;
                            status.FlightDate = latestDate;
                            status.FromAp = td[3].TextContent!.Trim();
                            status.ToAp = td[4].TextContent!.Trim();
                            status.FlightNumber = td[5].TextContent!.Trim();
                            status.Status = td[11].TextContent!.Trim();
                            status.Working = (DateTime.Now.Date < latestDate.Date) || ((DateTime.Now.Date - latestDate.Date) <= CompareTargetTimeSpan);
                            status.ExistPage = existPage;
                            status.ExistPhoto = existPhoto;
                            status.ExistOperation = existOperation;
                            status.UpdatedAt = DateTime.Now;

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
                            else if (status.Maintenancing.HasValue && status.Maintenancing.Value && a.OperationCode != OperationCode.RETIRE_REGISTERED)
                            {
                                mainteing.Add(a.RegistrationNumber, currentInfo.ToString());
                            }
                        }
                        else
                        {
                            if (status != null)
                            {
                                if (status.Working.HasValue && status.Working.Value)
                                {
                                    toNotWorking.Add(a.RegistrationNumber, $"{status.RegistrationNumber}({a.TypeDetailName}):{status.FlightDate} {status.FromAp} {status.ToAp} {status.FlightNumber} {status.Status}");
                                }
                                if (status.Maintenancing.HasValue && status.Maintenancing.Value && a.OperationCode != OperationCode.RETIRE_REGISTERED)
                                {
                                    mainteing.Add(a.RegistrationNumber, $"{status.RegistrationNumber}({a.TypeDetailName}):{status.FlightDate} {status.FromAp} {status.ToAp} {status.FlightNumber} {status.Status}");
                                }
                                status.Working = false;
                                status.ExistPage = existPage;
                                status.ExistPhoto = existPhoto;
                                status.ExistOperation = existOperation;
                                status.UpdatedAt = DateTime.Now;
                            }
                            else
                            {
                                status = new WorkingStatus()
                                {
                                    RegistrationNumber = a.RegistrationNumber,
                                    Working = false,
                                    ExistPage = existPage,
                                    ExistPhoto = existPhoto,
                                    ExistOperation = existOperation,
                                    UpdatedAt = DateTime.Now
                                };
                                context.WorkingStatuses.Add(status);
                            }
                        }
                        int interval = Convert.ToInt32(r.NextDouble() * _interval * 1000);
                        intervalSum += interval;
                        string existPageString = existPage ? "ページあり" : "ページなし";
                        string existPhotoString = existPhoto ? "写真あり" : "写真なし";
                        string existOperationString = existOperation ? "運航情報あり" : "運航情報なし";
                        currentInfo.Append($":{existPageString}:{existPhotoString}:{existOperationString}:{interval}ミリ秒待機");
                        this.JournalWriteLine(currentInfo.ToString());
                        Thread.Sleep(interval);

                        //テストレジのチェック
                        if (!string.IsNullOrEmpty(a.TestRegistration))
                        {
                            var htmlDocumentTest = parser.ParseDocument(await HttpClientManager.GetInstance().GetStringAsync(FlightradarConstant.FR24_DATA_URL + a.TestRegistration));
                            var rowTest = htmlDocumentTest.GetElementsByClassName("data-row");
                            if (rowTest!.Length != 0)
                            {
                                string? timestamp = rowTest[0].GetAttribute("data-timestamp");
                                DateTime latestDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(timestamp!)).LocalDateTime;
                                if (!status.TestFlightDate.HasValue || status.TestFlightDate.Value < latestDate)
                                {
                                    //テストフライトしている
                                    var previousTestFilight = status.TestFlightDate.HasValue ? $" ← {status.TestFlightDate.Value:yyyy/MM/dd HH:mm}" : string.Empty;
                                    status.TestFlightDate = latestDate;
                                    toWorkingTest.Add(a.RegistrationNumber, $"{a.RegistrationNumber}({a.TestRegistration}:{a.TypeDetailName}):{latestDate:yyyy/MM/dd HH:mm}{previousTestFilight}");
                                }
                            }
                            interval = Convert.ToInt32(r.NextDouble() * _interval * 1000);
                            intervalSum += interval;
                            this.JournalWriteLine($"テストレジあり:{interval}ミリ秒待機");
                            Thread.Sleep(interval);
                        }
                        success = true;
                    }
                    catch (Exception ex)
                    {
                        failCount++;
                        exBack = ex;
                        Thread.Sleep(60 * 1000); //Exceptionになったら1分待機
                    }
                }
                if (failCount > 5)
                {
                    this.JournalWriteLine(exBack?.ToString() ?? string.Empty);
                    await SlackUtil.PostAsync(SlackChannelEnum.jafleet.GetStringValue(), $"RefreshWorkingStatus異常終了:{DateTime.Now}\n");
                    await SlackUtil.PostAsync(SlackChannelEnum.jafleet.GetStringValue(), exBack!.ToString());
                    Processing = false;
                    return;
                }
            }

            allLog.Append($"RefreshWorkingStatus正常終了:{DateTime.Now}\n");
            if (toWorkingTest.Count > 0)
            {
                allLog.Append("--------テストレジが稼働--------\n");
                allLog.AppendJoin("\n", toWorkingTest.Values);
                allLog.Append('\n');
            }
            if (toWorking0.Count > 0)
            {
                allLog.Append("--------予約登録が稼働--------\n");
                allLog.AppendJoin("\n", toWorking0.Values);
                allLog.Append('\n');
            }
            if (toWorking1.Count > 0)
            {
                allLog.Append("--------製造中が稼働--------\n");
                allLog.AppendJoin("\n", toWorking1.Values);
                allLog.Append('\n');
            }
            if (toWorking2.Count > 0)
            {
                allLog.Append("--------デリバリーが稼働--------\n");
                allLog.AppendJoin("\n", toWorking2.Values);
                allLog.Append('\n');
            }
            if (toWorking3.Count > 0)
            {
                allLog.Append("--------運用中非稼働が稼働--------\n");
                allLog.AppendJoin("\n", toWorking3.Values);
                allLog.Append('\n');
            }
            if (toWorking7.Count > 0)
            {
                allLog.Append("--------退役未抹消が稼働--------\n");
                allLog.AppendJoin("\n", toWorking7.Values);
                allLog.Append('\n');
            }
            if (toNotWorking.Count > 0)
            {
                allLog.Append("--------稼働が非稼働--------\n");
                allLog.AppendJoin("\n", toNotWorking.Values);
                allLog.Append('\n');
            }
            if (mainteStart.Count > 0)
            {
                allLog.Append("--------整備入り--------\n");
                allLog.AppendJoin("\n", mainteStart.Values);
                allLog.Append('\n');
            }
            if (mainteEnd.Count > 0)
            {
                allLog.Append("--------整備終了--------\n");
                allLog.AppendJoin("\n", mainteEnd.Values);
                allLog.Append('\n');
            }
            if (mainteing.Count > 0)
            {
                allLog.Append("--------整備中--------\n");
                allLog.AppendJoin("\n", mainteing.Values);
                allLog.Append('\n');
            }

            var workingCheckLog = new Log
            {
                LogDate = DateTime.Now,
                LogType = LogType.WORKING_INFO,
                LogDetail = allLog.ToString(),
            };
            context.Logs.Add(workingCheckLog);

            sw.Stop();
            string nt = $"RefreshWorkingStatus正常終了:{DateTime.Now:yyyy/MM/dd HH:mm:ss}\n" +
                            $"処理時間: {sw.Elapsed},待機秒数: {intervalSum / 1000.0}\n" +
                            ((toWorkingTest.Count > 0) ? $"テストレジが稼働:{toWorkingTest.Count}件\n" : string.Empty) +
                            ((toWorking0.Count > 0) ? $"予約登録が稼働:{toWorking0.Count}件\n" : string.Empty) +
                            ((toWorking1.Count > 0) ? $"製造中が稼働:{toWorking1.Count}件\n" : string.Empty) +
                            ((toWorking2.Count > 0) ? $"デリバリーが稼働:{toWorking2.Count}件\n" : string.Empty) +
                            ((toWorking3.Count > 0) ? $"運用中非稼働が稼働:{toWorking3.Count}件\n" : string.Empty) +
                            ((toWorking7.Count > 0) ? $"退役未抹消が稼働:{toWorking7.Count}件\n" : string.Empty) +
                            ((toNotWorking.Count > 0) ? $"稼働が非稼働:{toNotWorking.Count}件\n" : string.Empty) +
                            ((mainteStart.Count > 0) ? $"整備入り:{mainteStart.Count}件\n" : string.Empty) +
                            ((mainteEnd.Count > 0) ? $"整備終了:{mainteEnd.Count}件\n" : string.Empty) +
                            ((mainteing.Count > 0) ? $"整備中:{mainteing.Count}件\n" : string.Empty) +
                            $@"<https://ja-fleet.noobow.me/WorkingCheckLog/Index/{DateTime.Now:yyyyMMdd}|リンク>";

            if (needsNotify)
            {
                await SlackUtil.PostAsync(SlackChannelEnum.jafleet.GetStringValue(), nt);
            }

            var wcNotify = new Log
            {
                LogDate = DateTime.Now,
                LogType = LogType.WORKING_NOTIFY_TEXT,
                LogDetail = nt,
            };
            context.Logs.Add(wcNotify);
            context.SaveChanges();

            Processing = false;
        }
    }
}
