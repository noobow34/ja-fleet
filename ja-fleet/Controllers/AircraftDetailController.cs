using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using jafleet.Models;
using jafleet.Commons.EF;
using jafleet.Util;
using jafleet.Commons.Constants;
using Microsoft.EntityFrameworkCore;

namespace jafleet.Controllers
{
    public class AircraftDetailController : Controller
    {

        private readonly jafleetContext _context;
        private readonly IServiceScopeFactory _services;

        public AircraftDetailController(jafleetContext context, IServiceScopeFactory serviceScopeFactory)
        {
            _context = context;
            _services = serviceScopeFactory;
        }

        public IActionResult Index(string id, [FromQuery] bool nohead, [FromQuery] bool needback, AircraftDetailModel model)
        {

            model.Title = id;
            model.TableId = id;
            model.api = "/api/aircraftwithhistory/" + id;
            model.IsDetail = true;

            model.NoHead = nohead;
            model.IsAdmin = CookieUtil.IsAdmin(HttpContext);
            model.Reg = id;
            model.NeedBack = needback;

            model.AV = _context.AircraftView.AsNoTracking().Where(av => av.RegistrationNumber == id).FirstOrDefault();
            model.OgImageUrl = (model.AV?.PhotoDirectUrl != null) ? $"https://cdn.jetphotos.com/full{model.AV?.PhotoDirectUrl}" : "https://ja-fleet.noobow.me/images/JA-Fleet_1_og.png";
            if (model.AV == null)
            {
                //存在しないレジが指定された場合はNotFound
                return NotFound();
            }
            if (!nohead)
            {
                model.AirlineGroupNmae = _context.AirlineGroup.AsNoTracking().Where(ag => ag.AirlineGroupCode == model.AV.AirlineGroupCode).FirstOrDefault()?.AirlineGroupName;
            }

            //非同期でCookieは取得できなくなるので退避
            bool isAdmin = CookieUtil.IsAdmin(HttpContext);

            //ログは非同期で書き込み
            Task.Run(() =>
            {
                using var serviceScope = _services.CreateScope();
                using jafleetContext? context = serviceScope.ServiceProvider.GetService<jafleetContext>();
                Log log = new()
                {
                    LogDate = DateTime.Now,
                    LogType = LogType.DETAIL,
                    LogDetail = id,
                    UserId = isAdmin.ToString(),
                };

                context!.Log.Add(log);
                context.SaveChanges();
            });

            return View("~/Views/AircraftDetail/index.cshtml", model);
        }

        public IActionResult IndexNohead(string id, AircraftDetailModel model)
        {
            return Index(id, true, false, model);
        }

        public IActionResult IndexNoheadBack(string id, AircraftDetailModel model)
        {
            return Index(id, true, true, model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
