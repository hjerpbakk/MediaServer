using System;
namespace Hjerpbakk.Media.Server.Configuration
{
    public interface IBlobStorageConfiguration
    {
        string BlobStorageConnectionString { get; }
    }
}
