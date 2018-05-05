using System;
using System.Net.Http;
using System.Threading.Tasks;
using Polly;

namespace CachePopulator.Clients
{
    public class CareFreeHttpClient
    {
		readonly HttpClient httpClient;

		public CareFreeHttpClient(HttpClient httpClient)
        {
			this.httpClient = httpClient;
        }

		public async Task TouchEndpointWithRetry(string endpoint)
        {
            var policyResult = await Policy
                        .Handle<Exception>()
                        .WaitAndRetryAsync(new[] {
                            TimeSpan.FromSeconds(1),
                            TimeSpan.FromSeconds(2),
                            TimeSpan.FromSeconds(3),
                            TimeSpan.FromSeconds(4)
                        }).ExecuteAndCaptureAsync(() => TouchEndpoint(endpoint));

            if (policyResult.FinalException != null)
            {
                Console.WriteLine($"Call failed to {endpoint} {policyResult.FinalException}");
            }
        }

        async Task TouchEndpoint(string endpoint)
        {
            Console.WriteLine($"Calling {endpoint}");
            await httpClient.GetAsync(endpoint);
        }
    }
}
