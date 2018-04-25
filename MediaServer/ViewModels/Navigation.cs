using System;
using MediaServer.Models;

namespace MediaServer.ViewModels {
	public struct Navigation : IEquatable<Navigation> {
        public Navigation(Conference conference, string controllerName) {
			Slug = conference.Id;
			Title = conference.Name;
			Controller = controllerName;
        }

		public string Slug { get; }    
		public string Title { get; }
		public string Controller { get; }
        
		public bool Equals(Navigation other) => Slug == other.Slug;     
	}
}
