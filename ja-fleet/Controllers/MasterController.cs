using jafleet.Manager;
using Microsoft.AspNetCore.Mvc;

namespace jafleet.Controllers
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

        public IActionResult NamedSearchCondition()
        {
            return Json(MasterManager.NamedSearchCondition);
        }
    }
}