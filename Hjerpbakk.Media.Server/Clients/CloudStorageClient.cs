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

        public CloudStorageClient(IBlobStorageConfiguration blobStorageConfiguration, FileStorageClient fileStorageClient)
        {
            this.fileStorageClient = fileStorageClient ?? throw new ArgumentNullException(nameof(fileStorageClient));
            var storageAccount = CloudStorageAccount.Parse(blobStorageConfiguration.BlobStorageConnectionString);

            blobClient = storageAccount.CreateCloudBlobClient();

            interestingContainer = blobClient.GetContainerReference("talks");
            interestingContainer.CreateIfNotExistsAsync().GetAwaiter();
        }

        public async Task<IReadOnlyList<Video>> GetAvailableNewVideos(Conference conference)
        {
            var token = new BlobContinuationToken();
            var conferenceContainerName = GetConferenceContainerName(conference.Name);
            var conferenceContainer = blobClient.GetContainerReference(conferenceContainerName);
            await conferenceContainer.CreateIfNotExistsAsync();
            var blobList = await conferenceContainer.ListBlobsSegmentedAsync(token);
            var blobs = blobList.Results.Cast<CloudBlockBlob>().Select(b => b.Name).ToArray();
            var videos = fileStorageClient.GetVideosOnDisk(conference.Path).ToArray();
            var availableNewVideos = new List<Video>();
            foreach (var video in videos)
            {
                if (!blobs.Contains(video.Id))
                {
                    availableNewVideos.Add(video);
                }
            }

            return availableNewVideos;
        }

        public async Task Save(Talk hourOfInterest)
        {
            if (hourOfInterest == null || string.IsNullOrEmpty(hourOfInterest.Id))
            {
                throw new ArgumentException(nameof(hourOfInterest));
            }

            var hourString = JsonConvert.SerializeObject(hourOfInterest);
            var summary = new TalkSummary
            {
                Id = hourOfInterest.Id,
                Description = hourOfInterest.Description,
                Title = hourOfInterest.Title
            };
            var hourSummaryString = JsonConvert.SerializeObject(summary);

            var blobRef = interestingContainer.GetBlockBlobReference(hourOfInterest.Id);
            await blobRef.UploadTextAsync(hourString);

            var summaryContainer = blobClient.GetContainerReference(GetConferenceContainerName(hourOfInterest.ConferenceName));
            // TODO: Create skjer tre ganger i denne klassen. Må flyttes og gjøres en gang der config leses... Ellers er det bare å hente ref
            await summaryContainer.CreateIfNotExistsAsync();
            blobRef = summaryContainer.GetBlockBlobReference(summary.Id);
            await blobRef.UploadTextAsync(hourSummaryString);
        }

        public async Task<IReadOnlyList<TalkSummary>> GetTalks(HttpRequest request, Conference conference)
        {
            var token = new BlobContinuationToken();
            var conferenceContainerName = GetConferenceContainerName(conference.Name);
            var conferenceContainer = blobClient.GetContainerReference(conferenceContainerName);
            await conferenceContainer.CreateIfNotExistsAsync();
            var blobList = await conferenceContainer.ListBlobsSegmentedAsync(token);

            var hoursOfInterest = new List<TalkSummary>();
            foreach (var blob in blobList.Results.Cast<CloudBlockBlob>())
            {
                using (var memoryStream = new MemoryStream())
                {
                    await blob.DownloadToStreamAsync(memoryStream);
                    var hourOfInterest = JsonConvert.DeserializeObject<TalkSummary>(Encoding.UTF8.GetString(memoryStream.ToArray()));
                    hourOfInterest.URL = hourOfInterest.GetURL(request);
                    hoursOfInterest.Add(hourOfInterest);
                }
            }


            return hoursOfInterest;
        }

        public async Task<Talk> Get(TalkSummary summary)
        {
            if (summary == null || string.IsNullOrEmpty(summary.Id))
            {
                throw new ArgumentException(nameof(summary));
            }

            Talk hourOfInterest = null;
            var blob = interestingContainer.GetBlobReference(summary.Id);
            using (var memoryStream = new MemoryStream())
            {
                await blob.DownloadToStreamAsync(memoryStream);
                hourOfInterest = JsonConvert.DeserializeObject<Talk>(Encoding.UTF8.GetString(memoryStream.ToArray()));
            }

            if (hourOfInterest == null)
            {
                throw new ArgumentException(nameof(summary));
            }

            return hourOfInterest;
        }

        // TODO: Create own container for talk proper also pr conference 
        // TODO: Move and use more checks like https://docs.microsoft.com/en-us/rest/api/storageservices/naming-and-referencing-containers--blobs--and-metadata
        string GetConferenceContainerName(string conferenceName) => "conference-" + conferenceName.Replace(' ', '-').ToLower();
    }
}
