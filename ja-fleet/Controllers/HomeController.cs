using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using jafleet.Models;
using Microsoft.AspNetCore.Diagnostics;
using System;

namespace jafleet.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            Console.WriteLine("test");
            Console.WriteLine(exceptionFeature.Error);

            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
