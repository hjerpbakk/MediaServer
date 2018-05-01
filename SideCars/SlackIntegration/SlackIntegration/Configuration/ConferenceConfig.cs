using System.Collections.Generic;
using SlackIntegration.Models;

namespace SlackIntegration.Configuration
{
	public class ConferenceConfig
    {
        public ConferenceConfig(IDictionary<string, Conference> conferences)
        {
            Conferences = conferences;
        }

        public IDictionary<string, Conference> Conferences { get; }
    }
}
