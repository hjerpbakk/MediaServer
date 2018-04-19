namespace MediaServer.Models
{
    public class Conference
    {
        public Conference(string id, string name, string slackChannelId, string videoPath)
        {            
            Id = id;
            Name = name;
			SlackChannelId = slackChannelId;
            VideoPath = videoPath;
        }

        public string Id { get; }
        public string Name { get; }
		public string SlackChannelId { get; }
        public string VideoPath { get; }
    }
}
