using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediaServer.Models;
using MediaServer.Services.Cache;
using Microsoft.Extensions.Caching.Memory;

namespace MediaServer.Services
{
    public class OldCachedTalkService : IOldTalkService
    {
        readonly OldTalkService talkService;
        readonly IMemoryCache memoryCache;
		readonly TalkCache talkCache;
        
		public OldCachedTalkService(OldTalkService talkService, IMemoryCache memoryCache, TalkCache talkCache)
        {
            this.talkService = talkService;
            this.memoryCache = memoryCache;
			this.talkCache = talkCache;
        }

        public async Task DeleteTalkFromConference(Conference conference, Talk talk)
        {
            talkCache.ClearCachesForTalk(talk);      

			// TODO: After deletion is supported in thumbnailservice, move this there
			memoryCache.Remove(Keys.GetThumbnailKey(talk.TalkName));

            await talkService.DeleteTalkFromConference(conference, talk);
        }

        public async Task<Talk> GetTalkByName(Conference conference, string name)
        {
			// TODO: This should be cached in the new service too...
			return await talkCache.GetOrSetTalk(
				TalkCache.GetTalkKey(conference.Id, name),
				() => talkService.GetTalkByName(conference, name));
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
            return await talkService.GetTalksFromConference(conference);
        }

        public async Task SaveTalkFromConference(Conference conference, Talk talk)
        {
            talkCache.ClearCachesForTalk(talk);      
            
            await talkService.SaveTalkFromConference(conference, talk);
			memoryCache.Set(talk.TalkName, talk, Keys.Options);
        }

        public async Task<IEnumerable<LatestTalk>> GetLatestTalks(IEnumerable<Conference> conferences) {
			return await talkService.GetLatestTalks(conferences);
        }
    }
}
