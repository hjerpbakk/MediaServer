using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MediaServer.Models;
using MediaServer.Services;
using MediaServer.Services.Cache;
using MediaServer.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MediaServer.Controllers {
	public class TalkController : NavigateableController {
		readonly MediaCache cache;
		readonly TalkService talkService;
		readonly ContentService contentService;

		public TalkController(Dictionary<string, Conference> conferences, MediaCache cache, TalkService talkService, ContentService contentService)
			: base(conferences) {
			this.cache = cache;
			this.talkService = talkService;
			this.contentService = contentService;
        }

        [ResponseCache(NoStore = true)]
        [HttpGet("/Conference/{conferenceId}/{talkName}")]
        public async Task<IActionResult> GetTalkView(string conferenceId, string talkName) {
            Console.WriteLine("GetTalkView " + conferenceId + " " + talkName);
            return await cache.GetOrSet(cache.GetTalkViewKey(conferenceId, talkName), GetTalkView);
            
            async Task<IActionResult> GetTalkView() {
                if (!ConferenceExists(conferenceId)) {
                    return PageNotFound();
                }

                // TODO: Fix this by specify constarint or something in the routing
                var extension = Path.GetExtension(talkName);
                if (extension == Video.SupportedVideoFileType || extension == Talk.DefaultSpeakerDeckFileExtension) {
                    return PageNotFound();
                }
                
                var conference = GetConferenceFromId(conferenceId);
                SetCurrentNavigation(conference, talkName);
                var talkViewModel = await talkService.GetTalkViewModel(conference, talkName);
                if (talkViewModel == null) {
                    return PageNotFound();
                }

                return View("Talk", talkViewModel);
            }
        }

        [ResponseCache(NoStore = true)]
        [HttpGet("/Conference/{conferenceId}/{talkName}/Edit")]
        public async Task<IActionResult> GetEditView(string conferenceId, string talkName) {
            Console.WriteLine("GetEditView " + conferenceId + " " + talkName);
            // TODO: SpeakerDeck dissapears if not from PDF??
            if (!ConferenceExists(conferenceId)) {
                return PageNotFound();
            }

            var conference = GetConferenceFromId(conferenceId);
            SetCurrentNavigation(conference, "Edit " + talkName);
			var talkMetadata = await talkService.GetTalkMetadata(conference, talkName);
			if (talkMetadata == null) {
				return PageNotFound();
			}

			ViewBag.VideoList = talkMetadata.AvailableVideos;         
			return View("Save", talkMetadata.TalkPersistenceViewModel);
        }
              
        [ResponseCache(NoStore = true)]
        [HttpGet("/Conference/{conferenceId}/Save")]
        public async Task<IActionResult> GetSaveView(string conferenceId) {
            Console.WriteLine("GetSaveView " + conferenceId);
            // TODO: Support choosing speaker name from Slack... https://leaverou.github.io/awesomplete/
            // TODO: Support uploading slides
            // TODO: Support uploading video
            if (!ConferenceExists(conferenceId)) {
                return PageNotFound();
            }

            var conference = GetConferenceFromId(conferenceId);         
            SetCurrentNavigation(conference, "Create new talk from " + conference.Name);
            
			var talkMetadata = await talkService.GetTalkMetadata(conference);
			ViewBag.VideoList = talkMetadata.AvailableVideos;
			return View("Save", talkMetadata.TalkPersistenceViewModel);
        }
              
        [ResponseCache(NoStore = true)]
        [HttpPost("/Conference/{conferenceId}/Save")]
        public async Task<IActionResult> SaveTalk(string conferenceId, [FromQuery] string oldName, [Bind("VideoName, Description, Speaker, SpeakerDeck, ThumbnailImageFile, TalkName, DateOfTalkString")] Talk talk) {
            // TODO: Get thumbnail from speaker notes or video if not set
            Console.WriteLine("SaveTalk " + conferenceId);
            if (!ConferenceExists(conferenceId)) {
                return PageNotFound();
            }

            var conference = GetConferenceFromId(conferenceId);
			string getUrlToTalk(Talk t) => GetUrlToTalk(HttpContext, GetTalkUrl(t));
			var escapedTalkName = await talkService.SaveTalk(conference, talk, oldName, getUrlToTalk);
            return new RedirectResult(escapedTalkName, false, false);
        }
    }
}
