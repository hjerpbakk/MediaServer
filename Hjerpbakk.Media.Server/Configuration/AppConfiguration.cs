using System;
using System.Linq;
using Hjerpbakk.Media.Server.Model;
using Newtonsoft.Json;

namespace Hjerpbakk.Media.Server.Configuration
{
    public class AppConfiguration : ISlackConfiguration, IBlobStorageConfiguration, IConferenceConfiguration
    {
        // TODO: Rewrite to proper config and support environment variables
        public string BlobStorageConnectionString { get; set; }
        public string SlackToken { get; set; }
        public string TestChannelId { get; set; }
        public string ProductionChannelId { get; set; }
        public Conference[] Conferences { get; set; }

        // TODO: Smoothify
        public Conference GetConference(string conferenceName) 
            => Conferences.Single(c => c.Name == conferenceName);
    }
}
