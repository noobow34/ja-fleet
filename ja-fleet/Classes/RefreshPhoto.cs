using AngleSharp;
using EnumStringValues;
using jafleet.Commons.Aircraft;
using jafleet.Commons.Constants;
using jafleet.Commons.EF;
using Microsoft.EntityFrameworkCore;
using Noobow.Commons.Constants;
using Noobow.Commons.Utils;

namespace jafleet
{
    public class RefreshPhoto
    {
        private IEnumerable<AircraftView> _targetRegistrationNumber;
        public static DbContextOptionsBuilder<JafleetContext>? Options { get; set; }

        public RefreshPhoto(IEnumerable<AircraftView> targetRegistrationNumber, int interval)
        {
            _targetRegistrationNumber = targetRegistrationNumber;
            if (Options == null)
            {
                Options = new DbContextOptionsBuilder<JafleetContext>();
                var config = new ConfigurationBuilder().SetBasePath(Environment.CurrentDirectory).AddJsonFile("appsettings.json").Build();
                Options.UseNpgsql(config.GetConnectionString("DefaultConnection"));
            }
        }

        public async Task ExecuteRefreshAsync()
        {
            using var context = new JafleetContext(Options!.Options);
            AngleSharp.IConfiguration? _config = Configuration.Default.WithDefaultLoader().WithDefaultCookies().WithXPath();
            IBrowsingContext _context = BrowsingContext.New(_config);
            foreach (var a in _targetRegistrationNumber) {
                bool success = false;
                int failCount = 0;
                Exception? exBack = null;
                while (!success && failCount <= 5)
                {
                    try
                    {
                        string url = $"{FlightradarConstant.FR24_DATA_URL}{a.RegistrationNumber}";
                        var doc = await _context.OpenAsync(url);
                        var row = doc?.Body?.GetElementsByClassName("data-row");
                        var ap = AircraftDataExtractor.ExtractPhotoDataFromJetphotos(doc);

                        AircraftPhoto? photo = context.AircraftPhotos.Where(p => p.RegistrationNumber == a.RegistrationNumber).FirstOrDefault();
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
                    catch (Exception ex)
                    {
                        failCount++;
                        exBack = ex;
                        Thread.Sleep(60 * 1000); //Exceptionになったら1分待機
                    }
                }
                if (failCount > 5)
                {
                    Console.WriteLine(exBack?.ToString());
                    await SlackUtil.PostAsync(SlackChannelEnum.jafleet.GetStringValue(), $"RefreshPhoto異常終了:{DateTime.Now}\n");
                    await SlackUtil.PostAsync(SlackChannelEnum.jafleet.GetStringValue(), exBack!.ToString());
                    return;
                }
            }
            context.SaveChanges();
        }
    }
}
