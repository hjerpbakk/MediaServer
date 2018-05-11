using System;

namespace MediaServer.Models {
	public struct Video : IEquatable<Video> {
        public const string SupportedVideoFileType = ".mp4";
       
        public Video(string name) {
			Name = name;
        }

        public string Name { get; }

		public bool Equals(Video other) => Name == other.Name;      
		public override bool Equals(object obj) => obj is Video && Equals((Video)obj);          
		public override int GetHashCode() => Name.GetHashCode();      
	}
}
