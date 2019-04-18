using Microsoft.AspNetCore.Mvc;

namespace jafleet.Controllers
{
    public class CheckErrorController : Controller
    {
        public IActionResult Index()
        {
            return new NotFoundResult();
        }
    }
}