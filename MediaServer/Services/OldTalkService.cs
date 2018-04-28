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
	public class OldTalkService : IOldTalkService
    {
		const string DbFileExtension = ".json";
        
        readonly static char[] dbFileExtension;
		readonly static char[] dbTalkPrefix;

		readonly CloudBlobClient cloudBlobClient;
		readonly IThumbnailService thumbnailService;

		static OldTalkService() {
			dbFileExtension = DbFileExtension.ToCharArray();
			dbTalkPrefix = BlobStoragePersistence.TalkPrefix.ToCharArray();
		}

		public OldTalkService(IBlogStorageConfig blobStorageConfig, IThumbnailService thumbnailService)
		{
			var storageAccount = CloudStorageAccount.Parse(blobStorageConfig.BlobStorageConnectionString);         
            cloudBlobClient = storageAccount.CreateCloudBlobClient();
			this.thumbnailService = thumbnailService;
		}

        // TODO: 404 på dette: Hvordan og hvorfor? dips.talk.Kyrre%20-%20Integrasjon%20mellom%20programpakker%20-%206.%20april%202018%2009.47.33.pdf.json

		public async Task<IEnumerable<Talk>> GetTalksFromConference(Conference conference) {
			var containerForConference = cloudBlobClient.GetContainerForConference(conference);
			if (!(await containerForConference.ExistsAsync())) {
				return new Talk[0];
			}

			// TODO: Support more than 200 items
            var token = new BlobContinuationToken();
			var blobs = await containerForConference.ListBlobsSegmentedAsync(BlobStoragePersistence.TalkPrefix, token);

            var talks = new List<Talk>();
            foreach (var blob in blobs.Results.Cast<CloudBlockBlob>())
            {
                using (var memoryStream = new MemoryStream())
                {
                    await blob.DownloadToStreamAsync(memoryStream);
					var talkContent = Encoding.UTF8.GetString(memoryStream.ToArray());
                    var talk = JsonConvert.DeserializeObject<Talk>(talkContent);
					talk.ConferenceId = conference.Id;
					talks.Add(talk);
                }
            }         
           
			return talks.OrderByDescending(talk => talk.DateOfTalk);
		}

        public async Task<Talk> GetTalkByName(Conference conference, string name)
		{
			var containerForConference = cloudBlobClient.GetContainerForConference(conference);

			var talkReferenceName = GetBlobNameFromTalkName(name);
			var blob = containerForConference.GetBlobReference(talkReferenceName);
            using (var memoryStream = new MemoryStream())
            {
				if (!(await blob.ExistsAsync())) {
					return null;
				}

                await blob.DownloadToStreamAsync(memoryStream);
				var talkContent = Encoding.UTF8.GetString(memoryStream.ToArray());
				var talk = JsonConvert.DeserializeObject<Talk>(talkContent);
				talk.ConferenceId = conference.Id;
                return talk;
            }
		}

		public async Task SaveTalkFromConference(Conference conference, Talk talk) {
			var containerForConference = cloudBlobClient.GetContainerForConference(conference);
            await containerForConference.CreateIfNotExistsAsync();

            var serializedTalk = JsonConvert.SerializeObject(talk);

            var talkReferenceName = GetBlobNameFromTalkName(talk.TalkName);
            var talkReference = containerForConference.GetBlockBlobReference(talkReferenceName);
            await talkReference.UploadTextAsync(serializedTalk);
                        
            talkReference.Properties.ContentType = "application/json";
            await talkReference.SetPropertiesAsync();             
		}

        public async Task DeleteTalkFromConference(Conference conference, Talk talk) {
			var containerForConference = cloudBlobClient.GetContainerForConference(conference);

            // TODO: Delete thumnail too
            var talkReferenceName = GetBlobNameFromTalkName(talk.TalkName);
			var talkReference = containerForConference.GetBlockBlobReference(talkReferenceName);
            if (await talkReference.ExistsAsync()) {
                await talkReference.DeleteAsync();
            }
		}

        public async Task<IReadOnlyList<string>> GetTalkNamesFromConference(Conference conference) {
			// TODO: Support more than 200 items
            var token = new BlobContinuationToken();
			var containerForConference = cloudBlobClient.GetContainerForConference(conference);
			var blobs = await containerForConference.ListBlobsSegmentedAsync(BlobStoragePersistence.TalkPrefix, token); 
            var usedVideos = new List<string>();
            foreach (var blob in blobs.Results.Cast<CloudBlockBlob>()) {
                using (var memoryStream = new MemoryStream()) {
                    await blob.DownloadToStreamAsync(memoryStream);
                    var talkContent = Encoding.UTF8.GetString(memoryStream.ToArray());
                    var talk = JObject.Parse(talkContent);
                    var videoName = (string)talk["VideoName"];
                    usedVideos.Add(videoName);
                }
            }

            return usedVideos;
		}
		            
		// TODO: Move to Keys and rename properly
		string GetBlobNameFromTalkName(string talkName)
		=> BlobStoragePersistence.TalkPrefix + talkName + DbFileExtension;
      
		// TODO: Create generic get talk with optional predicate...
		// TODO: cache every talk
        public async Task<IEnumerable<LatestTalk>> GetLatestTalks(IEnumerable<Conference> conferences) {			
            var talks = new List<LatestTalk>();
            foreach (var conference in conferences) {
				var containerForConference = cloudBlobClient.GetContainerForConference(conference);
                await containerForConference.CreateIfNotExistsAsync();
                // TODO: Support more than 200 items....
                var token = new BlobContinuationToken();
				var blobs = await containerForConference.ListBlobsSegmentedAsync(BlobStoragePersistence.TalkPrefix, token); 
                foreach (var blob in blobs.Results.Cast<CloudBlockBlob>()) {
                    using (var memoryStream = new MemoryStream()) {
                        await blob.DownloadToStreamAsync(memoryStream);
                        var talkContent = Encoding.UTF8.GetString(memoryStream.ToArray());
                        var talk = JsonConvert.DeserializeObject<Talk>(talkContent);
						talk.ConferenceId = conference.Id;
                        var latestTalk = new LatestTalk(conference, talk, blob.Properties.LastModified.Value);
                        talks.Add(latestTalk);
                    }
                }
            }

			return talks.OrderByDescending(t => t.TimeStamp).Take(9);
        }
    }
}
