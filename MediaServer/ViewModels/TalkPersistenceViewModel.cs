using MediaServer.Models;

namespace MediaServer.ViewModels {
	public abstract class TalkPersistenceViewModel {
		protected TalkPersistenceViewModel(Talk talk, bool isSave) {
			TalkName = talk.TalkName;
            Thumbnail = talk.Thumbnail;
			DateOfTalkString = talk.DateOfTalkString;
			Description = talk.Description;
			Speaker = talk.Speaker;
			SpeakerDeck = talk.SpeakerDeck;
			VideoName = talk.VideoName;
			IsSave = isSave;
            if (isSave) {
                return;
            }

			OldName = TalkName;
		}

		public string TalkName { get; }
		public string OldName { get; }
		public string Thumbnail { get; }
		public string DateOfTalkString { get; }
		public string Description { get; }
		public string Speaker { get; }
		public string SpeakerDeck { get; }
		public string VideoName { get; }
		public bool IsSave { get; }
	}
}

