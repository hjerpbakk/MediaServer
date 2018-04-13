using System;
using System.Linq;
using System.Diagnostics;
using MediaServer.Configuration;
using MediaServer.Models;
using MediaServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

namespace MediaServer.Controllers
{
	public class ConferenceController : Controller
	{
		readonly IConferenceConfig conferenceConfig;

		readonly ITalkService talkService;
		readonly IContentService contentService;
		readonly ISlackService slackService;

		public ConferenceController(IConferenceConfig conferenceConfig, ITalkService talkService, IContentService contentService, ISlackService slackService)
		{
			// TODO: Too many services, move around?
			this.conferenceConfig = conferenceConfig;
			this.talkService = talkService;
			this.contentService = contentService;
			this.slackService = slackService;
		}

		[HttpGet("/[controller]/{conferenceId}")]
		public async Task<IActionResult> GetConferenceView(string conferenceId)
		{
			var conference = GetConferenceFromId(conferenceId);
			if (conference == null)
			{
				return NotFound();
			}

			ViewData["Title"] = conference.Name;
			ViewData["ConferenceId"] = conferenceId;

            var talks = (await talkService.GetTalksFromConference(conference)).
                            OrderByDescending(t => t.TimeStamp).
                            Select(t => new TalkSummaryViewModel(t, talk => GetTalkUrl(conference, talk)));
			ViewData["Talks"] = talks;

			return View("Index");
		}

		[HttpGet("/[controller]/{conferenceId}/{talkName}")]
		public async Task<IActionResult> GetTalkView(string conferenceId, string talkName)
		{
			var conference = GetConferenceFromId(conferenceId);
			if (conference == null)
			{
				return NotFound();
			}

			var talk = await talkService.GetTalkByName(conference, talkName);
			if (talk == null)
			{
				// TODO: Lag en fin 404
				return NotFound();
			}

			ViewData["Title"] = talkName;
			var talkVM = new TalkViewModel(talk);
			ViewData["Talk"] = talkVM;

			return View("Talk");
		}

		[HttpGet("/[controller]/{conferenceId}/Save")]
		public async Task<IActionResult> GetSaveView(string conferenceId)
		{
			// TODO: Support Edit also...
			var conference = GetConferenceFromId(conferenceId);
			if (conference == null)
			{
				return NotFound();
			}

			ViewData["Title"] = $"Create new talk from {conference.Name}";

			var controllerName = ControllerContext.RouteData.Values["controller"].ToString();
			var availableVideos = await contentService.GetVideosFromConference(controllerName, conference);
			ViewBag.VideoList = new SelectList(availableVideos, "Name", "Name");

			return View("Save");
		}

		[HttpPost("/[controller]/{conferenceId}/Save")]
		public async Task<IActionResult> SaveTalk(string conferenceId, [Bind("Name", "Description, Speaker, SpeakerDeck")] Talk talk)
		{
			// TODO: Progressbar eller noe???
			var conference = conferenceConfig.Conferences[conferenceId];

			talk.TimeStamp = DateTime.UtcNow;
			await talkService.SaveTalkFromConference(conference, talk);

			var talkUrl = GetTalkUrl(conference, talk);
			await slackService.PostTalkToChannel(conference, talk, talkUrl);

			return new RedirectResult(talk.UriEncodedName, false, false);
		}

		Conference GetConferenceFromId(string conferenceId)
			=> conferenceConfig.Conferences.ContainsKey(conferenceId) ?
			   conferenceConfig.Conferences[conferenceId] :
			   null;

		string GetTalkUrl(Conference conference, Talk talk)
            => GetConferenceUrl(conference) + talk.UriEncodedName;

		string GetConferenceUrl(Conference conference) 
			=> $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/{ControllerContext.RouteData.Values["controller"]}/{conference.Id}/";
    }
}
