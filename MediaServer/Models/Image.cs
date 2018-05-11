using System;

namespace MediaServer.Models
{
	public struct Image : IEquatable<Image>
    {
        public Image(string contentType, byte[] imageData)
        {
            ContentType = contentType;
            ImageData = imageData;
        }

        public string ContentType { get; }
        public byte[] ImageData { get; }

		public bool Equals(Image other) => ContentType == other.ContentType && ImageData == other.ImageData;
		public override bool Equals(object obj) => obj is Image && Equals((Image)obj);          
		public override int GetHashCode() => ContentType.GetHashCode() ^ ImageData.GetHashCode();   
    }
}
