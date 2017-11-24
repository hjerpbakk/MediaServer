using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hjerpbakk.Media.Server.Configuration;
using Hjerpbakk.Media.Server.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace Hjerpbakk.Media.Server.Clients
{
    public class CloudStorageClient
    {
        readonly FileStorageClient fileStorageClient;

        readonly CloudBlobClient blobClient;

        readonly CloudBlobContainer interestingContainer;
        readonly CloudBlobContainer interestingSummaryContainer;

        public CloudStorageClient(IBlobStorageConfiguration blobStorageConfiguration, FileStorageClient fileStorageClient)
        {
            this.fileStorageClient = fileStorageClient ?? throw new ArgumentNullException(nameof(fileStorageClient));
            var storageAccount = CloudStorageAccount.Parse(blobStorageConfiguration.BlobStorageConnectionString);

            blobClient = storageAccount.CreateCloudBlobClient();

            interestingContainer = blobClient.GetContainerReference("interesting");
            interestingContainer.CreateIfNotExistsAsync().GetAwaiter();

            interestingSummaryContainer = blobClient.GetContainerReference("interesting-summary");
            interestingSummaryContainer.CreateIfNotExistsAsync().GetAwaiter();
        }

        public async Task<IReadOnlyList<Video>> GetAvailableNewVideos() {
            var token = new BlobContinuationToken();
            var blobList = await interestingSummaryContainer.ListBlobsSegmentedAsync(token);
            var blobs = blobList.Results.Cast<CloudBlockBlob>().Select(b => b.Name).ToArray();
            var videos = fileStorageClient.GetVideosOnDisk().ToArray();
            var availableNewVideos = new List<Video>();
            foreach (var video in videos)
            {
                if (!blobs.Contains(video.Id)) {
                    availableNewVideos.Add(video);
                }
            }

            return availableNewVideos;
        }

        public async Task Save(HourOfInterest hourOfInterest) {
            if (hourOfInterest == null || string.IsNullOrEmpty(hourOfInterest.Id))
            {
                throw new ArgumentException(nameof(hourOfInterest));
            }

            var hourString = JsonConvert.SerializeObject(hourOfInterest);
            var summary = new HourOfInterestSummary
            {
                Id = hourOfInterest.Id,
                Description = hourOfInterest.Description,
                Title = hourOfInterest.Title
            };
            var hourSummaryString = JsonConvert.SerializeObject(summary);

            var blobRef = interestingContainer.GetBlockBlobReference(hourOfInterest.Id);
            await blobRef.UploadTextAsync(hourString);    

            blobRef = interestingSummaryContainer.GetBlockBlobReference(summary.Id);
            await blobRef.UploadTextAsync(hourSummaryString);    
        }

        public async Task<IReadOnlyList<HourOfInterestSummary>> GetHoursOfInterest(HttpRequest request) {
            var token = new BlobContinuationToken();
            var blobList = await interestingSummaryContainer.ListBlobsSegmentedAsync(token);

            var hoursOfInterest = new List<HourOfInterestSummary>();
            foreach (var blob in blobList.Results.Cast<CloudBlockBlob>())
            {
                using (var memoryStream = new MemoryStream())
                {
                    await blob.DownloadToStreamAsync(memoryStream);
                    var hourOfInterest = JsonConvert.DeserializeObject<HourOfInterestSummary>(Encoding.UTF8.GetString(memoryStream.ToArray()));
                    hourOfInterest.URL = hourOfInterest.GetURL(request);
                    hoursOfInterest.Add(hourOfInterest);
                }
            }


            return hoursOfInterest;
        }

        public async Task<HourOfInterest> Get(HourOfInterestSummary summary) {
            if (summary == null || string.IsNullOrEmpty(summary.Id)) {
                throw new ArgumentException(nameof(summary));
            }

            HourOfInterest hourOfInterest = null;
            var blob = interestingContainer.GetBlobReference(summary.Id);
            using (var memoryStream = new MemoryStream())
            {
                await blob.DownloadToStreamAsync(memoryStream);
                hourOfInterest = JsonConvert.DeserializeObject<HourOfInterest>(Encoding.UTF8.GetString(memoryStream.ToArray()));
            }

            if (hourOfInterest == null) {
                throw new ArgumentException(nameof(summary));
            }

            return hourOfInterest;
        }
    }
}
