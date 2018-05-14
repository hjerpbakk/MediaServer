using System;
using MediaServer.Models;

namespace MediaServer.ViewModels {
	// TODO: This is not a ViewModel in any universe
	public struct Navigation : IEquatable<Navigation> {
        public Navigation(Conference conference, string controllerName) {
			Slug = conference.Id;
			Title = conference.Name;
			Controller = controllerName;
        }

		public Navigation(string slug, string title, string controller)
        {
            Slug = slug;
            Title = title;
			Controller = controller;
        }

		public string Slug { get; }    
		public string Title { get; }
		public string Controller { get; }
        
		public bool Equals(Navigation other) => Slug == other.Slug;    
		public override bool Equals(object obj) => obj is Navigation && Equals((Navigation)obj);          
		public override int GetHashCode() => Slug.GetHashCode();     
	}
}
