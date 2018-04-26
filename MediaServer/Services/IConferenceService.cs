using System.Collections.Generic;
using System.Threading.Tasks;
using MediaServer.Models;
using Microsoft.AspNetCore.Http;

namespace MediaServer.Services
{
	public interface IConferenceService
    {
		Task<IEnumerable<TalkSummary>> GetLatestTalks(HttpContext httpContext);
		Task<IEnumerable<TalkSummary>> GetTalksForConference(Conference conference, HttpContext httpContext);
    }
}
