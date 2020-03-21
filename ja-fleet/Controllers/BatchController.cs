using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using jafleet.Commons.Constants;
using jafleet.Commons.EF;
using jafleet.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace jafleet.Controllers
{
    public class BatchController : Controller
    {

        private readonly IServiceScopeFactory _services;

        public BatchController(IServiceScopeFactory serviceScopeFactory)
        {
            _services = serviceScopeFactory;
        }

        public IActionResult Index()
        {
            if (!CookieUtil.IsAdmin(HttpContext))
            {
                return NotFound();
            }
            return Content("OK!");
        }

        public IActionResult WorkingCheck(int? interval)
        {
            if (!CookieUtil.IsAdmin(HttpContext))
            {
                return NotFound();
            }
            _ = Task.Run(() =>
            {

                using (var serviceScope = _services.CreateScope())
                {
                    IEnumerable<Aircraft> targetReg;
                    using (var context = serviceScope.ServiceProvider.GetService<jafleetContext>())
                    {
                        targetReg = context.Aircraft.Where(a => a.OperationCode != OperationCode.RETIRE_UNREGISTERED).AsNoTracking().ToArray()
                                    .OrderBy(r => Guid.NewGuid());
                        var check = new WorkingCheck(targetReg,interval ?? 15);
                        _ = check.ExecuteCheckAsync();

                    }
                }
            });

            return Content("WorkingCheck Launch!");
        }
    }
}