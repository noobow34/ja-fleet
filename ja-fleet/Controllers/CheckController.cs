using jafleet.Commons.EF;
using Microsoft.AspNetCore.Mvc;

namespace jafleet.Controllers
{
    public class CheckController : Controller
    {
        private readonly JafleetContext _context;

        public CheckController(JafleetContext context) => _context = context;

        public IActionResult Index()
        {
            return Content(_context.AircraftViews.Count().ToString());
        }
    }
}