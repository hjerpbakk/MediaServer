using Newtonsoft.Json;

namespace MediaServer.Clients
{
    public struct CacheMetadata
    {
        [JsonConstructor]
        public CacheMetadata(string conference, string speaker)
        {
            Conference = conference;
            Speaker = speaker;
        }

        public string Conference { get; }
        public string Speaker { get; }
    }
}
