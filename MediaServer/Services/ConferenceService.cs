using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediaServer.Configuration;
using MediaServer.Models;
using Microsoft.AspNetCore.Http;

namespace MediaServer.Services
{
	public class ConferenceService : IConferenceService
    {
		readonly IEnumerable<Conference> conferences;
		readonly ITalkService talkService;

		public ConferenceService(IConferenceConfig conferenceConfig, ITalkService talkService)
        {
			conferences = conferenceConfig.Conferences.Values;
			this.talkService = talkService;
			// TODO: Create cache facade, remember invalidation
        }

		public async Task<IEnumerable<TalkSummary>> GetLatestTalks(HttpContext httpContext)
		{
			// TODO: Move implementation here
			var talks = await talkService.GetLatestTalks(conferences);
			var orderedSummaries = talks.Select(t => new TalkSummary(t.Conference, t.Talk, httpContext));
			return orderedSummaries;
		}

		public async Task<IEnumerable<TalkSummary>> GetTalksForConference(Conference conference, HttpContext httpContext)
		{
			// TODO: Move implementation here
			var talks = await talkService.GetTalksFromConference(conference);
			var orderedSummaries = talks.Select(talk => new TalkSummary(conference, talk, httpContext));
			return orderedSummaries;
		}
	}
}
