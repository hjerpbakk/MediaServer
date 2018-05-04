using System;

namespace MediaServer.Models
{
    public class TalkViewModel
    {
        readonly Talk talk;
        
        public TalkViewModel(Talk talk, User user)
        {
            this.talk = talk;
            UriEncodedVideoName = Uri.EscapeUriString(talk.VideoName);
			ProfileImageUrl = user.ProfileImageUrl;

            if (talk.SpeakerDeck == null) {
                return;
            }

            if (talk.SpeakerDeck.StartsWith("http", StringComparison.Ordinal)) {
                SpeakerDeck = talk.SpeakerDeck;
            } else {
                SpeakerDeck = Uri.EscapeUriString(talk.SpeakerDeck);    
            }
        }

        public string ZonedTimeStamp => talk.DateOfTalkString; 
        public string UriEncodedVideoName { get; }
        public string SpeakerDeck { get; }
		public string ProfileImageUrl { get; }

        public string TalkName => talk.TalkName;
        public string VideoName => talk.VideoName;
        public string Description => talk.Description;
        public string Speaker => talk.Speaker;
    }
}
