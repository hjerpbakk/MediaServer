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
		protected NavigateableController(IDictionary<string, Conference> conferences) {
			this.conferences = conferences;
			var navigations = new List<Navigation>(conferences.Count + 1) {
				new Navigation(Conference.CreateLatestTalks(), "Home")
			};
			navigations.AddRange(conferences.Values
			                     .Select(c => new Navigation(c, "Conference")));
			Navigations = navigations;
		}

		public IReadOnlyList<Navigation> Navigations { get; }
               
		protected void SetCurrentNavigationToHome() {
			var conference = Conference.CreateLatestTalks();         
			SetTitle(conference.Name);
			SetCurrentMenuItem(conference);
			SetAvailableMenuItems();
		}

		protected void SetCurrentNavigation(Conference conference, string title) {
			SetTitle(title);
			SetCurrentMenuItem(conference);
			SetAvailableMenuItems();
		}

		protected void SetCurrentNavigation(string title) {
			SetTitle(title);
			ClearCurrentMenuItem();
			SetAvailableMenuItems();
		}

		protected bool ConferenceExists(string conferenceId) => conferences.ContainsKey(conferenceId);
		protected Conference GetConferenceFromId(string conferenceId) => conferences[conferenceId];

		void SetCurrentMenuItem(Conference conference) => ViewData["Slug"] = conference.Id;
		void ClearCurrentMenuItem() => ViewData["Slug"] = string.Empty;
		void SetAvailableMenuItems() => ViewData["Navigations"] = Navigations;
		void SetTitle(string title) => ViewData["Title"] = title;
	}
}
