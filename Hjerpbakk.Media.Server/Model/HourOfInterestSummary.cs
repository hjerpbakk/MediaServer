using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Hjerpbakk.Media.Server.Model
{
    public class HourOfInterestSummary
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        [JsonIgnore]
        public string URL { get; set; }

        public string GetURL(HttpRequest request) =>
            request.Scheme + "://" + request.Host + "/hour/" + Id;
    }
}
