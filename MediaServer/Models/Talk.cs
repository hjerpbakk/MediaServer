using System;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using MediaServer.Extensions;

namespace MediaServer.Models
{
    public class Talk
    {
        public Talk()
        {
            // TODO: Get thumbnail from speaker notes or video
            Thumbnail = "http://placehold.it/700x400";
            DateOfTalk = DateTime.UtcNow;
        }

        public string VideoName { get; set; }
        public string TalkName { get; set; }
        public string Description { get; set; }

        // TODO: Set in service after get from BlobStore metadata
        [JsonIgnore]
        public DateTime TimeStamp { get; set; }

        public DateTime DateOfTalk { get; set; }

        [JsonIgnore]
        public string DateOfTalkString {
            get { return DateOfTalk.GetDateString(); }
            set { DateOfTalk = DateTimeExtensions.GetDateTime(value); }
        }

        // TODO: Get speaker from AD or something with auto-complete
        public string Speaker { get; set; }
        public string SpeakerDeck { get; set; }
        
		// TODO: Create VM and move around stuff
		[JsonIgnore]
		public IFormFile ThumbnailImageFile { get; set; }

		[JsonIgnore]
		public string Thumbnail { get; set; }
    }
}
