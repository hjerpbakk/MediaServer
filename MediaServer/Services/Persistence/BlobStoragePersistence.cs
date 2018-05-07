using System.Threading.Tasks;
using MediaServer.Configuration;
using MediaServer.Extensions;
using MediaServer.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace MediaServer.Services.Persistence
{
	public class BlobStoragePersistence
    {
		// TODO: Should not be public
		public const string TalkPrefix = "dips.talk.";
		public const string DbFileExtension = ".json";

        const string HashExtension = ".txt";

		readonly CloudBlobClient cloudBlobClient;

		public BlobStoragePersistence(IBlogStorageConfig blobStorageConfig)
        {
			var storageAccount = CloudStorageAccount.Parse(blobStorageConfig.BlobStorageConnectionString);
            cloudBlobClient = storageAccount.CreateCloudBlobClient();
        }

		public async Task SaveTalkFromConference(Conference conference, Talk talk)
		{
			var containerForConference = await GetContainerForConference(conference);
            
			var serializedTalk = JsonConvert.SerializeObject(talk);
            var talkReferenceName = GetBlobNameFromTalkName(talk.TalkName);
            var talkReference = containerForConference.GetBlockBlobReference(talkReferenceName);
            await talkReference.UploadTextAsync(serializedTalk);
                        
            talkReference.Properties.ContentType = "application/json";
            await talkReference.SetPropertiesAsync(); 
		}
        
		public async Task RenameBlob(Conference conference, string oldName, string newName) {
			var containerForConference = await GetContainerForConference(conference);
			var copyBlob = containerForConference.GetBlockBlobReference(newName);  
            if (!await copyBlob.ExistsAsync())  
            {  
				var blob = containerForConference.GetBlockBlobReference(oldName);             
                if (await blob.ExistsAsync())  
                {  
                    await copyBlob.StartCopyAsync(blob);  
                    await blob.DeleteIfExistsAsync();  
                } 
            } 
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
        }

		public static string GetThumbnailKey(string talkName) => "thumb" + talkName;
		public static string GetThumnnailHashName(string talkName) => talkName + HashExtension;    
        
		async Task<CloudBlobContainer> GetContainerForConference(Conference conference)
        {
            var containerId = conference.Id.ToLower();
			var containerForConference = cloudBlobClient.GetContainerReference(containerId);
			await containerForConference.CreateIfNotExistsAsync();
			return containerForConference;
        }

		string GetBlobNameFromTalkName(string talkName)
            => TalkPrefix + talkName + DbFileExtension;
    }
}
