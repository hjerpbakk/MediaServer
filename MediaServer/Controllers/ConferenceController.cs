using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediaServer.Models;
using MediaServer.Services;
using MediaServer.Services.Cache;
using Microsoft.AspNetCore.Mvc;

namespace MediaServer.Controllers {
	public class ConferenceController : NavigateableController {
		readonly MediaCache cache;      
		readonly ConferenceService conferenceService;
                
		public ConferenceController(Dictionary<string, Conference> conferences, MediaCache cache, ConferenceService conferenceService)
			: base(conferences) {         
			this.cache = cache;   
			this.conferenceService = conferenceService;         
		}
              
		[ResponseCache(NoStore = true)]
		[HttpGet("/Conference/{conferenceId}")]      
		public async Task<IActionResult> GetConferenceView(string conferenceId) {
			// TODO: User visible view count...
			Console.WriteLine("GetConferenceView " + conferenceId);         
			return await cache.GetOrSet(conferenceId, GetConferenceView);
            
			async Task<IActionResult> GetConferenceView() {
				if (!ConferenceExists(conferenceId)) {
                    return PageNotFound();
                }

                var conference = GetConferenceFromId(conferenceId);
                SetCurrentNavigation(conference, conference.Name);
				var conferenceViewModel = await conferenceService.GetConferenceWithContent(conference);            
				return View("Index", conferenceViewModel);
            }
		}      
    }
}
