using System;
using System.Linq;
using System.IO;
using MediaServer.Models;
using Microsoft.AspNetCore.Hosting;
using System.Threading.Tasks;

namespace MediaServer.Services
{
    public class ContentService
    {
        readonly string hostingPath;
        readonly IOldTalkService talkService;

        public ContentService(IHostingEnvironment hostingEnvironment, IOldTalkService talkService)
        {
            hostingPath = hostingEnvironment.WebRootPath;
            this.talkService = talkService;
        }

        // TODO: Add method to verify that slides exist on disk. Prepopulate in JS

        public async Task<Video[]> GetVideosFromConference(string conferenceBasePath, Conference conference) {
            var path = Path.Combine(hostingPath, conferenceBasePath, conference.Id);
            var directory = new DirectoryInfo(path);
            if (!directory.Exists) {
                Console.WriteLine($"Could not find directory for {conference.Id}. Server setup is wrong.");
                return new Video[0];
            }

            var talkNames = await talkService.GetTalkNamesFromConference(conference);
            var candidateFiles = directory.EnumerateFiles($"*{Video.SupportedVideoFileType}");
            var availableVideos = candidateFiles
                .Where(f => !talkNames.Contains(f.Name))
                .Select(f => new Video(f.Name));

            return availableVideos.ToArray();
        }
    }
}
