using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediaServer.Configuration;
using MediaServer.Models;
using MediaServer.Services.Cache;
using Microsoft.WindowsAzure.Storage.Blob;

namespace MediaServer.Services.Persistence
{
	public class ConferencePersistence : TalkPersistence {
		public ConferencePersistence(IBlogStorageConfig blobStorageConfig, MediaCache cache)
            : base(blobStorageConfig, cache) {
        }

		public async Task<List<Talk>> GetTalksFromConferences(string[] conferenceIds) {
			var talksFromConferences = new List<Talk>[conferenceIds.Length];
			var numberOfTalks = 0;
			for (int i = 0; i < talksFromConferences.Length; i++) {
				talksFromConferences[i] = await GetTalksFromConference(conferenceIds[i]);
				numberOfTalks += talksFromConferences[i].Count;
			}
            
			var talks = new List<Talk>(numberOfTalks);
			foreach (var talksFromConference in talksFromConferences) {
				talks.AddRange(talksFromConference);
			}
         
			return talks;
        }

		public async Task<List<Talk>> GetTalksFromConference(string conferenceId) {
			return await cache.GetOrSet(cache.GetConferenceTalksKey(conferenceId), GetTalksFromConference);

			async Task<List<Talk>> GetTalksFromConference() {
                var talks = new List<Talk>();
				var containerForConference = await GetContainerForConference(conferenceId);
                BlobContinuationToken blobContinuationToken = null;
                do {
                    var results = await containerForConference.ListBlobsSegmentedAsync(TalkPrefix, blobContinuationToken);
                    blobContinuationToken = results.ContinuationToken;
                    foreach (var cloudBlob in results.Results.Cast<CloudBlockBlob>()) {
						string talkName = GetTalkNameFromCloudBlobName(cloudBlob.Name);
						var talk = await GetTalkFromBlob(cloudBlob, talkName, conferenceId);
						talks.Add(talk);
					}
				} while (blobContinuationToken != null);            
				return talks;
            }         
        }
	}
}
