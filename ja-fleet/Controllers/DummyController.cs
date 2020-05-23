using Microsoft.AspNetCore.Mvc;

namespace jafleet.Controllers
{
    public class DummyController : Controller
    {
        public IActionResult Index()
        {
            return Content(string.Empty);
        }
    }
}