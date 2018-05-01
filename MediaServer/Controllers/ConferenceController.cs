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

namespace MediaServer.Controllers
{
	public class ConferenceController : NavigateableController
	{
		readonly IOldTalkService talkService;
		readonly IContentService contentService;
		readonly ISlackService slackService;
		readonly IConferenceService conferenceService;
		readonly IThumbnailService thumbnailService;
		readonly MediaCache cache;
        
		public ConferenceController(IConferenceConfig conferenceConfig, IOldTalkService talkService, IContentService contentService, ISlackService slackService, IConferenceService conferenceService, IThumbnailService thumbnailService, MediaCache cache)
			: base(conferenceConfig)
		{
			// TODO: Too many services, move around?         
			this.talkService = talkService;
			this.contentService = contentService;
			this.slackService = slackService;
			this.conferenceService = conferenceService;
			this.thumbnailService = thumbnailService;
			this.cache = cache;

			// TODO: Need speaker abstraction
		}
              
		[ResponseCache(NoStore = true)]
		[HttpGet("/Conference/{conferenceId}")]      
		public async Task<IActionResult> GetConferenceView(string conferenceId)
		{
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

			talk.Thumbnail = await thumbnailService.GetThumbnailUrl(conference, talk, HttpContext);
            var controllerName = ControllerContext.RouteData.Values["controller"].ToString();
            var availableVideos = new List<Video>() { new Video(talk.VideoName) };
			var videosFromConference = await contentService.GetVideosFromConference(controllerName, conference);
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
			var availableVideos = await contentService.GetVideosFromConference(controllerName, conference);
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

            // TODO: Client verification also and proper replace here
			// TODO: Talks with # in filename or name doesn't work now
            talk.TalkName = talk.TalkName.Replace("?", "").Replace(":", "");
			await talkService.SaveTalkFromConference(conference, talk);
			await thumbnailService.SaveThumbnail(conference, talk);
            
			var talkUrl = HttpContext.GetTalkUrl(conference, talk);

            if (oldName == null)
            {
				var thumbnailUrl = await thumbnailService.GetThumbnailUrl(conference, talk, HttpContext);
				await slackService.PostTalkToChannel(conference, talk, talkUrl, thumbnailUrl);
            }

            var escapedTalkName = Uri.EscapeUriString(talk.TalkName);
            return new RedirectResult(escapedTalkName, false, false);
		}
        
		async Task<IActionResult> GetAllTalksFromConferenceView(string conferenceId)
        {
			// TODO: Make conference header clickable and open Slack channel
            // TODO: Make O: link clickable
            // TODO: Button for add is blue on click
            // TODO: Button for add is ugly
            // TODO: Phone home so we can see most popular conference         
            if (!ConferenceExists(conferenceId))
            {
                return NotFound();
            }

            var conference = GetConferenceFromId(conferenceId);
            SetCurrentNavigation(conference, conference.Name);

            // TODO: Create a conference viewmodel and use model binding
            ViewData["VideoPath"] = conference.VideoPath;
            ViewData["Talks"] = await conferenceService.GetTalksForConference(conference, HttpContext);

            return View("Index");
        }

		async Task<IActionResult> GetTalkViewFromService(string conferenceId, string talkName)
        {
			// TODO: Fix this by specify constarint or something in the routing
			var extension = Path.GetExtension(talkName);         
			if (extension == Video.SupportedVideoFileType || extension == Talk.DefaultSpeakerDeckFileExtension) {
				return NotFound();
			}

			// TODO: Phone home so we can see most popular talk
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
            var talkVM = new TalkViewModel(talk);
            ViewData["Talk"] = talkVM;

            return View("Talk");
        }
    }
}
