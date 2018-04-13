using System.Collections.Generic;
using MediaServer.Models;

namespace MediaServer.Configuration
{
    public interface IConferenceConfig
    {
        IDictionary<string, Conference> Conferences { get; }
    }
}
