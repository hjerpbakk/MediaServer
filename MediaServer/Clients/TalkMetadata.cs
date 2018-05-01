using Newtonsoft.Json;

namespace MediaServer.Clients
{
	public class TalkMetadata
    {
        [JsonConstructor]
		public TalkMetadata(string conferenceId, string talkName, string url, string description, string speaker, string thumbnailUrl)
        {
            ConferenceId = conferenceId;
            TalkName = talkName;
            Url = url;
            Description = description;
            Speaker = speaker;
            ThumbnailUrl = thumbnailUrl;
        }

        public string ConferenceId { get; }
        public string TalkName { get; }
        public string Url { get; }
        public string Description { get; }
        public string Speaker { get; }
        public string ThumbnailUrl { get; }
    }
}
