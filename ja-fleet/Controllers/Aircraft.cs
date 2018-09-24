using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using jafleet.Models;
using jafleet.EF;

namespace jafleet.Controllers
{
    public class AircraftController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AirlineGroup(string id){
            id = id.ToUpper();

            string groupName;
            using (var context = new jafleetContext())
            {
                groupName = context.AirlineGroup.FirstOrDefault(p => p.AirlineGroupCode == id)?.AirlineGroupName;
            }

            ViewData["Title"] = groupName;
            ViewData["TableId"] = id;
            ViewData["api"] = "/api/airlinegroup/" + id;
            return View("~/Views/Aircraft/index.cshtml");
        }

        public IActionResult Airline(string id)
        {
            id = id.ToUpper();

            string airlineName;
            using (var context = new jafleetContext())
            {
                airlineName = context.Airline.FirstOrDefault(p => p.AirlineGroupCode == id)?.AirlineNameJpShort;
            }

            ViewData["Title"] = airlineName;
            ViewData["TableId"] = id;
            ViewData["api"] = "/api/airline/" + id;
            return View("~/Views/Aircraft/index.cshtml");
        }

        public IActionResult Type(string id)
        {
            id = id.ToUpper();

            string groupName;
            using (var context = new jafleetContext())
            {
                groupName = context.Type.FirstOrDefault(p => p.TypeCode == id)?.TypeName;
            }

            ViewData["Title"] = groupName;
            ViewData["TableId"] = id;
            ViewData["api"] = "/api/type/" + id;
            return View("~/Views/Aircraft/index.cshtml");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
