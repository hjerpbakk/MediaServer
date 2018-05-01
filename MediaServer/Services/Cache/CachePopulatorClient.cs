using System;
using System.Net.Http;
using System.Text;
using MediaServer.Models;
using Newtonsoft.Json;

namespace MediaServer.Services.Cache
{
    public class CachePopulatorClient
    {
        readonly HttpClient httpClient;

        public CachePopulatorClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public void RePopulateCaches(Talk talk) {
            try
            {
                var talkMetadata = new TalkMetadata(talk.ConferenceId, talk.Speaker);
                var metadataAsJson = JsonConvert.SerializeObject(talkMetadata);
                var content = new StringContent(metadataAsJson, Encoding.UTF8, "application/json");
                httpClient.PostAsync("http://cache-populator:1337/Populate", content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cache populator crashed {ex}");
            }
        }
    }
}
