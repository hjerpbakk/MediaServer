using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CachePopulator.Configuration;
using CachePopulator.Model;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace CachePopulator.InitialWarmup
{
	public class ConferenceMetaDataService
    {
        readonly CloudBlobClient cloudBlobClient;

        public ConferenceMetaDataService(IBlogStorageConfig blobStorageConfig)
        {
            var storageAccount = CloudStorageAccount.Parse(blobStorageConfig.BlobStorageConnectionString);
            cloudBlobClient = storageAccount.CreateCloudBlobClient();
        }
        
        public async Task<IEnumerable<Conference>> CreateConferenceConfig()
        {
            var containerForConference = cloudBlobClient.GetContainerReference("conferences"); ;
            await containerForConference.CreateIfNotExistsAsync();
            var conferences = new List<Conference>();
            var token = new BlobContinuationToken();
            var blobs = await containerForConference.ListBlobsSegmentedAsync(token);
            foreach (var blob in blobs.Results.Cast<CloudBlockBlob>())
            {
                using (var memoryStream = new MemoryStream())
                {
                    await blob.DownloadToStreamAsync(memoryStream);
                    var conferenceContent = Encoding.UTF8.GetString(memoryStream.ToArray());
                    var conference = JsonConvert.DeserializeObject<Conference>(conferenceContent);
                    conferences.Add(conference);
                }
            }
            
			return conferences;
        }
    }
}
