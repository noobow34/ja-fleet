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
            if (!CookieUtil.IsAdmin(HttpContext))
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(id))
            {
                id = DateTime.Now.ToString("yyyy-MM-dd");
            }
            else
            {
                id = "20" + id.Substring(0, 2) + "-" + id.Substring(2, 2) + "-" + id.Substring(4, 2);
            }

            List<Log> logs = null;
            using (var context = new jafleetContext())
            {
                logs = context.Log.Where(q => q.LogDate.StartsWith(id)).Where(q => q.UserId != "True").Where(q => q.UserId != "U68e05e69b6acbaaf565bc616fdef695d").OrderByDescending(q => q.LogDate).ToList();
            }

            var retsb = new StringBuilder();
            foreach(var log in logs)
            {
                retsb.Append($"【{log.LogDate}】【{LogType.GetLogTypeName(log.LogType)}】{log.LogDetail}{Environment.NewLine}");
            }

            if(retsb.Length == 0)
            {
                retsb.Append($"{id} no log!");
            }

            return retsb.ToString();
        }
    }
}