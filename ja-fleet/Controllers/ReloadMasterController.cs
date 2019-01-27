using System;
using Microsoft.AspNetCore.Mvc;
using jafleet.Manager;

namespace jafleet.Controllers
{
    public class ReloadMasterController : Controller
    {
        public IActionResult Index()
        {
            try{
                MasterManager.ReadAll();
                return Content("Success");
            }
            catch(Exception ex){
                return Content(ex.ToString());
            }
        }
    }
}
