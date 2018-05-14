using System.IO;
using System.Threading.Tasks;
using MediaServer.Controllers;
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
            if (talk.ThumbnailImageFile == null) {
				if (oldNameOfTalk != null && oldNameOfTalk != talk.TalkName) {
					await thumbnailPersistence.RenameThumbnail(conference, oldNameOfTalk, talk.TalkName);
                }            

				return;
            }
            
            var imageFile = talk.ThumbnailImageFile;
			if (imageFile.Length > 300_000L) {
				return;
			}

			if (imageFile.ContentType.Substring(0, 5) != "image") {
				return;
			}

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
			var baseThumbnailUrl = NavigateableController.GetThumbnailUrl(talk);
			var hash = await thumbnailPersistence.GetSavedHashOfThumbnail(talk);
            if (hash == string.Empty) {
                return baseThumbnailUrl;
            }

            var thumbNailUrl = baseThumbnailUrl + "?v=" + hash;
            return thumbNailUrl;
        }      
	}
}
