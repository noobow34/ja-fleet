using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using jafleet.Models;
using jafleet.EF;
using jafleet.Constants;

namespace jafleet.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            HomeModel hm = new HomeModel();
            using (var context = new jafleetContext())
            {
                hm.ana = context.Airline.Where(p => p.AirlineGroupCode == AirlineGroupCode.ANAGroup).OrderBy(p => p.DisplayOrder).ToList();
                hm.jal = context.Airline.Where(p => p.AirlineGroupCode == AirlineGroupCode.JALGroup).OrderBy(p => p.DisplayOrder).ToList();
                hm.lcc = context.Airline.Where(p => p.AirlineGroupCode == AirlineGroupCode.LCC).OrderBy(p => p.DisplayOrder).ToList();
                hm.other = context.Airline.Where(p => p.AirlineGroupCode == AirlineGroupCode.Other).OrderBy(p => p.DisplayOrder).ToList();
            }

            return View(hm);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
