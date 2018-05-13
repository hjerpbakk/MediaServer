using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediaServer.Models;
using MediaServer.Models.Helpers;
using MediaServer.Services.Persistence;

namespace MediaServer.Services
{
	public class SpeakerService {
		readonly string[] conferenceIds;
		readonly Users users;
		readonly ConferencePersistence conferencePersistence;

		public SpeakerService(string[] conferenceIds, Users users, ConferencePersistence conferencePersistence) {
			this.conferenceIds = conferenceIds;
			this.users = users;
			this.conferencePersistence = conferencePersistence;
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
    }
}
