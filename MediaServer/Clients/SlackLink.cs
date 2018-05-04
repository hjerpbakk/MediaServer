using Newtonsoft.Json;

namespace MediaServer.Clients
{
	public class SlackLink
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
