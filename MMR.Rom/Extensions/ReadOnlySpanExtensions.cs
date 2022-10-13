using System;

namespace MMR.Rom.Extensions
{
    public static class ReadOnlySpanExtensions
    {
        public static ReadOnlySpan<T> Slice<T>(this ReadOnlySpan<T> span, ValueRange range)
        {
            return span.Slice(checked((int)range.Start), checked((int)range.Length));
        }
    }
}
