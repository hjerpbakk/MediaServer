using Newtonsoft.Json;

namespace MediaServer.Models
{
	public struct User
    {
		public const string UnknownUser = "This person is a mystery to all beings.";

		[JsonConstructor]
		public User(string name, string profileImageUrl, string slackId, string description)
        {
			Name = name;
			ProfileImageUrl = profileImageUrl;
			SlackId = slackId;
			Description = string.IsNullOrEmpty(description) ? UnknownUser : description;
        }

        public string Name { get; }
		public string ProfileImageUrl { get; }
		public string SlackId { get; }
		public string Description { get; }
    }
}
