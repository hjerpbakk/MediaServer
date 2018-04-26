using MediaServer.Models;
using Microsoft.AspNetCore.Http;

namespace MediaServer.Extensions {
    public static class HttpContextExtensions {
		// TODO: Create proper pathing abstraction...
        public static string GetConferenceUrl(this HttpContext httpContext, Conference conference) 
            => $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/Conference/{conference.Id}/";

        public static string GetThumbnailUrl(this HttpContext httpContext, Conference conference, Talk talk)
            => GetConferenceUrl(httpContext, conference) + "Thumbnails/" + talk.TalkName;

        public static string GetTalkUrl(this HttpContext httpContext, Conference conference, Talk talk)
            => GetConferenceUrl(httpContext, conference) + talk.TalkName;
    }
}
