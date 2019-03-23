using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using jafleet.Commons.EF;
using jafleet.Commons.Constants;

namespace jafleet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AircraftWithHistoryController : Controller
    {

        private readonly jafleetContext _context;

        public AircraftWithHistoryController(jafleetContext context)
        {
            _context = context;
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(string id)
        {
            var list = new List<Object>();
            var latest = _context.AircraftView.Where(p => p.RegistrationNumber == id.ToUpper()).FirstOrDefault();
            var history = _context.AircraftHistoryView.Where(p => p.RegistrationNumber == id.ToUpper()).OrderByDescending(p => p.Seq).ToList();

            list.Add(latest);
            list.AddRange(history);

            return Json(list);
        }

    }
}
