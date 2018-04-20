using System;
using System.Globalization;

namespace MediaServer.Extensions
{
    public static class DateTimeExtensions
    {
        const string DateTimeFormat = "dd.MM.yyyy";

        public static string GetDateString(this DateTime dateTime) 
            => dateTime.ToLocalTime().ToString(DateTimeFormat);

        public static DateTime GetDateTime(string dateTimeString)
            => DateTime.Parse(dateTimeString, new CultureInfo("no-NB"));
    }
}
