using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using jafleet.EF;

namespace jafleet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegController : Controller
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
        public ActionResult<string> Get(string id)
        {
            List<AircraftView> list;
            String[] ids = id.ToUpper().Split(",");
            using (var context = new jafleetContext())
            {
                list = context.AircraftView.Where(p => ids.Contains(p.RegistrationNumber)).OrderBy(p => p.DisplayOrder).ToList();
            }
            return Json(list);
        }

    }
}
