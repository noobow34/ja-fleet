using EnumStringValues;
using jafleet.Commons.Constants;
using jafleet.Commons.EF;
using jafleet.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Noobow.Commons.Constants;
using Noobow.Commons.Utils;

namespace jafleet.Controllers
{
    public class BatchController : Controller
    {

        private readonly IServiceScopeFactory _services;

        public BatchController(IServiceScopeFactory serviceScopeFactory) => _services = serviceScopeFactory;

        public IActionResult Index()
        {
            if (!CookieUtil.IsAdmin(HttpContext))
            {
                return NotFound();
            }
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> WorkingCheckAsync(int? interval)
        {
            if (jafleet.RefreshWorkingStatusAndPhoto.Processing)
            {
                await SlackUtil.PostAsync(SlackChannelEnum.jafleet.GetStringValue(), "WorkingCheck 二重起動を検出");
                return Content("Now Processing!!");
            }

            _ = Task.Run(() =>
            {
                using var serviceScope = _services.CreateScope();
                IEnumerable<AircraftView> targetReg;
                using JafleetContext? context = serviceScope.ServiceProvider.GetService<JafleetContext>();
                targetReg = context!.AircraftViews.Where(a => a.OperationCode != OperationCode.RETIRE_UNREGISTERED).AsNoTracking().ToArray().OrderBy(r => Guid.NewGuid());
                var check = new RefreshWorkingStatusAndPhoto(targetReg, interval ?? 15);
                _ = check.ExecuteCheckAsync(true);
            });

            return Content("WorkingCheck Launch!");
        }
    }
}