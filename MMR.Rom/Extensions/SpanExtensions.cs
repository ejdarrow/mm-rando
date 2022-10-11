using System;

namespace MMR.Rom.Extensions
{
    public static class SpanExtensions
    {
        public static Span<T> Slice<T>(this Span<T> span, ValueRange range)
        {
            return span.Slice(checked((int)range.Start), checked((int)range.Length));
        }
    }
}
