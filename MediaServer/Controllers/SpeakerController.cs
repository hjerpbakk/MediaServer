using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediaServer.Clients;
using MediaServer.Models;
using MediaServer.Services;
using MediaServer.Services.Cache;
using Microsoft.AspNetCore.Mvc;

namespace MediaServer.Controllers {
	public class SpeakerController : NavigateableController {
		readonly ConferenceService conferenceService;
		readonly MediaCache talkCache;
		readonly Users users;
		readonly ISlackClient slackClient;
		readonly SpeakerService speakerService;

		public SpeakerController(Dictionary<string, Conference> conferences, ConferenceService conferenceService, MediaCache talkCache, Users users, ISlackClient slackClient, SpeakerService speakerService)
			: base(conferences) {
			// TODO: Too much stuff, move more to speaker service
			// TODO: Speaker count bubble has the wrong color
			// TODO: Speaker count bubble is too small
			this.conferenceService = conferenceService;
			this.talkCache = talkCache;
			this.users = users;
			this.slackClient = slackClient;
			this.speakerService = speakerService;
		}
              
		[ResponseCache(NoStore = true)]
		[HttpGet("/[controller]/{speakerName}")]
		public async Task<IActionResult> Index(string speakerName) {
			Console.WriteLine("GetTalksForSpeaker " + speakerName);
			var view = await talkCache.GetOrSet(
				speakerName, 
				() => GetViewForSpeaker());   
			return view;
            
			async Task<IActionResult> GetViewForSpeaker() {
                SetCurrentNavigation(speakerName);
                ViewData["Talks"] = await conferenceService.GetTalksBySpeaker(speakerName);
                var user = users.GetUser(speakerName);
                ViewData["User"] = user;
                ViewData["SlackUrl"] = slackClient.GetDmLink(user.Name, user.SlackId);
                return View("Views/Home/Index.cshtml");
            }
		}
        
		[ResponseCache(NoStore = true)]
		[HttpGet("/[controller]/List")]
		public async Task<IActionResult> List() {
			Console.WriteLine("GetSpeakers");
			var view = await talkCache.GetOrSet(MediaCache.SpeakersKey, () => List());
            return view;

			async Task<IActionResult> List() {            
				SetCurrentNavigationToSpeakerList();            
				var speakers = await speakerService.GetSpeakers();            
				return View("List", speakers);
			}
		}
	}
}
