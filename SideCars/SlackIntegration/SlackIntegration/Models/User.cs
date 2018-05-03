using SlackConnector.Models;

namespace SlackIntegration.Models
{
	public struct User
    {
		public User(SlackUser slackUser)
        {
			var name = "";
			if (!string.IsNullOrEmpty(slackUser.FirstName)) {
				name += slackUser.FirstName;
				if (!string.IsNullOrEmpty(slackUser.LastName)) {
					name += " " + slackUser.LastName;
				}
			} else {
				name = slackUser.Email;
			}

			Name = name;
			ProfileImage = slackUser.Image;
			SlackId = slackUser.Id;
        }

		public string Name { get; }
		public string ProfileImage { get; }
		public string SlackId;
    }
}
