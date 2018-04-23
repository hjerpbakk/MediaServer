using System;

namespace MediaServer.Models
{
    public class TalkSummaryViewModel
    {
        readonly Talk talk;

        public TalkSummaryViewModel(Talk talk, Func<Talk, string> getTalkUrl, Func<Talk, string> getThumbnailUrl)
        {
            this.talk = talk;
			Url = getTalkUrl(talk);
            Thumbnail = getThumbnailUrl(talk);
        }

        public string Url { get; }
        public string Thumbnail { get; }
        public string ZonedTimeStamp => talk.DateOfTalkString;
        
        public string TalkName => talk.TalkName;
        public string Description => talk.Description;
        public string Speaker => talk.Speaker;
    }
}
