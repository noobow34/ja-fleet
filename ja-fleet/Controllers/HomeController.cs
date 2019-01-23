using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using jafleet.Models;
using Microsoft.AspNetCore.Diagnostics;
using System;
using jafleet.Util;
using jafleet.Commons.EF;
using jafleet.Commons.Constants;

namespace jafleet.Controllers
{
    public class HomeController : Controller
    {

        private readonly jafleetContext _context;

        public HomeController(jafleetContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var ex = HttpContext.Features.Get<IExceptionHandlerPathFeature>().Error;
            ErrorViewModel model = new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier };
            model.IsAdmin = CookieUtil.IsAdmin(HttpContext);
            model.Ex = ex;

            Log log = new Log
            {
                LogDate = DateTime.Now
                ,LogType = LogType.EXCEPTION
                ,LogDetail = ex.ToString()
                ,UserId = CookieUtil.IsAdmin(HttpContext).ToString()
            };
            _context.Log.Add(log);
            _context.SaveChanges();

            return View(model);
        }
    }
}
