using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using jafleet.EF;
using jafleet.Constants;

namespace jafleet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AirlineGroupController : Controller
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
        [HttpGet("{id}/{id2?}")]
        public ActionResult<string> Get(string id, string id2, [FromQuery]Boolean includeRetire)
        {
            List<AircraftView> list;
            String[] ids = id.ToUpper().Split(",");
            using (var context = new jafleetContext())
            {
                var q = context.AircraftView.Where(p => ids.Contains(p.AirlineGroupCode));
                if (!string.IsNullOrEmpty(id2))
                {
                    q = q.Where(p => p.TypeCode == id2);
                }
                if (!includeRetire){
                    q = q.Where(p => p.OperationCode != OperationCode.RETIRE_UNREGISTERED);
                }
                list = q.OrderBy(p => p.DisplayOrder).ToList();
            }
            return Json(list);
        }

    }
}
