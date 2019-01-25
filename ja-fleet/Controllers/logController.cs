using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using jafleet.Commons.Constants;
using jafleet.Commons.EF;
using jafleet.Util;
using Microsoft.AspNetCore.Mvc;

namespace ja_fleet.Controllers
{
    public class logController : Controller
    {

        private readonly jafleetContext _context;

        public logController(jafleetContext context)
        {
            _context = context;
        }

        public IActionResult Index(string id)
        {
            if (!CookieUtil.IsAdmin(HttpContext))
            {
                return NotFound();
            }

            DateTime? targetDate = null;
            if (string.IsNullOrEmpty(id))
            {
                targetDate = DateTime.Now.Date;
            }
            else if(id.Length == 6)
            {
                DateTime outDate;
                id = "20" + id;
                DateTime.TryParseExact(id, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out outDate);
                targetDate = outDate;
            }else if(id == "y")
            {
                targetDate = DateTime.Now.AddDays(-1).Date;
            }

            List<Log> logs = null;
            logs = _context.Log.Where(q => q.LogDateYyyyMmDd == targetDate.Value.ToString("yyyyMMdd") && q.IsAdmin == "0").OrderByDescending(q => q.LogId).ToList();

            var retsb = new StringBuilder();
            retsb.Append(DateTime.Now.ToString($"--HH:mm:ss--{Environment.NewLine}"));
            foreach(var log in logs)
            {
                retsb.Append($"[{log.LogDate.Value.ToString("HH:mm:ss")}][{LogType.GetLogTypeName(log.LogType)}]{log.LogDetail}{Environment.NewLine}");
            }

            if(retsb.Length == 0)
            {
                retsb.Append($"{id} no log!");
            }

            return Content(retsb.ToString());

        }
    }
}