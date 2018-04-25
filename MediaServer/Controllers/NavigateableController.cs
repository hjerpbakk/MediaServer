using System.Collections.Generic;
using System.Linq;
using MediaServer.Configuration;
using MediaServer.Models;
using MediaServer.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace MediaServer.Controllers {
	public abstract class NavigateableController : Controller {
		protected readonly IDictionary<string, Conference> conferences;
        
		// TODO: Inject or something #singleton         
		protected NavigateableController(IConferenceConfig conferenceConfig) {
			conferences = conferenceConfig.Conferences;
			var navigations = new List<Navigation>(conferences.Count + 1) {
				new Navigation(Conference.CreateLatestTalks(), "Home")
			};
			navigations.AddRange(conferences.Values
			                     .Select(c => new Navigation(c, "Conference")));
			Navigations = navigations;
		}

		public IReadOnlyList<Navigation> Navigations { get; }
               
		protected void SetHomeNavigation() {
			var conference = Conference.CreateLatestTalks();
			ViewData["Title"] = conference.Name;
			SetNavigation(conference);
		}

		protected void SetCurrentNavigation(Conference conference, string title) {
			ViewData["Title"] = title;
			SetNavigation(conference);
		}

		protected bool ConferenceExists(string conferenceId) 
		    => conferences.ContainsKey(conferenceId);

		protected Conference GetConferenceFromId(string conferenceId)
			=> conferences[conferenceId];

		void SetNavigation(Conference conference) {
			ViewData["MenuTitle"] = conference.Name;
            ViewData["Slug"] = conference.Id;
			ViewData["Navigations"] = Navigations;         
		}
	}
}
