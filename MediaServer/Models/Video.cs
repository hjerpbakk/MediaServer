namespace MediaServer.Models
{
    public struct Video
    {
        public Video(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
