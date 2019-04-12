using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ja_fleet.Controllers
{
    public class CheckController : Controller
    {
        public IActionResult Index()
        {
            return Content("1");
        }
    }
}