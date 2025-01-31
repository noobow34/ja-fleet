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
                .Join(_context.AircraftView.Where(a => OperationCode.IN_OPERATION.Contains(a.OperationCode)), a => a.RegistrationNumber,ws => ws.RegistrationNumber
                    , (ws,a) => new{ ws.RegistrationNumber, FlightDate = ws.FlightDate!.ToString() ?? " 不明" ,ws.FromAp,ws.ToAp,ws.FlightNumber,ws.Status,ws.Working,a.TypeDetailName })
                .OrderBy(ws => ws.FlightDate).ToArray();

            return Json(list);
        }
    }
}