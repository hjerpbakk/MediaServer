using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MediaServer.Models;
using MediaServer.Services;
using MediaServer.Configuration;
using MediaServer.ViewModels;
using Microsoft.Extensions.Caching.Memory;

namespace MediaServer.Controllers
{
	public class HomeController : NavigateableController
    {
		readonly IConferenceService conferenceService;
        
		public HomeController(IConferenceConfig conferenceConfig, IConferenceService conferenceService) 
			: base(conferenceConfig) {
			this.conferenceService = conferenceService;
        }

		[ResponseCache(NoStore = true)]      
        public async Task<IActionResult> Index() {
			SetCurrentNavigationToHome();
   
			ViewData["Talks"] = await conferenceService.GetLatestTalks(HttpContext);

            // TODO: Show conference link on card
            // TODO: Use model binding
            return View();
        }

		public IActionResult Error() =>
            View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
