using jafleet.Manager;
using Microsoft.AspNetCore.Mvc;

namespace ja_fleet.Controllers
{
    public class MasterController : Controller
    {
        public IActionResult AirlineType(string id)
        {
            if(id == null)
            {
                return Json(MasterManager.AirlineType.Values);
            }
            return Json(MasterManager.AirlineType[id]);
        }
    }
}