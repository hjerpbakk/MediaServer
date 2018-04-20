using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaServer.Configuration;
using MediaServer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace MediaServer.Services
{
	public class TalkService : ITalkService
    {
		const string DbFileExtension = ".json";
		const string TalkPrefix = "dips.talk.";
        
        readonly static char[] dbFileExtension;
		readonly static char[] dbTalkPrefix;

		readonly CloudBlobClient blobClient;     

		static TalkService() {
			dbFileExtension = DbFileExtension.ToCharArray();
			dbTalkPrefix = TalkPrefix.ToCharArray();
		}

		public TalkService(IBlogStorageConfig blobStorageConfig)
		{
			var storageAccount = CloudStorageAccount.Parse(blobStorageConfig.BlobStorageConnectionString);         
            blobClient = storageAccount.CreateCloudBlobClient();         
		}

		public async Task<Talk[]> GetTalksFromConference(Conference conference) {
			var containerForConference = GetContainerFromConference(conference);
			if (!(await containerForConference.ExistsAsync())) {
				return new Talk[0];
			}

			// TODO: Support more than 200 items
            var token = new BlobContinuationToken();
			var blobs = await containerForConference.ListBlobsSegmentedAsync(TalkPrefix, token);

            var talks = new List<Talk>();
            foreach (var blob in blobs.Results.Cast<CloudBlockBlob>())
            {
                using (var memoryStream = new MemoryStream())
                {
                    await blob.DownloadToStreamAsync(memoryStream);
					var talkContent = Encoding.UTF8.GetString(memoryStream.ToArray());
                    var talk = JsonConvert.DeserializeObject<Talk>(talkContent);
                    await AddThumbnail(containerForConference, talk);
					talks.Add(talk);
                }
            }         
           
			return talks.ToArray();
		}

        public async Task<Talk> GetTalkByName(Conference conference, string name)
		{
			var containerForConference = GetContainerFromConference(conference);

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
                await AddThumbnail(containerForConference, talk);
                return talk;
            }
		}

		public async Task SaveTalkFromConference(Conference conference, Talk talk) {
			var containerForConference = await SaveTalk(conference, talk);
			await SaveThumbnail(containerForConference, talk);
		}

        public async Task DeleteTalkFromConference(Conference conference, Talk talk) {
			var containerForConference = GetContainerFromConference(conference);

			var talkReferenceName = GetBlobNameFromTalkName(talk.Name);
			var talkReference = containerForConference.GetBlockBlobReference(talkReferenceName);
            if (await talkReference.ExistsAsync()) {
                await talkReference.DeleteAsync();
            }
		}

		public async Task<string[]> GetTalkNamesFromConference(Conference conference) {
			// TODO: Support more than 200 items
            var token = new BlobContinuationToken();
			var containerForConference = GetContainerFromConference(conference);
			var blobs = await containerForConference.ListBlobsSegmentedAsync(TalkPrefix, token);    
            var talkNames = blobs.Results.Cast<CloudBlockBlob>().
                Select(b => Path.GetFileNameWithoutExtension(b.Name.TrimStart(dbTalkPrefix)) + Video.SupportedVideoFileType).
				ToArray();
   
			return talkNames;
		}
      
        /// <summary>
        /// Gets the container from conference.
		/// 
		/// Ref: https://docs.microsoft.com/en-us/rest/api/storageservices/naming-and-referencing-containers--blobs--and-metadata
        /// </summary>
        /// <returns>The container from conference.</returns>
        /// <param name="conference">Conference.</param>
		CloudBlobContainer GetContainerFromConference(Conference conference) {
			var containerId = conference.Id.ToLower();
			return blobClient.GetContainerReference(containerId);
		}
		            
		string GetBlobNameFromTalkName(string talkName)
		    => TalkPrefix + talkName + DbFileExtension;

		async Task<CloudBlobContainer> SaveTalk(Conference conference, Talk talk)
        {
            var containerForConference = GetContainerFromConference(conference);
            await containerForConference.CreateIfNotExistsAsync();

            var serializedTalk = JsonConvert.SerializeObject(talk);

            var talkReferenceName = GetBlobNameFromTalkName(talk.Name);
            var talkReference = containerForConference.GetBlockBlobReference(talkReferenceName);
            await talkReference.UploadTextAsync(serializedTalk);
                        
			talkReference.Properties.ContentType = "application/json";
			await talkReference.SetPropertiesAsync();        
            return containerForConference;
        }

        static async Task SaveThumbnail(CloudBlobContainer containerForConference, Talk talk) {
			if (talk.ThumbnailImageFile == null) {
                return;
            }

			// TODO: Protect against other filetypes and set a max size. Can maxsize be checked i JS too?
            // https://blogs.msdn.microsoft.com/dotnet/2017/01/19/net-core-image-processing/
            var imageFile = talk.ThumbnailImageFile;
            var extension = Path.GetExtension(imageFile.FileName);
			var thumbnailReference = containerForConference.GetBlockBlobReference(talk.Name);
            var exists = await thumbnailReference.ExistsAsync();
			if (exists && thumbnailReference.Properties.Length == imageFile.Length) {
                return;
            }

            using (var image = imageFile.OpenReadStream()) {
                await thumbnailReference.UploadFromStreamAsync(image, imageFile.Length);
            }

            thumbnailReference.Properties.ContentType = imageFile.ContentType;
            await thumbnailReference.SetPropertiesAsync();
		}

        static async Task AddThumbnail(CloudBlobContainer containerForConference, Talk talk)
        {
            var thumbnailReference = containerForConference.GetBlockBlobReference(talk.Name);
            var exists = await thumbnailReference.ExistsAsync();
            if (exists)
            {
                var imageData = new byte[thumbnailReference.Properties.Length];
                await thumbnailReference.DownloadToByteArrayAsync(imageData, 0);
                var imageAsBase64String = Convert.ToBase64String(imageData);
                talk.Thumbnail = $"data:{thumbnailReference.Properties.ContentType};base64, {imageAsBase64String}";
            }
            else
            {
                talk.Thumbnail = "http://placehold.it/700x400";
            }
        }
    }
}
