﻿using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using jafleet.Models;
using jafleet.Commons.EF;
using jafleet.Manager;
using System.Net.Http;
using AngleSharp.Html.Parser;
using System;
using jafleet.Util;
using jafleet.Commons.Constants;

namespace jafleet.Controllers
{
    public class AircraftController : Controller
    {

        private readonly jafleetContext _context;

        public AircraftController(jafleetContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AirlineGroup(string id, string id2, [FromQuery]Boolean includeRetire, AircraftModel model)
        {
            id = id.ToUpper();
            id2 = id2?.ToUpper();

            string groupName;
            groupName = _context.AirlineGroup.FirstOrDefault(p => p.AirlineGroupCode == id)?.AirlineGroupName;

            ViewData["Title"] = groupName;
            ViewData["TableId"] = id;
            ViewData["api"] = "/api/airlinegroup/" + id;
            if (!string.IsNullOrEmpty(id2))
            {
                ViewData["Title"] += ("・" + MasterManager.Type.Where(p => p.TypeCode == id2).First()?.TypeName);
                ViewData["api"] += ("/" + id2);
            }

            model.IncludeRetire = includeRetire;

            return View("~/Views/Aircraft/index.cshtml",model);
        }

        public IActionResult Airline(string id, string id2, [FromQuery]Boolean includeRetire,AircraftModel model)
        {
            id = id.ToUpper();
            id2 = id2?.ToUpper();

            string airlineName;
            airlineName = _context.Airline.FirstOrDefault(p => p.AirlineCode == id)?.AirlineNameJpShort;

            ViewData["Title"] = airlineName;
            ViewData["TableId"] = id;
            ViewData["api"] = "/api/airline/" + id;
            if (!string.IsNullOrEmpty(id2))
            {
                ViewData["Title"] += ("・" + MasterManager.Type.Where(p => p.TypeCode == id2).First()?.TypeName);
                ViewData["api"] += ("/" + id2);
            }

            model.IncludeRetire = includeRetire;

            return View("~/Views/Aircraft/index.cshtml",model);
        }

        public IActionResult Type(string id, [FromQuery]Boolean includeRetire, AircraftModel model)
        {
            id = id.ToUpper();

            string typeName;
            typeName = _context.Type.FirstOrDefault(p => p.TypeCode == id)?.TypeName;

            ViewData["Title"] = typeName;
            ViewData["TableId"] = id;
            ViewData["api"] = "/api/type/" + id;

            model.IncludeRetire = includeRetire;

            return View("~/Views/Aircraft/index.cshtml",model);
        }

        public async System.Threading.Tasks.Task<IActionResult> Photo(string id)
        {
            string jetphotoUrl = string.Format("https://www.jetphotos.com/showphotos.php?keywords-type=reg&keywords={0}&search-type=Advanced&keywords-contain=0&sort-order=2", id);
            string redirectUrl = string.Empty;

            Log log = new Log
            {
                LogDate = DateTime.Now
                , LogType = LogType.PHOTO
                , LogDetail = id
                , UserId = CookieUtil.IsAdmin(HttpContext).ToString()
            };

            _context.Log.Add(log);
            _context.SaveChanges();
            redirectUrl = _context.Aircraft.Where(p => p.RegistrationNumber == id.ToUpper()).FirstOrDefault()?.LinkUrl;

            if(redirectUrl == null){
                var parser = new HtmlParser();
                var htmlDocument = parser.ParseDocument(await HttpClientManager.GetInstance().GetStringAsync(jetphotoUrl));
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