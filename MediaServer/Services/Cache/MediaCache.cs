using System;
using System.Threading.Tasks;
using MediaServer.Models;
using MediaServer.Services.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace MediaServer.Services.Cache
{
    public class MediaCache
    {
		public const string LatestTalksKey = "lastettalks";

		readonly IMemoryCache memoryCache;      
        
		// TODO: Set private when no longer used
		public readonly MemoryCacheEntryOptions options;

        public MediaCache(IMemoryCache memoryCache)
        {
			this.memoryCache = memoryCache;
			options = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromDays(730))
                .SetSlidingExpiration(TimeSpan.FromDays(365));
        }
                
		public async Task<T> GetOrSet<T>(string key, Func<Task<T>> create)
        {
			if (!memoryCache.TryGetValue(key, out T view))
            {
				view = await create();
				memoryCache.Set(key, view, options);
            }

            return view;
        }

		public void ClearForTalk(Talk talk) {
			memoryCache.Remove(LatestTalksKey);
			memoryCache.Remove(talk.Speaker);
			memoryCache.Remove(talk.ConferenceId);
			memoryCache.Remove(GetTalkKey(talk.ConferenceId, talk.TalkName));         
		}

		public void ClearForThumbnail(Talk talk) {
			memoryCache.Remove(BlobStoragePersistence.GetThumbnailKey(talk.TalkName));
			memoryCache.Remove(BlobStoragePersistence.GetThumnnailHashName(talk.TalkName));
		}

		public static string GetTalkKey(string conferenceId, string talkName)
		    => conferenceId = talkName;
    }
}
