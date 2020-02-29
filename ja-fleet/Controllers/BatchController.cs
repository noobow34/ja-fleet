using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using jafleet.Commons.Constants;
using jafleet.Commons.EF;
using jafleet.Util;
using Microsoft.AspNetCore.Mvc;
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

        public IActionResult WorkingCheck()
        {
            if (!CookieUtil.IsAdmin(HttpContext))
            {
                return NotFound();
            }
            _ = Task.Run(() =>
            {

                using (var serviceScope = _services.CreateScope())
                {
                    IEnumerable<string> targetReg;
                    using (var context = serviceScope.ServiceProvider.GetService<jafleetContext>())
                    {
                        targetReg = context.Aircraft.Where(a => a.OperationCode != OperationCode.RETIRE_UNREGISTERED).ToList().Select(a => a.RegistrationNumber);
                        var check = new WorkingCheck(targetReg);
                        _ = check.ExecuteCheckAsync();
                    }
                }
            });

            return Content("WorkingCheck Launch!");
        }
    }
}