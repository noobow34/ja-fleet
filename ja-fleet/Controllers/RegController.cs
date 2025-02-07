using Microsoft.AspNetCore.Mvc;
using jafleet.Commons.EF;
using Microsoft.EntityFrameworkCore;

namespace jafleet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegController : Controller
    {

        private readonly JafleetContext _context;

        public RegController(JafleetContext context) => _context = context;

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            List<AircraftView> list;
            list = _context.AircraftViews.AsNoTracking().OrderBy(p => p.DisplayOrder).ToList();

            return Json(list);
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(string id)
        {
            List<AircraftView> list;
            string[] ids = id.ToUpper().Split(",");
            list = _context.AircraftViews.AsNoTracking().Where(p => ids.Contains(p.RegistrationNumber)).OrderBy(p => p.DisplayOrder).ToList();

            return Json(list);
        }

    }
}
