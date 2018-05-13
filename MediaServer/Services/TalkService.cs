using System.Threading.Tasks;
using MediaServer.Models;
using MediaServer.Services.Persistence;

namespace MediaServer.Services {
	public class TalkService {
		readonly TalkPersistence talkPersistence;
      
		public TalkService(TalkPersistence talkPersistence, ConferencePersistence conferencePersistence) {
			this.talkPersistence = talkPersistence;
		}

		public async Task<Talk> GetTalkByName(Conference conference, string name)
		    => await talkPersistence.GetTalkByName(conference, name);
        
        public async Task SaveTalkFromConference(Conference conference, Talk talk)
		    => await talkPersistence.SaveTalkFromConference(conference, talk);

        public async Task DeleteTalkFromConference(Conference conference, Talk talk)
		    => await talkPersistence.DeleteTalk(conference, talk);  
    }
}
