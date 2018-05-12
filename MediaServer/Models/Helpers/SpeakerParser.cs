using System.Linq;

namespace MediaServer.Models.Helpers {
	public static class SpeakerParser {
		public static string[] GetSpeakers(string speaker)
		    => speaker.Split(',').Select(n => n.Trim()).ToArray();
    }
}
