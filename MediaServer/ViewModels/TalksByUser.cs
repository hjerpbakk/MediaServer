using System;
using MediaServer.Models;

namespace MediaServer.ViewModels {
	public struct TalksByUser : IEquatable<TalksByUser>, ITalksViewModel {
		public TalksByUser(User user, TalkSummary[] talks, string slackDmLink) {
			User = user;
			Talks = talks;
			SlackDmLink = slackDmLink;
			ShowConference = true;
		}

		public TalkSummary[] Talks { get; }
        public bool ShowConference { get; }
		public User User { get; }      
		public string SlackDmLink { get; }

		public bool Equals(TalksByUser other) => User.Equals(other.User);
		public override bool Equals(object obj) => obj is TalksByUser && Equals((TalksByUser)obj);          
		public override int GetHashCode() => User.GetHashCode();     
	}
}
