﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using jafleet.Models;
using jafleet.Excel;

namespace jafleet.Controllers
{
    public class JALController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public JsonResult Data()
        {
            ExcelReader excelReader = new ExcelReader();
            return Json(excelReader.GetAircraftInfo("JAL"));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
