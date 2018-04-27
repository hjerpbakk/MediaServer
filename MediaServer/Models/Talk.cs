using System;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using MediaServer.Extensions;

namespace MediaServer.Models
{
    public class Talk
    {
        public Talk() {
            DateOfTalk = DateTime.UtcNow;
        }

		// TODO: Legg til sammendragsfelt i markdown, ref https://dipsasa.slack.com/archives/D1TCWMBRT/p1524836698000185

		[JsonIgnore]
		public string ConferenceId { get; set; }
        public string VideoName { get; set; }
        public string TalkName { get; set; }
        public string Description { get; set; }

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
