﻿using Microsoft.AspNetCore.Mvc;
using jafleet.Manager;
using jafleet.Models;
using jafleet.Commons.EF;
using jafleet.Util;
using jafleet.Commons.Constants;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace jafleet.Controllers
{
    public class EController : Controller
    {

        private readonly JafleetContext _context;

        public EController(JafleetContext context) => _context = context;

        public IActionResult Index(string id, EditModel model, [FromQuery] bool nohead)
        {
            if (!CookieUtil.IsAdmin(HttpContext))
            {
                return NotFound();
            }

            model.AirlineList = MasterManager.AllAirline!;
            model.TypeList = MasterManager.Type!;
            model.TypeDetailList = _context.TypeDetails.OrderBy(t => t.TypeDetailName).ToArray();
            model.OperationList = MasterManager.Operation!;
            model.WiFiList = MasterManager.Wifi!;
            model.NotUpdateDate = true;
            model.NoHead = nohead;

            if (string.IsNullOrEmpty(id))
            {
                model.Aircraft = new Aircraft();
                model.IsNew = true;
            }
            else
            {
                model.Aircraft = _context.Aircrafts.Where(p => p.RegistrationNumber == id.ToUpper()).FirstOrDefault();
            }

            if (model.Aircraft == null)
            {
                model.Aircraft = new Aircraft();
                model.Aircraft.RegistrationNumber = id.ToUpper();
                model.IsNew = true;
                model.LinkPage = $"https://ja-fleet.noobow.me/AD/{id.ToUpper()}";
            }
            else
            {
                AircraftView? av = _context.AircraftViews.Where(av => av.RegistrationNumber == id.ToUpper()).SingleOrDefault();
                if (av != null) 
                {
                    model.LinkPage = $"https://ja-fleet.noobow.me/AD/{av.RegistrationNumber}";
                }
            }
            var type = MasterManager.TypeDetailGroup?.Where(td => td.TypeDetailId == model.Aircraft.TypeDetailId).FirstOrDefault()?.TypeCode;
            IEnumerable<SeatConfiguration>? q = MasterManager.SeatConfiguration;
            if (!string.IsNullOrEmpty(model.Aircraft.Airline))
            {
                q = q?.Where(sc => sc.Airline == model.Aircraft.Airline);
            }
            if (!string.IsNullOrEmpty(type))
            {
                q = q?.Where(sc => sc.Type == type);
            }
            model.SeatConfigurationList = q?.ToArray();

            return View(model);
        }

        [HttpPost]
        public IActionResult Store(EditModel model)
        {
            try
            {
                DateTime storeDate = DateTime.Now;
                string? reg = model.Aircraft!.RegistrationNumber;
                var origin = _context.Aircrafts.AsNoTracking().Where(a => a.RegistrationNumber == reg).FirstOrDefault();
                if (!model.NotUpdateDate || model.IsNew)
                {
                    model.Aircraft.UpdateTime = storeDate;
                }
                model.Aircraft.ActualUpdateTime = storeDate;
                if (model.IsNew)
                {
                    model.Aircraft.CreationTime = storeDate;
                    _context.Aircrafts.Add(model.Aircraft);
                }
                else
                {
                    if (!model.NotUpdateDate)
                    {
                        //Historyにコピー
                        var configuration = new MapperConfiguration(cfg =>
                        {
                            cfg.CreateMap<Aircraft, AircraftHistory>();
                        });
                        var mapper = configuration.CreateMapper();
                        var ah = mapper.Map<AircraftHistory>(origin);
                        ah.HistoryRegisterAt = storeDate;

                        //HistoryのSEQのMAXを取得
                        var maxseq = _context.AircraftHistories.AsNoTracking().Where(ahh => ahh.RegistrationNumber == ah.RegistrationNumber).GroupBy(ahh => ahh.RegistrationNumber)
                                                .Select(ahh => new { maxseq = ahh.Max(x => x.Seq) }).FirstOrDefault();
                        ah.Seq = (maxseq?.maxseq ?? 0) + 1;
                        _context.AircraftHistories.Add(ah);
                    }
                    _context.Entry(model.Aircraft).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                }
                //デリバリーされたらテストレジはクリア
                if (!OperationCode.PRE_DELIVERY.Contains(model.Aircraft.OperationCode))
                {
                    model.Aircraft.TestRegistration = null;
                }
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                model.ex = ex;
            }

            model.AirlineList = MasterManager.AllAirline!;
            model.TypeList = MasterManager.Type!;
            model.OperationList = MasterManager.Operation!;
            model.WiFiList = MasterManager.Wifi!;
            string noheadString = string.Empty;
            if (model.NoHead)
            {
                noheadString = "?nohead=" + model.NoHead.ToString();
            }

            //写真を更新
            _ = HttpClientManager.GetInstance().GetStringAsync($"http://localhost:5000/Aircraft/Photo/{model.Aircraft!.RegistrationNumber}?force=true");

            return Redirect("/E/" + model.Aircraft.RegistrationNumber + noheadString);
        }
    }
}
