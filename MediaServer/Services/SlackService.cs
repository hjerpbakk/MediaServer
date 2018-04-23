using System;
using System.Threading.Tasks;
using MediaServer.Configuration;
using MediaServer.Models;
using SlackConnector;
using SlackConnector.Models;

namespace MediaServer.Services
{
	public class SlackService : ISlackService
    {
        const string TestSlackChannel = "G73TGULC9";

		readonly ISlackConnector connector;
		readonly ISlackConfig config;
        
		public SlackService(ISlackConnector connector, ISlackConfig config)
        {
			this.connector = connector;
			this.config = config;
        }

		public async Task PostTalkToChannel(Conference conference, Talk talk, string talkUrl)
		{
			var connection = await connector.Connect(config.SlackToken);
            if (connection == null)
			{
				// TODO: Try again etc
				return;
			}

            var channelId = config.UseTestSlackChannel ? TestSlackChannel : conference.SlackChannelId;
            var channel = new SlackChatHub { Id = channelId };
            var message = new BotMessage
            {
				// TODO: Ta med Thumbnail når vi har støtte for det
                ChatHub = channel,
                Attachments = new[] {
                    new SlackAttachment {
                        Title = talk.TalkName,
						TitleLink = talkUrl,
						Text = talk.Description,
						Fallback = talk.Description,                 
						ColorHex = "#36a64f",
						AuthorName = talk.Speaker
                    }
                }
            };

            await connection.Say(message);

            await connection.Close();
		}
    }
}
