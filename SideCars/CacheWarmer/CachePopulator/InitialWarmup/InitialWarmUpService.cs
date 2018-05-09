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
            var endpoints = new List<string> { mediaServerConfig.BaseUrl };
            foreach (var conference in conferences)
            {
                endpoints.Add($"{mediaServerConfig.ConferenceUrl}/{conference.Id}");
                endpoints.Add($"{mediaServerConfig.ConferenceUrl}/{conference.Id}/Save");
            }

            // TODO: too aggressive, wait for conferences to finish before fetching pr. speaker
			foreach (var speaker in speakers)
			{
				endpoints.Add($"{mediaServerConfig.SpeakerUrl}/{speaker}");            
			}

			var tasks = endpoints.Select(careFreeHttpClient.TouchEndpointWithRetry).ToArray();
			var partitions = tasks.Partition(10);
			foreach (var partition in partitions)
            {
				await Task.WhenAll(partition);
            }
        }
    }
}
