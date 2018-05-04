using System.Threading.Tasks;
using MediaServer.Clients;
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
		readonly ISlackClient slackClient;

		public SpeakerController(ConferenceConfig conferenceConfig, ConferenceService conferenceService, MediaCache talkCache, Users users, ISlackClient slackClient)
			: base(conferenceConfig)
		{
			this.conferenceService = conferenceService;
			this.talkCache = talkCache;
			this.users = users;
			this.slackClient = slackClient;
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
			var user = users.GetUser(speakerName);
			ViewData["User"] = user;
			ViewData["SlackUrl"] = slackClient.GetDmLink(user.Name, user.SlackId);
			return View("Views/Home/Index.cshtml");
		}
	}
}
