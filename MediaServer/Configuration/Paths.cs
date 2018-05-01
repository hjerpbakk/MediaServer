using MediaServer.Models;
using Microsoft.AspNetCore.Http;

namespace MediaServer.Configuration
{
	public static class Paths
    {
		// TODO: Use this everywhere
		// TODO: Make less brittle
		public static string GetConferenceUrl(Conference conference)
            => $"/Conference/{conference.Id}/";

        public static string GetThumbnailUrl(Conference conference, Talk talk)
            => GetConferenceUrl(conference) + "Thumbnails/" + talk.TalkName;

        public static string GetTalkUrl(Conference conference, Talk talk)
            => GetConferenceUrl(conference) + talk.TalkName;

		public static string GetFullPath(HttpContext httpContext, string urlPart)
		    => $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{urlPart}";
    }
}
