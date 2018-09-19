using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using jafleet.Models;
using jafleet.Excel;
using jafleet.EF;
using jafleet.Constants;

namespace jafleet.Controllers
{
    public class ANAController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public JsonResult Data(){
            List<AircraftView> list;
            using (var context = new jafleetContext()){
                list = context.AircraftView.Where(p => p.AirlineGroupCode == AirlineGroupCpode.ANAGroup).OrderBy(p => p.DisplayOrder).ToList();
            }
            return Json(list);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
