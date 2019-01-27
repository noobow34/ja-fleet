﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using jafleet.Commons.EF;
using jafleet.Commons.Constants;

namespace jafleet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AirlineController : Controller
    {

        private readonly jafleetContext _context;

        public AirlineController(jafleetContext context)
        {
            _context = context;
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            List<AircraftView> list;
            list = _context.AircraftView.OrderBy(p => p.DisplayOrder).ToList();

            return Json(list);
        }

        // GET api/values/5
        [HttpGet("{id}/{id2?}")]
        public ActionResult<string> Get(string id, string id2, [FromQuery]Boolean includeRetire)
        {
            List<AircraftView> list;
            String[] ids = id.ToUpper().Split(",");
            id2 = id2?.ToUpper();
            var q = _context.AircraftView.Where(p => ids.Contains(p.Airline));
            if (!string.IsNullOrEmpty(id2))
            {
                q = q.Where(p => p.TypeCode == id2);
            }
            if (!includeRetire)
            {
                q = q.Where(p => p.OperationCode != OperationCode.RETIRE_UNREGISTERED);
            }
            list = q.OrderBy(p => p.DisplayOrder).ToList();

            return Json(list);
        }

    }
}
