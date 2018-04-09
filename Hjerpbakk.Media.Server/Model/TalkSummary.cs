using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Hjerpbakk.Media.Server.Model
{
    public class TalkSummary
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        [JsonIgnore]
        public string URL { get; set; }
        public string ConferenceName { get; set; }

        // TODO: Must be agnostic regarding hosting environment
        public string GetURL(HttpRequest request) =>
            request.Scheme + "://" + request.Host + "/hour/" + Id;
    }
}
