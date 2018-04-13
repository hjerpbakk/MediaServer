using System.Collections.Generic;
using MediaServer.Models;

namespace MediaServer.Configuration
{
	public class AppConfig : IConferenceConfig, IBlogStorageConfig, ISlackConfig
    {
        public AppConfig()
        {
            // TODO: Move from here?
            Conferences = new Dictionary<string, Conference> {
				{ "DevDays2018", new Conference("DevDays2018", "Dev Days 2018", "G73TGULC9") },
				{ "HoursOfInterest", new Conference("HoursOfInterest", "Hours of Interest", "G73TGULC9") }
            };
        }

        public IDictionary<string, Conference> Conferences { get; }

        public string BlobStorageConnectionString { get; set; }
		public string SlackToken { get; set; }
    }
}
