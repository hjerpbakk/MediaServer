using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediaServer.Clients;
using MediaServer.Controllers;
using MediaServer.Models;
using MediaServer.Services.Persistence;
using MediaServer.ViewModels;

namespace MediaServer.Services
{
	public class ConferenceService {
		readonly string[] conferenceIds;
		readonly Dictionary<string, Conference> conferences;
		readonly ThumbnailService thumbnailService;
		readonly ConferencePersistence conferencePersistence;
		readonly ISlackClient slackClient;
        
		public ConferenceService(string[] conferenceIds, Dictionary<string, Conference> conferences, ThumbnailService thumbnailService, ConferencePersistence conferencePersistence, ISlackClient slackClient) {
			this.conferenceIds = conferenceIds;
			this.conferences = conferences;
			this.thumbnailService = thumbnailService;
			this.conferencePersistence = conferencePersistence;
			this.slackClient = slackClient;
        }

		public async Task<TalkSummary[]> GetLatestTalks() {
			var talks = await conferencePersistence.GetTalksFromConferences(conferenceIds);         
			var orderedTalks = talks.OrderByDescending(t => t.TimeStamp).Take(9);
			var orderedSummaries = await CreateTalkSummaries(orderedTalks);
			return orderedSummaries.ToArray();
		}
                                                      
		public async Task<ConferenceViewModel> GetConferenceWithContent(Conference conference) {
			var talks = await conferencePersistence.GetTalksFromConference(conference.Id);         
			var orderedTalks = talks.OrderByDescending(t => t.DateOfTalk);
			var orderedSummaries = (await CreateTalkSummaries(orderedTalks)).ToArray();
			var videoPath = conference.VideoPath;
            var slackUrl = slackClient.GetChannelLink(conference.Name, conference.SlackChannelId);
			var conferenceViewModel = new ConferenceViewModel(orderedSummaries, videoPath, slackUrl);
			return conferenceViewModel;
		}
              
		public async Task<IEnumerable<string>> GetTalkNamesFromConference(Conference conference) {
			var talks = await conferencePersistence.GetTalksFromConference(conference.Id);
			return talks.Select(talk => talk.VideoName);
        }
        
		protected async Task<IEnumerable<TalkSummary>> CreateTalkSummaries(IEnumerable<Talk> talks) {
			return await Task.WhenAll(talks.Select(CreateTalkSummary));

			async Task<TalkSummary> CreateTalkSummary(Talk talk) {
				var url = NavigateableController.GetTalkUrl(talk);
                var thumbnail = await thumbnailService.GetThumbnailUrl(talk);
				var conferenceName = conferences[talk.ConferenceId].Name;
				return new TalkSummary(talk, url, thumbnail, conferenceName);
            }
		}      
	}
}
