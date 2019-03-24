using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using jafleet.Models;
using Microsoft.AspNetCore.Diagnostics;
using System;
using jafleet.Util;
using jafleet.Commons.EF;
using jafleet.Commons.Constants;
using Microsoft.Extensions.Configuration;
using Noobow.Commons.Utils;
using jafleet.Manager;

namespace jafleet.Controllers
{
    public class HomeController : Controller
    {

        private readonly jafleetContext _context;
        private readonly IConfiguration _configuration;

        public HomeController(jafleetContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View(new HomeModel());
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var ex = HttpContext.Features.Get<IExceptionHandlerPathFeature>().Error;
            ErrorViewModel model = new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier };
            model.IsAdmin = CookieUtil.IsAdmin(HttpContext);
            model.Ex = ex;

            LineUtil.PushMe($"【エラー発生】\n" +
                            $"{ex.ToString().Split(Environment.NewLine)[0]}\n" +
                            $"{ex.ToString().Split(Environment.NewLine)[1]}",HttpClientManager.GetInstance());

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
