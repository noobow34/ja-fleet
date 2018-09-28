using System;
using Microsoft.AspNetCore.Mvc;
using jafleet.Manager;

namespace jafleet.Controllers
{
    public class ReloadMasterController : Controller
    {
        public String Index()
        {
            try{
                MasterManager.ReadAll();
                return "Success";
            }
            catch(Exception ex){
                return ex.ToString();
            }
        }
    }
}
