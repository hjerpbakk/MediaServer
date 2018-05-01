using System;
using System.Net.Http;
using System.Threading.Tasks;
using CachePopulator.Model;
using Polly;

namespace CachePopulator.Services {
    public class FireAndForgetService {
        const string BaseUrl = "http://media-server:5000";
        const string ConferenceUrl = BaseUrl + "/Conference";

        readonly HttpClient httpClient;

        readonly string[] conferenceEndpoints;

        public FireAndForgetService(HttpClient httpClient) {
            this.httpClient = httpClient;
            // TODO: Add from config...
            // TODO: Add all speakers to warmup
            conferenceEndpoints = new[] {
                BaseUrl,
                $"{ConferenceUrl}/DevDays2018",
                $"{ConferenceUrl}/Interesting",
                $"{ConferenceUrl}/OptimusInteresting",
                $"{BaseUrl}/Speaker/Runar%20Ovesen%20Hjerpbakk"
            };
        }

        public async Task TouchEndpoints(TalkMetadata talkMetadata)
        {
            await TouchEndpointWithRetry(BaseUrl);
            var conferenceUrl = $"{ConferenceUrl}/{talkMetadata.Conference}";
            await TouchEndpointWithRetry(conferenceUrl);
            var speakerUrl = $"{BaseUrl}/Speaker/{talkMetadata.Speaker}";
            await TouchEndpointWithRetry(speakerUrl);
        }

        public async Task TouchEndpoints() {
            foreach (var endpoint in conferenceEndpoints) {
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
