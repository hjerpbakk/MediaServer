using MediaServer.Extensions;
using Microsoft.AspNetCore.Http;

namespace MediaServer.Models
{
    public class TalkSummaryViewModel
    {
        readonly Talk talk;

        public TalkSummaryViewModel(Conference conference, Talk talk, HttpContext httpContext)
        {
            this.talk = talk;
            Url = httpContext.GetTalkUrl(conference, talk);
			// TODO: Do what the tag helper does, images/asplogo.png?v=Kl_dqr9NVtnMdsM2MUg4qthUnWZm5T1fCEimBPWDNgM
            // Sha512, recalulate on save of new thumb...
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
