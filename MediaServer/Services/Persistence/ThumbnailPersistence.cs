using System.Threading.Tasks;
using MediaServer.Configuration;
using MediaServer.Extensions;
using MediaServer.Models;
using MediaServer.Services.Cache;

namespace MediaServer.Services.Persistence
{
	public class ThumbnailPersistence : TalkPersistence
    {
        public ThumbnailPersistence(IBlogStorageConfig blobStorageConfig, MediaCache cache)
			: base(blobStorageConfig, cache)
        {
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

            var hashName = GetThumnnailHashName(talkName);
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
            if (exists) {
                var hash = await hashRefrence.DownloadTextAsync();
                return hash;
            }

            return string.Empty;
        }

		public async Task RenameThumbnail(Conference conference, string oldNameOfTalk, string newNameOfTalk) {
            await RenameBlob(conference, oldNameOfTalk, newNameOfTalk);
            await RenameBlob(conference, GetThumnnailHashName(oldNameOfTalk), GetThumnnailHashName(newNameOfTalk));
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
    }
}
