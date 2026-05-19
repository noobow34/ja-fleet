using jafleet.Commons.Constants;
using jafleet.Commons.EF;
using jafleet.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace jafleet.Controllers
{
    public class WorkingCheckLogController : Controller
    {
        private readonly JafleetContext _context;

        private const string DATE_PAT = @"\d{4}/\d{2}/\d{2} \d{1,2}:\d{2}(?::\d{2})?";

        // REG◎(TYPE):DATE:ROUTE  or  REG◎(TYPE):DATE ROUTE（秒あり時はスペース区切り）
        // TYPE に () が含まれるケース（ATR42-600(型式-500) 等）に対応するため貪欲マッチ
        private static readonly Regex EntryRegex = new(
            @"^(?<reg>JA\w+)(?<mark>[◎☆]?)\((?<type>.+)\)" +
            @":(?<date>" + DATE_PAT + @")[:\ ]" +
            @"(?<route>.+?)$",
            RegexOptions.Compiled);

        // "Tokyo (NRT) Guangzhou (CAN) IJ9811 Landed 15:14 ← 2026/05/14 19:41"
        // 空港名: "名前 (CODE)" または "—"
        private static readonly Regex RouteRegex = new(
            @"^(?<from>.+? \([A-Z0-9]+\)|—) (?<to>.+? \([A-Z0-9]+\)|—) (?<fn>\S+) (?<status>.+?)(?: ← (?<prev>" + DATE_PAT + @"))?$",
            RegexOptions.Compiled);

        public WorkingCheckLogController(JafleetContext context) => _context = context;

        public IActionResult Index(string id)
        {
            DateTime searchDate;
            if (string.IsNullOrEmpty(id))
            {
                searchDate = DateTime.Now.Date;
            }
            else
            {
                if (!DateTime.TryParseExact(id, "yyyyMMdd", null,
                    System.Globalization.DateTimeStyles.None, out searchDate))
                {
                    searchDate = DateTime.Now.Date;
                }
            }

            var rawLogs = _context.Logs
                .Where(l => l.LogDate!.Value.Date == searchDate && l.LogType == LogType.WORKING_INFO)
                .AsNoTracking()
                .OrderByDescending(l => l.LogDate)
                .Select(l => l.LogDetail)
                .ToList();

            var model = new WorkingCheckLogModel
            {
                Title = $"稼働チェックログ {searchDate:yyyy/MM/dd}",
                SearchDate = searchDate,
                Batches = rawLogs.Select(ParseBatch).ToList()
            };

            return View(model);
        }

        private static WorkingCheckLogBatch ParseBatch(string? rawText)
        {
            var batch = new WorkingCheckLogBatch();
            if (string.IsNullOrWhiteSpace(rawText)) return batch;

            var lines = rawText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            WorkingCheckLogSection? currentSection = null;

            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim('\r').Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;

                if (line.StartsWith("RefreshWorkingStatus正常終了:"))
                {
                    batch.FinishedAt = line.Replace("RefreshWorkingStatus正常終了:", "").Trim();
                    continue;
                }

                if (line.StartsWith("--------") && line.EndsWith("--------"))
                {
                    var title = line.Trim('-').Trim();
                    currentSection = new WorkingCheckLogSection { SectionTitle = title };
                    batch.Sections.Add(currentSection);
                    continue;
                }

                if (currentSection == null) continue;
                currentSection.Entries.Add(ParseEntry(line));
            }

            return batch;
        }

        private static WorkingCheckLogEntry ParseEntry(string line)
        {
            var m = EntryRegex.Match(line);
            if (!m.Success) return new WorkingCheckLogEntry { RawLine = line };

            var routeRaw = m.Groups["route"].Value;
            var rm = RouteRegex.Match(routeRaw);

            string? from = null, to = null, fn = null, status = null, prev = null;
            if (rm.Success)
            {
                from   = rm.Groups["from"].Value;
                to     = rm.Groups["to"].Value;
                fn     = rm.Groups["fn"].Value;
                status = rm.Groups["status"].Value;
                prev   = rm.Groups["prev"].Success && !string.IsNullOrEmpty(rm.Groups["prev"].Value)
                         ? rm.Groups["prev"].Value : null;
            }
            else
            {
                var arrowIdx = routeRaw.IndexOf(" ← ", StringComparison.Ordinal);
                status = arrowIdx >= 0 ? routeRaw[..arrowIdx] : routeRaw;
                prev   = arrowIdx >= 0 ? routeRaw[(arrowIdx + 3)..] : null;
            }

            var (fromCode, fromName) = SplitAp(from);
            var (toCode,   toName)   = SplitAp(to);

            return new WorkingCheckLogEntry
            {
                RegistrationNumber = m.Groups["reg"].Value,
                NotifyMark   = string.IsNullOrEmpty(m.Groups["mark"].Value) ? null : m.Groups["mark"].Value,
                TypeName     = m.Groups["type"].Value,
                FlightDate   = m.Groups["date"].Value,
                FromAp       = from,
                FromApCode   = fromCode,
                FromApName   = fromName,
                ToAp         = to,
                ToApCode     = toCode,
                ToApName     = toName,
                FlightNumber = fn,
                Status       = status,
                PreviousDate = prev,
            };
        }

        /// <summary>"Tokyo (NRT)" → ("NRT", "Tokyo")、"—" → ("—", "")</summary>
        private static (string code, string name) SplitAp(string? ap)
        {
            if (string.IsNullOrEmpty(ap)) return ("", "");
            if (ap == "—") return ("—", "");
            var idx = ap.LastIndexOf('(');
            if (idx < 0) return (ap, "");
            var code = ap[(idx + 1)..].TrimEnd(')');
            var name = ap[..idx].Trim();
            return (code, name);
        }
    }
}
