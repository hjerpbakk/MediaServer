using System.Collections.Generic;
using MediaServer.Models;

namespace MediaServer.Configuration
{
    public class AppConfig : IConferenceConfig, IBlogStorageConfig, ISlackConfig
    {
        public IDictionary<string, Conference> Conferences { get; set; }
        public string BlobStorageConnectionString { get; set; }
        public string SlackToken { get; set; }
        public bool UseTestSlackChannel { get; set; }
    }
}
