using System;
using Newtonsoft.Json;

namespace MediaServer.Models {
	public class Conference : IEquatable<Conference> {
		public Conference()
		{
			// TODO: Write bug report to MS with simplest reprod, then blog about it when fixed
            // Also wrong about struct and parameterless constructor
			// Tags: configurationbuilder AddJsonFile jsonconstructor
		}

		public Conference(string id, string name) {
			Id = id;
			Name = name;
			SlackChannelId = "";
			VideoPath = "";
		}

        [JsonConstructor]
		public Conference(string id, string name, string slackChannelId, string videoPath) {
			Id = id;
			Name = name;
			SlackChannelId = slackChannelId;
			VideoPath = videoPath;
		}

		public string Id { get; set; }
		public string Name { get; set; }
		public string SlackChannelId { get; set; }
		public string VideoPath { get; set; }

		public static Conference CreateLatestTalks() => new Conference("Index", "Latest Talks");

		public bool Equals(Conference other) => Id == other.Id;
	}
}
