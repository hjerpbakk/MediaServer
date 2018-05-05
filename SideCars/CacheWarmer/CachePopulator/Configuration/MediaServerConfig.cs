namespace CachePopulator.Configuration
{
	public class MediaServerConfig
    {
        public MediaServerConfig(string baseUrl, string conferenceUrl, string speakerUrl)
		{
			BaseUrl = baseUrl;
			ConferenceUrl = conferenceUrl;
			SpeakerUrl = speakerUrl;
		}

		public string BaseUrl { get; }
		public string ConferenceUrl { get; }
		public string SpeakerUrl { get; }
    }
}
