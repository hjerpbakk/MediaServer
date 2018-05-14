using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MediaServer.Models;
using MediaServer.Services;
using MediaServer.Services.Cache;
using System.Collections.Generic;
using System;
using MediaServer.ViewModels;

namespace MediaServer.Controllers {
	public class HomeController : NavigateableController {
		readonly ConferenceService conferenceService;
		readonly MediaCache cache;
        
		public HomeController(Dictionary<string, Conference> conferences, ConferenceService conferenceService, MediaCache cache) 
			: base(conferences) {
			this.conferenceService = conferenceService;
			this.cache = cache;
        }
        
		[ResponseCache(NoStore = true)]      
        public async Task<IActionResult> Index() {
			Console.WriteLine("GetLatestTalks ");
			return await cache.GetOrSet(MediaCache.LatestTalksKey, GetView);
            
			async Task<IActionResult> GetView() {
                SetCurrentNavigationToHome();
				var talks = await conferenceService.GetLatestTalks();
				var talksViewModel = new TalksViewModel(talks);            
				return View(talksViewModel);
            }
        }      
    }
}
