using System.Threading.Tasks;
using MediaServer.Configuration;
using MediaServer.Models;
using MediaServer.Services;
using MediaServer.Services.Cache;
using Microsoft.AspNetCore.Mvc;

namespace MediaServer.Controllers
{
	public class SpeakerController : NavigateableController
	{
		readonly ConferenceService conferenceService;
		readonly MediaCache talkCache;
		readonly Users users;

		public SpeakerController(ConferenceConfig conferenceConfig, ConferenceService conferenceService, MediaCache talkCache, Users users)
			: base(conferenceConfig)
		{
			this.conferenceService = conferenceService;
			this.talkCache = talkCache;
			this.users = users;
		}

		// TODO: Add a top speaker list
		[ResponseCache(NoStore = true)]
		[HttpGet("/[controller]/{speakerName}")]
		public async Task<IActionResult> Index(string speakerName)
		{
			var view = await talkCache.GetOrSet(
				speakerName, 
				() => GetViewForSpeaker(speakerName));         
			return view;
		}

		async Task<IActionResult> GetViewForSpeaker(string speakerName)
		{
			SetCurrentNavigation(speakerName);
			ViewData["Talks"] = await conferenceService.GetTalksBySpeaker(speakerName);
			ViewData["User"] = users.GetUser(speakerName);
			return View("Views/Home/Index.cshtml");
		}
	}
}
