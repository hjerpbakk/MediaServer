using Microsoft.AspNetCore.Mvc.Rendering;

namespace MediaServer.ViewModels {
    public class TalkMetadata {
		public TalkMetadata(TalkPersistenceViewModel talkPersistenceViewModel, SelectList availableVideos) {
			TalkPersistenceViewModel = talkPersistenceViewModel;
			AvailableVideos = availableVideos;
		}

		public TalkPersistenceViewModel TalkPersistenceViewModel { get; }
		public SelectList AvailableVideos { get; }
    }
}
