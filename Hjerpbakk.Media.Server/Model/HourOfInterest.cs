using System;

namespace Hjerpbakk.Media.Server.Model
{
    public class HourOfInterest : HourOfInterestSummary
    {
        public string VideoURL { get; set; }
        public string SpeakerDeck { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Author { get; set; }
    }
}
