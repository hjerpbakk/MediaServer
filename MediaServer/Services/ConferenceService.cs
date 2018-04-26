using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediaServer.Configuration;
using MediaServer.Extensions;
using MediaServer.Models;
using Microsoft.AspNetCore.Http;

namespace MediaServer.Services
{
	public class ConferenceService : IConferenceService
    {
		readonly IEnumerable<Conference> conferences;
		readonly ITalkService talkService;
		readonly IThumbnailService thumbnailService;

		public ConferenceService(IConferenceConfig conferenceConfig, ITalkService talkService, IThumbnailService thumbnailService)
        {
			conferences = conferenceConfig.Conferences.Values;
			this.talkService = talkService;
			this.thumbnailService = thumbnailService;
			// TODO: Create cache facade, remember invalidation
        }

		public async Task<IEnumerable<TalkSummary>> GetLatestTalks(HttpContext httpContext)
		{
            // TODO: Move implementation of this part of talkservice here
			var talks = await talkService.GetLatestTalks(conferences);
			var orderedSummaries = await Task.WhenAll(talks.Select(t => CreateTalkSummary(t.Conference, t.Talk, httpContext)));
			return orderedSummaries;
		}

		public async Task<IEnumerable<TalkSummary>> GetTalksForConference(Conference conference, HttpContext httpContext)
		{
			// TODO: Move implementation of this part of talkservice here
			var talks = await talkService.GetTalksFromConference(conference);
			var orderedSummaries = await Task.WhenAll(talks.Select(talk => CreateTalkSummary(conference, talk, httpContext)));
			return orderedSummaries;
		}

		async Task<TalkSummary> CreateTalkSummary(Conference conference, Talk talk, HttpContext httpContext) {
			var url = httpContext.GetTalkUrl(conference, talk);
			var thumbnail = await thumbnailService.GetThumbnailUrl(conference, talk, httpContext);
			return new TalkSummary(talk, url, thumbnail);
		}
	}
}
