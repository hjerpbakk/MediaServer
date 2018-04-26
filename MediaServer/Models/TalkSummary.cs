using MediaServer.Extensions;
using Microsoft.AspNetCore.Http;

namespace MediaServer.Models
{
    public class TalkSummary
    {
        readonly Talk talk;

        public TalkSummary(Talk talk, string url, string thumbnail)
        {
            this.talk = talk;
			Url = url;
			Thumbnail = thumbnail;
        }

        public string Url { get; }
        public string Thumbnail { get; }
        public string ZonedTimeStamp => talk.DateOfTalkString;
        public string TalkName => talk.TalkName;
        public string Description => talk.Description;
        public string Speaker => talk.Speaker;
    }
}
