using MediaServer.Models;
using Microsoft.WindowsAzure.Storage.Blob;

namespace MediaServer.Extensions
{
	// TODO: delete this
    public static class CloudBlobClientExtensions
    {
		public static CloudBlobContainer GetContainerForConference(this CloudBlobClient cloudBlobClient, Conference conference) {
			var containerId = conference.Id.ToLower();
			return cloudBlobClient.GetContainerReference(containerId);
		}

		public static CloudBlobContainer GetContainerForTalk(this CloudBlobClient cloudBlobClient, Talk talk)
        {
			var containerId = talk.ConferenceId.ToLower();
            return cloudBlobClient.GetContainerReference(containerId);
        }
    }
}
