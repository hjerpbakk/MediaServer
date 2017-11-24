using System;
using Newtonsoft.Json;

namespace Hjerpbakk.Media.Server.Configuration
{
    public class AppConfiguration : ISlackConfiguration, IBlobStorageConfiguration
    {
        public string BlobStorageConnectionString { get; set; }
        public string SlackToken { get; set; }
        public string TestChannelId { get; set; }
        public string ProductionChannelId { get; set; }
    }
}
