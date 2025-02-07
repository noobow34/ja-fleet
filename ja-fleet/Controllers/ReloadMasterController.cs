using Microsoft.AspNetCore.Mvc;
using jafleet.Manager;
using jafleet.Commons.EF;

namespace jafleet.Controllers
{
    public class ReloadMasterController : Controller
    {
        private readonly JafleetContext _context;

        public ReloadMasterController(JafleetContext context) => _context = context;

        public IActionResult Index()
        {
            try
            {
                MasterManager.ReadAll(_context);
                return Content("Success");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }
    }
}
