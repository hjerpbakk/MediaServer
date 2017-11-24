using System;
using Microsoft.AspNetCore.Hosting;

namespace Hjerpbakk.Media.Server.Configuration
{
    public class Paths
    {
        public Paths(IHostingEnvironment hostingEnvironment)
        {
            WebRootPath = hostingEnvironment.WebRootPath;
        }

        public string WebRootPath { get; }
    }
}
