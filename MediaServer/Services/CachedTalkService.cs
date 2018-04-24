using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediaServer.Models;
using Microsoft.Extensions.Caching.Memory;

namespace MediaServer.Services
{
    public class CachedTalkService : ITalkService
    {
        const string LatestTalksKey = "lastettalks";

        readonly TalkService talkService;
        readonly IMemoryCache memoryCache;

        readonly MemoryCacheEntryOptions cacheEntryOptions;

        public CachedTalkService(TalkService talkService, IMemoryCache memoryCache)
        {
            this.talkService = talkService;
            this.memoryCache = memoryCache;

            cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromDays(730))
                .SetSlidingExpiration(TimeSpan.FromDays(365));
        }

        public async Task DeleteTalkFromConference(Conference conference, Talk talk)
        {
            memoryCache.Remove(talk.TalkName);
            memoryCache.Remove(conference.Id);
            memoryCache.Remove(GetConferenceTalkKey(conference.Id));
            memoryCache.Remove(LatestTalksKey);
            await talkService.DeleteTalkFromConference(conference, talk);
        }

        public async Task<Talk> GetTalkByName(Conference conference, string name)
        {
            if (!memoryCache.TryGetValue(name, out Talk talk)) {
                talk = await talkService.GetTalkByName(conference, name);
                memoryCache.Set(name, talk, cacheEntryOptions);
            }

            return talk;
        }

        public async Task<IReadOnlyList<string>> GetTalkNamesFromConference(Conference conference)
        {
            if (!memoryCache.TryGetValue(conference.Id, out IReadOnlyList<string> talks)) {
                talks = await talkService.GetTalkNamesFromConference(conference);
                memoryCache.Set(conference.Id, talks, cacheEntryOptions);
            }

            return talks;
        }

        public async Task<IReadOnlyList<Talk>> GetTalksFromConference(Conference conference)
        {
            var key = GetConferenceTalkKey(conference.Id);
            if(!memoryCache.TryGetValue(key, out IReadOnlyList<Talk> talks)) {
                talks = await talkService.GetTalksFromConference(conference);
                memoryCache.Set(key, talks, cacheEntryOptions);
            }

            return talks;
        }

        public async Task<Image> GetTalkThumbnail(Conference conference, string name)
        {
            var key = GetThumbnailKey(name);
            if (!memoryCache.TryGetValue(key, out Image image)) {
                image = await talkService.GetTalkThumbnail(conference, name);
                memoryCache.Set(key, image, cacheEntryOptions);
            }

            return image;
        }

        public async Task SaveTalkFromConference(Conference conference, Talk talk)
        {
            memoryCache.Remove(conference.Id);
            memoryCache.Remove(GetConferenceTalkKey(conference.Id));
            memoryCache.Remove(LatestTalksKey);
            await talkService.SaveTalkFromConference(conference, talk);
            memoryCache.Set(talk.TalkName, talk, cacheEntryOptions);
        }

        public async Task<IEnumerable<LatestTalk>> GetLatestTalks(IEnumerable<Conference> conferences) {
            if (!memoryCache.TryGetValue(LatestTalksKey, out IEnumerable<LatestTalk> latestTalks)) {
                latestTalks = await talkService.GetLatestTalks(conferences);
                memoryCache.Set(LatestTalksKey, latestTalks, cacheEntryOptions);
            }

            return latestTalks;
        }

        string GetConferenceTalkKey(string conferenceId) => conferenceId + "talk";
        string GetThumbnailKey(string talkName) => talkName + "thumb";
    }
}
