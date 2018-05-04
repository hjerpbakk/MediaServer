using MediaServer.Models;

namespace MediaServer.Clients {
    public interface ISlackClient {
        void PublishToSlack(Talk talk, string talkUrl);
        string GetChannelLink(string conferenceName, string slackId);
        string GetDmLink(string speakerName, string slackId);
    }
}