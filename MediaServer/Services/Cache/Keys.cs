using System;
using Microsoft.Extensions.Caching.Memory;

namespace MediaServer.Services.Cache
{
    public static class Keys
    {
		static Keys() {
			Options = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromDays(730))
                .SetSlidingExpiration(TimeSpan.FromDays(365));
		}

		public static MemoryCacheEntryOptions Options { get; }

		public static string GetThumbnailKey(string talkName) 
		    => talkName + "thumb";
    }
}
