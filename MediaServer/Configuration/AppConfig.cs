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
                { 
                    "DevDays2018", 
                    new Conference("DevDays2018", 
                                    "Dev Days 2018", 
                                    "CA0G970P5", 
                                    @"\\p-fs01\DIPS\Linjeorganisasjon\Utviklingsavdelingen\Video utviklerforum, sprintdemo, møter og kurs\Utviklerforum\Dev days 2018\DevDays2018Video") }
				//{ "HoursOfInterest", new Conference("HoursOfInterest", "Hours of Interest", "G73TGULC9", "TODO") }
            };
        }

        public IDictionary<string, Conference> Conferences { get; }

        public string BlobStorageConnectionString { get; set; }
		public string SlackToken { get; set; }
        public bool UseTestSlackChannel { get; set; }
    }
}
