using System;
using System.Linq;
using MediaServer.Configuration;
using MediaServer.Extensions;
using MediaServer.Models;
using MediaServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using MediaServer.ViewModels;
using MediaServer.Services.Cache;
using System.IO;
using MediaServer.Clients;

namespace MediaServer.Controllers
{
	public class ConferenceController : NavigateableController
	{
		readonly TalkService talkService;
		readonly ContentService contentService;
		readonly ISlackClient slackClient;
		readonly ConferenceService conferenceService;
		readonly ThumbnailService thumbnailService;
		readonly MediaCache cache;
		readonly Users users;
        
		public ConferenceController(ConferenceConfig conferenceConfig, TalkService talkService, ContentService contentService, ISlackClient slackClient, ConferenceService conferenceService, ThumbnailService thumbnailService, MediaCache cache, Users users)
			: base(conferenceConfig)
		{
			// TODO: Too many services, move around?         
			this.talkService = talkService;
			this.contentService = contentService;
			this.slackClient = slackClient;
			this.conferenceService = conferenceService;
			this.thumbnailService = thumbnailService;
			this.cache = cache;
			this.users = users;
			// TODO: Need speaker abstraction
		}
              
		[ResponseCache(NoStore = true)]
		[HttpGet("/Conference/{conferenceId}")]      
		public async Task<IActionResult> GetConferenceView(string conferenceId)
		{
			// TODO: User visible view count...
			var view = await cache.GetOrSet(
				conferenceId,
				() => GetAllTalksFromConferenceView(conferenceId));
            return view;         
		}

		[ResponseCache(NoStore = true)]
		[HttpGet("/Conference/{conferenceId}/{talkName}")]
		public async Task<IActionResult> GetTalkView(string conferenceId, string talkName)
		{
			var view = await cache.GetOrSet(
				cache.GetTalkViewKey(conferenceId, talkName),
				() => GetTalkViewFromService(conferenceId, talkName));
            return view;    
		}
              
		[ResponseCache(NoStore = true)]
		[HttpGet("/Conference/{conferenceId}/{talkName}/Edit")]
		public async Task<IActionResult> GetEditView(string conferenceId, string talkName)
		{
            // TODO: SpeakerDeck dissapears if not from PDF
			if (!ConferenceExists(conferenceId))
            {
                return NotFound();
            }

            var conference = GetConferenceFromId(conferenceId);


            var talk = await talkService.GetTalkByName(conference, talkName);
            if (talk == null)
            {
                return NotFound();
            }

			SetCurrentNavigation(conference, $"Edit {talk.TalkName}");

			talk.Thumbnail = await thumbnailService.GetThumbnailUrl(talk);
            var controllerName = ControllerContext.RouteData.Values["controller"].ToString();
            var availableVideos = new List<Video>() { new Video(talk.VideoName) };
			var videosFromConference = await contentService.GetVideosFromConference(conference);
			availableVideos.AddRange(videosFromConference);
            ViewBag.VideoList = new SelectList(availableVideos, "Name", "Name", talk.VideoName);
                     
            ViewData["IsSave"] = false;
            ViewData["OldName"] = talk.TalkName;
			return View("Save", talk);
		}
              
		[ResponseCache(NoStore = true)]
		[HttpGet("/Conference/{conferenceId}/Save")]
		public async Task<IActionResult> GetSaveView(string conferenceId)
		{
            // TODO: Support choosing speaker name from Slack...
			// TODO: Support uploading slides
			// TODO: Support uploading video
			if (!ConferenceExists(conferenceId))
            {
                return NotFound();
            }

            var conference = GetConferenceFromId(conferenceId);

			SetCurrentNavigation(conference, $"Create new talk from {conference.Name}");

			// TODO: Find all usage and remove...
            var controllerName = ControllerContext.RouteData.Values["controller"].ToString();
			var availableVideos = await contentService.GetVideosFromConference(conference);
			ViewBag.VideoList = new SelectList(availableVideos, "Name", "Name");
            
			ViewData["IsSave"] = true;         
			ViewData["OldName"] = null;
            return View("Save", new Talk { Thumbnail = "/Placeholder.png" });
		}
              
		[ResponseCache(NoStore = true)]
		[HttpPost("/Conference/{conferenceId}/Save")]
        public async Task<IActionResult> SaveTalk(string conferenceId, [FromQuery] string oldName, [Bind("VideoName, Description, Speaker, SpeakerDeck, ThumbnailImageFile, TalkName, DateOfTalkString")] Talk talk)
		{
			// TODO: Get thumbnail from speaker notes or video if not set
			if (!ConferenceExists(conferenceId))
            {
                return NotFound();
            }

            var conference = GetConferenceFromId(conferenceId);

			if (oldName != null)
			{
                var oldTalk = new Talk { ConferenceId = conferenceId, TalkName = oldName, Speaker = talk.Speaker };
				await talkService.DeleteTalkFromConference(conference, oldTalk);
            }

            // TODO: Client verification also
			// TODO: proper replace here
			talk.TalkName = talk.TalkName.Replace("?", "").Replace(":", " - ").Replace("/", "-").Replace("\"", "-").Replace("#", "");
			contentService.VerifySlides(talk);
			await talkService.SaveTalkFromConference(conference, talk);
			await thumbnailService.SaveThumbnail(conference, talk, oldName);
                     
            if (oldName == null) {
				var talkUrl = Paths.GetFullPath(HttpContext, Paths.GetTalkUrl(talk));
				slackClient.PublishToSlack(talk, talkUrl);
            }

            var escapedTalkName = Uri.EscapeUriString(talk.TalkName);
            return new RedirectResult(escapedTalkName, false, false);
		}
        
		async Task<IActionResult> GetAllTalksFromConferenceView(string conferenceId)
        {
            if (!ConferenceExists(conferenceId))
            {
                return NotFound();
            }

            var conference = GetConferenceFromId(conferenceId);
            SetCurrentNavigation(conference, conference.Name);

            // TODO: Create a conference viewmodel and use model binding
            ViewData["VideoPath"] = conference.VideoPath;
            ViewData["Talks"] = await conferenceService.GetTalksForConference(conference);
			ViewData["SlackUrl"] = slackClient.GetChannelLink(conferenceId, conference.SlackChannelId);

            return View("Index");
        }

		async Task<IActionResult> GetTalkViewFromService(string conferenceId, string talkName)
        {
			// TODO: Fix this by specify constarint or something in the routing
			var extension = Path.GetExtension(talkName);         
			if (extension == Video.SupportedVideoFileType || extension == Talk.DefaultSpeakerDeckFileExtension) {
				return NotFound();
			}
                     
            // TODO: Show image of speaker from Slack
            if (!ConferenceExists(conferenceId))
            {
                return NotFound();
            }

            var conference = GetConferenceFromId(conferenceId);
            var talk = await talkService.GetTalkByName(conference, talkName);
            if (talk == null)
            {
                // TODO: Lag en fin 404
                return NotFound();
            }

            SetCurrentNavigation(conference, talk.TalkName);
			/// TODO: Support single click pause / resume
			/// 
			/// TODO: hotkeys:
			// -space: play / pause
			//- f: fullscreen
			//- opp / ned: volumkontroll
			//- venstre / høyre: skip back/ frem 5 sec elns
			/// 
			var user = users.GetUser(talk.Speaker);         
			var talkVM = new TalkViewModel(talk, user);
            ViewData["Talk"] = talkVM;

            return View("Talk");
        }
    }
}
