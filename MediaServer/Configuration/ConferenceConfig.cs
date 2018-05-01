using System.Collections.Generic;
using MediaServer.Models;

namespace MediaServer.Configuration
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
