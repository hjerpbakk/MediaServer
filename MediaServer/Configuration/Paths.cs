using MediaServer.Models;
using Microsoft.AspNetCore.Http;

namespace MediaServer.Configuration
{
	public static class Paths
    {
		// TODO: Use this everywhere
		// TODO: Make less brittle
		public static string GetConferenceUrl(string conferenceId)
		    => $"/Conference/{conferenceId}/";

		public static string GetThumbnailUrl(Talk talk)
		    => GetConferenceUrl(talk.ConferenceId) + "Thumbnails/" + talk.TalkName;

        public static string GetTalkUrl(Talk talk)
		    => GetConferenceUrl(talk.ConferenceId) + talk.TalkName;

		public static string GetFullPath(HttpContext httpContext, string urlPart)
		    => $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{urlPart}";
    }
}
