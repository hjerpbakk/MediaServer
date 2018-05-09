using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaServer.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace MediaServer.Configuration
{
	public class ConferenceConfiguration {
		readonly CloudBlobClient cloudBlobClient;

		public ConferenceConfiguration(IBlogStorageConfig blobStorageConfig) {
			var storageAccount = CloudStorageAccount.Parse(blobStorageConfig.BlobStorageConnectionString);
            cloudBlobClient = storageAccount.CreateCloudBlobClient();
        }
        
		public async Task<Dictionary<string, Conference>> GetConferences() {
            var conferenceConfigContainer = cloudBlobClient.GetContainerReference("conferences"); ;
            await conferenceConfigContainer.CreateIfNotExistsAsync();

            var conferences = new List<Conference>();
            BlobContinuationToken blobContinuationToken = null;
            do {
                var results = await conferenceConfigContainer.ListBlobsSegmentedAsync(blobContinuationToken);
                blobContinuationToken = results.ContinuationToken;
                foreach (var cloudBlob in results.Results.Cast<CloudBlockBlob>()) {
                    using (var memoryStream = new MemoryStream()) {
                        await cloudBlob.DownloadToStreamAsync(memoryStream);
                        var conferenceContent = Encoding.UTF8.GetString(memoryStream.ToArray());
                        var conference = JsonConvert.DeserializeObject<Conference>(conferenceContent);
                        conferences.Add(conference);
                    }
                }
            } while (blobContinuationToken != null);

            return conferences.ToDictionary(conf => conf.Id); ;
        }
    }
}
