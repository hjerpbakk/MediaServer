using System;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace MediaServer.Models
{
    public class Talk
    {
        string name;

        public Talk()
        {
            // TODO: Get thumbnail from speaker notes or video
            Thumbnail = "http://placehold.it/700x400";
        }

        public string UriEncodedName { get; private set; }

        public string Name { 
            get { return name; } 
            set { 
				name = value;
				UriEncodedName = Uri.EscapeUriString(name);
            } 
        }

        public string Description { get; set; }
        public DateTime TimeStamp { get; set; }
        // TODO: Get speaker from AD or something with auto-complete
        public string Speaker { get; set; }
        public string SpeakerDeck { get; set; }
        
		// TODO: Create VM and move around stuff
		[JsonIgnore]
		public IFormFile ThumbnailImageFile { get; set; }

		[JsonIgnore]
		public string ThumbnailName { get; set; }

		[JsonIgnore]
		public string Thumbnail { get; set; }
    }
}
