using System.Linq;
using System.Threading.Tasks;
using CachePopulator.Clients;
using CachePopulator.Configuration;
using CachePopulator.Model;

namespace CachePopulator.Services
{
    public class ContinuousWarmupService
    {
		readonly MediaServerConfig mediaServerConfig;
        readonly CareFreeHttpClient careFreeHttpClient;
        
		public ContinuousWarmupService(MediaServerConfig mediaServerConfig, CareFreeHttpClient careFreeHttpClient)
        {
            this.mediaServerConfig = mediaServerConfig;
            this.careFreeHttpClient = careFreeHttpClient;
        }

		public async Task TouchEndpoints(TalkMetadata talkMetadata)
        {
            var endpoints = new[] {
                mediaServerConfig.BaseUrl,
                $"{mediaServerConfig.ConferenceUrl}/{talkMetadata.Conference}",
                $"{mediaServerConfig.SpeakerUrl}/{talkMetadata.Speaker}",
				$"{mediaServerConfig.SpeakerUrl}/List"
            };
            
			foreach (var endpoint in endpoints)
			{
				await careFreeHttpClient.TouchEndpointWithRetry(endpoint);
			}
        }
    }
}
