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

        public async System.Threading.Tasks.Task<IActionResult> Photo(string id,[FromQuery] bool force)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }

            var photo = _context.AircraftPhoto.Where(p => p.RegistrationNumber == id).SingleOrDefault();
            Aircraft a = null;

            //Jetphotos暫定
            if (photo?.PhotoUrl != null)
            {
                return Redirect($"https://www.jetphotos.com{photo.PhotoUrl}");
            }
            else
            {
                a = _context.Aircraft.Where(p => p.RegistrationNumber == id.ToUpper()).FirstOrDefault();
                if (string.IsNullOrEmpty(a.LinkUrl))
                {
                    return Redirect("/nophoto.html");
                }
                else
                {
                    return ReturnLinkUrl(a.LinkUrl);
                }
            }

            /*if (photo != null && DateTime.Now.Date == photo.LastAccess.Date && !force)
            {
                if(photo.PhotoUrl != null)
                {
                    //1日以内のキャッシュがあれば、キャッシュから返す
                    return Redirect($"https://www.jetphotos.com{photo.PhotoUrl}");
                }
                else
                {
                    //キャッシュがNULLの場合は、リンクURLもNULLの場合のみnophotoを返す
                    //そうしないとキャッシュがNULLで、あとからLinkUrlを登録した場合に最大1日待つ必要が出る。
                    a = _context.Aircraft.Where(p => p.RegistrationNumber == id.ToUpper()).FirstOrDefault();
                    if (string.IsNullOrEmpty(a.LinkUrl))
                    {
                        return Redirect("/nophoto.html");
                    }
                    else
                    {
                        return ReturnLinkUrl(a.LinkUrl);
                    }
                }
            }

            string jetphotoUrl = string.Format("https://www.jetphotos.com/showphotos.php?keywords-type=reg&keywords={0}&search-type=Advanced&keywords-contain=0&sort-order=2", id);
            if(a == null)
            {
                a = _context.Aircraft.Where(p => p.RegistrationNumber == id.ToUpper()).FirstOrDefault();
            }
            var parser = new HtmlParser();
            try
            {
                var htmlDocument = parser.ParseDocument(await HttpClientManager.GetInstance().GetStringAsync(jetphotoUrl));
                var photos = htmlDocument.GetElementsByClassName("result__photoLink");
                if (photos.Length != 0)
                {
                    //Jetphotosに写真があった場合
                    string newestPhotoLink = photos[0].GetAttribute("href");
                    var photoTag = htmlDocument.GetElementsByClassName("result__photo");
                    string directUrl = null;
                    if (photoTag.Length != 0)
                    {
                        directUrl = photoTag[0].GetAttribute("src").Replace("//cdn.jetphotos.com/400", string.Empty);
                    }

                    _ = Task.Run(() =>
                    {
                        //写真をキャッシュに登録する
                        using var serviceScope = _services.CreateScope();
                        using var context = serviceScope.ServiceProvider.GetService<jafleetContext>();
                        if (!string.IsNullOrEmpty(a.LinkUrl))
                        {
                            //Jetphotosから取得できるのにDBにも登録されている場合は、DBから消す
                            a.LinkUrl = null;
                            a.ActualUpdateTime = DateTime.Now;
                            context.Aircraft.Update(a);
                            LineUtil.PushMe($"{id}のLinkUrlを削除しました", HttpClientManager.GetInstance());
                        }
                        StoreAircraftPhoto(context, photo, newestPhotoLink, id, directUrl);
                    });
                    return Redirect($"https://www.jetphotos.com{newestPhotoLink}");
                }
                else
                {
                    if (string.IsNullOrEmpty(a?.LinkUrl))
                    {
                        //Jetphotosに写真がなかった場合
                        _ = Task.Run(() =>
                        {
                            //写真がないという情報を登録する
                            using var serviceScope = _services.CreateScope();
                            using var context = serviceScope.ServiceProvider.GetService<jafleetContext>();
                            StoreAircraftPhoto(context, photo, null, id, null);
                        });
                        return Redirect("/nophoto.html");
                    }
                    else
                    {
                        return ReturnLinkUrl(a.LinkUrl);
                    }
                }
            }
            catch
            {
                return Redirect($"/failphotoload.html?reg={id}");
            }*/
        }

        private IActionResult ReturnLinkUrl(string linkUrl)
        {
            if (linkUrl.StartsWith("<"))
            {
                ViewBag.Tag = linkUrl;
                return View("~/Views/AircraftDetail/Emb.cshtml");
            }
            else if (linkUrl.Contains("twitter.com"))
            {
                //ツイート埋め込みを登録している場合
                ViewBag.TweetUrl = linkUrl;
                return View("~/Views/AircraftDetail/Emb.cshtml");
            }
            else
            {
                //それ意外のサイトを登録している場合
                return Redirect(linkUrl);
            }

        }

        private void StoreAircraftPhoto(jafleetContext context,AircraftPhoto photo, string photoUrl,string reg,string directUrl)
        {
            if (photo != null)
            {
                photo.PhotoUrl = photoUrl;
                photo.PhotoDirectUrl = directUrl;
                photo.LastAccess = DateTime.Now;
                context.AircraftPhoto.Update(photo);
            }
            else
            {
                context.AircraftPhoto.Add(new AircraftPhoto { RegistrationNumber = reg, PhotoUrl = photoUrl, LastAccess = DateTime.Now });
            }
            context.SaveChanges();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
