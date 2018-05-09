using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaServer.Configuration;
using MediaServer.Extensions;
using MediaServer.Models;
using MediaServer.Services.Cache;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace MediaServer.Services.Persistence
{
	public class BlobStoragePersistence {
		const string TalkPrefix = "dips.talk.";
		const string DbFileExtension = ".json";      
        const string HashExtension = ".txt";
		const string HashContentType = "text/plain";

		static readonly char[] talkPrefix;

		readonly CloudBlobClient cloudBlobClient;
		readonly MediaCache cache;

		static BlobStoragePersistence() {
			talkPrefix = TalkPrefix.ToCharArray();
		}

		public BlobStoragePersistence(IBlogStorageConfig blobStorageConfig, MediaCache cache) {
			var storageAccount = CloudStorageAccount.Parse(blobStorageConfig.BlobStorageConnectionString);
            cloudBlobClient = storageAccount.CreateCloudBlobClient();
			this.cache = cache;
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
						var talkName = Path.GetFileNameWithoutExtension(cloudBlob.Name).TrimStart(talkPrefix);
                        var talk = await GetTalkFromBlob(cloudBlob, talkName, conference.Id);
                        talks.Add(talk);
                    }
                } while (blobContinuationToken != null);

                return talks;
			}         
        }
        
		public async Task<Talk> GetTalkByName(Conference conference, string name) {
			var containerForConference = await GetContainerForConference(conference);

            var talkReferenceName = GetBlobNameFromTalkName(name);
            var cloudBlob = containerForConference.GetBlobReference(talkReferenceName);
			var talk = await GetTalkFromBlob(cloudBlob, name, conference.Id);

            return talk;
        }

		public async Task SaveTalkFromConference(Conference conference, Talk talk) {
			talk.ConferenceId = conference.Id;
			var containerForConference = await GetContainerForConference(conference);
            
			var serializedTalk = JsonConvert.SerializeObject(talk);
            var talkReferenceName = GetBlobNameFromTalkName(talk.TalkName);
            var talkReference = containerForConference.GetBlockBlobReference(talkReferenceName);
            await talkReference.UploadTextAsync(serializedTalk);
                        
            talkReference.Properties.ContentType = "application/json";
            await talkReference.SetPropertiesAsync();

			cache.CacheTalk(talk);
		}

		public async Task DeleteTalk(Conference conference, Talk talk) {
			var containerForConference = await GetContainerForConference(conference);         
            var talkReferenceName = GetBlobNameFromTalkName(talk.TalkName);
            var talkReference = containerForConference.GetBlockBlobReference(talkReferenceName);
            if (await talkReference.ExistsAsync()) {
                await talkReference.DeleteAsync();
            }

			var thumbnailReference = containerForConference.GetBlockBlobReference(talk.TalkName);  
			if (await thumbnailReference.ExistsAsync()) {
				await thumbnailReference.DeleteAsync();
            }

			var hashName = GetThumnnailHashName(talk.TalkName);
            var hashReference = containerForConference.GetBlockBlobReference(hashName);
			if (await hashReference.ExistsAsync()) {
				await hashReference.DeleteAsync();
            }

			cache.ClearCache(talk);
            cache.ClearForThumbnail(talk);
        }
        
		public async Task<(bool exists, Image image)> GetThumbnail(Conference conference, string talkName) {
			var containerForConference = await GetContainerForConference(conference);         
			var thumbnailReference = containerForConference.GetBlockBlobReference(talkName);
            var exists = await thumbnailReference.ExistsAsync();
			if (!exists) {
				return (false, new Image());
			}

			var imageData = new byte[thumbnailReference.Properties.Length];
            await thumbnailReference.DownloadToByteArrayAsync(imageData, 0);
			return (true, new Image(thumbnailReference.Properties.ContentType, imageData));
		}

		public async Task SaveThumbnail(Conference conference, string talkName, string contentType, byte[] imageData) {
			var containerForConference = await GetContainerForConference(conference); 

			var thumbnailReference = containerForConference.GetBlockBlobReference(talkName);
            await thumbnailReference.UploadFromByteArrayAsync(imageData, 0, imageData.Length);
			thumbnailReference.Properties.ContentType = contentType;
            await thumbnailReference.SetPropertiesAsync();

			var hashName = BlobStoragePersistence.GetThumnnailHashName(talkName);
            var hashReference = containerForConference.GetBlockBlobReference(hashName);
            var hash = Hasher.GetHashOfImage(imageData);
            await hashReference.UploadTextAsync(hash);
            hashReference.Properties.ContentType = HashContentType;
            await hashReference.SetPropertiesAsync();
		}
        
		public async Task<string> GetSavedHashOfThumbnail(Talk talk) {
            var hashName = GetThumnnailHashName(talk.TalkName);
            var containerForConference = cloudBlobClient.GetContainerForTalk(talk);
            var hashRefrence = containerForConference.GetBlockBlobReference(hashName);
            var exists = await hashRefrence.ExistsAsync();
            if (exists)
            {
                var hash = await hashRefrence.DownloadTextAsync();
                return hash;
            }

            return string.Empty;
        }

		public async Task RenameThumbnail(Conference conference, string oldNameOfTalk, string newNameOfTalk) {
			await RenameBlob(conference, oldNameOfTalk, newNameOfTalk);
            await RenameBlob(conference, GetThumnnailHashName(oldNameOfTalk), GetThumnnailHashName(newNameOfTalk));
		}

		// TODO: Where to put these really
		public static string GetThumbnailKey(string talkName) => "thumb" + talkName;
		public static string GetThumnnailHashName(string talkName) => talkName + HashExtension;

		string GetBlobNameFromTalkName(string talkName) => TalkPrefix + talkName + DbFileExtension;
        
		async Task<CloudBlobContainer> GetContainerForConference(Conference conference) {
            var containerId = conference.Id.ToLower();
			var containerForConference = cloudBlobClient.GetContainerReference(containerId);
			await containerForConference.CreateIfNotExistsAsync();
			return containerForConference;
        }
        
		async Task RenameBlob(Conference conference, string oldName, string newName) {
            var containerForConference = await GetContainerForConference(conference);
            var copyBlob = containerForConference.GetBlockBlobReference(newName);
            if (!await copyBlob.ExistsAsync()) {
                var blob = containerForConference.GetBlockBlobReference(oldName);
                if (await blob.ExistsAsync()) {
                    await copyBlob.StartCopyAsync(blob);
                    await blob.DeleteIfExistsAsync();
                }
            }
        }

        async Task<Talk> GetTalkFromBlob(CloudBlob cloudBlob, string name, string conferenceId) {
			return await cache.GetOrSet(
				cache.GetTalkKey(conferenceId, name),
				() => GetTalk());
			
			async Task<Talk> GetTalk() {
				using (var memoryStream = new MemoryStream()) {
                    if (!(await cloudBlob.ExistsAsync())) {
                        return null;
                    }

                    await cloudBlob.DownloadToStreamAsync(memoryStream);
                    var talkContent = Encoding.UTF8.GetString(memoryStream.ToArray());
                    var talk = JsonConvert.DeserializeObject<Talk>(talkContent);
					talk.ConferenceId = conferenceId;
					talk.TimeStamp = cloudBlob.Properties.LastModified.Value;
                    return talk;
                }
			}
        }
    }
}
