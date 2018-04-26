using MediaServer.Models;
using Microsoft.WindowsAzure.Storage.Blob;

namespace MediaServer.Extensions
{
    public static class CloudBlobClientExtensions
    {
		public static CloudBlobContainer GetContainerForConference(this CloudBlobClient cloudBlobClient, Conference conference) {
			var containerId = conference.Id.ToLower();
			return cloudBlobClient.GetContainerReference(containerId);
		}
    }
}
