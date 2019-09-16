using jafleet.Manager;
using Microsoft.AspNetCore.Mvc;

namespace jafleet.Controllers
{
    public class ClearCacheController : Controller
    {
        public IActionResult Index()
        {
            int beforeCount = MasterManager.GetScCacheCount();
            MasterManager.ClearSearchConditionDisp();
            return Content($"{beforeCount}→{MasterManager.GetScCacheCount()}");
        }
    }
}