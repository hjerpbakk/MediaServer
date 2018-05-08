using System;
using System.IO;
using System.Security.Cryptography;
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

namespace MediaServer.Services
{
	public class ThumbnailService
    {
        const string HashContentType = "text/plain";

		readonly CloudBlobClient cloudBlobClient;
		readonly IFileProvider fileProvider;
		readonly MediaCache cache;
		readonly BlobStoragePersistence blobStoragePersistence;

		public ThumbnailService(IBlogStorageConfig blobStorageConfig, IFileProvider fileProvider, MediaCache cache, BlobStoragePersistence blobStoragePersistence)
        {
			var storageAccount = CloudStorageAccount.Parse(blobStorageConfig.BlobStorageConnectionString);
            cloudBlobClient = storageAccount.CreateCloudBlobClient();
			this.fileProvider = fileProvider;
			this.cache = cache;
			this.blobStoragePersistence = blobStoragePersistence;
        }

		public async Task<Image> GetTalkThumbnail(Conference conference, string name)
		{
			var containerForConference = cloudBlobClient.GetContainerForConference(conference);
            var thumbnailReference = containerForConference.GetBlockBlobReference(name);
            var exists = await thumbnailReference.ExistsAsync();
            if (exists)
            {
                var imageData = new byte[thumbnailReference.Properties.Length];
                await thumbnailReference.DownloadToByteArrayAsync(imageData, 0);
                return new Image(thumbnailReference.Properties.ContentType, imageData);
            }

            var fileInfo = fileProvider.GetFileInfo("wwwroot/Placeholder.png");
            using (var readStream = fileInfo.CreateReadStream())
            {
                using (var ms = new MemoryStream())
                {
                    await readStream.CopyToAsync(ms);
                    return new Image("image/png", ms.ToArray());
                }
            }
		}

		public async Task SaveThumbnail(Conference conference, Talk talk, string oldNameOfTalk = null)
		{
			cache.ClearForThumbnail(talk);
            if (talk.ThumbnailImageFile == null) {
				if (oldNameOfTalk != null) {
					await blobStoragePersistence.RenameBlob(conference, oldNameOfTalk, talk.TalkName);
					await blobStoragePersistence.RenameBlob(conference, 
					                                        BlobStoragePersistence.GetThumnnailHashName(oldNameOfTalk), 
					                                        BlobStoragePersistence.GetThumnnailHashName(talk.TalkName));
                }            

				return;
            }
                    
            // TODO: Protect against other filetypes and set a max size. Can maxsize be checked i JS too?
            // https://blogs.msdn.microsoft.com/dotnet/2017/01/19/net-core-image-processing/
            var imageFile = talk.ThumbnailImageFile;
            var extension = Path.GetExtension(imageFile.FileName);
			byte[] imageData;
			using (var readStream = imageFile.OpenReadStream()) {
                using (MemoryStream ms = new MemoryStream()) {
                    await readStream.CopyToAsync(ms);
					imageData = ms.ToArray();
                }
            }
            
			var containerForConference = cloudBlobClient.GetContainerForConference(conference);

            var thumbnailReference = containerForConference.GetBlockBlobReference(talk.TalkName);  
			await thumbnailReference.UploadFromByteArrayAsync(imageData, 0, imageData.Length);
            thumbnailReference.Properties.ContentType = imageFile.ContentType;
            await thumbnailReference.SetPropertiesAsync();
                     
			var hashName = BlobStoragePersistence.GetThumnnailHashName(talk.TalkName);
            var hashReference = containerForConference.GetBlockBlobReference(hashName);
			var hash = GetHashOfImage(imageData);
			await hashReference.UploadTextAsync(hash);
			hashReference.Properties.ContentType = HashContentType;
			await hashReference.SetPropertiesAsync();
		}

		public async Task<string> GetThumbnailUrl(Talk talk)
		{
			var thumbnailUrl = await cache.GetOrSet(
				BlobStoragePersistence.GetThumnnailHashName(talk.TalkName), 
				() => CreateThumbnailUrl(talk));
			return thumbnailUrl;
		}

		async Task<string> CreateThumbnailUrl(Talk talk)
        {
			var baseThumbnailUrl = Paths.GetThumbnailUrl(talk);
            var hash = await GetSavedHashOfThumbnail(talk);
            if (hash == string.Empty)
            {
                return baseThumbnailUrl;
            }

            var thumbNailUrl = $"{baseThumbnailUrl}?v={hash}";
            return thumbNailUrl;
        }

		async Task<string> GetSavedHashOfThumbnail(Talk talk) {
			var hashName = BlobStoragePersistence.GetThumnnailHashName(talk.TalkName);
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
        
		string GetHashOfImage(byte[] imageData) {
			var sha512 = new SHA512Managed();
			var hash = sha512.ComputeHash(imageData);
			var stringRepresentation = BitConverter.ToString(hash).Replace("-", "");
			return stringRepresentation;
		} 
	}
}
