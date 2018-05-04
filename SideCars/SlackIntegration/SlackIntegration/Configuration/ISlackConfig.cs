namespace SlackIntegration.Configuration
{
	public interface ISlackConfig
    {
        string SlackToken { get; }
		string SlackTeamId { get; }
		bool UseTestSlackChannel { get; }     
    }
}
