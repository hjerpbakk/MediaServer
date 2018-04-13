namespace MediaServer.Models
{
    public struct Video
    {
        public const string SupportedVideoFileType = ".mp4";
        
        public Video(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
