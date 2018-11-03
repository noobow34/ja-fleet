using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using jafleet.Models;
using jafleet.EF;
using jafleet.Manager;

namespace jafleet.Controllers
{
    public class AircraftController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AirlineGroup(string id, string id2)
        {
            id = id.ToUpper();
            id2 = id2?.ToUpper();

            string groupName;
            using (var context = new jafleetContext())
            {
                groupName = context.AirlineGroup.FirstOrDefault(p => p.AirlineGroupCode == id)?.AirlineGroupName;
            }

            ViewData["Title"] = groupName;
            ViewData["TableId"] = id;
            ViewData["api"] = "/api/airlinegroup/" + id;
            if (!string.IsNullOrEmpty(id2))
            {
                ViewData["Title"] += ("・" + MasterManager.Type.Where(p => p.TypeCode == id2).First()?.TypeName);
                ViewData["api"] += ("/" + id2);
            }
            return View("~/Views/Aircraft/index.cshtml");
        }

        public IActionResult Airline(string id, string id2)
        {
            id = id.ToUpper();
            id2 = id2?.ToUpper();

            string airlineName;
            using (var context = new jafleetContext())
            {
                airlineName = context.Airline.FirstOrDefault(p => p.AirlineCode == id)?.AirlineNameJpShort;
            }

            ViewData["Title"] = airlineName;
            ViewData["TableId"] = id;
            ViewData["api"] = "/api/airline/" + id;
            if (!string.IsNullOrEmpty(id2))
            {
                ViewData["Title"] += ("・" + MasterManager.Type.Where(p => p.TypeCode == id2).First()?.TypeName);
                ViewData["api"] += ("/" + id2);
            }
            return View("~/Views/Aircraft/index.cshtml");
        }

        public IActionResult Type(string id)
        {
            id = id.ToUpper();

            string typeName;
            using (var context = new jafleetContext())
            {
                typeName = context.Type.FirstOrDefault(p => p.TypeCode == id)?.TypeName;
            }

            ViewData["Title"] = typeName;
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
