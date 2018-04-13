using System;
namespace MediaServer.Extensions
{
    public static class DateTimeExtensions
    {
        public static string GetDateString(this DateTime dateTime) 
            => dateTime.ToLocalTime().ToString("dd.MM.yyyy");
    }
}
