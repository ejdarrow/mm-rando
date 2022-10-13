using System;
using System.Buffers.Binary;

namespace MMR.Rom
{
    /// <summary>
    /// Range type with underlying <see cref="uint"/> values.
    /// </summary>
    public readonly struct ValueRange
    {
        /// <summary>
        /// Type used to match private constructor which does not perform validation.
        /// </summary>
        /// <remarks>This should be optimized out by JIT stage.</remarks>
        private readonly ref struct NotValidated { }

        /// <summary>
        /// Range start value.
        /// </summary>
        public readonly uint Start;

        /// <summary>
        /// Range end value.
        /// </summary>
        public readonly uint End;

        /// <summary>
        /// Get the length of this range.
        /// </summary>
        public readonly uint Length => End - Start;

        /// <summary>
        /// Construct with start and end values.
        /// </summary>
        /// <param name="start">Range start value</param>
        /// <param name="end">Range end value</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public ValueRange(uint start, uint end)
        {
            ValidateStartEndOrdering(start, end);
            Start = start;
            End = end;
        }

        /// <summary>
        /// Construct with non-validated start and end values.
        /// </summary>
        /// <param name="start">Range start value</param>
        /// <param name="end">Range end value</param>
        /// <param name="_"></param>
        private ValueRange(uint start, uint end, NotValidated _)
        {
            Start = start;
            End = end;
        }

        /// <summary>
        /// Create a <see cref="ValueRange"/> with a given start value and length.
        /// </summary>
        /// <param name="start">Range start value</param>
        /// <param name="length">Range length</param>
        /// <returns></returns>
        public static ValueRange WithLength(uint start, uint length)
        {
            // Use constructor without validation because checked() addition already serves as such.
            return new ValueRange(start, checked(start + length), new NotValidated());
        }

        /// <summary>
        /// Check whether or not a given value is within this range.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public readonly bool Contains(uint value)
        {
            return Start <= value && value < End;
        }

        /// <summary>
        /// Check whether or not the given <see cref="ValueRange"/> is a subset or equivalent to this <see cref="ValueRange"/>.
        /// </summary>
        /// <param name="subset">Subset range</param>
        /// <returns></returns>
        public readonly bool Contains(ValueRange subset)
        {
            return Start <= subset.Start && subset.End <= End;
        }

        /// <summary>
        /// Get the offset of a given subset value relative to this <see cref="ValueRange"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public uint OffsetOf(uint value)
        {
            if (!Contains(value))
            {
                throw new ArgumentOutOfRangeException("value");
            }
            return value - Start;
        }

        /// <summary>
        /// Get the offset of a given subset <see cref="ValueRange"/> relative to this <see cref="ValueRange"/>.
        /// </summary>
        /// <param name="subset"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public uint OffsetOf(ValueRange subset)
        {
            if (!Contains(subset))
            {
                throw new ArgumentOutOfRangeException("subset");
            }
            return subset.Start - Start;
        }

        /// <summary>
        /// Read a <see cref="ValueRange"/> from a <see cref="ReadOnlySpan{T}"/> containing big-endian integers.
        /// </summary>
        /// <param name="span"></param>
        /// <returns></returns>
        public static ValueRange Read(ReadOnlySpan<byte> span)
        {
            var start = BinaryPrimitives.ReadUInt32BigEndian(span);
            var end = BinaryPrimitives.ReadUInt32BigEndian(span.Slice(4));
            return new ValueRange(start, end);
        }

        /// <summary>
        /// Write this <see cref="ValueRange"/> to a <see cref="Span{T}"/> as big-endian integers.
        /// </summary>
        /// <param name="span"></param>
        public void Write(Span<byte> span)
        {
            BinaryPrimitives.WriteUInt32BigEndian(span, Start);
            BinaryPrimitives.WriteUInt32BigEndian(span.Slice(4), End);
        }

        static void ValidateStartEndOrdering(uint start, uint end)
        {
            if (end < start)
            {
                throw new ArgumentOutOfRangeException("end", "End value must be equal to or greater than start value.");
            }
        }

        public readonly override string ToString()
        {
            return string.Format("ValueRange(0x{0:X8}, 0x{1:X8})", Start, End);
        }

        #region Value Equality

        public readonly override bool Equals(object? obj)
        {
            throw new NotSupportedException();
        }

        public readonly bool Equals(ValueRange other)
        {
            return this == other;
        }

        public static bool operator ==(ValueRange lhs, ValueRange rhs)
        {
            return lhs.Start == rhs.Start && lhs.End == rhs.End;
        }

        public static bool operator !=(ValueRange lhs, ValueRange rhs) => !(lhs == rhs);

        public readonly override int GetHashCode() => (Start, End).GetHashCode();

        #endregion
    }
}
