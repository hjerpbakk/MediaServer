using System.Collections.Generic;
using System.Threading.Tasks;
using MediaServer.Models;

namespace MediaServer.Services
{
    public interface ITalkService
    {
		// TODO: Move to IConferenceService
		Task<IEnumerable<Talk>> GetTalksFromConference(Conference conference);
        Task<Talk> GetTalkByName(Conference conference, string name);
        Task SaveTalkFromConference(Conference conference, Talk talk);
        Task DeleteTalkFromConference(Conference conference, Talk talk);
		// TODO: Move to IConferenceService
		Task<IReadOnlyList<string>> GetTalkNamesFromConference(Conference conference);
        Task<Image> GetTalkThumbnail(Conference conference, string name);
		// TODO: Move to IConferenceService
        Task<IEnumerable<LatestTalk>> GetLatestTalks(IEnumerable<Conference> conferences);
    }
}
