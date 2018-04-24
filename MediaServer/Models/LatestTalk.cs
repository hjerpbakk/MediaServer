using System;

namespace MediaServer.Models {
    public class LatestTalk {
        public LatestTalk(Conference conference, Talk talk, DateTimeOffset timeStamp) {
            Conference = conference;
            Talk = talk;
            TimeStamp = timeStamp;
        }

        public Conference Conference { get; }
        public Talk Talk { get; }
        public DateTimeOffset TimeStamp { get; }
    }
}
