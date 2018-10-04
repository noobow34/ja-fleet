using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using jafleet.EF;

namespace jafleet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AirlineController : Controller
    {
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            List<AircraftView> list;
            using (var context = new jafleetContext())
            {
                list = context.AircraftView.ToList();
            }
            return Json(list);
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(string id, [FromQuery]Boolean includeRetire)
        {
            List<AircraftView> list;
            String[] ids = id.ToUpper().Split(",");
            using (var context = new jafleetContext())
            {
                var q = context.AircraftView.Where(p => ids.Contains(p.AirlineGroupCode));
                if (!includeRetire)
                {
                    q = q.Where(p => p.OperationCode != "8");
                }
                list = q.OrderBy(p => p.DisplayOrder).ToList();
            }
            return Json(list);
        }

    }
}
