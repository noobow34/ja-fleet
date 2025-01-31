using jafleet.Commons.EF;
using jafleet.Manager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace jafleet.Controllers
{
    public class MasterController : Controller
    {
        private readonly jafleetContext _context;

        public MasterController(jafleetContext context) => _context = context;

        public IActionResult AirlineType(string id)
        {
            if (id == null)
            {
                return Json(MasterManager.AirlineType.Values);
            }
            return Json(MasterManager.AirlineType[id]);
        }

        public IActionResult NamedSearchCondition()
        {
            return Json(MasterManager.NamedSearchCondition);
        }

        public IActionResult SeatConfiguration(string airline, int typeDetailId)
        {
            var type = MasterManager.TypeDetailGroup.Where(td => td.TypeDetailId == typeDetailId).FirstOrDefault()?.TypeCode;
            IEnumerable<SeatConfiguration> q = MasterManager.SeatConfiguration;
            if (!string.IsNullOrEmpty(airline))
            {
                q = q.Where(sc => sc.Airline == airline);
            }
            if (!string.IsNullOrEmpty(type))
            {
                q = q.Where(sc => sc.Type == type);
            }

            return Json(q.ToArray());
        }

        public IActionResult GetAllReg()
        {
            return Json(_context.Aircraft.AsNoTracking().Select(a => a.RegistrationNumber.Substring(2)).ToArray());
        }
    }
}