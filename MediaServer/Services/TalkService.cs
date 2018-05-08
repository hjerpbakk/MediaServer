using System.Threading.Tasks;
using MediaServer.Models;
using MediaServer.Services.Persistence;

namespace MediaServer.Services {
	public class TalkService {
		readonly BlobStoragePersistence blobStoragePersistence;
      
		public TalkService(BlobStoragePersistence blobStoragePersistence) {
			this.blobStoragePersistence = blobStoragePersistence;
		}

		public async Task<Talk> GetTalkByName(Conference conference, string name)
		    => await blobStoragePersistence.GetTalkByName(conference, name);
        
        public async Task SaveTalkFromConference(Conference conference, Talk talk)
		    => await blobStoragePersistence.SaveTalkFromConference(conference, talk);

        public async Task DeleteTalkFromConference(Conference conference, Talk talk)
		    => await blobStoragePersistence.DeleteTalk(conference, talk);  
    }
}
