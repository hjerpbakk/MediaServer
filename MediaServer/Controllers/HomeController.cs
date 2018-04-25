using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MediaServer.Models;
using MediaServer.Services;
using MediaServer.Configuration;
using MediaServer.ViewModels;

namespace MediaServer.Controllers
{
	public class HomeController : NavigateableController
    {
        readonly ITalkService talkService;

		public HomeController(IConferenceConfig conferenceConfig, ITalkService talkService) 
			: base(conferenceConfig) {
            this.talkService = talkService;
        }

        public async Task<IActionResult> Index() {
			SetHomeNavigation();

            var talks = (await talkService.GetLatestTalks(conferences.Values))
                .Select(latestTalk => new TalkSummaryViewModel(latestTalk.Conference, latestTalk.Talk, HttpContext));
            ViewData["Talks"] = talks;

            // TODO: Show conference link on card
            // TODO: Use model binding
            return View();
        }

		public IActionResult Error() =>
            View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
