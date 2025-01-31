using jafleet.Commons.EF;
using Microsoft.AspNetCore.Mvc;

namespace jafleet.Controllers
{
    public class CheckController : Controller
    {
        private readonly jafleetContext _context;

        public CheckController(jafleetContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return Content(_context.AircraftView.Count().ToString());
        }
    }
}