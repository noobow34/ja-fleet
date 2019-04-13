using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using jafleet.Models;
using jafleet.Commons.EF;
using System;
using jafleet.Util;
using jafleet.Commons.Constants;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
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

        public IActionResult Index(String id, [FromQuery]Boolean nohead, [FromQuery]Boolean needback, AircraftDetailModel model)
        {

            model.Title = id;
            model.TableId = id;
            model.api = "/api/aircraftwithhistory/" + id;
            model.IsDetail = true;

            model.NoHead = nohead;
            model.IsAdmin = CookieUtil.IsAdmin(HttpContext);
            model.Reg = id;
            model.NeedBack = needback;

            if (!nohead)
            {
                model.AV = _context.AircraftView.AsNoTracking().Where(av => av.RegistrationNumber == id).FirstOrDefault();
                model.AirlineGroupNmae = _context.AirlineGroup.AsNoTracking().Where(ag => ag.AirlineGroupCode == model.AV.AirlineGroupCode).FirstOrDefault()?.AirlineGroupName;
            }

            //非同期でCookieは取得できなくなるので退避
            Boolean isAdmin = CookieUtil.IsAdmin(HttpContext);

            //ログは非同期で書き込み
            //Twitterbotは無視
            string ua = HttpContext.Request.Headers["User-Agent"].ToString() ?? string.Empty;
            if (!ua.ToLower().Contains("twitterbot"))
            {
                Task.Run(() =>
                {
                    using (var serviceScope = _services.CreateScope())
                    {
                        using (var context = serviceScope.ServiceProvider.GetService<jafleetContext>())
                        {
                            Log log = new Log
                            {
                                LogDate = DateTime.Now,
                                LogType = LogType.DETAIL,
                                LogDetail = id,
                                UserId = isAdmin.ToString()
                            };

                            context.Log.Add(log);
                            context.SaveChanges();
                        }
                    }
                });
                Console.WriteLine(ua);
            }

            return View("~/Views/AircraftDetail/index.cshtml",model);
        }

        public IActionResult IndexNohead(String id,  AircraftDetailModel model)
        {
            return Index(id, true, false, model);
        }

        public IActionResult IndexNoheadBack(String id, AircraftDetailModel model)
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
