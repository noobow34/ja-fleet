﻿using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using jafleet.Models;
using jafleet.EF;
using jafleet.Manager;
using AngleSharp.Dom.Html;
using System.Net.Http;
using AngleSharp.Parser.Html;
using System;
using jafleet.Util;

namespace jafleet.Controllers
{
    public class AircraftController : Controller
    {
        private static NLog.Logger infologger = NLog.LogManager.GetLogger("infologger");
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AirlineGroup(string id, string id2)
        {
            id = id.ToUpper();
            id2 = id2?.ToUpper();

            string groupName;
            using (var context = new jafleetContext())
            {
                groupName = context.AirlineGroup.FirstOrDefault(p => p.AirlineGroupCode == id)?.AirlineGroupName;
            }

            ViewData["Title"] = groupName;
            ViewData["TableId"] = id;
            ViewData["api"] = "/api/airlinegroup/" + id;
            if (!string.IsNullOrEmpty(id2))
            {
                ViewData["Title"] += ("・" + MasterManager.Type.Where(p => p.TypeCode == id2).First()?.TypeName);
                ViewData["api"] += ("/" + id2);
            }
            return View("~/Views/Aircraft/index.cshtml");
        }

        public IActionResult Airline(string id, string id2)
        {
            id = id.ToUpper();
            id2 = id2?.ToUpper();

            string airlineName;
            using (var context = new jafleetContext())
            {
                airlineName = context.Airline.FirstOrDefault(p => p.AirlineCode == id)?.AirlineNameJpShort;
            }

            ViewData["Title"] = airlineName;
            ViewData["TableId"] = id;
            ViewData["api"] = "/api/airline/" + id;
            if (!string.IsNullOrEmpty(id2))
            {
                ViewData["Title"] += ("・" + MasterManager.Type.Where(p => p.TypeCode == id2).First()?.TypeName);
                ViewData["api"] += ("/" + id2);
            }
            return View("~/Views/Aircraft/index.cshtml");
        }

        public IActionResult Type(string id)
        {
            id = id.ToUpper();

            string typeName;
            using (var context = new jafleetContext())
            {
                typeName = context.Type.FirstOrDefault(p => p.TypeCode == id)?.TypeName;
            }

            ViewData["Title"] = typeName;
            ViewData["TableId"] = id;
            ViewData["api"] = "/api/type/" + id;
            return View("~/Views/Aircraft/index.cshtml");
        }

        public async System.Threading.Tasks.Task<IActionResult> Photo(string id)
        {
            string jetphotoUrl = string.Format("https://www.jetphotos.com/showphotos.php?keywords-type=reg&keywords={0}&search-type=Advanced&keywords-contain=0&sort-order=2", id);
            string redirectUrl = string.Empty;

            if (!CookieUtil.IsAdmin(HttpContext))
            {
                infologger.Info("写真クリック：" + id);
            }

            using (var context = new jafleetContext())
            {
                redirectUrl = context.Aircraft.Where(p => p.RegistrationNumber == id.ToUpper()).FirstOrDefault()?.LinkUrl;
            }

            if(redirectUrl == null){
                var parser = new HtmlParser();
                HttpClient client = new HttpClient();
                var htmlDocument = parser.Parse(await client.GetStringAsync(jetphotoUrl));
                var photos = htmlDocument.GetElementsByClassName("result__photoLink");
                if (photos.Length != 0)
                {
                    string newestPhotoLink = photos[0].GetAttribute("href");
                    redirectUrl = "https://www.jetphotos.com" + newestPhotoLink;
                    return Redirect(redirectUrl);
                }
                else
                {
                    return Redirect(jetphotoUrl);
                }
            }
            else
            {
                return Redirect(redirectUrl);
            }

        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
