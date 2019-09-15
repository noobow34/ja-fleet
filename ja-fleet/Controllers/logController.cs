using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using jafleet.Commons.Constants;
using jafleet.Commons.EF;
using jafleet.Manager;
using jafleet.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace jafleet.Controllers
{
    public class logController : Controller
    {

        private readonly jafleetContext _context;

        public logController(jafleetContext context)
        {
            _context = context;
        }

        public IActionResult Yesterday()
        {
            return Index("y");
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
            logs = _context.Log.AsNoTracking().Where(q => q.LogDateYyyyMmDd == targetDate.Value.ToString("yyyyMMdd") && q.IsAdmin == "0").OrderByDescending(q => q.LogId).ToList();

            var logScKeys = logs.Where(sc => sc.LogType == LogType.SEARCH).Select(scc => scc.LogDetail);
            var scCache = MasterManager.GetSearchConditionDisps(logScKeys, _context);

            var retsb = new StringBuilder();
            foreach(var log in logs)
            {
                string logDetail;
                if(log.LogType == LogType.SEARCH)
                {
                    logDetail = scCache[log.LogDetail] + log.Additional;
                }
                else
                {
                    logDetail = log.LogDetail;
                }
                retsb.Append($"[{log.LogDate.Value.ToString("HH:mm:ss")}][{LogType.GetLogTypeName(log.LogType)}]{logDetail}{Environment.NewLine}");
            }

            string head = DateTime.Now.ToString($"--HH:mm:ss--:{MasterManager.GetScCacheCount()}{Environment.NewLine}");

            return Content(head + retsb.ToString());

        }
    }
}