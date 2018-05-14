using System;
namespace MediaServer.ViewModels
{
	public struct ConferenceViewModel : IEquatable<ConferenceViewModel>, ITalksViewModel
	{
		public ConferenceViewModel(TalkSummary[] talks, string videoPath, string slackUrl) {
            Talks = talks;
			ShowConference = false;
			VideoPath = videoPath;
			SlackUrl = slackUrl;
        }

        public TalkSummary[] Talks { get; }
        public bool ShowConference { get; }
		public string VideoPath { get; }
		public string SlackUrl { get; }
        
		public bool Equals(ConferenceViewModel other) => Talks.Equals(other.Talks);
		public override bool Equals(object obj) => obj is ConferenceViewModel && Equals((ConferenceViewModel)obj);          
        public override int GetHashCode() => Talks.GetHashCode();     
	}
}
