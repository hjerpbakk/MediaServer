namespace Hjerpbakk.Media.Server.Configuration
{
    public interface ISlackConfiguration
    {
        string SlackToken { get; }
        string TestChannelId { get; }
        string ProductionChannelId { get; }
    }
}
