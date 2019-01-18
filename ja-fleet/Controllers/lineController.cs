using jafleet.Commons.Constants;
using jafleet.Commons.EF;
using jafleet.Util;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Hosting.Internal.HostingApplication;

namespace ja_fleet.Controllers
{
    public class lineController : Controller
    {
        public IActionResult Index()
        {
            Task.Run(() => {
                var lineLinklog = new Log
                {
                    LogType = LogType.LINE_LINK,
                    UserId = CookieUtil.IsAdmin(HttpContext).ToString(),
                    LogDate = DateTime.Now
                };
                using (var context = new jafleetContext())
                {
                    context.Log.Add(lineLinklog);
                    context.SaveChanges();
                }

            });
            return Redirect("https://line.me/R/ti/p/BTy1CuBCzF");
        }
    }
}