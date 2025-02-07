using Microsoft.AspNetCore.Mvc;
using jafleet.Commons.EF;
using Microsoft.EntityFrameworkCore;

namespace jafleet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AircraftWithHistoryController : Controller
    {

        private readonly JafleetContext _context;

        public AircraftWithHistoryController(JafleetContext context) => _context = context;

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(string id)
        {
            var list = new List<AircraftViewBase>();
            var latest = _context.AircraftViews.AsNoTracking().Where(p => p.RegistrationNumber == id.ToUpper()).First();
            var history = _context.AircraftHistoryViews.AsNoTracking().Where(p => p.RegistrationNumber == id.ToUpper()).OrderByDescending(p => p.Seq).ToList();

            list.Add(latest);
            list.AddRange(history);

            for (int i = 0; i <= list.Count - 2; i++)
            {
                list[i].getDifferenceWith(list[i + 1]);
            }

            return Json(list);
        }

    }
}
