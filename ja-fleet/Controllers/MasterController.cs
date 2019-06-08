using jafleet.Manager;
using Microsoft.AspNetCore.Mvc;

namespace ja_fleet.Controllers
{
    public class MasterController : Controller
    {
        public IActionResult AirlineType(string id) => Json(MasterManager.AirlineType[id]);
    }
}