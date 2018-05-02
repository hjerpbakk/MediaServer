using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaServer.Configuration;
using MediaServer.Extensions;
using MediaServer.Models;
using MediaServer.Services.Cache;
using MediaServer.Services.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace MediaServer.Services
{
	public class ConferenceService
    {
		readonly IEnumerable<Conference> conferences;
		readonly CloudBlobClient cloudBlobClient;
		readonly IOldTalkService talkService;
		readonly ThumbnailService thumbnailService;

		public ConferenceService(ConferenceConfig conferenceConfig, IBlogStorageConfig blobStorageConfig, IOldTalkService talkService, ThumbnailService thumbnailService)
        {
			conferences = conferenceConfig.Conferences.Values;
			var storageAccount = CloudStorageAccount.Parse(blobStorageConfig.BlobStorageConnectionString);
            cloudBlobClient = storageAccount.CreateCloudBlobClient();
			this.talkService = talkService;
			this.thumbnailService = thumbnailService;
        }

		public async Task<IEnumerable<TalkSummary>> GetLatestTalks()
		{
            // TODO: Move implementation of this part of talkservice here
			var talks = await talkService.GetLatestTalks(conferences);
			var orderedSummaries = await Task.WhenAll(talks.Select(t => CreateTalkSummary(t.Conference, t.Talk)));
			return orderedSummaries;
		}

		public async Task<IEnumerable<TalkSummary>> GetTalksForConference(Conference conference, HttpContext httpContext)
		{
			// TODO: Move implementation of this part of talkservice here
			var talks = await talkService.GetTalksFromConference(conference);
			var orderedSummaries = await Task.WhenAll(talks.Select(talk => CreateTalkSummary(conference, talk)));
			return orderedSummaries;
		}

		public async Task<IEnumerable<TalkSummary>> GetTalksBySpeaker(string speakerName) {
			var talks = new List<LatestTalk>();
            foreach (var conference in conferences)
            {
                var containerForConference = cloudBlobClient.GetContainerForConference(conference);
                await containerForConference.CreateIfNotExistsAsync();
                // TODO: Support more than 200 items....
                var token = new BlobContinuationToken();
				var blobs = await containerForConference.ListBlobsSegmentedAsync(BlobStoragePersistence.TalkPrefix, token);
                foreach (var blob in blobs.Results.Cast<CloudBlockBlob>())
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await blob.DownloadToStreamAsync(memoryStream);
                        var talkContent = Encoding.UTF8.GetString(memoryStream.ToArray());
                        var talk = JsonConvert.DeserializeObject<Talk>(talkContent);
						talk.ConferenceId = conference.Id;
						if (talk.Speaker == speakerName) {
							var latestTalk = new LatestTalk(conference, talk);
                            talks.Add(latestTalk);
						}                  
                    }
                }
            }

			var orderedSummaries = await Task.WhenAll(talks
				.OrderByDescending(t => t.Talk.DateOfTalk)
                .Select(t => CreateTalkSummary(t.Conference, t.Talk)));
			return orderedSummaries;
		}

		async Task<TalkSummary> CreateTalkSummary(Conference conference, Talk talk) {
			var url = Paths.GetTalkUrl(conference, talk);
			var thumbnail = await thumbnailService.GetThumbnailUrl(conference, talk);
			return new TalkSummary(talk, url, thumbnail);
		}
	}
}
