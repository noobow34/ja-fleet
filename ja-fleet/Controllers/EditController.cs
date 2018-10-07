using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using jafleet.Manager;
using jafleet.Models;
using jafleet.EF;

namespace jafleet.Controllers
{
    public class EditController : Controller
    {
        public IActionResult Index(String id,EditModel model)
        {
            using (var context = new jafleetContext())
            {
                model.AirlineList = MasterManager.AllAirline;
                model.TypeList = MasterManager.Type;
                model.OperationList = MasterManager.Operation;
                model.WiFiList = MasterManager.Wifi;

                if (string.IsNullOrEmpty(id))
                {
                    model.Aircraft = new Aircraft();
                    model.IsNew = true;
                }
                else
                {
                    model.Aircraft = context.Aircraft.Where(p => p.RegistrationNumber == id.ToUpper()).FirstOrDefault();
                }

            }
            return View(model);
        }

        [HttpPost]
        public IActionResult Store( EditModel model){
            try{
                String reg = model.Aircraft.RegistrationNumber;
                using (var context = new jafleetContext())
                {
                    if (model.IsNew)
                    {
                        context.Aircraft.Add(model.Aircraft);
                    }
                    else
                    {
                        context.Entry(model.Aircraft).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    }
                    context.SaveChanges();
                }
            }catch(Exception ex){
                model.ex = ex;
            }

            model.AirlineList = MasterManager.AllAirline;
            model.TypeList = MasterManager.Type;
            model.OperationList = MasterManager.Operation;
            model.WiFiList = MasterManager.Wifi;
            return View("Index", model);
            //return Redirect("/Edit/" + model.Aircraft.RegistrationNumber);
        }
    }
}
