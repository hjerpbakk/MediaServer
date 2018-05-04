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
		readonly SlackIntegrationClient slackIntegrationClient;

		public SpeakerController(ConferenceConfig conferenceConfig, ConferenceService conferenceService, MediaCache talkCache, Users users, SlackIntegrationClient slackIntegrationClient)
			: base(conferenceConfig)
		{
			this.conferenceService = conferenceService;
			this.talkCache = talkCache;
			this.users = users;
			this.slackIntegrationClient = slackIntegrationClient;
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
			ViewData["SlackUrl"] = slackIntegrationClient.GetDmLink(user.Name, user.SlackId);
			return View("Views/Home/Index.cshtml");
		}
	}
}
