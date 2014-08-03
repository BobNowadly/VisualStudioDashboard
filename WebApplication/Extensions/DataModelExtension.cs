using System;

namespace WebApplication.Extensions
{
    public static class DataModelExtension
    {
        public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Converts a DateTime to its Unix timestamp value. This is the number of seconds
        /// passed since the Unix Epoch (1/1/1970 UTC)
        /// </summary>
        /// <param name="aDate">DateTime to convert</param>
        /// <returns>Number of milliseconds passed since 1/1/1970 UTC </returns>
        public static long ToInt(this DateTime aDate)
        {
            if (aDate == DateTime.MinValue)
            {
                return -1;
            }
            TimeSpan span = (aDate - UnixEpoch);
            return (long) Math.Floor(span.TotalMilliseconds);
        }

        /// <summary>
        /// Converts the specified 32 bit integer to a DateTime based on the number of seconds
        /// since the Unix epoch (1/1/1970 UTC)
        /// </summary>
        /// <param name="anInt">Integer value to convert</param>
        /// <returns>DateTime for the Unix int time value</returns>
        public static DateTime ToDateTime(this int anInt)
        {
            if (anInt == -1)
            {
                return DateTime.MinValue;
            }
            return UnixEpoch.AddSeconds(anInt);
        }
    }
}