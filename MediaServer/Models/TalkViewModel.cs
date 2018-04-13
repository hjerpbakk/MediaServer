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
            ZonedTimeStamp = talk.TimeStamp.GetDateString();
			Video = talk.UriEncodedName + Talk.SupportedVideoFileType;

            if (talk.SpeakerDeck == null) {
                return;
            }

            if (talk.SpeakerDeck.StartsWith("http", StringComparison.Ordinal)) {
                SpeakerDeck = talk.SpeakerDeck;
            } else {
                SpeakerDeck = Uri.EscapeUriString(talk.SpeakerDeck);    
            }
        }

        public string ZonedTimeStamp { get; } 
        public string Video { get; }
        public string SpeakerDeck { get; }

        public string Name => talk.Name;
        public string Description => talk.Description;
        public string Speaker => talk.Speaker;
        //public string SpeakerDeck => talk.SpeakerDeck;
    }
}
