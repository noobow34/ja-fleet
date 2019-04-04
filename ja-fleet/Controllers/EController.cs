﻿using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
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

        private readonly jafleetContext _context;

        public EController(jafleetContext context)
        {
            _context = context;
        }

        public IActionResult Index(String id,EditModel model, [FromQuery]Boolean nohead)
        {
            if (!CookieUtil.IsAdmin(HttpContext))
            {
                return NotFound();
            }

            model.AirlineList = MasterManager.AllAirline;
            model.TypeList = MasterManager.Type;
            model.TypeDetailList = _context.TypeDetail.ToArray();
            model.OperationList = MasterManager.Operation;
            model.WiFiList = MasterManager.Wifi;
            model.NotUpdateDate = true;
            model.NoHead = nohead;

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
            else
            {
                var av = _context.AircraftView.Find(id.ToUpper());
                string additional = string.Empty;
                if (av.OperationCode == OperationCode.RETIRE_UNREGISTERED)
                {
                    additional = "?includeRetire=true";
                }
                model.LinkPage = $"https://ja-fleet.noobow.me/AD/{av.RegistrationNumber}";
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult Store( EditModel model){
            try{
                DateTime storeDate = DateTime.Now;
                string reg = model.Aircraft.RegistrationNumber;
                var origin = _context.Aircraft.AsNoTracking().Where(a => a.RegistrationNumber == reg).FirstOrDefault();
                if (!model.NotUpdateDate || model.IsNew){
                    model.Aircraft.UpdateTime = storeDate;
                }
                model.Aircraft.ActualUpdateTime = storeDate;
                if (model.IsNew)
                {
                    model.Aircraft.CreationTime = storeDate;
                    _context.Aircraft.Add(model.Aircraft);
                }
                else
                {
                    if (!model.NotUpdateDate)
                    {
                        //Historyにコピー
                        var ah = new AircraftHistory();
                        Mapper.Map(origin, ah);
                        ah.HistoryRegisterAt = storeDate;
                        //HistoryのSEQのMAXを取得
                        var maxseq = _context.AircraftHistory.AsNoTracking().Where(ahh => ahh.RegistrationNumber == ah.RegistrationNumber).GroupBy(ahh => ahh.RegistrationNumber)
                                                .Select(ahh => new { maxseq = ahh.Max(x => x.Seq) }).FirstOrDefault();
                        ah.Seq = (maxseq?.maxseq ?? 0) + 1;
                        _context.AircraftHistory.Add(ah);
                    }
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
            string noheadString = string.Empty;
            if (model.NoHead)
            {
                noheadString = "?nohead=" + model.NoHead.ToString();
            }
            return Redirect("/E/" + model.Aircraft.RegistrationNumber + noheadString);
        }
    }
}
