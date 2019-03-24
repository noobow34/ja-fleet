using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using jafleet.Models;
using jafleet.Commons.EF;
using jafleet.Manager;
using System.Net.Http;
using AngleSharp.Html.Parser;
using System;
using jafleet.Util;
using jafleet.Commons.Constants;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

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
            ViewData["api"] = "/api/aircraftwithhistory/" + id;
            model.IsDetail = true;

            model.NoHead = nohead;
            model.IsAdmin = CookieUtil.IsAdmin(HttpContext);
            model.Reg = id;
            model.NeedBack = needback;

            //ログは非同期で書き込み
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
                            UserId = CookieUtil.IsAdmin(HttpContext).ToString()
                        };

                        context.Log.Add(log);
                        context.SaveChanges();
                    }
                }
            });

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
