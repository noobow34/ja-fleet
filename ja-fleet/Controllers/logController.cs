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

            //LEFT JOIN
            var logs = _context.Log.AsNoTracking()
                .GroupJoin(_context.SearchCondition,l => l.LogDetail,sc => sc.SearchConditionKey,(l,sc) => new {l,sc})
                .SelectMany(x => x.sc.DefaultIfEmpty(),(l,sc) => new { l.l.LogDetail,l.l.LogDateYyyyMmDd,l.l.IsAdmin,l.l.LogId,l.l.LogType,l.l.Additional,l.l.LogDate,sc.SearchConditionJson })
                .Where(q => q.LogDateYyyyMmDd == targetDate.Value.ToString("yyyyMMdd") && q.IsAdmin == "0").OrderByDescending(q => q.LogId).ToList();

            var retsb = new StringBuilder();
            retsb.Append(DateTime.Now.ToString($"--HH:mm:ss--{Environment.NewLine}"));
            foreach(var log in logs)
            {
                string logDetail = log.SearchConditionJson ?? log.LogDetail + log.Additional;
                retsb.Append($"[{log.LogDate.Value.ToString("HH:mm:ss")}][{LogType.GetLogTypeName(log.LogType)}]{logDetail}{Environment.NewLine}");
            }

            return Content(retsb.ToString());

        }
    }
}