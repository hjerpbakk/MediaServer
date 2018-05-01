using Newtonsoft.Json;

namespace SlackIntegration.Models
{
    public class Talk
    {
		[JsonConstructor]
		public Talk(string conferenceId, string talkName, string url, string description, string speaker)
        {
			ConferenceId = conferenceId;                
			TalkName = talkName;
			Url = url;
			Description = description;
			Speaker = speaker;         
        }

		public string ConferenceId { get; }
		public string TalkName { get; }
		public string Url { get; }
		public string Description { get; }
		public string Speaker { get; }      
    }
}
