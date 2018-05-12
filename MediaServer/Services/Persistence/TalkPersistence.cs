using System.IO;
using System.Text;
using System.Threading.Tasks;
using MediaServer.Configuration;
using MediaServer.Models;
using MediaServer.Services.Cache;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace MediaServer.Services.Persistence
{
	public class TalkPersistence {
        protected const string TalkPrefix = "dips.talk.";
		const string DbFileExtension = ".json";   
		static readonly int talkNameStartIndex;
		static readonly int talkNameLengthModifier;

		protected const string HashExtension = ".txt";
		protected const string HashContentType = "text/plain";
        
		protected readonly CloudBlobClient cloudBlobClient;
		protected readonly MediaCache cache;

		static TalkPersistence() {
			talkNameStartIndex = TalkPrefix.Length;
			talkNameLengthModifier = talkNameStartIndex + DbFileExtension.Length;
		}

		public TalkPersistence(IBlogStorageConfig blobStorageConfig, MediaCache cache) {
			var storageAccount = CloudStorageAccount.Parse(blobStorageConfig.BlobStorageConnectionString);
            cloudBlobClient = storageAccount.CreateCloudBlobClient();
			this.cache = cache;
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

            cache.ClearCache(talk);
        }

		// TODO: Where to put these really
        public static string GetThumbnailKey(string talkName) 
		    => "thumb" + talkName;

		public static string GetThumnnailHashName(string talkName) 
		    => talkName + HashExtension;

		string GetBlobNameFromTalkName(string talkName) 
		    => TalkPrefix + talkName + DbFileExtension;

		protected string GetTalkNameFromCloudBlobName(string cloudBlobName)
		    => cloudBlobName.Substring(talkNameStartIndex, cloudBlobName.Length - talkNameLengthModifier);
        
		protected async Task<CloudBlobContainer> GetContainerForConference(Conference conference)
		    => await GetContainerForConference(conference.Id);

		protected async Task<CloudBlobContainer> GetContainerForConference(string conferenceId) {
			var containerId = conferenceId.ToLower();
			var containerForConference = cloudBlobClient.GetContainerReference(containerId);
            await containerForConference.CreateIfNotExistsAsync();
            return containerForConference;
        }    

		protected async Task<Talk> GetTalkFromBlob(CloudBlob cloudBlob, string talkName, string conferenceId) {
			return await cache.GetOrSet(
				cache.GetTalkKey(conferenceId, talkName),
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
