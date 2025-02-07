using Microsoft.AspNetCore.Mvc;
using jafleet.Commons.EF;
using jafleet.Commons.Constants;
using Microsoft.EntityFrameworkCore;

namespace jafleet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TypeController : Controller
    {

        private readonly JafleetContext _context;

        public TypeController(JafleetContext context) => _context = context;

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
        public ActionResult<string> Get(string id, [FromQuery] bool includeRetire)
        {
            List<AircraftView> list;
            string[] ids = id.ToUpper().Split(",");
            var q = _context.AircraftViews.AsNoTracking().Where(p => ids.Contains(p.TypeCode));
            if (!includeRetire)
            {
                q = q.Where(p => p.OperationCode != OperationCode.RETIRE_UNREGISTERED);
            }
            list = q.OrderBy(p => p.DisplayOrder).ToList();

            return Json(list);
        }

    }
}
