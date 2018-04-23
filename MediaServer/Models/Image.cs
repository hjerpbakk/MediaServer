namespace MediaServer.Models
{
    public struct Image
    {
        public Image(string contentType, byte[] imageData)
        {
            ContentType = contentType;
            ImageData = imageData;
        }

        public string ContentType { get; }
        public byte[] ImageData { get; }
    }
}
