using System;
using MediaServer.Models;

namespace MediaServer.Services
{
    public class DummyContentService : IContentService
    {
        public DummyContentService()
        {
        }

        public Video[] GetVideosFromConference(Conference conference) {
            return new[] { new Video("Gudd talk.mp4"), new Video("IoC-container i Arena-plugin.mp4") };
        }
    }
}
