using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using jafleet.Commons.EF;
using jafleet.Commons.Constants;
using Microsoft.EntityFrameworkCore;

namespace jafleet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AirlineGroupController : Controller
    {

        private readonly jafleetContext _context;

        public AirlineGroupController(jafleetContext context)
        {
            _context = context;
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            List<AircraftView> list;
            list = _context.AircraftView.AsNoTracking().OrderBy(p => p.DisplayOrder).ToList();

            return Json(list);
        }

        // GET api/values/5
        [HttpGet("{id}/{id2?}")]
        public ActionResult<string> Get(string id, string id2, [FromQuery]bool includeRetire)
        {
            List<AircraftView> list;
            String[] ids = id.ToUpper().Split(",");
            id2 = id2?.ToUpper();
            var q = _context.AircraftView.AsNoTracking().Where(p => ids.Contains(p.AirlineGroupCode));
            if (!string.IsNullOrEmpty(id2))
            {
                q = q.Where(p => p.TypeCode == id2);
            }
            if (!includeRetire){
                q = q.Where(p => p.OperationCode != OperationCode.RETIRE_UNREGISTERED);
            }
            list = q.OrderBy(p => p.DisplayOrder).ToList();

            return Json(list);
        }

    }
}
