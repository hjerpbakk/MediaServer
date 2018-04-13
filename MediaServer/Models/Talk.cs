using System;
using Newtonsoft.Json;

namespace MediaServer.Models
{
    public class Talk
    {
        readonly static char[] supportedVideoFileType;

        string name;


		static Talk() {
			supportedVideoFileType = Video.SupportedVideoFileType.ToCharArray();
		}

        public Talk()
        {
            // TODO: Get thumbnail from speaker notes or video
            Thumbnail = "http://placehold.it/700x400";
        }

        [JsonConstructor]
        public Talk(string name, string description, string thumbnail, DateTime timeStamp, string speaker, string speakerDeck = null)
        {
            Name = name;
            Description = description;
            Thumbnail = thumbnail;
            TimeStamp = timeStamp;
            Speaker = speaker;
            SpeakerDeck = speakerDeck;

            // TODO: If no thumbnail: http://placehold.it/700x393
        }

        public string UriEncodedName { get; private set; }

        public string Name { 
            get { return name; } 
            set { 
				name = value.TrimEnd(supportedVideoFileType);
				UriEncodedName = Uri.EscapeUriString(name);
            } 
        }

        public string Description { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Speaker { get; set; }
        public string SpeakerDeck { get; set; }

        public string Thumbnail { get; }
    }
}
