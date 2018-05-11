using System;
using Newtonsoft.Json;

namespace MediaServer.Models {
	public struct User : IEquatable<User> {
		public const string UnknownUser = "This person is a mystery to all beings.";

		[JsonConstructor]
		public User(string name, string profileImageUrl, string slackId, string description) {
			Name = name;
			ProfileImageUrl = profileImageUrl;
			SlackId = slackId;
			Description = string.IsNullOrEmpty(description) ? UnknownUser : description;
        }

        public string Name { get; }
		public string ProfileImageUrl { get; }
		public string SlackId { get; }
		public string Description { get; }

		public bool Equals(User other) => Name == other.Name;      
		public override bool Equals(object obj) => obj is User && Equals((User)obj);          
        public override int GetHashCode() => Name.GetHashCode();     
    }
}
