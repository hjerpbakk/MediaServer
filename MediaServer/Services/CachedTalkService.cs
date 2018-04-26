using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediaServer.Models;
using MediaServer.Services.Cache;
using Microsoft.Extensions.Caching.Memory;

namespace MediaServer.Services
{
    public class CachedTalkService : ITalkService
    {
        const string LatestTalksKey = "lastettalks";

        readonly TalkService talkService;
        readonly IMemoryCache memoryCache;
        
        public CachedTalkService(TalkService talkService, IMemoryCache memoryCache)
        {
            this.talkService = talkService;
            this.memoryCache = memoryCache;
        }

        public async Task DeleteTalkFromConference(Conference conference, Talk talk)
        {
            memoryCache.Remove(talk.TalkName);
            memoryCache.Remove(conference.Id);
            memoryCache.Remove(GetConferenceTalkKey(conference.Id));
            memoryCache.Remove(LatestTalksKey);

			// TODO: After deletion is supported in thumbnailservice, move this there
			memoryCache.Remove(Keys.GetThumbnailKey(talk.TalkName));

            await talkService.DeleteTalkFromConference(conference, talk);
        }

        public async Task<Talk> GetTalkByName(Conference conference, string name)
        {
            if (!memoryCache.TryGetValue(name, out Talk talk)) {
                talk = await talkService.GetTalkByName(conference, name);
                memoryCache.Set(name, talk, Keys.Options);
            }

            return talk;
        }

        public async Task<IReadOnlyList<string>> GetTalkNamesFromConference(Conference conference)
        {
            if (!memoryCache.TryGetValue(conference.Id, out IReadOnlyList<string> talks)) {
                talks = await talkService.GetTalkNamesFromConference(conference);
				memoryCache.Set(conference.Id, talks, Keys.Options);
            }

            return talks;
        }

		public async Task<IEnumerable<Talk>> GetTalksFromConference(Conference conference)
        {
            var key = GetConferenceTalkKey(conference.Id);
			if(!memoryCache.TryGetValue(key, out IEnumerable<Talk> talks)) {
                talks = await talkService.GetTalksFromConference(conference);
				memoryCache.Set(key, talks, Keys.Options);
            }

            return talks;
        }

        public async Task SaveTalkFromConference(Conference conference, Talk talk)
        {
            memoryCache.Remove(conference.Id);
            memoryCache.Remove(GetConferenceTalkKey(conference.Id));
            memoryCache.Remove(LatestTalksKey);
            
            await talkService.SaveTalkFromConference(conference, talk);
			memoryCache.Set(talk.TalkName, talk, Keys.Options);
        }

        public async Task<IEnumerable<LatestTalk>> GetLatestTalks(IEnumerable<Conference> conferences) {
            if (!memoryCache.TryGetValue(LatestTalksKey, out IEnumerable<LatestTalk> latestTalks)) {
                latestTalks = await talkService.GetLatestTalks(conferences);
				memoryCache.Set(LatestTalksKey, latestTalks, Keys.Options);
            }

            return latestTalks;
        }

        string GetConferenceTalkKey(string conferenceId) => conferenceId + "talk";
    }
}
