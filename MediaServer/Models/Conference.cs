namespace MediaServer.Models
{
    public class Conference
    {
        public Conference(string id, string name, string slackChannelId)
        {            
            Id = id;
            Name = name;
			SlackChannelId = slackChannelId;
        }

        public string Id { get; }
        public string Name { get; }
		public string SlackChannelId { get; }
    }
}
