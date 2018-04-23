using System;
using MediaServer.Extensions;

namespace MediaServer.Models
{
    public class TalkViewModel
    {
        readonly Talk talk;

        public TalkViewModel(Talk talk)
        {
            this.talk = talk;
            UriEncodedVideoName = Uri.EscapeUriString(talk.VideoName);

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

        public string TalkName => talk.TalkName;
        public string VideoName => talk.VideoName;
        public string Description => talk.Description;
        public string Speaker => talk.Speaker;
    }
}
