using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediaServer.Configuration;
using MediaServer.Models;
using MediaServer.Services.Persistence;

namespace MediaServer.Services
{
	public class ConferenceService {
		readonly IEnumerable<Conference> conferences;      
		readonly ThumbnailService thumbnailService;
		readonly BlobStoragePersistence blobStoragePersistence;
        
		public ConferenceService(ConferenceConfig conferenceConfig, ThumbnailService thumbnailService, BlobStoragePersistence blobStoragePersistence) {
			conferences = conferenceConfig.Conferences.Values;
			this.thumbnailService = thumbnailService;
			this.blobStoragePersistence = blobStoragePersistence;
        }

		public async Task<IEnumerable<TalkSummary>> GetLatestTalks() {
			var talks = await blobStoragePersistence.GetTalksFromConferences(conferences);         
			var orderedTalks = talks.OrderByDescending(t => t.TimeStamp).Take(9);
			var orderedSummaries = await CreateTalkSummaries(orderedTalks);
			return orderedSummaries;
		}
                                                      
		public async Task<IEnumerable<TalkSummary>> GetTalksForConference(Conference conference) {
			var talks = await blobStoragePersistence.GetTalksFromConference(conference);         
			var orderedTalks = talks.OrderByDescending(t => t.DateOfTalk);
			var orderedSummaries = await CreateTalkSummaries(orderedTalks);
			return orderedSummaries;
		}
              
		public async Task<IEnumerable<TalkSummary>> GetTalksBySpeaker(string speakerName) {
			var talks = await blobStoragePersistence.GetTalksFromConferences(conferences);
			var orderedTalks = talks.Where(t => t.Speaker == speakerName).OrderByDescending(t => t.DateOfTalk);
			var orderedSummaries = await CreateTalkSummaries(orderedTalks);         
			return orderedSummaries;
		}

		public async Task<IEnumerable<string>> GetTalkNamesFromConference(Conference conference) {
            var talks = await blobStoragePersistence.GetTalksFromConference(conference);
            return talks.Select(talk => talk.TalkName);
        }

		async Task<IEnumerable<TalkSummary>> CreateTalkSummaries(IEnumerable<Talk> talks) {
			return await Task.WhenAll(talks.Select(CreateTalkSummary));

			async Task<TalkSummary> CreateTalkSummary(Talk talk) {
                var url = Paths.GetTalkUrl(talk);
                var thumbnail = await thumbnailService.GetThumbnailUrl(talk);
                return new TalkSummary(talk, url, thumbnail);
            }
		}      
	}
}
