using System;
using MediaServer.Models;

namespace MediaServer.ViewModels {
	public struct TalksByUser : IEquatable<TalksByUser> {
		public TalksByUser(User user, TalkSummary[] talkSummaries, string slackDmLink) {
			User = user;
			TalkSummaries = talkSummaries;
			SlackDmLink = slackDmLink;
		}

		public User User { get; }
		public TalkSummary[] TalkSummaries { get; }
		public string SlackDmLink { get; }

		public bool Equals(TalksByUser other) => User.Equals(other.User);
		public override bool Equals(object obj) => obj is TalksByUser && Equals((TalksByUser)obj);          
		public override int GetHashCode() => User.GetHashCode();     
	}
}
