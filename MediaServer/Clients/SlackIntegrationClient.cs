using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MediaServer.Models;
using Newtonsoft.Json;

namespace MediaServer.Clients {
	public class SlackIntegrationClient {
        readonly HttpClient httpClient;

		SlackLink slackLink;

		// TODO: create two different interfaces. Getusers and populat metadata only needed at startup
		public SlackIntegrationClient(HttpClient httpClient) {
            this.httpClient = httpClient;
        }

		public void PublishToSlack(Talk talk, string talkUrl) {
            try {
				var talkMetadata = new TalkMetadata(talk.ConferenceId, 
				                                    talk.TalkName, 
				                                    talkUrl, 
				                                    talk.Description, 
				                                    talk.Speaker);
                var metadataAsJson = JsonConvert.SerializeObject(talkMetadata);
                var content = new StringContent(metadataAsJson, Encoding.UTF8, "application/json");
				httpClient.PostAsync("http://slack-integration:1338/Slack", content);
            }
            catch (Exception ex) {
				Console.WriteLine($"Slack Integration failed for {talk} {ex}");
            }
        }

		public async Task<User[]> GetUsers() {
			try
            {
                var response = await httpClient.GetStringAsync("http://slack-integration:1338/Slack");
				var users = JsonConvert.DeserializeObject<User[]>(response);
				return users;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not get users from Slack {ex}");
				return new User[0];
            }
		}

		public async Task PopulateMetaData() {
			try
            {
                var response = await httpClient.GetStringAsync("http://slack-integration:1338/Slack/Links");
				slackLink = JsonConvert.DeserializeObject<SlackLink>(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not get links from Slack {ex}");
            }
		}

		public string GetChannelLink(string conferenceName, string slackId) 
		    => slackLink == null || slackId == null ? $"./{conferenceName}" : slackLink.ChannelLink + slackId;

		public string GetDmLink(string speakerName, string slackId) 
		    => slackLink == null || slackId == null ? $"./{speakerName}" : slackLink.DmLink + slackId;
    }
}
