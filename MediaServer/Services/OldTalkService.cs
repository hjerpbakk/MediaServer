using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaServer.Configuration;
using MediaServer.Extensions;
using MediaServer.Models;
using MediaServer.Services.Cache;
using MediaServer.Services.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MediaServer.Services
{
	public class OldTalkService
    {
		readonly BlobStoragePersistence blobStoragePersistence;
      
		public OldTalkService(BlobStoragePersistence blobStoragePersistence)
		{
			this.blobStoragePersistence = blobStoragePersistence;
		}
        
		// TODO: Move to IConferenceService
		public async Task<IEnumerable<Talk>> GetTalksFromConference(Conference conference) {
			var talks = await blobStoragePersistence.GetTalksFromConference(conference);
			return talks.OrderByDescending(talk => talk.DateOfTalk);
		}

		public async Task<Talk> GetTalkByName(Conference conference, string name)
		    => await blobStoragePersistence.GetTalkByName(conference, name);
        
        public async Task SaveTalkFromConference(Conference conference, Talk talk)
		    => await blobStoragePersistence.SaveTalkFromConference(conference, talk);

        public async Task DeleteTalkFromConference(Conference conference, Talk talk)
		    => await blobStoragePersistence.DeleteTalk(conference, talk);  

		// TODO: Move to IConferenceService
		public async Task<IEnumerable<string>> GetTalkNamesFromConference(Conference conference) {
			var talks = await blobStoragePersistence.GetTalksFromConference(conference);
            return talks.Select(talk => talk.TalkName);
		}
      
		// TODO: Create generic get talk with optional predicate...

		// TODO: Move to IConferenceService
		public async Task<IEnumerable<Talk>> GetLatestTalks(IEnumerable<Conference> conferences) {			
			var talks = await blobStoragePersistence.GetTalksFromConferences(conferences);
			return talks.OrderByDescending(t => t.TimeStamp).Take(9);
        }
    }
}
