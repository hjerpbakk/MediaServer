using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MediaServer.Models;
using MediaServer.Services;
using MediaServer.Configuration;
using MediaServer.ViewModels;
using Microsoft.Extensions.Caching.Memory;
using MediaServer.Services.Cache;

namespace MediaServer.Controllers
{
	public class HomeController : NavigateableController
    {
		readonly IConferenceService conferenceService;
		readonly TalkCache talkCache;
        
		public HomeController(IConferenceConfig conferenceConfig, IConferenceService conferenceService, TalkCache talkCache) 
			: base(conferenceConfig) {
			this.conferenceService = conferenceService;
			this.talkCache = talkCache;
        }
        
		[ResponseCache(NoStore = true)]      
        public async Task<IActionResult> Index() {
			var view = await talkCache.GetOrSetView(GetView);
            return view;
        }

		public IActionResult Error() =>
            View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

		async Task<IActionResult> GetView() {
			SetCurrentNavigationToHome();         
            ViewData["Talks"] = await conferenceService.GetLatestTalks(HttpContext);         
            // TODO: Show conference link on card
            // TODO: Use model binding
            return View();
		}
    }
}
