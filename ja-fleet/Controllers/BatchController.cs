using EnumStringValues;
using jafleet.Commons.EF;
using jafleet.Util;
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
            return Content("OK!");
        }

        public async Task<IActionResult> WorkingCheckAsync(int? interval)
        {
            if (!CookieUtil.IsAdmin(HttpContext))
            {
                return NotFound();
            }

            if (jafleet.WorkingCheck.Processing)
            {
                await SlackUtil.PostAsync(SlackChannelEnum.jafleet.GetStringValue(), "WorkingCheck 二重起動を検出");
                return Content("Now Processing!!");
            }

            _ = Task.Run(() =>
            {

                using var serviceScope = _services.CreateScope();
                IEnumerable<AircraftView> targetReg;
                using jafleetContext? context = serviceScope.ServiceProvider.GetService<jafleetContext>();
#if DEBUG
                targetReg = context!.AircraftView.Where(a => a.RegistrationNumber == "JA26LR").AsNoTracking().ToArray();
#else
                        targetReg = context!.AircraftView.Where(a => a.OperationCode != OperationCode.RETIRE_UNREGISTERED).AsNoTracking().ToArray().OrderBy(r => Guid.NewGuid());
#endif
                var check = new WorkingCheck(targetReg, interval ?? 15);
                _ = check.ExecuteCheckAsync();
            });

            return Content("WorkingCheck Launch!");
        }
    }
}