using System.Linq;
using System.Threading.Tasks;
using SlackConnector;
using SlackConnector.Models;
using SlackIntegration.Configuration;
using SlackIntegration.Models;

namespace SlackIntegration.Services {
	public class SlackService {
		const string TestSlackChannel = "G73TGULC9";

        readonly ISlackConnector connector;
        readonly ISlackConfig config;

        public SlackService(ISlackConnector connector, ISlackConfig config) {
            this.connector = connector;
            this.config = config;
        }

        public async Task PostTalkToChannel(Conference conference, Talk talk) {
            var connection = await connector.Connect(config.SlackToken);
            if (connection == null) {
                // TODO: Try again etc
                return;
            }
            
            var channelId = config.UseTestSlackChannel ? TestSlackChannel : conference.SlackChannelId;
            var channel = new SlackChatHub { Id = channelId };
            var message = new BotMessage {
                ChatHub = channel,
                Attachments = new[] {
                    new SlackAttachment {
                        Title = talk.TalkName,
						TitleLink = talk.Url,
                        Text = talk.Description,
                        Fallback = talk.Description,
                        AuthorName = talk.Speaker,
                        ColorHex = "#36a64f"
                    }
                }
            };

			// TODO: Try again etc
            await connection.Say(message);

            await connection.Close();
        }

		public async Task<User[]> ListUsers() {
			var connection = await connector.Connect(config.SlackToken);
            if (connection == null)
            {
                // TODO: Try again etc
				return new User[0];
            }

			// TODO: Bug in SlackIntegration -> SlackBot has no bot flag set
			var users = (await connection.GetUsers())
				.Where(u => !u.Deleted && !u.IsBot && !u.IsGuest)
				.Select(u => new User(u))
				.ToArray();

			await connection.Close();
			return users;
		}
    }
}
