using System.Text;
using jafleet.Commons.Constants;
using jafleet.Commons.EF;
using jafleet.Manager;
using jafleet.Models;
using jafleet.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace jafleet.Controllers
{
    public class logController : Controller
    {

        private readonly jafleetContext _context;

        public logController(jafleetContext context) => _context = context;

        public IActionResult Yesterday()
        {
            return Index("y");
        }

        public IActionResult Index(string id)
        {
            if (!CookieUtil.IsAdmin(HttpContext))
            {
                return NotFound();
            }

            DateTime? targetDate = null;
            if (string.IsNullOrEmpty(id))
            {
                targetDate = DateTime.Now.Date;
            }
            else if (id.Length == 6)
            {
                id = "20" + id;
                DateTime.TryParseExact(id, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime outDate);
                targetDate = outDate;
            }
            else if (id == "y")
            {
                targetDate = DateTime.Now.AddDays(-1).Date;
            }

            List<Log>? logs = null;
            logs = _context.Log.AsNoTracking().Where(q => q.LogDate == targetDate && !MasterManager.AdminUser!.Contains(q.UserId ?? string.Empty) && q.LogType != "8").OrderByDescending(q => q.LogId).ToList();

            var logScKeys = logs.Where(sc => sc.LogType == LogType.SEARCH).Select(scc => scc.LogDetail).Distinct();
            var scCache = GetSearchConditionDisps(logScKeys!);

            var retsb = new StringBuilder();
            foreach (var log in logs)
            {
                string logDetail;
                if (log.LogType == LogType.SEARCH)
                {
                    logDetail = scCache[log.LogDetail!] + log.Additional;
                }
                else
                {
                    logDetail = log.LogDetail!;
                }
                retsb.Append($"[{log.LogDate?.ToString("HH:mm:ss")}][{LogType.GetLogTypeName(log.LogType!)}]{logDetail}{Environment.NewLine}");
            }

            string head = DateTime.Now.ToString($"--HH:mm:ss--{Environment.NewLine}");

            return Content(head + retsb.ToString());

        }

        public Dictionary<string, string> GetSearchConditionDisps(IEnumerable<string> scKeys)
        {
            var ret = new Dictionary<string, string>();
            var scs = _context.SearchCondition.AsNoTracking().Where(sc => scKeys.Contains(sc.SearchConditionKey)).ToList();
            foreach (var sc in scs)
            {
                string scJson;
                string searchConditionDisp = string.Empty;
                scJson = sc.SearchConditionJson ?? string.Empty;
                if (!string.IsNullOrEmpty(scJson) && scJson.Contains("TypeDetail"))
                {
                    var scm = JsonConvert.DeserializeObject<SearchConditionInModel>(scJson, new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Populate })!;
                    var typeDetails = MasterManager.TypeDetailGroup?.Where(td => scm!.TypeDetail!.Split("|").ToList().Contains(td.TypeDetailId.ToString()));
                    scm.TypeDetail = string.Join("|", typeDetails!.Select(td => td.TypeDetailName));
                    searchConditionDisp = scm.ToString();
                }
                else
                {
                    searchConditionDisp = scJson;
                }
                ret.Add(sc.SearchConditionKey, searchConditionDisp);
            }
            return ret;
        }
    }
}