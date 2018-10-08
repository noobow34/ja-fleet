using Microsoft.AspNetCore.Mvc;

namespace jafleet.Controllers
{
    public class Sandbox : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Ex(){
            throw new System.Exception();
        }

    }
}
