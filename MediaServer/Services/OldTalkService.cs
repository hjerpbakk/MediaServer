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
		readonly ThumbnailService thumbnailService;
		readonly MediaCache cache;

		static OldTalkService() {
			dbFileExtension = DbFileExtension.ToCharArray();
			dbTalkPrefix = BlobStoragePersistence.TalkPrefix.ToCharArray();
		}

		public OldTalkService(IBlogStorageConfig blobStorageConfig, ThumbnailService thumbnailService, MediaCache cache)
		{
			var storageAccount = CloudStorageAccount.Parse(blobStorageConfig.BlobStorageConnectionString);         
            cloudBlobClient = storageAccount.CreateCloudBlobClient();
			this.thumbnailService = thumbnailService;
			this.cache = cache;
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
			foreach (var cloudBlob in blobs.Results.Cast<CloudBlockBlob>())
            {
				var talkName = GetTalkNameFromBlobName(cloudBlob.Name);
				var talk = await GetCachedTalkFromBlob(conference, talkName, cloudBlob);
				talks.Add(talk);
            }         
           
			return talks.OrderByDescending(talk => talk.DateOfTalk);
		}
        
        public async Task<Talk> GetTalkByName(Conference conference, string name)
		{
			var containerForConference = cloudBlobClient.GetContainerForConference(conference);
            var talkReferenceName = GetBlobNameFromTalkName(name);
            var cloudBlob = containerForConference.GetBlobReference(talkReferenceName);
			var talk = await GetCachedTalkFromBlob(conference, name, cloudBlob);
			return talk;
		}

        public async Task SaveTalkFromConference(Conference conference, Talk talk) {
			var containerForConference = cloudBlobClient.GetContainerForConference(conference);
            await containerForConference.CreateIfNotExistsAsync();

            talk.ConferenceId = conference.Id;
            var serializedTalk = JsonConvert.SerializeObject(talk);

            var talkReferenceName = GetBlobNameFromTalkName(talk.TalkName);
            var talkReference = containerForConference.GetBlockBlobReference(talkReferenceName);
            await talkReference.UploadTextAsync(serializedTalk);
                        
            talkReference.Properties.ContentType = "application/json";
            await talkReference.SetPropertiesAsync(); 

			cache.CacheTalk(talk);            
		}

        public async Task DeleteTalkFromConference(Conference conference, Talk talk) {
			cache.ClearCache(talk);
			var containerForConference = cloudBlobClient.GetContainerForConference(conference);         
            // TODO: Delete thumnail too
            var talkReferenceName = GetBlobNameFromTalkName(talk.TalkName);
			var talkReference = containerForConference.GetBlockBlobReference(talkReferenceName);
            if (await talkReference.ExistsAsync()) {
                await talkReference.DeleteAsync();
            }
		}

        public async Task<IReadOnlyList<string>> GetTalkNamesFromConference(Conference conference)
		{
			var usedVideos = await cache.GetOrSet(
				cache.GetTalkNamesKey(conference.Id),
				() => GetUsedVideosFromConference(conference));
			return usedVideos;         
		}
      
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

		async Task<Talk> GetCachedTalkFromBlob(Conference conference, string name, CloudBlob cloudBlob) {
			var talk = await cache.GetOrSet(
                cache.GetTalkKey(conference.Id, name),
                () => GetTalkFromBlob(conference, cloudBlob));
			return talk;                
		}

		async Task<Talk> GetTalkFromBlob(Conference conference,  CloudBlob cloudBlob)
		{
			using (var memoryStream = new MemoryStream())
            {
                if (!(await cloudBlob.ExistsAsync()))
                {
                    return null;
                }

                await cloudBlob.DownloadToStreamAsync(memoryStream);
                var talkContent = Encoding.UTF8.GetString(memoryStream.ToArray());
                var talk = JsonConvert.DeserializeObject<Talk>(talkContent);
                talk.ConferenceId = conference.Id;
                return talk;
            }
		}

		async Task<IReadOnlyList<string>> GetUsedVideosFromConference(Conference conference)
        {
            // TODO: Support more than 200 items
            var token = new BlobContinuationToken();
            var containerForConference = cloudBlobClient.GetContainerForConference(conference);
            var blobs = await containerForConference.ListBlobsSegmentedAsync(BlobStoragePersistence.TalkPrefix, token);
            var usedVideos = new List<string>();
            foreach (var blob in blobs.Results.Cast<CloudBlockBlob>())
            {
                using (var memoryStream = new MemoryStream())
                {
                    await blob.DownloadToStreamAsync(memoryStream);
                    var talkContent = Encoding.UTF8.GetString(memoryStream.ToArray());
                    var talk = JObject.Parse(talkContent);
                    var videoName = (string)talk["VideoName"];
                    usedVideos.Add(videoName);
                }
            }

            return usedVideos;
        }

		string GetBlobNameFromTalkName(string talkName)
            => BlobStoragePersistence.TalkPrefix + talkName + DbFileExtension;

		// TODO: save char array or do differently
		string GetTalkNameFromBlobName(string blobName)
		    => Path.GetFileNameWithoutExtension(blobName).TrimStart(BlobStoragePersistence.TalkPrefix.ToCharArray());
    }
}
