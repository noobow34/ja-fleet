using System;
using System.Linq;
using jafleet.Commons.Constants;
using jafleet.Commons.EF;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace jafleet.Controllers
{
    public class WorkingCheckLogController : Controller
    {
        private readonly jafleetContext _context;

        public WorkingCheckLogController(jafleetContext context)
        {
            _context = context;
        }
        public IActionResult Index(string id)
        {
            if(id == null)
            {
                id = DateTime.Now.ToString("yyyyMMdd");
            }

            string log = string.Join("-----------------",_context.Log.Where(l => l.LogDateYyyyMmDd == id && l.LogType == LogType.WORKING_INFO)
                        .AsNoTracking().OrderByDescending(l => l.LogDate).Select(l => l.LogDetail));

            return Content(log);
        }
    }
}