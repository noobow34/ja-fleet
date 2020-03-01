using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using jafleet.Commons.Constants;
using jafleet.Commons.EF;
using jafleet.Models;
using Microsoft.AspNetCore.Mvc;

namespace jafleet.Controllers
{
    public class NotWorkingInfoController : Controller
    {
        private readonly jafleetContext _context;

        public NotWorkingInfoController(jafleetContext context)
        {
            _context = context;
        }

        public IActionResult Index(NotWorkingInfoModel model)
        {
            model.Title = "非稼働情報";

            return View(model);
        }

        public IActionResult GetInfo(string fromDate)
        {
            DateTime searchFromDate;
            if(!DateTime.TryParse(fromDate, out searchFromDate))
            {
                searchFromDate = DateTime.Now.AddDays(-3).Date;
            }
            var list = _context.WorkingStatus.Where(ws => !ws.Working.Value && (!ws.FlightDate.HasValue || ws.FlightDate.Value.Date <= searchFromDate))
                .Join(_context.Aircraft.Where(a => OperationCode.IN_OPERATION.Contains(a.OperationCode)), a => a.RegistrationNumber,ws => ws.RegistrationNumber
                    , (ws,a) => new{ RegistrationNumber = ws.RegistrationNumber, FlightDate = ws.FlightDate!.ToString() ?? " 不明" ,FromAp = ws.FromAp,ToAp = ws.ToAp,FlightNumber = ws.FlightNumber,Status = ws.Status,Working = ws.Working })
                .ToArray();

            return Json(list);
        }
    }
}