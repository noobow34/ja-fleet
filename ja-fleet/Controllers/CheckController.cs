using Microsoft.AspNetCore.Mvc;

namespace jafleet.Controllers
{
    public class CheckController : Controller
    {
        public IActionResult Index()
        {
            return Content("1");
        }
    }
}