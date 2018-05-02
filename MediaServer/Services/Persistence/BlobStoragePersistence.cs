using System.Threading.Tasks;
using MediaServer.Configuration;
using MediaServer.Extensions;
using MediaServer.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace MediaServer.Services.Persistence
{
	public class BlobStoragePersistence
    {
		public const string TalkPrefix = "dips.talk.";

        const string HashExtension = ".txt";

		readonly CloudBlobClient cloudBlobClient;

		public BlobStoragePersistence(IBlogStorageConfig blobStorageConfig)
        {
			var storageAccount = CloudStorageAccount.Parse(blobStorageConfig.BlobStorageConnectionString);
            cloudBlobClient = storageAccount.CreateCloudBlobClient();
        }

		public async Task RenameBlob(Conference conference, string oldName, string newName) {
			var containerForConference = cloudBlobClient.GetContainerForConference(conference);
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

		public static string GetThumbnailKey(string talkName) => "thumb" + talkName;
		public static string GetThumnnailHashName(string talkName) => talkName + HashExtension;      
    }
}
