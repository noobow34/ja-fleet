using Microsoft.AspNetCore.Mvc;

namespace ja_fleet.Controllers
{
    public class lineController : Controller
    {
        public IActionResult Index()
        {
            return Redirect("https://line.me/R/ti/p/BTy1CuBCzF");
        }
    }
}