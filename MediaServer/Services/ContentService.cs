using System;
using System.Linq;
using System.IO;
using MediaServer.Models;
using Microsoft.AspNetCore.Hosting;
using System.Threading.Tasks;

namespace MediaServer.Services
{
    public class ContentService : IContentService
    {
        readonly string hostingPath;
        readonly ITalkService talkService;

        public ContentService(IHostingEnvironment hostingEnvironment, ITalkService talkService)
        {
            hostingPath = hostingEnvironment.WebRootPath;
            this.talkService = talkService;
        }

        public async Task<Video[]> GetVideosFromConference(string conferenceBasePath, Conference conference) {
            var path = Path.Combine(hostingPath, conferenceBasePath, conference.Id);
            var directory = new DirectoryInfo(path);
            if (!directory.Exists) {
                // TODO: Logg her at oppsett er feil
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
