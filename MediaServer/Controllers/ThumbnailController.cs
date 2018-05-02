using System;
using System.Threading.Tasks;
using MediaServer.Configuration;
using MediaServer.Services;
using MediaServer.Services.Cache;
using MediaServer.Services.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace MediaServer.Controllers
{
	public class ThumbnailController : NavigateableController
    {
		readonly ThumbnailService thumbnailService;
		readonly MediaCache cache;

		public ThumbnailController(ConferenceConfig conferenceConfig, ThumbnailService thumbnailService, MediaCache cache)
			: base(conferenceConfig)
        {
			this.thumbnailService = thumbnailService;
			this.cache = cache;
        }

		[HttpGet("/Conference/{conferenceId}/Thumbnails/{talkName}")]
        public async Task<IActionResult> GetTalkThumbnail(string conferenceId, string talkName)
		{
			var view = await cache.GetOrSet(
				BlobStoragePersistence.GetThumbnailKey(talkName), 
				() => GetThumbnail(conferenceId, talkName));
			return view;
		}

		async Task<IActionResult> GetThumbnail(string conferenceId, string talkName)
		{
			var conference = GetConferenceFromId(conferenceId);
			var thumbnail = await thumbnailService.GetTalkThumbnail(conference, talkName);
			return File(thumbnail.ImageData, thumbnail.ContentType);
		}
	}
}
