using System;

namespace MediaServer.ViewModels {
	public struct TalksViewModel : IEquatable<TalksViewModel>, ITalksViewModel {
		public TalksViewModel(TalkSummary[] talks) {
			Talks = talks;
			ShowConference = true;
		}

		public TalkSummary[] Talks { get; }
		public bool ShowConference { get; }
        
		public bool Equals(TalksViewModel other) => Talks.Equals(other.Talks);
		public override bool Equals(object obj) => obj is TalksViewModel && Equals((TalksViewModel)obj);          
		public override int GetHashCode() => Talks.GetHashCode();     
    }
}
