using System;
using Newtonsoft.Json;

namespace SlackIntegration.Models
{
	public struct Conference : IEquatable<Conference>
	{
		[JsonConstructor]
		public Conference(string id, string slackChannelId)
        {
            Id = id;
            SlackChannelId = slackChannelId;
        }

		public string Id { get; }
		public string SlackChannelId { get; }

		public bool Equals(Conference other) => Id == other.Id;
	}
}
