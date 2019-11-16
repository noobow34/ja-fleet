using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using jafleet.Models;
using jafleet.Commons.EF;
using jafleet.Manager;
using jafleet.Util;
using jafleet.Commons.Constants;
using AutoMapper;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;
using Noobow.Commons.Utils;

namespace jafleet.Controllers
{
    public class SearchController : Controller
    {

        private readonly jafleetContext _context;
        private readonly IServiceScopeFactory _services;

        public SearchController(jafleetContext context, IServiceScopeFactory serviceScopeFactory)
        {
            _context = context;
            _services = serviceScopeFactory;
        }

        public IActionResult Index(SearchModel model,[FromQuery]string sc)
        {
            model.IsAdmin = CookieUtil.IsAdmin(HttpContext);

            model.AirlineList = MasterManager.AllAirline;
            model.TypeDetailList = MasterManager.TypeDetailGroup;
            model.OperationList = MasterManager.Operation;
            model.WiFiList = MasterManager.Wifi;

            if (!string.IsNullOrEmpty(sc))
            {
                //GETパラメーターで検索条件キーが指定されたら
                //保存されている検索条件を取得
                var sce = _context.SearchCondition.AsNoTracking().Where(e => e.SearchConditionKey == sc).FirstOrDefault();
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
            int[] typeDetail = new int[] { };
            String[] operation;
            String[] wifi;
            string registrationDate;
            var addSpecialLiveryReg = new List<string>();

            if (model.RegistrationNumber != null){
                //|区切りで複数件を処理
                foreach(string r in model.RegistrationNumber.ToUpper().Split("|")){
                    string reg = string.Empty;
                    if (!r.StartsWith("JA"))
                    {
                        //JAがついていなければ付加
                        reg = "JA";
                    }
                    reg = reg += r.ToUpper();
                    regList.Add(reg);
                }
            }

            //検索
            IQueryable<AircraftView> query = _context.AircraftView.AsNoTracking();
            if (!String.IsNullOrEmpty(model.Airline))
            {
                airline = model.Airline.Split("|");
                query = query.Where(p => airline.Contains(p.Airline));
            }

            if (!String.IsNullOrEmpty(model.TypeDetail))
            {
                typeDetail = model.TypeDetail.Split("|").Select(t => Convert.ToInt32(t)).ToArray();
                query = query.Where(p => typeDetail.Contains(p.TypeDetailId));
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
            }else if (model.Remarks == "3")
            {
                query = query.Where(p => p.Remarks.Contains(model.RemarksKeyword));
            }

            if (model.SpecialLivery == "1")
            {
                query = query.Where(p => String.IsNullOrEmpty(p.SpecialLivery));
            }
            else if (model.SpecialLivery == "2")
            {
                query = query.Where(p => !String.IsNullOrEmpty(p.SpecialLivery));
            }
            else if (model.SpecialLivery == "3")
            {
                query = query.Where(p => p.SpecialLivery.Contains(model.SpecialLiveryKeyword));
            }
            else if(model.SpecialLivery == "4")
            {
                //あり（履歴含む）
                //一旦履歴テーブルを検索して、履歴の中の該当のレジを取得
                addSpecialLiveryReg.AddRange(_context.AircraftHistory.Where(p => !String.IsNullOrEmpty(p.SpecialLivery)).Select(s => s.RegistrationNumber).Distinct().ToList());
                regList.AddRange(addSpecialLiveryReg.Distinct().ToList());
                regList.AddRange(_context.Aircraft.Where(p => p.SpecialLivery.Contains(model.SpecialLiveryKeyword)).Select(s => s.RegistrationNumber).Distinct().ToList());
            }
            else if (model.SpecialLivery == "5")
            {
                //キーワード指定（履歴含む）
                //一旦履歴テーブルを検索して、履歴の中の該当のレジを取得
                addSpecialLiveryReg.AddRange(_context.AircraftHistory.Where(p => p.SpecialLivery.Contains(model.SpecialLiveryKeyword)).Select(s => s.RegistrationNumber).Distinct().ToList());
                regList.AddRange(addSpecialLiveryReg.Distinct().ToList());
                regList.AddRange(_context.Aircraft.Where(p => p.SpecialLivery.Contains(model.SpecialLiveryKeyword)).Select(s => s.RegistrationNumber).Distinct().ToList());
            }

            if (regList.Count == 1)
            {
                if (StringUtil.ContainsMetaCharacter(regList[0]))
                {
                    //メタ文字を含む場合正規表現検索
                    query = query.Where(a => Regex.IsMatch(a.RegistrationNumber, regList[0]));
                }
                else
                {
                    //含まない場合＝検索
                    query = query.Where(a => a.RegistrationNumber == regList[0]);
                }
            }
            else if (regList.Count > 1)
            {
                //2件以上の場合は正規表現無効でIN検索
                query = query.Where(p => regList.Contains(p.RegistrationNumber));
            }

            searchResult = query.OrderBy(p => p.DisplayOrder).ToArray();

            //履歴から検索している場合、その旨追記
            foreach(var reg in addSpecialLiveryReg)
            {
                searchResult.Where(s => s.RegistrationNumber == reg).Single().SpecialLivery += "（履歴あり）";
            }

            //検索条件保持用クラスにコピー
            var scm = new SearchConditionInModel();
            Mapper.Map(model, scm);

            //検索条件保持用クラスをJsonにシリアライズ
            string scjson = scm.ToString();
            string schash = HashUtil.CalcCRC32(scjson);
            //Cookieの値はここで退避しておかないと、↓のTask.Runではちゃんと取れなくなる
            bool isAdmin = CookieUtil.IsAdmin(HttpContext);

            //検索結果を速く返すためにログと検索条件のDB書き込みは非同期で行う
            Task.Run(() =>
            {
                using (var serviceScope = _services.CreateScope())
                {
                    using (var context = serviceScope.ServiceProvider.GetService<jafleetContext>())
                    {
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

                        //ログ用にTypeDetailをIDからNAMEに置換
                        var scm2 = new SearchConditionInModel();
                        Mapper.Map(model, scm2);
                        var typeDetails = MasterManager.TypeDetailGroup.Where(td => typeDetail.Contains(td.TypeDetailId)).ToArray();
                        if (typeDetail.Count() > 0)
                        {
                            scm2.TypeDetail = string.Join("|", typeDetails.Select(td => td.TypeDetailName));
                        }
                        var scjson2 = scm2.ToString();

                        //ログ
                        Log log = new Log
                        {
                            LogDate = DateTime.Now,
                            LogType = LogType.SEARCH,
                            LogDetail = schash,
                            Additional = $"{model.IsDirect},{searchResult.Length.ToString()}",
                            UserId = isAdmin.ToString()
                        };
                        context.Log.Add(log);

                        context.SaveChanges();
                    }
                }
            });

            return Json(new SearchResult { ResultList = searchResult,SearchConditionKey = schash });
        }
    }
}
