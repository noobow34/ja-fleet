﻿using Microsoft.AspNetCore.Mvc;

namespace jafleet.Controllers
{
    public class SandboxController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Ex()
        {
            throw new System.Exception();
        }

    }
}
