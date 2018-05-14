using System;
using System.Linq;
using System.IO;
using MediaServer.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using MediaServer.Controllers;

namespace MediaServer.Services
{
	public class ContentService {
		readonly string hostingPath;
		readonly ConferenceService conferenceService;
        
		public ContentService(IHostingEnvironment hostingEnvironment, ConferenceService conferenceService) {
			hostingPath = hostingEnvironment.WebRootPath;
			this.conferenceService = conferenceService;
        }
        
        public async Task<Video[]> GetVideosFromConference(Conference conference) {
			var path = GetConferencePath(conference.Id);
            var directory = new DirectoryInfo(path);
            if (!directory.Exists) {
                Console.WriteLine($"Could not find directory for {conference.Id}. Server setup is wrong.");
                return new Video[0];
            }

			var talkNames = await conferenceService.GetTalkNamesFromConference(conference);
            var candidateFiles = directory.EnumerateFiles("*" + Video.SupportedVideoFileType);
            var availableVideos = candidateFiles
                .Where(f => !talkNames.Contains(f.Name))
                .Select(f => new Video(f.Name))
				.ToArray();

			return availableVideos;
        }

		public void VerifySlides(Talk talk) {
			if (talk.SpeakerDeck == null) {
				return;
			}

			if (talk.SpeakerDeck.StartsWith("http", StringComparison.Ordinal)) {
				return;        
			}

			var pathToSpeakerDeck = GetSpeakerDeckPath(talk.ConferenceId, talk.SpeakerDeck);
			if (File.Exists(pathToSpeakerDeck)) {
				return;            
			}

			talk.SpeakerDeck = null;
		}

		string GetConferencePath(string conferenceId) 
            => Path.Combine(hostingPath, NavigateableController.Conference, conferenceId);

        string GetSpeakerDeckPath(string conferenceId, string speakerDeckName)
            => Path.Combine(hostingPath, NavigateableController.Conference, conferenceId, speakerDeckName);       
    }
}
