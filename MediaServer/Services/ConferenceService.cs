using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediaServer.Configuration;
using MediaServer.Models;
using MediaServer.Services.Persistence;

namespace MediaServer.Services
{
	public class ConferenceService {
		readonly string[] conferenceIds;   
		readonly ThumbnailService thumbnailService;
		readonly ConferencePersistence conferencePersistence;
        
		public ConferenceService(string[] conferenceIds, ThumbnailService thumbnailService, ConferencePersistence conferencePersistence) {
			this.conferenceIds = conferenceIds;
			this.thumbnailService = thumbnailService;
			this.conferencePersistence = conferencePersistence;
        }

		public async Task<IEnumerable<TalkSummary>> GetLatestTalks() {
			var talks = await conferencePersistence.GetTalksFromConferences(conferenceIds);         
			var orderedTalks = talks.OrderByDescending(t => t.TimeStamp).Take(9).ToArray();
			var orderedSummaries = await CreateTalkSummaries(orderedTalks);
			return orderedSummaries;
		}
                                                      
		public async Task<IEnumerable<TalkSummary>> GetTalksForConference(Conference conference) {
			var talks = await conferencePersistence.GetTalksFromConference(conference.Id);         
			var orderedTalks = talks.OrderByDescending(t => t.DateOfTalk);
			var orderedSummaries = await CreateTalkSummaries(orderedTalks);
			return orderedSummaries;
		}
              
		public async Task<IEnumerable<string>> GetTalkNamesFromConference(Conference conference) {
			var talks = await conferencePersistence.GetTalksFromConference(conference.Id);
			return talks.Select(talk => talk.VideoName);
        }
        
		protected async Task<IEnumerable<TalkSummary>> CreateTalkSummaries(IEnumerable<Talk> talks) {
			return await Task.WhenAll(talks.Select(CreateTalkSummary));

			async Task<TalkSummary> CreateTalkSummary(Talk talk) {
                var url = Paths.GetTalkUrl(talk);
                var thumbnail = await thumbnailService.GetThumbnailUrl(talk);
                return new TalkSummary(talk, url, thumbnail);
            }
		}      
	}
}
