using System;

namespace Analysis
{
    /// <summary>
    /// http://stackoverflow.com/a/1393726
    /// </summary>
    public static class DateExtensions
    {
        public static DateTimeOffset Round(this DateTimeOffset date, TimeSpan span)
        {
            long ticks = (date.Ticks + (span.Ticks / 2) + 1) / span.Ticks;
            return new DateTimeOffset(ticks * span.Ticks, date.Offset);
        }
        public static DateTimeOffset Floor(this DateTimeOffset date, TimeSpan span)
        {
            long ticks = (date.Ticks / span.Ticks);
            return new DateTimeOffset(ticks * span.Ticks, date.Offset);
        }
        public static DateTimeOffset Ceil(this DateTimeOffset date, TimeSpan span)
        {
            long ticks = (date.Ticks + span.Ticks - 1) / span.Ticks;
            return new DateTimeOffset(ticks * span.Ticks, date.Offset);
        }
    }
}
