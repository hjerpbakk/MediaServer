using MediaServer.Models;
using MediaServer.Models.Helpers;

namespace MediaServer.ViewModels {
    public abstract class TalkViewModelBase {
		protected TalkViewModelBase(Talk talk) {
			TalkName = talk.TalkName;
			Description = talk.Description;
			ZonedTimeStamp = talk.DateOfTalkString;
			Speakers = SpeakerParser.GetSpeakers(talk.Speaker);
        }

		public string TalkName { get; }
		public string Description { get; } 
		public string ZonedTimeStamp { get; }
		public string[] Speakers { get; }
    }
}
