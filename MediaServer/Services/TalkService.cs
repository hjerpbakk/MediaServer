using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaServer.Configuration;
using MediaServer.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace MediaServer.Services
{
	public class TalkService : ITalkService
    {
		const string DbFileExtension = ".txt";

		readonly static char[] dbFileExtension;

		readonly CloudBlobClient blobClient;

		static TalkService() {
			dbFileExtension = DbFileExtension.ToCharArray();
		}

		public TalkService(IBlogStorageConfig blobStorageConfig)
		{
			var storageAccount = CloudStorageAccount.Parse(blobStorageConfig.BlobStorageConnectionString);         
            blobClient = storageAccount.CreateCloudBlobClient();
		}

		public async Task<Talk[]> GetTalksFromConference(Conference conference) {
			var containerForConference = GetContainerFromConference(conference);
			if (!(await containerForConference.ExistsAsync())) {
				return new Talk[0];
			}

			// TODO: Support more than 200 items
            var token = new BlobContinuationToken();
			var blobs = await containerForConference.ListBlobsSegmentedAsync(token);

            var talks = new List<Talk>();
            foreach (var blob in blobs.Results.Cast<CloudBlockBlob>())
            {
                using (var memoryStream = new MemoryStream())
                {
                    await blob.DownloadToStreamAsync(memoryStream);
					var talkContent = Encoding.UTF8.GetString(memoryStream.ToArray());
                    var talk = JsonConvert.DeserializeObject<Talk>(talkContent);
					talks.Add(talk);
                }
            }         
           
			return talks.ToArray();
		}

		public async Task<Talk> GetTalkByName(Conference conference, string name)
		{
			var containerForConference = GetContainerFromConference(conference);

			var talkReferenceName = GetBlobNameFromTalkName(name);
			var blob = containerForConference.GetBlobReference(talkReferenceName);
            using (var memoryStream = new MemoryStream())
            {
				if (!(await blob.ExistsAsync())) {
					return null;
				}

                await blob.DownloadToStreamAsync(memoryStream);
				var talkContent = Encoding.UTF8.GetString(memoryStream.ToArray());
				var talk = JsonConvert.DeserializeObject<Talk>(talkContent);
				return talk;
            }
		}

		public async Task SaveTalkFromConference(Conference conference, Talk talk)
        {
			var containerForConference = GetContainerFromConference(conference);
			await containerForConference.CreateIfNotExistsAsync();
            
			var serializedTalk = JsonConvert.SerializeObject(talk);

			var talkReferenceName = GetBlobNameFromTalkName(talk.Name);
			var talkReference = containerForConference.GetBlockBlobReference(talkReferenceName);
			await talkReference.UploadTextAsync(serializedTalk);
        }

		public async Task DeleteTalkFromConference(Conference conference, Talk talk) {
			var containerForConference = GetContainerFromConference(conference);

			var talkReferenceName = GetBlobNameFromTalkName(talk.Name);
			var talkReference = containerForConference.GetBlockBlobReference(talkReferenceName);
			await talkReference.DeleteAsync();
		}

		public async Task<string[]> GetTalkNamesFromConference(Conference conference) {
			// TODO: Support more than 200 items
            var token = new BlobContinuationToken();
			var containerForConference = GetContainerFromConference(conference);
			var blobs = await containerForConference.ListBlobsSegmentedAsync(token);
			var talkNames = blobs.Results.Cast<CloudBlockBlob>().
				Select(b => b.Name.TrimEnd(dbFileExtension) + Video.SupportedVideoFileType).
				ToArray();

			return talkNames;
		}

        /// <summary>
        /// Gets the container from conference.
		/// 
		/// Ref: https://docs.microsoft.com/en-us/rest/api/storageservices/naming-and-referencing-containers--blobs--and-metadata
        /// </summary>
        /// <returns>The container from conference.</returns>
        /// <param name="conference">Conference.</param>
		CloudBlobContainer GetContainerFromConference(Conference conference) {
			var containerId = conference.Id.ToLower();
			return blobClient.GetContainerReference(containerId);
		}
		            
		string GetBlobNameFromTalkName(string talkName)
		    => talkName + DbFileExtension;
    }
}
