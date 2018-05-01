using System.Collections.Generic;
using MediaServer.Models;

namespace MediaServer.Configuration
{
    public class AppConfig : IBlogStorageConfig, ISlackConfig
    {
        public string BlobStorageConnectionString { get; set; }
        public string SlackToken { get; set; }
        public bool UseTestSlackChannel { get; set; }
    }
}
