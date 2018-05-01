using Newtonsoft.Json;

namespace MediaServer.Models
{
    public struct TalkMetadata
    {
        [JsonConstructor]
        public TalkMetadata(string conference, string speaker)
        {
            Conference = conference;
            Speaker = speaker;
        }

        public string Conference { get; }
        public string Speaker { get; }
    }
}
