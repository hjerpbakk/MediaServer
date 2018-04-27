using System;
using System.Threading.Tasks;
using MediaServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace MediaServer.Services.Cache
{
    public class TalkCache
    {
		const string LatestTalksKey = "lastettalks";

		readonly IMemoryCache memoryCache;      
        readonly MemoryCacheEntryOptions options;

		public TalkCache(IMemoryCache memoryCache)
        {
			this.memoryCache = memoryCache;
			options = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromDays(730))
                .SetSlidingExpiration(TimeSpan.FromDays(365));
        }
        
		// TODO: remove if not needed
		public ViewResult GetOrSetView(string key, Func<ViewResult> create) {
			var viewKey = GetViewKey(key);
			if (!memoryCache.TryGetValue(viewKey, out ViewResult view))
			{
				view = create();
				memoryCache.Set(viewKey, view, options);
			}

			return view;
		}

		public async Task<ViewResult> GetOrSetView(Func<Task<ViewResult>> create)
            => await GetOrSetView(LatestTalksKey, create);
        
		public async Task<ViewResult> GetOrSetView(string key, Func<Task<ViewResult>> create)
        {
            var viewKey = GetViewKey(key);
			if (!memoryCache.TryGetValue(viewKey, out ViewResult view))
            {
				view = await create();
                memoryCache.Set(viewKey, view, options);
            }

            return view;
        }

		public void ClearCachesForTalk(Talk talk) {
			memoryCache.Remove(LatestTalksKey);
			memoryCache.Remove(GetViewKey(talk.Speaker));
		}
  
		string GetViewKey(string key) => key + "view";
    }
}
