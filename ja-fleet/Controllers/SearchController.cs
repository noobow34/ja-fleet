using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using jafleet.Models;
using jafleet.EF;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices.WindowsRuntime;
using jafleet.Manager;

namespace jafleet.Controllers
{
    public class SearchController : Controller
    {
        public IActionResult Index(SearchModel model)
        {
            using (var context = new jafleetContext())
            {
                model.AirlineList = MasterManager.AllAirline;
                model.TypeList = MasterManager.Type;
                model.OperationList = MasterManager.Operation;
                model.WiFiList = MasterManager.Wifi;
            }
            return View(model);
        }

        public IActionResult DoSearch(SearchModel model)
        {
            if(model.IsLoading){
                return Json(new List<AircraftView>());
            }
            AircraftView[] searchResult = null;
            String reg;
            String[] airline;
            String[] type;
            String[] operation;
            String wifi;
            String registrationDate;
            if (model.RegistrationNumber == null){
                reg = "*";
            }else{
                reg = model.RegistrationNumber.ToUpper().Replace("*", ".*").Replace("_", ".");
            }
            using (var context = new jafleetContext())
            {
                var regex = new Regex("^" + reg + "$");
                var query = context.AircraftView.Where(p => regex.IsMatch(p.RegistrationNumber));

                if(!String.IsNullOrEmpty(model.Airline)){
                    airline = model.Airline.Split("|");
                    query = query.Where(p => airline.Contains(p.Airline));
                }

                if(!String.IsNullOrEmpty(model.Type)){
                    type = model.Type.Split("|");
                    query = query.Where(p => type.Contains(p.TypeCode));
                }

                if(!String.IsNullOrEmpty(model.WiFiCode)){
                    wifi = model.WiFiCode;
                    query = query.Where(p => p.WifiCode == wifi);
                }

                if(!String.IsNullOrEmpty(model.RegistrationDate)){
                    registrationDate = model.RegistrationDate;
                    if(model.RegistrationSelection == "0"){
                        query = query.Where(p => p.RegisterDate.StartsWith(registrationDate));
                    }else if(model.RegistrationSelection == "1"){
                        registrationDate += "zzz";
                        query = query.Where(p => p.RegisterDate.CompareTo(registrationDate) <= 0);
                    }else if (model.RegistrationSelection == "2")
                    {
                        registrationDate += "zzz";
                        query = query.Where(p => p.RegisterDate.CompareTo(registrationDate) >= 0);
                    }

                }

                if(!String.IsNullOrEmpty(model.OperationCode)){
                    operation = model.OperationCode.Split("|");
                    query = query.Where(p => operation.Contains(p.OperationCode));
                }

                /*if(model.ExistRemarks){
                    query = query.Where(p => !String.IsNullOrEmpty(p.Remarks));
                }*/
                searchResult = query.OrderBy(p => p.DisplayOrder).ToArray();
            }
            return Json(searchResult);
        }
    }
}
