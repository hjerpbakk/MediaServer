using System;
using System.Threading.Tasks;
using MediaServer.Clients;
using MediaServer.Models;
using MediaServer.Services.Persistence;
using Microsoft.Extensions.Caching.Memory;

namespace MediaServer.Services.Cache
{
	public class MediaCache
    {
		public const string LatestTalksKey = "latesttalks";

		readonly IMemoryCache memoryCache;      

		readonly MemoryCacheEntryOptions options;
		readonly CacheWarmerClient cacheWarmerClient;

		public MediaCache(IMemoryCache memoryCache, CacheWarmerClient cacheWarmerClient)
        {
			this.memoryCache = memoryCache;
			this.cacheWarmerClient = cacheWarmerClient;
			options = new MemoryCacheEntryOptions()
                .SetPriority(CacheItemPriority.NeverRemove);
        }
                
		public async Task<T> GetOrSet<T>(string key, Func<Task<T>> create)
        {
			if (!memoryCache.TryGetValue(key, out T view))
            {
				Console.WriteLine($"Cache miss for {key}");
				view = await create();
				memoryCache.Set(key, view, options);
            }

            return view;
        }

		public void CacheTalk(Talk talk) {
			var key = ClearCache(talk);
			memoryCache.Set(key, talk, options);
			cacheWarmerClient.RePopulateCaches(talk);
		}

		public string ClearCache(Talk talk) {
			var talkKey = GetTalkKey(talk.ConferenceId, talk.TalkName);
            memoryCache.Remove(LatestTalksKey);
            memoryCache.Remove(talk.Speaker);
            memoryCache.Remove(talk.ConferenceId);
			memoryCache.Remove(GetConferenceTalksKey(talk.ConferenceId));
			memoryCache.Remove(GetTalkViewKey(talk.ConferenceId, talk.TalkName));
			memoryCache.Remove(TalkPersistence.GetThumbnailKey(talk.TalkName));
            memoryCache.Remove(TalkPersistence.GetThumnnailHashName(talk.TalkName));
            memoryCache.Remove(talkKey);
			return talkKey;
		}

		public string GetTalkKey(string conferenceId, string talkName)
		    => talkName + conferenceId;

		public string GetTalkViewKey(string conferenceId, string talkName)
            => "view" + talkName + conferenceId;

		public string GetConferenceTalksKey(string conferenceId)
		    => "conf" + conferenceId;
    }
}
