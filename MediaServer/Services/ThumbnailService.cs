using System;
using System.IO;
using System.Threading.Tasks;
using MediaServer.Configuration;
using MediaServer.Extensions;
using MediaServer.Models;
using Microsoft.Extensions.FileProviders;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace MediaServer.Services
{
	public class ThumbnailService : IThumbnailService
    {
		readonly CloudBlobClient cloudBlobClient;
		readonly IFileProvider fileProvider;

		public ThumbnailService(IBlogStorageConfig blobStorageConfig, IFileProvider fileProvider)
        {
			var storageAccount = CloudStorageAccount.Parse(blobStorageConfig.BlobStorageConnectionString);
            cloudBlobClient = storageAccount.CreateCloudBlobClient();
			this.fileProvider = fileProvider;
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
                using (MemoryStream ms = new MemoryStream())
                {
                    await readStream.CopyToAsync(ms);
                    return new Image("image/png", ms.ToArray());
                }
            }
		}

		// TODO: Add Get Thumbnail URL
        // TODO: Use SHA512 in URL method
		// TODO: Do what the tag helper does, images/asplogo.png?v=Kl_dqr9NVtnMdsM2MUg4qthUnWZm5T1fCEimBPWDNgM
        // Sha512, recalulate on save of new thumb...

		public async Task SaveThumbnail(Conference conference, Talk talk)
		{
			if (talk.ThumbnailImageFile == null)
            {
                return;
            }

            // TODO: Save SHA512 also

            // TODO: Protect against other filetypes and set a max size. Can maxsize be checked i JS too?
            // https://blogs.msdn.microsoft.com/dotnet/2017/01/19/net-core-image-processing/
            var imageFile = talk.ThumbnailImageFile;
            var extension = Path.GetExtension(imageFile.FileName);
			var containerForConference = cloudBlobClient.GetContainerForConference(conference);
            var thumbnailReference = containerForConference.GetBlockBlobReference(talk.TalkName);
            var exists = await thumbnailReference.ExistsAsync();
            if (exists && thumbnailReference.Properties.Length == imageFile.Length)
            {
                return;
            }

            using (var image = imageFile.OpenReadStream())
            {
                await thumbnailReference.UploadFromStreamAsync(image, imageFile.Length);
            }

            thumbnailReference.Properties.ContentType = imageFile.ContentType;
            await thumbnailReference.SetPropertiesAsync();
		}
	}
}
