using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using jafleet.Manager;
using jafleet.Models;
using jafleet.Commons.EF;
using jafleet.Util;
using jafleet.Commons.Constants;

namespace jafleet.Controllers
{
    public class EController : Controller
    {

        private readonly jafleetContext _context;

        public EController(jafleetContext context)
        {
            _context = context;
        }

        public IActionResult Index(String id,EditModel model)
        {
            model.IsAdmin = CookieUtil.IsAdmin(HttpContext);

            model.AirlineList = MasterManager.AllAirline;
            model.TypeList = MasterManager.Type;
            model.TypeDetailList = _context.TypeDetail.ToArray();
            model.OperationList = MasterManager.Operation;
            model.WiFiList = MasterManager.Wifi;
            model.NotUpdateDate = true;

            if (string.IsNullOrEmpty(id))
            {
                model.Aircraft = new Aircraft();
                model.IsNew = true;
            }
            else
            {
                model.Aircraft = _context.Aircraft.Where(p => p.RegistrationNumber == id.ToUpper()).FirstOrDefault();
            }

            if(model.Aircraft == null){
                model.Aircraft = new Aircraft();
                model.Aircraft.RegistrationNumber = id.ToUpper();
                model.IsNew = true;
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult Store( EditModel model){
            try{
                String reg = model.Aircraft.RegistrationNumber;
                if (!model.NotUpdateDate){
                    model.Aircraft.UpdateTime = DateTime.Now;
                }
                model.Aircraft.ActualUpdateTime = DateTime.Now;
                if (model.IsNew)
                {
                    model.Aircraft.CreationTime = DateTime.Now;
                    _context.Aircraft.Add(model.Aircraft);
                }
                else
                {
                    _context.Entry(model.Aircraft).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                }
                _context.SaveChanges();
            }catch(Exception ex){
                model.ex = ex;
            }

            model.AirlineList = MasterManager.AllAirline;
            model.TypeList = MasterManager.Type;
            model.OperationList = MasterManager.Operation;
            model.WiFiList = MasterManager.Wifi;
            return Redirect("/E/" + model.Aircraft.RegistrationNumber);
        }
    }
}
