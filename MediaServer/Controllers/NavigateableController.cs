using System.Collections.Generic;
using System.Linq;
using MediaServer.Configuration;
using MediaServer.Models;
using MediaServer.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace MediaServer.Controllers {
	public abstract class NavigateableController : Controller {
		protected readonly Dictionary<string, Conference> conferences;

		readonly int LastNavigation;      
        
		// TODO: Inject or something #singleton         
		protected NavigateableController(Dictionary<string, Conference> conferences) {
			this.conferences = conferences;
			var navigations = new List<Navigation>(conferences.Count + 2) {
				new Navigation("Index", "Latest Talks", "Home")            
			};
			navigations.AddRange(conferences.Values.Select(c => new Navigation(c, "Conference")));
			navigations.Add(new Navigation("List", "Speakers", "Speaker"));
			Navigations = navigations;
			LastNavigation = navigations.Count - 1;
		}

		public List<Navigation> Navigations { get; }
               
		protected void SetCurrentNavigationToHome() {
			var navigation = Navigations[0];
			SetCurrentNavigation(navigation);
		}

		protected void SetCurrentNavigationToSpeakerList() {
			var navigation = Navigations[LastNavigation];
            SetCurrentNavigation(navigation);
		}

		protected void SetCurrentNavigation(Conference conference, string title) {
			SetTitle(title);
			SetSlug(conference);
			SetAvailableMenuItems();
		}

		protected void SetCurrentNavigation(string title) {
			SetTitle(title);
			ClearCurrentMenuItem();
			SetAvailableMenuItems();
		}      

		protected bool ConferenceExists(string conferenceId) => conferences.ContainsKey(conferenceId);
		protected Conference GetConferenceFromId(string conferenceId) => conferences[conferenceId];

		void SetSlug(Conference conference) => ViewData["Slug"] = conference.Id;
		void ClearCurrentMenuItem() => ViewData["Slug"] = string.Empty;
		void SetAvailableMenuItems() => ViewData["Navigations"] = Navigations;
		void SetTitle(string title) => ViewData["Title"] = title;

		// TODO: Improve entire setup here, ref use of conference and dictionary
		void SetCurrentNavigation(Navigation navigation) {
			ViewData["Title"] = navigation.Title;
			ViewData["Slug"] = navigation.Slug;
			ViewData["Navigations"] = Navigations;
		}
	}
}
