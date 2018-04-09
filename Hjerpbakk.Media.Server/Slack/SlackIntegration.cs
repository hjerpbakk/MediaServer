using System;
using System.Threading.Tasks;
using Hjerpbakk.Media.Server.Configuration;
using Hjerpbakk.Media.Server.Environment;
using Hjerpbakk.Media.Server.Model;
using Microsoft.AspNetCore.Http;
using SlackConnector;
using SlackConnector.Models;

namespace Hjerpbakk.Media.Server.Slack
{
    public class SlackIntegration
    {
        readonly ISlackConfiguration configuration;
        readonly ISlackConnector connector;
        readonly DebuggingService debuggingService;

        public SlackIntegration(ISlackConnector connector, ISlackConfiguration configuration, DebuggingService debuggingService)
        {
            this.connector = connector;
            this.configuration = configuration;
            this.debuggingService = debuggingService;
        }

        public async Task PostInterestingHourToChannel(Talk hourOfInterest) {
            // TODO: Try again etc
            var connection = await connector.Connect(configuration.SlackToken);
            if (connection == null)
            {
                throw new ArgumentException("Could not connect to Slack.");
            }

            var channelId = debuggingService.RunningInDebugMode ? configuration.TestChannelId : configuration.ProductionChannelId;
            var channel = new SlackChatHub { Id = channelId };
            var message = new BotMessage { 
                ChatHub = channel, 
                Attachments = new [] { 
                    new SlackAttachment {
                        Title = hourOfInterest.Title,
                        TitleLink = hourOfInterest.URL,
                        Text = hourOfInterest.Description,
                        Fallback = hourOfInterest.Description,
                        ColorHex = "#36a64f",
                        AuthorName = hourOfInterest.Author
                    }
                }
            };

            await connection.Say(message);

            await connection.Close();
        }
    }
}
