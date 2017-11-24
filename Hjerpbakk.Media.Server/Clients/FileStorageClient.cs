using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hjerpbakk.Media.Server.Configuration;
using Hjerpbakk.Media.Server.Model;

namespace Hjerpbakk.Media.Server.Clients
{
    public class FileStorageClient
    {
        readonly DirectoryInfo videos;

        public FileStorageClient(Paths paths)
        {
            if (paths == null) {
                throw new ArgumentNullException(nameof(paths));
            }

            var videoPath = Path.Combine(paths.WebRootPath, "videos");
            videos = new DirectoryInfo(videoPath);
        }

        public IEnumerable<Video> GetVideosOnDisk() => videos.EnumerateFiles($"*{Video.SupportedFileType}").Select(f => new Video(f.Name));
    }
}
