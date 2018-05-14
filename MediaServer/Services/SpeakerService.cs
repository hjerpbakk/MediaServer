using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediaServer.Clients;
using MediaServer.Models;
using MediaServer.Models.Helpers;
using MediaServer.Services.Persistence;
using MediaServer.ViewModels;

namespace MediaServer.Services
{
	public class SpeakerService : ConferenceService {
		readonly string[] conferenceIds;
		readonly Users users;
		readonly ConferencePersistence conferencePersistence;
		readonly ISlackClient slackClient;

		public SpeakerService(string[] conferenceIds, Users users, ConferencePersistence conferencePersistence, ThumbnailService thumbnailService, ISlackClient slackClient)
			: base(conferenceIds, thumbnailService, conferencePersistence) {
			this.conferenceIds = conferenceIds;
			this.users = users;
			this.conferencePersistence = conferencePersistence;
			this.slackClient = slackClient;
        }
        
		public async Task<Speaker[]> GetSpeakers() {
            var talks = await conferencePersistence.GetTalksFromConferences(conferenceIds);
			var nameAndTalkCounts = new Dictionary<string, int>();
			foreach (var talk in talks) {
				var speakerNames = talk.Speaker;
				foreach (var name in SpeakerParser.GetSpeakers(speakerNames)) {
					if (nameAndTalkCounts.ContainsKey(name)) {
						nameAndTalkCounts[name] += 1; 
					} else {
						nameAndTalkCounts.Add(name, 1);
					}
				}
			}

			var speakers = new List<Speaker>(nameAndTalkCounts.Count);
			foreach (var nameAndTalkCount in nameAndTalkCounts) {
				var user = users.GetUser(nameAndTalkCount.Key);
				var speaker = new Speaker(nameAndTalkCount.Key, user.ProfileImageUrl, nameAndTalkCount.Value);
				speakers.Add(speaker);
			}

			var orderedSpeakers = speakers.OrderByDescending(s => s.TalkCount).ToArray();
			return orderedSpeakers;
		}

		public async Task<TalksByUser> GetTalksBySpeaker(string speakerName) {
            var talks = await conferencePersistence.GetTalksFromConferences(conferenceIds);
            var orderedTalks = talks.Where(t => t.Speaker.Contains(speakerName)).OrderByDescending(t => t.DateOfTalk);

			var orderedSummaries = (await CreateTalkSummaries(orderedTalks)).ToArray();
			var user = users.GetUser(speakerName);
			var slackDmLink = slackClient.GetDmLink(user.Name, user.SlackId);

			var talksByUser = new TalksByUser(user, orderedSummaries, slackDmLink);         
			return talksByUser;
        }
    }
}
