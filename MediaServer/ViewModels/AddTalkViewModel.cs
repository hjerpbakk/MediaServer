using MediaServer.Models;

namespace MediaServer.ViewModels {
	public class AddTalkViewModel : TalkPersistenceViewModel {
		public AddTalkViewModel(Talk talk)
			: base(talk, true) {
        }
    }
}
