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
using AutoMapper;

namespace jafleet.Controllers
{
    public class SearchController : Controller
    {

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
            if(model.IsLoading && !model.IsDirect){
                //初回ロードかつダイレクト指定ではない場合は空リストを返す
                return Json(new List<AircraftView>());
            }
            AircraftView[] searchResult = null;
            var regList = new List<string>();
            String[] airline;
            String[] type;
            String[] operation;
            String[] wifi;
            String registrationDate;
            string schash;

            if (model.RegistrationNumber == null){
                //指定されていない場合は全県
                regList.Add("*");
            }else{
                //|区切りで複数件を処理
                foreach(string r in model.RegistrationNumber.ToUpper().Split("|")){
                    string reg = string.Empty;
                    if (!r.StartsWith("JA"))
                    {
                        //JAがついていなければ付加
                        reg = "JA";
                    }
                    //画面のワイルドカード仕様から.NETのワイルドカード仕様に変換
                    reg = reg += r.ToUpper().Replace("*", ".*").Replace("_", ".");
                    regList.Add(reg);
                }
            }
            using (var context = new jafleetContext())
            {
                //検索
                IQueryable<AircraftView> query;
                if(regList.Count == 1)
                {
                    //1件の場合はワイルドカードで検索
                    var regex = new Regex("^" + regList[0] + "$");
                    query = context.AircraftView.Where(p => regex.IsMatch(p.RegistrationNumber));
                }
                else
                {
                    //2件以上の場合はワイルドカード無効でIN検索
                    query = context.AircraftView.Where(p => regList.Contains(p.RegistrationNumber));
                }

                if (!String.IsNullOrEmpty(model.Airline)){
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

                //ログ
                string logDetail = model.ToString() + ",件数：" + searchResult.Length.ToString();
                Log log = new Log
                {
                    LogDate = DateTime.Now
                    ,LogType = LogType.SEARCH
                    ,LogDetail = logDetail
                    ,UserId = CookieUtil.IsAdmin(HttpContext).ToString()
                };
                context.Log.Add(log);

                //検索条件保存

                //検索条件保持用クラスにコピー
                var sc = new SearchCondition();
                Mapper.Map(model, sc);

                //検索条件保持用クラスをJsonにシリアライズ
                string scjson = Newtonsoft.Json.JsonConvert.SerializeObject(sc);
                schash = HashUtil.CalcCRC32(scjson);

                context.SaveChanges();

            }
            return Json(new SearchResult { ResultList = searchResult,SearchConditionKey = schash });
        }
    }
}
