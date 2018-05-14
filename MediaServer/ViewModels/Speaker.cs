using System;

namespace MediaServer.ViewModels {
	public struct Speaker : IEquatable<Speaker> {
		public Speaker(string name, string profileImageUrl, int talkCount) {
			Name = name;
			ProfileImageUrl = profileImageUrl;
			TalkCount = talkCount;
		}

		public string Name { get; }
		public string ProfileImageUrl { get; }
		public int TalkCount { get; }

		public bool Equals(Speaker other) => Name == other.Name;
		public override bool Equals(object obj) => obj is Speaker && Equals((Speaker)obj);          
        public override int GetHashCode() => Name.GetHashCode();     
	}
}
