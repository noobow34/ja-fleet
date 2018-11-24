using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using jafleet.Models;
using jafleet.EF;
using System.Text.RegularExpressions;
using jafleet.Manager;
using jafleet.Util;
using jafleet.Constants;

namespace jafleet.Controllers
{
    public class SearchController : Controller
    {
        private static NLog.Logger infologger = NLog.LogManager.GetLogger("infologger");

        public IActionResult Index(SearchModel model)
        {
            model.IsAdmin = CookieUtil.IsAdmin(HttpContext);

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
            String reg = string.Empty;
            String[] airline;
            String[] type;
            String[] operation;
            String[] wifi;
            String registrationDate;

            if (model.RegistrationNumber == null){
                reg = "*";
            }else{
                if (!model.RegistrationNumber.ToUpper().StartsWith("JA"))
                {
                    reg = "JA";
                }
                reg = reg += model.RegistrationNumber.ToUpper().Replace("*", ".*").Replace("_", ".");
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
                    wifi = model.WiFiCode.Split("|");
                    query = query.Where(p => wifi.Contains(p.WifiCode));
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

                if(model.Remarks == "1"){
                    query = query.Where(p => String.IsNullOrEmpty(p.Remarks));
                }else if (model.Remarks == "2")
                {
                    query = query.Where(p => !String.IsNullOrEmpty(p.Remarks));
                }

                searchResult = query.OrderBy(p => p.DisplayOrder).ToArray();
                string logDetail = model.ToString() + ",件数：" + searchResult.Length.ToString();
                if (!CookieUtil.IsAdmin(HttpContext))
                {
                    infologger.Info(logDetail);
                }

                Log log = new Log
                {
                    LogDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    ,LogType = LogType.SEARCH
                    ,LogDetail = logDetail
                    ,UserId = CookieUtil.IsAdmin(HttpContext).ToString()
                };
                context.Log.Add(log);
                context.SaveChanges();

            }
            return Json(searchResult);
        }
    }
}
