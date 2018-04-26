using MediaServer.Extensions;
using Microsoft.AspNetCore.Http;

namespace MediaServer.Models
{
    public class TalkSummary
    {
        readonly Talk talk;

        public TalkSummary(Conference conference, Talk talk, HttpContext httpContext)
        {
            this.talk = talk;
            Url = httpContext.GetTalkUrl(conference, talk);

            Thumbnail = httpContext.GetThumbnailUrl(conference, talk);
        }

        public string Url { get; }
        public string Thumbnail { get; }
        public string ZonedTimeStamp => talk.DateOfTalkString;
        
        public string TalkName => talk.TalkName;
        public string Description => talk.Description;
        public string Speaker => talk.Speaker;
    }
}
