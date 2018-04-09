﻿using System;

namespace Hjerpbakk.Media.Server.Model
{
    public class HourOfInterest : HourOfInterestSummary
    {
        public const string SpeakerDeckFileType = ".pdf";

        public string VideoURL { get; set; }
        public string SpeakerDeckURL { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Author { get; set; }
    }
}
