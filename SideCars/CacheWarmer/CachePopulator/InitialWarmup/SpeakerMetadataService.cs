using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CachePopulator.Configuration;
using CachePopulator.Model;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json.Linq;

namespace CachePopulator.InitialWarmup
{
	public class SpeakerMetadataService
    {      
		readonly CloudBlobClient cloudBlobClient;
		readonly IEnumerable<Conference> conferences;

		public SpeakerMetadataService(IBlogStorageConfig blobStorageConfig, IEnumerable<Conference> conferences)
        {
            var storageAccount = CloudStorageAccount.Parse(blobStorageConfig.BlobStorageConnectionString);
            cloudBlobClient = storageAccount.CreateCloudBlobClient();
			this.conferences = conferences;
        }
        
		public async Task<IEnumerable<string>> GetAllSpeakers()
		{
			var speakers = new HashSet<string>();
			foreach (var conference in conferences)
			{
				// TODO: Support more than 200 items
				var token = new BlobContinuationToken();
				var containerForConference = cloudBlobClient.GetContainerReference(conference.Id.ToLower());
				// TODO: Should not use implicit details, create shareable library?
				var blobs = await containerForConference.ListBlobsSegmentedAsync("dips.talk.", token);            
				foreach (var blob in blobs.Results.Cast<CloudBlockBlob>())
				{
					using (var memoryStream = new MemoryStream())
					{
						await blob.DownloadToStreamAsync(memoryStream);
						var talkContent = Encoding.UTF8.GetString(memoryStream.ToArray());
						var talk = JObject.Parse(talkContent);
						var speaker = (string)talk["Speaker"];
						speakers.Add(speaker);
					}
				}
			}

			return speakers;
		}      
    }
}
