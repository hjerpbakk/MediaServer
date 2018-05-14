using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediaServer.Models;
using MediaServer.Services;
using MediaServer.Services.Cache;
using MediaServer.Services.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace MediaServer.Controllers {
	public class ThumbnailController : NavigateableController {
		readonly ThumbnailService thumbnailService;
		readonly MediaCache cache;

		public ThumbnailController(Dictionary<string, Conference> conferences, ThumbnailService thumbnailService, MediaCache cache)
			: base(conferences) {
			this.thumbnailService = thumbnailService;
			this.cache = cache;
        }

		[HttpGet("/Conference/{conferenceId}/Thumbnails/{talkName}")]
        public async Task<IActionResult> GetTalkThumbnail(string conferenceId, string talkName) {
			Console.WriteLine($"GetTalkThumbnail " + conferenceId + " " + talkName);
			var view = await cache.GetOrSet(TalkPersistence.GetThumbnailKey(talkName), GetThumbnail);
			return view;

			async Task<IActionResult> GetThumbnail() {
                var conference = GetConferenceFromId(conferenceId);
                var thumbnail = await thumbnailService.GetTalkThumbnail(conference, talkName);
                return File(thumbnail.ImageData, thumbnail.ContentType);
            }
		}      
	}
}
