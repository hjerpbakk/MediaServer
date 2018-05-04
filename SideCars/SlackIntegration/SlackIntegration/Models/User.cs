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
			ProfileImageUrl = slackUser.Image;
			SlackId = slackUser.Id;
			Description = slackUser.WhatIDo;
        }

		public string Name { get; }
		public string ProfileImageUrl { get; }
		public string SlackId { get; }
		public string Description { get; }
    }
}
