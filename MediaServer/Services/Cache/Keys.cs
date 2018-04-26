using System;
using MediaServer.Models;
using Microsoft.Extensions.Caching.Memory;

namespace MediaServer.Services.Cache
{
    public static class Keys
    {
		public const string TalkPrefix = "dips.talk.";

		const string HashExtension = ".txt";

		// TODO: Move all memory caches to cache results of controller actions, not results of business services
		static Keys() {
			Options = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromDays(730))
                .SetSlidingExpiration(TimeSpan.FromDays(365));
		}

		public static MemoryCacheEntryOptions Options { get; }

		public static string GetThumbnailKey(string talkName) => talkName + "thumb";
		public static string GetThumnnailHashName(Talk talk) => talk.TalkName + HashExtension;
		public static string GetSpeakerKey(string speakerName) => speakerName + "speaker";
    }
}
