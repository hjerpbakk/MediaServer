using System;
namespace MediaServer.Configuration
{
    public interface ISlackConfig
    {
		string SlackToken { get; }
    }
}
