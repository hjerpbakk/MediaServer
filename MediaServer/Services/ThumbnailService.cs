using System.IO;
using System.Threading.Tasks;
using MediaServer.Configuration;
using MediaServer.Models;
using MediaServer.Services.Cache;
using MediaServer.Services.Persistence;
using Microsoft.Extensions.FileProviders;

namespace MediaServer.Services {
	public class ThumbnailService {      
		readonly IFileProvider fileProvider;
		readonly MediaCache cache;
		readonly ThumbnailPersistence thumbnailPersistence;

		public ThumbnailService(IFileProvider fileProvider, MediaCache cache, ThumbnailPersistence thumbnailPersistence) {
			this.fileProvider = fileProvider;
			this.cache = cache;
			this.thumbnailPersistence = thumbnailPersistence;
        }

		public async Task<Image> GetTalkThumbnail(Conference conference, string talkName) {
			var (exists, image) = await thumbnailPersistence.GetThumbnail(conference, talkName);
			if (exists) {
				return image;
            }

            var fileInfo = fileProvider.GetFileInfo("wwwroot/Placeholder.png");
            using (var readStream = fileInfo.CreateReadStream()) {
                using (var ms = new MemoryStream()) {
                    await readStream.CopyToAsync(ms);
                    return new Image("image/png", ms.ToArray());
                }
            }
		}

		public async Task SaveThumbnail(Conference conference, Talk talk, string oldNameOfTalk = null) {
			cache.ClearForThumbnail(talk);
            if (talk.ThumbnailImageFile == null) {
				if (oldNameOfTalk != null) {
					await thumbnailPersistence.RenameThumbnail(conference, oldNameOfTalk, talk.TalkName);
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

			await thumbnailPersistence.SaveThumbnail(conference, talk.TalkName, imageFile.ContentType, imageData);
		}

		public async Task<string> GetThumbnailUrl(Talk talk) { 
			var thumbnailUrl = await cache.GetOrSet(
				TalkPersistence.GetThumnnailHashName(talk.TalkName), 
				() => CreateThumbnailUrl(talk));
			return thumbnailUrl;
		}

		async Task<string> CreateThumbnailUrl(Talk talk) {
			var baseThumbnailUrl = Paths.GetThumbnailUrl(talk);
			var hash = await thumbnailPersistence.GetSavedHashOfThumbnail(talk);
            if (hash == string.Empty) {
                return baseThumbnailUrl;
            }

            var thumbNailUrl = $"{baseThumbnailUrl}?v={hash}";
            return thumbNailUrl;
        }      
	}
}
