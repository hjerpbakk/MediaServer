using System;
using System.Threading.Tasks;
using MediaServer.Models;

namespace MediaServer.Services
{
    public interface IContentService
    {
        Task<Video[]> GetVideosFromConference(string conferenceBasePath, Conference conference);
    }
}
