using System;

namespace MediaServer.Models {
    public class LatestTalk {
		// TODO: This class should be unescesary
        public LatestTalk(Conference conference, Talk talk, DateTimeOffset timeStamp) {
            Conference = conference;
            Talk = talk;
            TimeStamp = timeStamp;
        }

		public LatestTalk(Conference conference, Talk talk)
        {
            Conference = conference;
            Talk = talk;
        }

        public Conference Conference { get; }
        public Talk Talk { get; }
        public DateTimeOffset TimeStamp { get; }
    }
}
