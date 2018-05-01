namespace SlackIntegration.Configuration
{
	public interface ISlackConfig
    {
        string SlackToken { get; }
        bool UseTestSlackChannel { get; }
    }
}
