using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CachePopulator.Model;
using Polly;

namespace CachePopulator.Services {
    public class FireAndForgetService {
		// TODO: Add from config...
        const string BaseUrl = "http://media-server:5000";
        const string ConferenceUrl = BaseUrl + "/Conference";

        readonly HttpClient httpClient;
		IEnumerable<Conference> conferences;

        public FireAndForgetService(HttpClient httpClient, IEnumerable<Conference> conferences) {
            this.httpClient = httpClient;
			this.conferences = conferences;
            // TODO: Add all speakers to warmup
        }

        public async Task TouchEndpoints(TalkMetadata talkMetadata)
        {
			var endpoints = new[] {
				BaseUrl,
				$"{ConferenceUrl}/{talkMetadata.Conference}",
				$"{BaseUrl}/Speaker/{talkMetadata.Speaker}"
			};

			foreach (var endpoint in endpoints) {
                await TouchEndpointWithRetry(endpoint);
            }
        }

        public async Task TouchEndpoints() {
			var endpoints = new List<string> { BaseUrl };
            foreach (var conference in conferences) {
				endpoints.Add($"{ConferenceUrl}/{conference.Id}");
				endpoints.Add($"{ConferenceUrl}/{conference.Id}/Save");
            }

			endpoints.Add($"{BaseUrl}/Speaker/Runar%20Ovesen%20Hjerpbakk");


			foreach (var endpoint in endpoints) {
                await TouchEndpointWithRetry(endpoint);
            }
        }

        async Task TouchEndpointWithRetry(string endpoint) {
            var policyResult = await Policy
                        .Handle<Exception>()
                        .WaitAndRetryAsync(new[] {
                            TimeSpan.FromSeconds(1),
                            TimeSpan.FromSeconds(2),
                            TimeSpan.FromSeconds(3),
                            TimeSpan.FromSeconds(4)
                        }).ExecuteAndCaptureAsync(() => TouchEndpoint(endpoint));

            if (policyResult.FinalException != null) {
                Console.WriteLine($"Call failed to {endpoint} {policyResult.FinalException}");    
            }
        }

        async Task TouchEndpoint(string endpoint) {
            Console.WriteLine($"Calling {endpoint}");
            await httpClient.GetAsync(endpoint);
        }
    }
}
