﻿using jafleet.Commons.Constants;
using jafleet.Commons.EF;
using jafleet.Models;
using Microsoft.AspNetCore.Mvc;

namespace jafleet.Controllers
{
    public class NotWorkingInfoController : Controller
    {
        private readonly JafleetContext _context;

        public NotWorkingInfoController(JafleetContext context) => _context = context;

        public IActionResult Index(NotWorkingInfoModel model)
        {
            model.Title = "非稼働情報";

            return View(model);
        }

        public IActionResult GetInfo(string fromDate)
        {
            if (!DateTime.TryParse(fromDate, out DateTime searchFromDate))
            {
                searchFromDate = DateTime.Now.AddDays(-3).Date;
            }
            var list = _context.WorkingStatuses.Where(ws => (ws.Working != null && !ws.Working.Value) && (!ws.FlightDate.HasValue || ws.FlightDate.Value.Date <= searchFromDate))
                .Join(_context.AircraftViews.Where(a => OperationCode.IN_OPERATION.Contains(a.OperationCode)), a => a.RegistrationNumber, ws => ws.RegistrationNumber
                    , (ws, a) => new { ws.RegistrationNumber, FlightDate = ws.FlightDate!.ToString() ?? " 不明", ws.FromAp, ws.ToAp, ws.FlightNumber, ws.Status, ws.Working, a.TypeDetailName })
                .OrderBy(ws => ws.FlightDate).ToArray();

            return Json(list);
        }
    }
}