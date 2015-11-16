using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace System
{

    public static class DateTimeExtensions
    {

        private static string Iso8601DateFormat = "yyyy-MM-dd'T'HH:mm:ss'Z'";

        public static string ToIsoUtcString(this DateTime d)
        {
            // DateTime.Now.ToUniversalTime();
            // DateTime.UtcNow.ToLocalTime();
            return d.ToUniversalTime().ToString(Iso8601DateFormat, CultureInfo.InvariantCulture);
        }

        public static bool FromIsoUtcString(string s, out DateTime result)
        {
            // "2010-08-20T15:00:00Z"
            //TODO: check if this results in (result.Kind == DateTimeKind.Utc)
            return DateTime.TryParseExact(s, @"yyyy-MM-dd\THH:mm:ss\Z", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out result);
        }

        public static DateTime GetYearMonth(this DateTime d)
        {
            return new DateTime(d.Year, d.Month, 1);
        }

        public static DateTime FirstDayOfMonth(this DateTime d)
        {
            return new DateTime(d.Year, d.Month, 1);
        }

        public static DateTime LastDayOfMonth(this DateTime d)
        {
            return new DateTime(d.Year, d.Month, DateTime.DaysInMonth(d.Year, d.Month));
        }

        public static DateTime FirstDayOfYear(this DateTime d)
        {
            return new DateTime(d.Year, 1, 1);
        }

        public static DateTime LastDayOfYear(this DateTime d)
        {
            return new DateTime(d.Year, 12, DateTime.DaysInMonth(d.Year, d.Month));
        }

        public static bool SameMonth(this DateTime thisDate, DateTime other)
        {
            return SameMonth(thisDate, other.Year, other.Month);
        }

        public static bool SameMonth(this DateTime thisDate, int year, int month)
        {
            return (thisDate.Year == year) && (thisDate.Month == month);
        }

        public static bool SameDay(this DateTime thisDate, DateTime other)
        {
            return (thisDate.Year == other.Year) && (thisDate.Month == other.Month) && (thisDate.Day == other.Day);
        }
    }
}
