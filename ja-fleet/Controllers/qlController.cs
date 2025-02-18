using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace jafleet.Controllers
{
    [Authorize]
    public class qlController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
