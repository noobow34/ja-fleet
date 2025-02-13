﻿using jafleet.Commons.Constants;
using jafleet.Commons.EF;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace jafleet.Controllers
{
    public class WorkingCheckLogController : Controller
    {
        private readonly JafleetContext _context;

        public WorkingCheckLogController(JafleetContext context) => _context = context;
        public IActionResult Index(string id)
        {
            DateTime searchDate;
            if (string.IsNullOrEmpty(id))
            {
                searchDate = DateTime.Now.Date;
            }
            else
            {
                DateTime.TryParseExact(id, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out searchDate);
            }

            string log = string.Join("-----------------", _context.Logs.Where(l => l.LogDate!.Value.Date == searchDate && l.LogType == LogType.WORKING_INFO)
                        .AsNoTracking().OrderByDescending(l => l.LogDate).Select(l => l.LogDetail));

            return Content(log);
        }
    }
}