using System;
using System.Net.Http;
using MediaServer.Models;

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
                httpClient.GetAsync("http://cache-populator:1337/Populate");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cache populator crashed {ex}");
            }
        }
    }
}
