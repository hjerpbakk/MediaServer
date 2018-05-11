using System.Collections.Generic;
using System.IO;
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
            : base(blobStorageConfig, cache)
        {
        }

        public async Task<IEnumerable<Talk>> GetTalksFromConferences(IEnumerable<Conference> conferences) {
            var talks = new List<Talk>();
            foreach (var conference in conferences) {
                talks.AddRange(await GetTalksFromConference(conference));
            }

            return talks;
        }

        public async Task<IEnumerable<Talk>> GetTalksFromConference(Conference conference) {
            return await cache.GetOrSet(cache.GetConferenceTalksKey(conference.Id), GetTalksFromConference);

            async Task<IEnumerable<Talk>> GetTalksFromConference() {
                var talks = new List<Talk>();
                var containerForConference = await GetContainerForConference(conference);
                BlobContinuationToken blobContinuationToken = null;
                do {
                    var results = await containerForConference.ListBlobsSegmentedAsync(TalkPrefix, blobContinuationToken);
                    blobContinuationToken = results.ContinuationToken;
                    foreach (var cloudBlob in results.Results.Cast<CloudBlockBlob>()) {
						string talkName = GetTalkNameFromCloudBlobName(cloudBlob.Name);
						var talk = await GetTalkFromBlob(cloudBlob, talkName, conference.Id);
						talks.Add(talk);
					}
				} while (blobContinuationToken != null);

                return talks;
            }         
        }
	}
}
