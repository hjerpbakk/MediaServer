using System;
using System.IO;
using System.Threading.Tasks;
using MediaServer.Models;
using Microsoft.AspNetCore.Http;

namespace MediaServer.Services
{
    public interface ITalkService
    {
        Task<Talk[]> GetTalksFromConference(Conference conference);
        Task<Talk> GetTalkByName(Conference conference, string name);
        Task SaveTalkFromConference(Conference conference, Talk talk);
        Task DeleteTalkFromConference(Conference conference, Talk talk);
        Task<string[]> GetTalkNamesFromConference(Conference conference);     
    }
}
