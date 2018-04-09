namespace Hjerpbakk.Media.Server.Configuration
{
    public interface ISlackConfiguration
    {
        // TODO: Add slack channel pr conference
        string SlackToken { get; }
        string TestChannelId { get; }
        string ProductionChannelId { get; }
    }
}
