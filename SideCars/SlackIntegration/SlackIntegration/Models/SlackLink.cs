using Newtonsoft.Json;

namespace SlackIntegration.Models
{
	public struct SlackLink
    {
        [JsonConstructor]
        public SlackLink(string channelLink, string dmLink)
        {
            ChannelLink = channelLink;
            DmLink = dmLink;
        }

        public string ChannelLink { get; }
        public string DmLink { get; }
    }
}
