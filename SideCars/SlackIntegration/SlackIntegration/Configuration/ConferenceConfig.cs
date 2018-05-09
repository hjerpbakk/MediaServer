using System.Collections.Generic;
using SlackIntegration.Models;

namespace SlackIntegration.Configuration
{
	public class ConferenceConfig
    {
        public ConferenceConfig(Dictionary<string, Conference> conferences)
        {
            Conferences = conferences;
        }

        public Dictionary<string, Conference> Conferences { get; }
    }
}
