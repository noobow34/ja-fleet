using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using jafleet.Models;
using jafleet.Commons.EF;
using System.Text.RegularExpressions;
using jafleet.Manager;
using jafleet.Util;
using jafleet.Commons.Constants;
using AutoMapper;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace jafleet.Controllers
{
    public class SearchController : Controller
    {

        private readonly jafleetContext _context;

        public SearchController(jafleetContext context)
        {
            _context = context;
        }

        public IActionResult Index(SearchModel model,[FromQuery]string sc)
        {
            model.IsAdmin = CookieUtil.IsAdmin(HttpContext);

            model.AirlineList = MasterManager.AllAirline;
            model.TypeList = MasterManager.Type;
            model.OperationList = MasterManager.Operation;
            model.WiFiList = MasterManager.Wifi;

            if (!string.IsNullOrEmpty(sc))
            {
                //GETパラメーターで検索条件キーが指定されたら
                //保存されている検索条件を取得
                var sce = _context.SearchCondition.Where(e => e.SearchConditionKey == sc).FirstOrDefault();
                if (sce != null)
                {
                    //取得したjsonから復元
                    var scm = JsonConvert.DeserializeObject<SearchConditionInModel>(sce.SearchConditionJson, new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Populate });
                    //modelにコピー
                    Mapper.Map(scm, model);
                    model.IsDirect = true;
                }
            }

            return View(model);
        }

        public IActionResult DoSearch(SearchModel model)
        {
            if(model.IsLoading && !model.IsDirect){
                //初回ロードかつダイレクト指定ではない場合は空リストを返す
                return Json(new SearchResult {ResultList = new AircraftView[] { },SearchConditionKey = string.Empty});
            }
            AircraftView[] searchResult = null;
            var regList = new List<string>();
            String[] airline;
            String[] type;
            String[] operation;
            String[] wifi;
            String registrationDate;;

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

            //検索
            IQueryable<AircraftView> query;
            if (regList.Count == 1)
            {
                //1件の場合はワイルドカードで検索
                var regex = new Regex("^" + regList[0] + "$");
                query = _context.AircraftView.Where(p => regex.IsMatch(p.RegistrationNumber));
            }
            else
            {
                //2件以上の場合はワイルドカード無効でIN検索
                query = _context.AircraftView.Where(p => regList.Contains(p.RegistrationNumber));
            }

            if (!String.IsNullOrEmpty(model.Airline))
            {
                airline = model.Airline.Split("|");
                query = query.Where(p => airline.Contains(p.Airline));
            }

            if (!String.IsNullOrEmpty(model.Type))
            {
                type = model.Type.Split("|");
                query = query.Where(p => type.Contains(p.TypeCode));
            }

            if (!String.IsNullOrEmpty(model.WiFiCode))
            {
                wifi = model.WiFiCode.Split("|");
                query = query.Where(p => wifi.Contains(p.WifiCode));
            }

            if (!String.IsNullOrEmpty(model.RegistrationDate))
            {
                registrationDate = model.RegistrationDate;
                if (model.RegistrationSelection == "0")
                {
                    query = query.Where(p => p.RegisterDate.StartsWith(registrationDate));
                }
                else if (model.RegistrationSelection == "1")
                {
                    registrationDate += "zzz";
                    query = query.Where(p => p.RegisterDate.CompareTo(registrationDate) <= 0);
                }
                else if (model.RegistrationSelection == "2")
                {
                    registrationDate += "zzz";
                    query = query.Where(p => p.RegisterDate.CompareTo(registrationDate) >= 0);
                }

            }

            if (!String.IsNullOrEmpty(model.OperationCode))
            {
                operation = model.OperationCode.Split("|");
                query = query.Where(p => operation.Contains(p.OperationCode));
            }

            if (model.Remarks == "1")
            {
                query = query.Where(p => String.IsNullOrEmpty(p.Remarks));
            }
            else if (model.Remarks == "2")
            {
                query = query.Where(p => !String.IsNullOrEmpty(p.Remarks));
            }

            searchResult = query.OrderBy(p => p.DisplayOrder).ToArray();

            //検索条件保持用クラスにコピー
            var scm = new SearchConditionInModel();
            Mapper.Map(model, scm);

            //検索条件保持用クラスをJsonにシリアライズ
            string scjson = scm.ToString();
            string schash = HashUtil.CalcCRC32(scjson);
            //Cookieの値はここで退避しておかないと、↓のTask.Runではちゃんと取れなくなる
            Boolean isAdmin = CookieUtil.IsAdmin(HttpContext);

            //検索結果を速く返すためにログと検索条件のDB書き込みは非同期で行う
            Task.Run(() =>
            {
                //_contextは破棄されてしまうのでここだけnewする
                using(var context = new jafleetContext())
                {
                    //ログ
                    string logDetail = scjson + $"{model.IsDirect},件数：" + searchResult.Length.ToString();
                    Log log = new Log
                    {
                        LogDate = DateTime.Now,
                        LogType = LogType.SEARCH,
                        LogDetail = logDetail,
                        UserId = isAdmin.ToString()
                    };
                    context.Log.Add(log);

                    //検索条件保存
                    var sc = context.SearchCondition.Where(e => e.SearchConditionKey == schash).FirstOrDefault();
                    if (sc == null)
                    {
                        sc = new SearchCondition
                        {
                            SearchConditionKey = schash,
                            SearchConditionJson = scjson,
                            SearchCount = 0
                        };

                        //検索回数、検索日時は管理者じゃないい場合のみ
                        if (!isAdmin)
                        {
                            sc.SearchCount = 1;
                            sc.FirstSearchDate = DateTime.Now;
                            sc.LastSearchDate = sc.FirstSearchDate;
                        }
                        context.SearchCondition.Add(sc);
                    }
                    else if (!isAdmin)
                    {
                        //管理者じゃない場合のみ検索回数、検索日時を更新
                        sc.SearchCount++;
                        sc.LastSearchDate = DateTime.Now;
                        if (sc.FirstSearchDate == null)
                        {
                            //初回の検索が管理者だった場合に初回検索日時がセットされてないのでここでセット
                            sc.FirstSearchDate = sc.LastSearchDate;
                        }
                    }

                    context.SaveChanges();
                }
            });

            return Json(new SearchResult { ResultList = searchResult,SearchConditionKey = schash });
        }
    }
}
