using System.IO;
using MediaServer.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace MediaServer.Configuration {
	// TODO: Put file paths and urls in different files
	public class Paths {
		const string Conference = "Conference";

		readonly string hostingPath;

		public Paths(IHostingEnvironment hostingEnvironment) {
			hostingPath = hostingEnvironment.WebRootPath;
		}

		public string GetConferencePath(string conferenceId) 
		    => Path.Combine(hostingPath, Conference, conferenceId);

		public string GetSpeakerDeckPath(string conferenceId, string speakerDeckName)
		    => Path.Combine(hostingPath, Conference, conferenceId, speakerDeckName);

		// TODO: Code special words like Conference only once
		// TODO: Use this everywhere
		// TODO: Make less brittle
		// TODO: Remember that we have Navigation
		public static string GetConferenceUrl(string conferenceId)
		    => "/" + Conference + "/" + conferenceId + "/";

		public static string GetThumbnailUrl(Talk talk)
		    => GetConferenceUrl(talk.ConferenceId) + "Thumbnails/" + talk.TalkName;

        public static string GetTalkUrl(Talk talk)
		    => GetConferenceUrl(talk.ConferenceId) + talk.TalkName;

		public static string GetFullPath(HttpContext httpContext, string urlPart)
		    => httpContext.Request.Scheme + "://" + httpContext.Request.Host + urlPart;
    }
}
