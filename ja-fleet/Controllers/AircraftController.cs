using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using jafleet.Models;
using jafleet.Commons.EF;
using jafleet.Manager;
using AngleSharp.Html.Parser;
using jafleet.Util;
using Microsoft.EntityFrameworkCore;
using jafleet.Commons.Constants;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Noobow.Commons.Utils;
using System;

namespace jafleet.Controllers
{
    public class AircraftController : Controller
    {

        private readonly jafleetContext _context;
        private readonly IServiceScopeFactory _services;

        public AircraftController(jafleetContext context, IServiceScopeFactory serviceScopeFactory)
        {
            _context = context;
            _services = serviceScopeFactory;
        }

        public IActionResult Index(AircraftModel model)
        {

            model.Title = "all";
            model.TableId = "all";
            model.api = "/api/airlinegroup/";

            model.IsAdmin = CookieUtil.IsAdmin(HttpContext);

            return View("~/Views/Aircraft/index.cshtml", model);
        }

        public IActionResult AirlineGroup(string id, string id2, [FromQuery]bool includeRetire, AircraftModel model)
        {
            id = id?.ToUpper();
            id2 = id2?.ToUpper();

            string groupName;
            groupName = _context.AirlineGroup.AsNoTracking().FirstOrDefault(p => p.AirlineGroupCode == id)?.AirlineGroupName;

            model.Title = groupName ?? "all";
            model.TableId = id ?? "all";
            model.api = "/api/airlinegroup/" + id;
            if (!string.IsNullOrEmpty(id2))
            {
                var typeName = MasterManager.Type.Where(p => p.TypeCode == id2).FirstOrDefault()?.TypeName;
                if (!string.IsNullOrEmpty(typeName))
                {
                    model.Title += ("・" + typeName);
                    model.api += ("/" + id2);
                }
            }

            model.IncludeRetire = includeRetire;
            model.IsAdmin = CookieUtil.IsAdmin(HttpContext);

            return View("~/Views/Aircraft/index.cshtml",model);
        }

        public IActionResult Airline(string id, string id2, [FromQuery]bool includeRetire,AircraftModel model)
        {
            id = id?.ToUpper();
            id2 = id2?.ToUpper();

            string airlineName;
            airlineName = _context.Airline.AsNoTracking().FirstOrDefault(p => p.AirlineCode == id)?.AirlineNameJpShort;

            model.Title = airlineName ?? "all";
            model.TableId = id ?? "all";
            model.api = "/api/airline/" + id;
            if (!string.IsNullOrEmpty(id2))
            {
                var typeName = MasterManager.Type.Where(p => p.TypeCode == id2).FirstOrDefault()?.TypeName;
                if (!string.IsNullOrEmpty(typeName))
                {
                    model.Title += ("・" + typeName);
                    model.api += ("/" + id2);
                }
            }

            model.IncludeRetire = includeRetire;
            model.IsAdmin = CookieUtil.IsAdmin(HttpContext);

            return View("~/Views/Aircraft/index.cshtml",model);
        }

        public IActionResult Type(string id, [FromQuery]bool? includeRetire, AircraftModel model)
        {
            id = id?.ToUpper();

            string typeName;
            typeName = _context.Type.AsNoTracking().FirstOrDefault(p => p.TypeCode == id)?.TypeName;

            model.Title = typeName ?? "all";
            model.TableId = id ?? "all";
            model.api = "/api/type/" + id;

            //全機退役かどうか確認
            int operatingCount = _context.AircraftView.AsNoTracking().Where(p => p.TypeCode == id && p.OperationCode != OperationCode.RETIRE_UNREGISTERED).Count();

            if(operatingCount == 0 && !includeRetire.HasValue)
            {
                //全機退役済みなら強制的に退役済みを表示
                model.IncludeRetire = true;
                model.IsAllRetire = true;
            }
            else
            {
                model.IncludeRetire = includeRetire.HasValue ? includeRetire.Value : false;
            }

            model.IsAdmin = CookieUtil.IsAdmin(HttpContext);

            return View("~/Views/Aircraft/index.cshtml",model);
        }

        public async System.Threading.Tasks.Task<IActionResult> Photo(string id)
        {
            string jetphotoUrl = string.Format("https://www.jetphotos.com/showphotos.php?keywords-type=reg&keywords={0}&search-type=Advanced&keywords-contain=0&sort-order=2", id);
            var a = _context.Aircraft.Where(p => p.RegistrationNumber == id.ToUpper()).FirstOrDefault();
            var parser = new HtmlParser();
            var htmlDocument = parser.ParseDocument(await HttpClientManager.GetInstance().GetStringAsync(jetphotoUrl));
            var photos = htmlDocument.GetElementsByClassName("result__photoLink");
            if (photos.Length != 0)
            {
                //Jetphotosに写真があった場合
                string newestPhotoLink = photos[0].GetAttribute("href");
                if (!string.IsNullOrEmpty(a.LinkUrl))
                {
                    //Jetphotosから取得できるのにDBにも登録されている場合は、DBから消す
                    a.LinkUrl = null;
                    a.ActualUpdateTime = DateTime.Now;
                    _context.SaveChanges();
                    LineUtil.PushMe($"{id}のLinkUrlを削除じました", HttpClientManager.GetInstance());
                }

                return Redirect($"https://www.jetphotos.com{newestPhotoLink}");
            }
            else
            {
                //Jetphotosに写真がなかった場合
                return Redirect(a?.LinkUrl ?? "/nophoto.html");
            }

        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
