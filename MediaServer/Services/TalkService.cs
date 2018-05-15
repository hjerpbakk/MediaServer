using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediaServer.Clients;
using MediaServer.Models;
using MediaServer.Services.Persistence;
using MediaServer.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MediaServer.Services {
	public class TalkService {
		const string SelectListFieldName = "Name";

		readonly TalkPersistence talkPersistence;
		readonly Users users;
		readonly ContentService contentService;
		readonly ThumbnailService thumbnailService;
		readonly ISlackClient slackClient;
      
		public TalkService(TalkPersistence talkPersistence, Users users, ContentService contentService, ThumbnailService thumbnailService, ISlackClient slackClient) {
			this.talkPersistence = talkPersistence;
			this.users = users;
			this.contentService = contentService;
			this.thumbnailService = thumbnailService;
			this.slackClient = slackClient;
		}

		public async Task<TalkViewModel> GetTalkViewModel(Conference conference, string talkName) {
			var talk = await talkPersistence.GetTalkByName(conference, talkName);
			if (talk == null) {
				return null;
			}
          
            /// TODO: Support single click pause / resume
            /// 
            /// TODO: hotkeys:
            // -space: play / pause
            //- f: fullscreen
            //- opp / ned: volumkontroll
            //- venstre / høyre: skip back/ frem 5 sec elns
            /// 
            var user = users.GetUser(talk.Speaker);
            var talkViewModel = new TalkViewModel(talk, user);
			return talkViewModel;
		}

        public async Task<ViewModels.TalkMetadata> GetTalkMetadata(Conference conference, string talkName) {
			var talk = await talkPersistence.GetTalkByName(conference, talkName);
            if (talk == null) {
				return null;
            }

			var thumbnail = await thumbnailService.GetThumbnailUrl(talk);
			talk.Thumbnail = thumbnail;
            var editTalkViewModel = new EditTalkViewModel(talk);

			var availableVideos = new List<Video> { new Video(talk.VideoName) };
            var videosFromConference = await contentService.GetVideosFromConference(conference);
            availableVideos.AddRange(videosFromConference);
			var videoList = new SelectList(availableVideos, SelectListFieldName, SelectListFieldName, talk.VideoName);
            
			return new ViewModels.TalkMetadata(editTalkViewModel, videoList);
		}

		public async Task<ViewModels.TalkMetadata> GetTalkMetadata(Conference conference) {
			var talk = new Talk { Thumbnail = "/Placeholder.png" };
			var addTalkViewModel = new AddTalkViewModel(talk);

			var availableVideos = await contentService.GetVideosFromConference(conference);
			var videoList = new SelectList(availableVideos, SelectListFieldName, SelectListFieldName);

			return new ViewModels.TalkMetadata(addTalkViewModel, videoList);
		}

		public async Task<string> SaveTalk(Conference conference, Talk talk, string oldName, Func<Talk, string> getUrlToTalk) {
			if (oldName != null) {
				var oldTalk = new Talk { ConferenceId = conference.Id, TalkName = oldName, Speaker = talk.Speaker };
				await talkPersistence.DeleteTalk(conference, talk);
            }

            // TODO: Client verification also
            // TODO: proper replace here
            talk.TalkName = talk.TalkName.Replace("?", "").Replace(":", " - ").Replace("/", "-").Replace("\"", "-").Replace("#", "");
			talk.ConferenceId = conference.Id;
            talk.TimeStamp = DateTimeOffset.UtcNow;
            contentService.VerifySlides(talk);
			await talkPersistence.SaveTalkFromConference(conference, talk);
            await thumbnailService.SaveThumbnail(conference, talk, oldName);

            if (oldName == null) {
				var talkUrl = getUrlToTalk(talk);
                slackClient.PublishToSlack(talk, talkUrl);
            }

            var escapedTalkName = Uri.EscapeUriString(talk.TalkName);
			return escapedTalkName;
		}
    }
}
