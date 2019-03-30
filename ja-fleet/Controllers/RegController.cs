using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using jafleet.Commons.EF;
using Microsoft.EntityFrameworkCore;

namespace jafleet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegController : Controller
    {

        private readonly jafleetContext _context;

        public RegController(jafleetContext context)
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
        [HttpGet("{id}")]
        public ActionResult<string> Get(string id)
        {
            List<AircraftView> list;
            String[] ids = id.ToUpper().Split(",");
            list = _context.AircraftView.AsNoTracking().Where(p => ids.Contains(p.RegistrationNumber)).OrderBy(p => p.DisplayOrder).ToList();

            return Json(list);
        }

    }
}
