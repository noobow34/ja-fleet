using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using jafleet.Constants;
using jafleet.EF;
using jafleet.Util;
using Microsoft.AspNetCore.Mvc;

namespace ja_fleet.Controllers
{
    public class logController : Controller
    {
        public String Index(string id)
        {
            DateTime? targetDate = null;
            if (!CookieUtil.IsAdmin(HttpContext))
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(id))
            {
                targetDate = DateTime.Now.Date;
            }
            else if(id.Length == 6)
            {
                DateTime outDate;
                id = "20" + id.Substring(0, 2) + "-" + id.Substring(2, 2) + "-" + id.Substring(4, 2);
                DateTime.TryParseExact(id, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out outDate);
                targetDate = outDate;
            }else if(id == "y")
            {
                targetDate = DateTime.Now.AddDays(-1).Date;
            }

            DateTime target2 = targetDate.Value.AddDays(1);

            List<Log> logs = null;
            using (var context = new jafleetContext())
            {
                logs = context.Log.Where(q => q.LogDate.Value >= targetDate && q.LogDate.Value < target2 && q.UserId != "True" && q.UserId != "U68e05e69b6acbaaf565bc616fdef695d").OrderByDescending(q => q.LogDate).ToList();
            }

            var retsb = new StringBuilder();
            foreach(var log in logs)
            {
                retsb.Append($"[{log.LogDate.Value.ToString("HH:mm:ss")}][{LogType.GetLogTypeName(log.LogType)}]{log.LogDetail}{Environment.NewLine}");
            }

            if(retsb.Length == 0)
            {
                retsb.Append($"{id} no log!");
            }

            return retsb.ToString().Replace(", ",Environment.NewLine);

        }
    }
}