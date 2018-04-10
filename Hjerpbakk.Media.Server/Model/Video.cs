using System;

namespace Hjerpbakk.Media.Server.Model
{
    public struct Video
    {
        public const string SupportedFileType = ".mp4";

        public Video(string name)
        {
            Name = name.TrimEnd(SupportedFileType.ToCharArray());
            Id = Uri.EscapeUriString(name).TrimEnd(SupportedFileType.ToCharArray());
        }

        // TODO: Må ha full sti også til video
        public string Id { get; }
        public string Name { get; }

        public override string ToString() => string.Format("[Video: Id={0}, Name={1}]", Id, Name);
    }
}
