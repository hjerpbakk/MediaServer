using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CachePopulator.Clients;
using CachePopulator.Configuration;
using CachePopulator.Extensions;
using CachePopulator.Model;

namespace CachePopulator.InitialWarmup
{
    public class InitialWarmupService
    {
		readonly MediaServerConfig mediaServerConfig;
		readonly CareFreeHttpClient careFreeHttpClient;
        readonly IEnumerable<Conference> conferences;
		readonly IEnumerable<string> speakers;

		public InitialWarmupService(MediaServerConfig mediaServerConfig, CareFreeHttpClient careFreeHttpClient, IEnumerable<Conference> conferences, IEnumerable<string> speakers)
        {
			this.mediaServerConfig = mediaServerConfig;
            this.careFreeHttpClient = careFreeHttpClient;
            this.conferences = conferences;
			this.speakers = speakers;
        }

		public async Task TouchEndpoints()
		{
			await careFreeHttpClient.TouchEndpointWithRetry(mediaServerConfig.BaseUrl);

			var endpoints = new List<string>();
			foreach (var conference in conferences)
			{
				endpoints.Add($"{mediaServerConfig.ConferenceUrl}/{conference.Id}");
				endpoints.Add($"{mediaServerConfig.ConferenceUrl}/{conference.Id}/Save");
			}

			await TouchEndpoints(endpoints);         
			endpoints.Clear();

			await careFreeHttpClient.TouchEndpointWithRetry($"{mediaServerConfig.SpeakerUrl}/List");
			foreach (var speaker in speakers)
			{
				endpoints.Add($"{mediaServerConfig.SpeakerUrl}/{speaker}");
			}

			await TouchEndpoints(endpoints);
		}

		async Task TouchEndpoints(List<string> endpoints)
		{
			var tasks = endpoints.Select(careFreeHttpClient.TouchEndpointWithRetry).ToArray();
			var partitions = tasks.Partition(10);
			foreach (var partition in partitions)
			{
				await Task.WhenAll(partition);
			}
		}
	}
}
