﻿using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using jafleet.Models;
using Microsoft.AspNetCore.Diagnostics;
using System;
using jafleet.Util;
using jafleet.EF;
using jafleet.Constants;

namespace jafleet.Controllers
{
    public class HomeController : Controller
    {
        private Microsoft.AspNetCore.Hosting.IHostingEnvironment _hostingEnvironment = null;

        public HomeController(Microsoft.AspNetCore.Hosting.IHostingEnvironment hostingEnvironment)
        {
            this._hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var ex = HttpContext.Features.Get<IExceptionHandlerPathFeature>().Error;
            ErrorViewModel model = new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier };
            model.IsAdmin = CookieUtil.IsAdmin(HttpContext);
            model.Ex = ex;

            Log log = new Log
            {
                LogDate = DateTime.Now.ToString(DBConstant.SQLITE_DATETIME)
                ,LogType = LogType.EXCEPTION
                ,LogDetail = ex.ToString()
                ,UserId = CookieUtil.IsAdmin(HttpContext).ToString()
            };
            using (var context = new jafleetContext())
            {
                context.Log.Add(log);
                context.SaveChanges();
            }
            return View(model);
        }
    }
}
