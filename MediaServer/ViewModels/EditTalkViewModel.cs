using MediaServer.Models;

namespace MediaServer.ViewModels {
	public class EditTalkViewModel : TalkPersistenceViewModel {
		public EditTalkViewModel(Talk talk)
			: base(talk, false) {
        }
    }
}
