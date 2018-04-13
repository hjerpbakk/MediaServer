using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MediaServer.Models;

namespace MediaServer.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // TODO: Ta inn konferansene programmatisk fra config i _Layout.cshtml
            ViewData["Title"] = "Latest Talks";

            // TODO: Vis siste opplastede uansett conference
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
