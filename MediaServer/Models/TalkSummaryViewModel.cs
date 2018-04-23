using System;
using MediaServer.Extensions;

namespace MediaServer.Models
{
    public class TalkSummaryViewModel
    {
        readonly Talk talk;

		public TalkSummaryViewModel(Talk talk, Func<Talk, string> getTalkUrl)
        {
            this.talk = talk;
			Url = getTalkUrl(talk);
        }

        public string Url { get; }
        public string ZonedTimeStamp => talk.DateOfTalkString;
        
        public string TalkName => talk.TalkName;
        public string Description => talk.Description;
        public string Thumbnail => talk.Thumbnail;
        public string Speaker => talk.Speaker;
    }
}
