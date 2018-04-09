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
        readonly string videoPath;

        public FileStorageClient(Paths paths)
        {
            if (paths == null) {
                throw new ArgumentNullException(nameof(paths));
            }

            videoPath = Path.Combine(paths.WebRootPath, "videos");
        }

        public IEnumerable<Video> GetVideosOnDisk(string conferencePath) {
            var path = Path.Combine(videoPath, conferencePath);
            var directory = new DirectoryInfo(path);
            return directory.EnumerateFiles($"*{Video.SupportedFileType}").Select(f => new Video(f.Name));
        } 
    }
}
