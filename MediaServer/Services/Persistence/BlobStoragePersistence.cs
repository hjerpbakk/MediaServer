using System;
using MediaServer.Models;

namespace MediaServer.Services.Persistence
{
    public class BlobStoragePersistence
    {
		public const string TalkPrefix = "dips.talk.";

		const string HashExtension = ".txt";

        public BlobStoragePersistence()
        {
        }

		public static string GetThumbnailKey(string talkName) => "thumb" + talkName;
		public static string GetThumnnailHashName(string talkName) => talkName + HashExtension;      
    }
}
