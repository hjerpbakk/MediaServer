using System;
using MediaServer.Models;

namespace MediaServer.Services
{
    public interface IContentService
    {
        Video[] GetVideosFromConference(Conference conference);
    }
}
