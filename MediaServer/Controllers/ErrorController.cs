using System.Collections.Generic;
using System.Diagnostics;
using MediaServer.Models;
using MediaServer.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace MediaServer.Controllers
{
	public class ErrorController : NavigateableController
    {
		public ErrorController(Dictionary<string, Conference> conferences)
			: base(conferences) {         
		}

		[Route("error/404")]
        public IActionResult Error404() {
			return PageNotFound();
        }

		[Route("error/")]
		[Route("error/{code:int}")]
		[ResponseCache(NoStore = true)]   
        public IActionResult Error() {
            SetCurrentNavigation("Error");
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
