using System;
using System.Linq;
using System.IO;
using MediaServer.Models;
using Microsoft.AspNetCore.Hosting;
using System.Threading.Tasks;

namespace MediaServer.Services {
    public class ContentService {
        readonly string hostingPath;
		readonly ConferenceService conferenceService;

		public ContentService(IHostingEnvironment hostingEnvironment, ConferenceService conferenceService) {
            hostingPath = hostingEnvironment.WebRootPath;
			this.conferenceService = conferenceService;
        }
        
        public async Task<Video[]> GetVideosFromConference(Conference conference) {
			var path = Path.Combine(hostingPath, "Conference", conference.Id);
            var directory = new DirectoryInfo(path);
            if (!directory.Exists) {
                Console.WriteLine($"Could not find directory for {conference.Id}. Server setup is wrong.");
                return new Video[0];
            }

			var talkNames = await conferenceService.GetTalkNamesFromConference(conference);
            var candidateFiles = directory.EnumerateFiles($"*{Video.SupportedVideoFileType}");
            var availableVideos = candidateFiles
                .Where(f => !talkNames.Contains(f.Name))
                .Select(f => new Video(f.Name));

            return availableVideos.ToArray();
        }

		public void VerifySlides(Talk talk) {
			if (talk.SpeakerDeck.StartsWith("http", StringComparison.Ordinal)) {
				return;        
			}

			var pathToSpeakerDeck = Path.Combine(hostingPath, "Conference", talk.ConferenceId, talk.SpeakerDeck);
			if (File.Exists(pathToSpeakerDeck)) {
				return;            
			}

			talk.SpeakerDeck = null;
		}
    }
}
