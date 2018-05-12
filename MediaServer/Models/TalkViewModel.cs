using System;

namespace MediaServer.Models {
	public class TalkViewModel : TalkModel {
        public TalkViewModel(Talk talk, User user)
			: base(talk) {
            UriEncodedVideoName = Uri.EscapeUriString(talk.VideoName);
			ProfileImageUrl = user.ProfileImageUrl;
			VideoName = talk.VideoName;

            if (talk.SpeakerDeck == null) {
                return;
            }

            if (talk.SpeakerDeck.StartsWith("http", StringComparison.Ordinal)) {
                SpeakerDeck = talk.SpeakerDeck;
            } else {
                SpeakerDeck = Uri.EscapeUriString(talk.SpeakerDeck);    
            }
        }
              
        public string UriEncodedVideoName { get; }
        public string SpeakerDeck { get; }
		public string ProfileImageUrl { get; }      
		public string VideoName { get; }
    }
}
