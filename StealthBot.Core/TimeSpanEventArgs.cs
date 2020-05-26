using System;

namespace StealthBot.Core
{
    public class TimeSpanEventArgs : EventArgs
    {
        public TimeSpan Span { get; set; }

        public TimeSpanEventArgs(TimeSpan span)
        {
            Span = span;
        }
    }
}
