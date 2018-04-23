using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaServer.Configuration;
using MediaServer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MediaServer.Services
{
	public class TalkService : ITalkService
    {
		const string DbFileExtension = ".json";
		const string TalkPrefix = "dips.talk.";
        
        readonly static char[] dbFileExtension;
		readonly static char[] dbTalkPrefix;

		readonly CloudBlobClient blobClient;
        readonly IFileProvider fileProvider;

		static TalkService() {
			dbFileExtension = DbFileExtension.ToCharArray();
			dbTalkPrefix = TalkPrefix.ToCharArray();
		}

        public TalkService(IBlogStorageConfig blobStorageConfig, IFileProvider fileProvider)
		{
			var storageAccount = CloudStorageAccount.Parse(blobStorageConfig.BlobStorageConnectionString);         
            blobClient = storageAccount.CreateCloudBlobClient();

            this.fileProvider = fileProvider;
		}

        public async Task<IReadOnlyList<Talk>> GetTalksFromConference(Conference conference) {
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
                return talk;
            }
		}

		public async Task SaveTalkFromConference(Conference conference, Talk talk) {
			var containerForConference = await SaveTalk(conference, talk);
			await SaveThumbnail(containerForConference, talk);
		}

        public async Task DeleteTalkFromConference(Conference conference, Talk talk) {
			var containerForConference = GetContainerFromConference(conference);

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
			var containerForConference = GetContainerFromConference(conference);
			var blobs = await containerForConference.ListBlobsSegmentedAsync(TalkPrefix, token); 
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

            var talkReferenceName = GetBlobNameFromTalkName(talk.TalkName);
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
            var thumbnailReference = containerForConference.GetBlockBlobReference(talk.TalkName);
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

        public async Task<Image> GetTalkThumbnail(Conference conference, string name) {
            var containerForConference = GetContainerFromConference(conference);
            var thumbnailReference = containerForConference.GetBlockBlobReference(name);
            var exists = await thumbnailReference.ExistsAsync();
            if (exists) {
                var imageData = new byte[thumbnailReference.Properties.Length];
                await thumbnailReference.DownloadToByteArrayAsync(imageData, 0);
                return new Image(thumbnailReference.Properties.ContentType, imageData);
            }

            var fileInfo = fileProvider.GetFileInfo("wwwroot/Placeholder.png");
            using(var readStream = fileInfo.CreateReadStream()) {
                using (MemoryStream ms = new MemoryStream()) {
                    await readStream.CopyToAsync(ms);
                    return new Image("image/png", ms.ToArray());
                }
            }
        }
    }
}
