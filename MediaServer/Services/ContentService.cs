using System;
using System.Linq;
using System.IO;
using MediaServer.Models;
using System.Threading.Tasks;
using MediaServer.Configuration;

namespace MediaServer.Services
{
	public class ContentService {
		readonly ConferenceService conferenceService;
		readonly Paths paths;

		public ContentService(Paths paths, ConferenceService conferenceService) {
			this.paths = paths;
			this.conferenceService = conferenceService;
        }
        
        public async Task<Video[]> GetVideosFromConference(Conference conference) {
			var path = paths.GetConferencePath(conference.Id);
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
			if (talk.SpeakerDeck == null) {
				return;
			}

			if (talk.SpeakerDeck.StartsWith("http", StringComparison.Ordinal)) {
				return;        
			}

			var pathToSpeakerDeck = paths.GetSpeakerDeckPath(talk.ConferenceId, talk.SpeakerDeck);
			if (File.Exists(pathToSpeakerDeck)) {
				return;            
			}

			talk.SpeakerDeck = null;
		}
    }
}
