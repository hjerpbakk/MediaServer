namespace MediaServer.Models {
	public class TalkSummary : TalkModel {      
        public TalkSummary(Talk talk, string url, string thumbnail)
			: base(talk) {
			Url = url;
			Thumbnail = thumbnail;
        }

        public string Url { get; }
        public string Thumbnail { get; }      
    }
}
