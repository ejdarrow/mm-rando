using System;
using System.Buffers.Binary;

namespace MMR.Rom
{
    public readonly struct VirtualFile
    {
        /// <summary>
        /// Virtual address range.
        /// </summary>
        public readonly ValueRange VirtualRange;

        /// <summary>
        /// Physical start address.
        /// </summary>
        public readonly uint PhysicalStart;

        /// <summary>
        /// Physical end address, may be 0 if <see cref="VirtualFile"/> is decompressed.
        /// </summary>
        public readonly uint PhysicalEnd;

        /// <summary>
        /// Virtual start address.
        /// </summary>
        public readonly uint VirtualStart => VirtualRange.Start;

        /// <summary>
        /// Virtual end address.
        /// </summary>
        public readonly uint VirtualEnd => VirtualRange.End;

        /// <summary>
        /// Whether or not this <see cref="VirtualFile"/> contains non-compressed data.
        /// </summary>
        public readonly bool IsPlain => Storage == StorageType.Plain;

        /// <summary>
        /// Whether or not this <see cref="VirtualFile"/> contains compressed data.
        /// </summary>
        public readonly bool IsCompressed => Storage == StorageType.Compressed;

        /// <summary>
        /// Whether or not this <see cref="VirtualFile"/> is empty.
        /// </summary>
        public readonly bool IsEmpty => Storage == StorageType.Unused;

        /// <summary>
        /// Whether or not the physical start and end addresses are both <c>0xFFFFFFFF</c>, indicating that file data is not present.
        /// </summary>
        public readonly bool IsDeleted => Storage == StorageType.Deleted;

        /// <summary>
        /// Get the storage type.
        /// </summary>
        public readonly StorageType Storage
        {
            get
            {
                // NOTE: Maybe check PhysicalRange start and end are 0 as well, this is a shortcut.
                if (VirtualRange.Start == 0 && VirtualRange.End == 0)
                    return StorageType.Unused;
                else if (PhysicalStart == uint.MaxValue && PhysicalEnd == uint.MaxValue)
                    return StorageType.Deleted;
                else if (PhysicalEnd != 0)
                    return StorageType.Compressed;
                else
                    return StorageType.Plain;
            }
        }

        public VirtualFile(ValueRange virtualRange, ValueRange physicalRange)
        {
            VirtualRange = virtualRange;
            (PhysicalStart, PhysicalEnd) = (physicalRange.Start, physicalRange.End);
        }

        public VirtualFile(ValueRange virtualRange, (uint, uint) physicalRange)
        {
            VirtualRange = virtualRange;
            (PhysicalStart, PhysicalEnd) = physicalRange;
        }

        public VirtualFile(uint vstart, uint vend, uint pstart, uint pend)
        {
            VirtualRange = new ValueRange(vstart, vend);
            PhysicalStart = pstart;
            PhysicalEnd = pend;
        }

        /// <summary>
        /// Get the physical address range as a <see cref="ValueRange"/>.
        /// </summary>
        /// <returns></returns>
        public readonly ValueRange GetPhysicalRange()
        {
            return new ValueRange(PhysicalStart, PhysicalEnd);
        }

        /// <summary>
        /// Read a <see cref="VirtualFile"/> from a <see cref="ReadOnlySpan{T}"/>.
        /// </summary>
        /// <param name="span"></param>
        /// <returns></returns>
        public static VirtualFile Read(ReadOnlySpan<byte> span)
        {
            var word1 = BinaryPrimitives.ReadUInt32BigEndian(span);
            var word2 = BinaryPrimitives.ReadUInt32BigEndian(span.Slice(0x4));
            var word3 = BinaryPrimitives.ReadUInt32BigEndian(span.Slice(0x8));
            var word4 = BinaryPrimitives.ReadUInt32BigEndian(span.Slice(0xC));
            return new VirtualFile(word1, word2, word3, word4);
        }

        /// <summary>
        /// Write this <see cref="VirtualFile"/> to a <see cref="Span{T}"/>.
        /// </summary>
        /// <param name="span"></param>
        public readonly void Write(Span<byte> span)
        {
            BinaryPrimitives.WriteUInt32BigEndian(span, VirtualRange.Start);
            BinaryPrimitives.WriteUInt32BigEndian(span.Slice(0x4), VirtualRange.End);
            BinaryPrimitives.WriteUInt32BigEndian(span.Slice(0x8), PhysicalStart);
            BinaryPrimitives.WriteUInt32BigEndian(span.Slice(0xC), PhysicalEnd);
        }

        /// <summary>
        /// Get the <see cref="ValueRange"/> of the physical file data relative to ROM start.
        /// </summary>
        /// <remarks>Certain files may return a range outside of the ROM bounds.</remarks>
        /// <returns></returns>
        public readonly ValueRange ToRange() => Storage switch
        {
            StorageType.Plain => ValueRange.WithLength(PhysicalStart, VirtualRange.Length),
            _ => new ValueRange(PhysicalStart, PhysicalEnd),
        };

        /// <summary>
        /// Get whether or not a given virtual address is within this file's data.
        /// </summary>
        /// <param name="addr">Virtual address.</param>
        /// <returns></returns>
        public readonly bool Within(uint addr) => VirtualRange.Contains(addr);

        public readonly override string ToString()
        {
            return $"[0x{VirtualStart:X8}, 0x{VirtualEnd:X8}, 0x{PhysicalStart:X8}, 0x{PhysicalEnd:X8}]";
        }
    }
}
