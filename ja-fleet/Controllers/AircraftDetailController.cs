using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using jafleet.Models;
using jafleet.Commons.EF;
using jafleet.Manager;
using System.Net.Http;
using AngleSharp.Html.Parser;
using System;
using jafleet.Util;
using jafleet.Commons.Constants;

namespace jafleet.Controllers
{
    public class AircraftDetailController : Controller
    {

        private readonly jafleetContext _context;

        public AircraftDetailController(jafleetContext context)
        {
            _context = context;
        }

        public IActionResult Index(String id, [FromQuery]Boolean nohead, AircraftDetailModel model)
        {

            ViewData["Title"] = id;
            ViewData["api"] = "/api/airlinegroup/";

            model.NoHead = nohead;
            model.IsAdmin = CookieUtil.IsAdmin(HttpContext);

            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
