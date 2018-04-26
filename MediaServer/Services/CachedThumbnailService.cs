using System;
using System.Threading.Tasks;
using MediaServer.Models;
using MediaServer.Services.Cache;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace MediaServer.Services
{
	public class CachedThumbnailService : IThumbnailService
    {
		readonly ThumbnailService thumbnailService;
		readonly IMemoryCache memoryCache;
        
		public CachedThumbnailService(ThumbnailService thumbnailService, IMemoryCache memoryCache)
        {
			this.thumbnailService = thumbnailService;
			this.memoryCache = memoryCache;         
        }

		public async Task<Image> GetTalkThumbnail(Conference conference, string name)
		{
			var key = Keys.GetThumbnailKey(name);
            if (!memoryCache.TryGetValue(key, out Image image))
            {
				image = await thumbnailService.GetTalkThumbnail(conference, name);
                memoryCache.Set(key, image, Keys.Options);
            }

            return image;
		}

        public async Task SaveThumbnail(Conference conference, Talk talk)
		{
            memoryCache.Remove(Keys.GetThumbnailKey(talk.TalkName));
			memoryCache.Remove(Keys.GetThumnnailHashName(talk));
			await thumbnailService.SaveThumbnail(conference, talk);
		}

		public async Task<string> GetThumbnailUrl(Conference conference, Talk talk, HttpContext httpContext)
        {
			var key = Keys.GetThumnnailHashName(talk);
			if (!memoryCache.TryGetValue(key, out string url))
            {
				url = await thumbnailService.GetThumbnailUrl(conference, talk, httpContext);
				memoryCache.Set(key, url, Keys.Options);
            }

			return url;
        }      
	}
}
