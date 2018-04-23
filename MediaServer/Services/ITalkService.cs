using System.Collections.Generic;
using System.Threading.Tasks;
using MediaServer.Models;

namespace MediaServer.Services
{
    public interface ITalkService
    {
        Task<IReadOnlyList<Talk>> GetTalksFromConference(Conference conference);
        Task<Talk> GetTalkByName(Conference conference, string name);
        Task SaveTalkFromConference(Conference conference, Talk talk);
        Task DeleteTalkFromConference(Conference conference, Talk talk);
        Task<IReadOnlyList<string>> GetTalkNamesFromConference(Conference conference);
        Task<Image> GetTalkThumbnail(Conference conference, string name);
    }
}
