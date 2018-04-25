using System;

namespace MediaServer.Models {
	public struct Conference : IEquatable<Conference> {
		public Conference(string id, string name) {
			Id = id;
			Name = name;
			SlackChannelId = "";
			VideoPath = "";
		}

		public Conference(string id, string name, string slackChannelId, string videoPath) {
			Id = id;
			Name = name;
			SlackChannelId = slackChannelId;
			VideoPath = videoPath;
		}

		public string Id { get; }
		public string Name { get; }
		public string SlackChannelId { get; }
		public string VideoPath { get; }

		public static Conference CreateLatestTalks() => new Conference("Index", "Latest Talks");

		public bool Equals(Conference other) => Id == other.Id;
	}
}
