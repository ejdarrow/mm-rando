using System;
using System.Collections.Generic;
using System.Linq;

namespace MMR.Rom
{
    using Yaz = Yaz.Yaz;

    /// <summary>
    /// ROM (<c>dmadata</c>) file table interface.
    /// </summary>
    public class FileTable
    {
        /// <summary>
        /// Underlying ROM data.
        /// </summary>
        public RomFile Rom { get; private set; }

        /// <summary>
        /// List of decompressed, modified and/or appended file data.
        /// </summary>
        FileData[] DataList { get; set; }

        /// <summary>
        /// Length of data list.
        /// </summary>
        public int Length => DataList.Length;

        /// <summary>
        /// Alignment type to use for virtual start addresses.
        /// </summary>
        public AlignmentType Alignment { get; private set; }

        /// <summary>
        /// Accessor for <see cref="FileData"/> by file index without loading data into cache.
        /// </summary>
        /// <param name="index">File index</param>
        /// <returns></returns>
        public FileData this[int index] => DataList[index];

        public FileTable(RomFile rom, AlignmentType alignment = AlignmentType.Lenient)
        {
            Rom = rom;
            DataList = rom.Files.Select(static (row, index) => new FileData(index, in row)).ToArray();
            Alignment = alignment;
        }

        /// <summary>
        /// Append some given data as a new file using an empty file table slot.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="compress"></param>
        /// <returns></returns>
        /// <remarks>Appending data with a length of 0 will append a <see cref="StorageType.Deleted"/> file.</remarks>
        public FileData Append(byte[] data, bool compress = false)
        {
            var length = (uint)data.Length;
            if (!AlignmentHelper.IsAlignedLenient(length))
            {
                throw new ArgumentException("Appended data length must be aligned to a 0x10-byte boundary.", "data");
            }

            var index = GetNextAppendSlot();
            var prev = index > 0 ? DataList[index - 1].AddressRange.End : 0;
            var start = Alignment.Align(prev);

            FileData entry;
            if (0 < length)
            {
                var end = checked(start + length);
                var range = new ValueRange(start, end);
                entry = new FileData(range, data, index, compress, true);
            }
            else
            {
                // If length is 0, append file with StorageType.Deleted.
                var range = ValueRange.WithLength(start, 0x10);
                entry = new FileData(range, index);
            }

            DataList[index] = entry;
            return entry;
        }

        /// <summary>
        /// Delete the given file.
        /// </summary>
        /// <param name="index">File index</param>
        /// <returns></returns>
        public FileData Delete(int index)
        {
            var cached = DataList[index];
            cached.Delete();
            return cached;
        }


        /// <summary>
        /// Delete and shrink the given file.
        /// </summary>
        /// <param name="index">File index</param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public FileData DeleteAndShrink(int index, AddressDirection direction)
        {
            var cached = Delete(index);
            Shrink(index, direction);
            return cached;
        }

        /// <summary>
        /// Get an enumerable of files with data to compress.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<FileData> GetFilesToCompress() => DataList
                .Where(x => x.HasData && x.Modified && x.Storage == StorageType.Compressed);

        /// <summary>
        /// Get the virtual start address of the given file by index.
        /// </summary>
        /// <param name="index">File index</param>
        /// <returns></returns>
        public uint GetAddress(int index)
        {
            return DataList[index].AddressRange.Start;
        }

        /// <summary>
        /// Get the virtual address range of the given file by index.
        /// </summary>
        /// <param name="index">File index</param>
        /// <returns></returns>
        public ValueRange GetAddressRange(int index)
        {
            return DataList[index].AddressRange;
        }

        /// <summary>
        /// Get the leading empty file index at the end of the file table.
        /// </summary>
        /// <returns></returns>
        public int GetNextAppendSlot()
        {
            return GetTailIndex() + 1;
        }

        /// <summary>
        /// Get the file index of the tail-most file which is non-empty.
        /// </summary>
        /// <returns></returns>
        public int GetTailIndex()
        {
            for (int i = DataList.Length - 1; i >= 0; i--)
            {
                var cached = DataList[i];
                if (cached.Storage != StorageType.Unused)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Whether or not the virtual address range or <see cref="StorageType"/> has been changed for a given file.
        /// </summary>
        /// <param name="index">File index</param>
        /// <returns></returns>
        public bool IsMetaModified(int index)
        {
            var cached = DataList[index];
            ref readonly var original = ref Rom[index];

            return (cached.AddressRange != original.VirtualRange) || (cached.Storage != original.Storage);
        }

        /// <summary>
        /// Get the unmodified, decompressed data of a given file by file index.
        /// </summary>
        /// <param name="index">File index</param>
        /// <returns></returns>
        /// <remarks>May perform decompression.</remarks>
        public (byte[], bool) LoadFromRom(int index)
        {
            ref readonly var file = ref Rom[index];
            var span = Rom.GetReadOnlySpan(index);
            if (file.IsCompressed)
            {
                return (Yaz.Decode(span), true);
            }
            return (span.ToArray(), false);
        }

        public (byte[], bool) LoadFromRomWithSize(int index, uint length)
        {
            var (original, compressed) = LoadFromRom(index);

            // If original data length matches desired length, return unmodified.
            if (length == original.Length)
            {
                return (original, compressed);
            }

            // Copy data into new buffer with updated length.
            var buffer = new byte[length];
            var amount = Math.Min(checked((int)length), original.Length);
            original.AsSpan(0, amount).CopyTo(buffer);
            return (buffer, compressed);
        }

        /// <summary>
        /// Insert original file data into the data list.
        /// </summary>
        /// <param name="index">File index of original file</param>
        /// <returns></returns>
        FileData LoadFromRomIntoCache(int index)
        {
            var cached = DataList[index];
            if (cached.HasData)
                throw new ArgumentException($"Data for this file index already exists in the list: {index}", "index");
            var (original, _) = LoadFromRomWithSize(index, cached.AddressRange.Length);
            cached.UpdateWithOriginal(original);
            return cached;
        }

        /// <summary>
        /// Get the <see cref="FileData"/> for a given file index, loading original data from ROM if needed.
        /// </summary>
        /// <param name="index">File index</param>
        /// <returns></returns>
        public FileData GetCached(int index)
        {
            var cached = DataList[index];
            return cached.HasData ? cached : LoadFromRomIntoCache(index);
        }

        /// <summary>
        /// Get a <see cref="ReadOnlySpan{T}"/> of the file data for the given index.
        /// </summary>
        /// <param name="index">File index</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public ReadOnlySpan<byte> GetReadOnlySpan(int index)
        {
            var cached = DataList[index];

            // If storage type cannot hold data, throw exception.
            if (!cached.Storage.ContainsData())
            {
                throw new ArgumentException($"File[{index}]: Cannot get {typeof(ReadOnlySpan<byte>)} for file with no data-backing storage type: {cached.Storage}", "index");
            }

            // If data already cached, use that.
            if (cached.HasData)
            {
                return cached.ToReadOnlySpan();
            }

            // If compressed, load decompressed file data into staging area.
            if (Rom[index].IsCompressed)
            {
                var file = GetCached(index);
                return file.ToReadOnlySpan();
            }

            // If not compressed, can get span of immutable rom data.
            // NOTE: We are using the staging address range when referencing the original file data.
            // This will throw an exception if staging address range is larger than original file data.
            return Rom.GetReadOnlySpan(index).Slice(0, checked((int)cached.AddressRange.Length));
        }

        /// <summary>
        /// Get a <see cref="ReadOnlySpan{T}"/> of the file data for the given index and virtual start address.
        /// </summary>
        /// <param name="index">File index</param>
        /// <param name="start">Virtual start address</param>
        /// <returns></returns>
        public ReadOnlySpan<byte> GetReadOnlySpan(int index, uint start)
        {
            var cached = DataList[index];
            var span = GetReadOnlySpan(index);
            var offset = cached.AddressRange.OffsetOf(start);
            return span.Slice(checked((int)offset));
        }

        /// <summary>
        /// Get a <see cref="ReadOnlySpan{T}"/> of the file data for the given file index and virtual address range.
        /// </summary>
        /// <param name="index">File index</param>
        /// <param name="range">Virtual address range</param>
        /// <returns></returns>
        public ReadOnlySpan<byte> GetReadOnlySpan(int index, ValueRange range)
        {
            var cached = DataList[index];
            var span = GetReadOnlySpan(index);
            var offset = cached.AddressRange.OffsetOf(range);
            return span.Slice(checked((int)offset), checked((int)range.Length));
        }

        /// <summary>
        /// Get a <see cref="ReadOnlySpan{T}"/> of the file data at the given virtual start address.
        /// </summary>
        /// <param name="start">Virtual start address</param>
        /// <returns></returns>
        public ReadOnlySpan<byte> GetReadOnlySpanAt(uint start)
        {
            var cached = Resolve(start)!;
            var offset = cached.AddressRange.OffsetOf(start);
            var span = GetReadOnlySpan(cached.Index);
            return span.Slice(checked((int)offset));
        }

        /// <summary>
        /// Get a <see cref="ReadOnlySpan{T}"/> of the file data at the given virtual start address, with the given length.
        /// </summary>
        /// <param name="start">Virtual start address</param>
        /// <param name="length"></param>
        /// <returns></returns>
        public ReadOnlySpan<byte> GetReadOnlySpanAt(uint start, uint length)
        {
            var range = ValueRange.WithLength(start, length);
            return GetReadOnlySpanAt(range);
        }

        /// <summary>
        /// Get a <see cref="ReadOnlySpan{T}"/> of the file data at the given virtual address range.
        /// </summary>
        /// <param name="range">Virtual address range</param>
        /// <returns></returns>
        public ReadOnlySpan<byte> GetReadOnlySpanAt(ValueRange range)
        {
            var cached = Resolve(range)!;
            var offset = cached.AddressRange.OffsetOf(range);
            var span = GetReadOnlySpan(cached.Index);
            return span.Slice(checked((int)offset), checked((int)range.Length));
        }

        /// <summary>
        /// Get a <see cref="ReadOnlySpan{T}"/> of the physical data (for writing to ROM) of a given file index.
        /// </summary>
        /// <param name="index">File index</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        /// <remarks>This function does not perform any compression or decompression.</remarks>
        ReadOnlySpan<byte> GetReadOnlySpanForROM(int index)
        {
            static NotSupportedException NewCannotCompressDataException(int index) =>
                new NotSupportedException($"File[{index}]: Cannot perform data compression, should have been provided.");
            static NotSupportedException NewCannotDecompressDataException(int index) =>
                new NotSupportedException($"File[{index}]: Cannot perform data decompression, should already exist within file cache.");

            // If copying from original file data, address range length should be unmodified.
            static NotSupportedException NewCannotUseOriginalDataOfDifferentLength(int index) =>
                new NotSupportedException($"File[{index}]: Cannot copy original ROM data if address range length has changed.");

            ref readonly var original = ref Rom[index];
            var cached = DataList[index];

            // If deleted or unused, no physical data is present in ROM.
            if (cached.Storage == StorageType.Deleted || cached.Storage == StorageType.Unused)
            {
                return ReadOnlySpan<byte>.Empty;
            }

            var combo = (original.Storage, cached.Storage);
            switch (combo)
            {
                case (StorageType.Plain, StorageType.Plain):
                    {
                        // If plain => plain, may directly copy original data if unmodified.
                        if (cached.HasData)
                            return cached.ToReadOnlySpan();
                        // Address range length should be unchanged.
                        if (cached.AddressRange.Length != original.VirtualRange.Length)
                            throw NewCannotUseOriginalDataOfDifferentLength(index);
                        return Rom.GetReadOnlySpan(in original);
                    }
                case (StorageType.Compressed, StorageType.Plain):
                case (StorageType.Deleted, StorageType.Plain):
                case (StorageType.Unused, StorageType.Plain):
                    {
                        // If non-plain => plain, data must be present in cache.
                        if (!cached.HasData)
                            throw NewCannotDecompressDataException(index);
                        return cached.ToReadOnlySpan();
                    }
                case (StorageType.Compressed, StorageType.Compressed):
                    {
                        // If compressed => compressed, may directly copy original data if unmodified.
                        if (cached.HasData && cached.Modified)
                            throw NewCannotCompressDataException(index);
                        // Address range length should be unchanged.
                        if (cached.AddressRange.Length != original.VirtualRange.Length)
                            throw NewCannotUseOriginalDataOfDifferentLength(index);
                        return Rom.GetReadOnlySpan(in original);
                    }
                case (StorageType.Plain, StorageType.Compressed):
                case (StorageType.Deleted, StorageType.Compressed):
                case (StorageType.Unused, StorageType.Compressed):
                    {
                        // If non-compressed => compressed, should be provided.
                        throw NewCannotCompressDataException(index);
                    }
            }

            throw new NotSupportedException();
        }

        /// <summary>
        /// Get a <see cref="Span{T}"/> of the file data for the given index.
        /// </summary>
        /// <param name="index">File index</param>
        /// <returns></returns>
        public Span<byte> GetSpan(int index)
        {
            return GetCached(index).ToSpan();
        }

        /// <summary>
        /// Get a <see cref="Span{T}"/> of the file data for the given file index and virtual start address.
        /// </summary>
        /// <param name="index">File index</param>
        /// <param name="start">Virtual start address</param>
        /// <returns></returns>
        public Span<byte> GetSpan(int index, uint start)
        {
            return GetCached(index).ToSpan(start);
        }

        /// <summary>
        /// Get a <see cref="Span{T}"/> of the file data for the given file index and virtual address range.
        /// </summary>
        /// <param name="index">File index</param>
        /// <param name="range">Virtual address range</param>
        /// <returns></returns>
        public Span<byte> GetSpan(int index, ValueRange range)
        {
            return GetCached(index).ToSpan(range);
        }

        /// <summary>
        /// Get a <see cref="Span{T}"/> of the file data at the given virtual start address.
        /// </summary>
        /// <param name="start">Virtual start address</param>
        /// <returns></returns>
        public Span<byte> GetSpanAt(uint start)
        {
            var cached = Resolve(start);
            if (cached == null)
            {
                throw new ArgumentOutOfRangeException("start", $"Unable to resolve virtual address: ${start}");
            }
            return GetSpan(cached.Index, start);
        }

        /// <summary>
        /// Get a <see cref="Span{T}"/> of the file data at the given virtual start address, with the given length.
        /// </summary>
        /// <param name="start">Virtual start address</param>
        /// <param name="length">Span length</param>
        /// <returns></returns>
        public Span<byte> GetSpanAt(uint start, uint length)
        {
            var range = ValueRange.WithLength(start, length);
            return GetSpanAt(range);
        }

        /// <summary>
        /// Get a <see cref="Span{T}"/> of the file data at the given virtual address range.
        /// </summary>
        /// <param name="range">Virtual address range</param>
        /// <returns></returns>
        public Span<byte> GetSpanAt(ValueRange range)
        {
            var cached = Resolve(range);
            if (cached == null)
            {
                throw new ArgumentOutOfRangeException("range", $"Unable to resolve virtual address range: {range}");
            }
            return GetSpan(cached.Index, range);
        }

        /// <summary>
        /// Resolve the <see cref="FileData"/> by virtual address.
        /// </summary>
        /// <param name="address">Virtual address</param>
        /// <returns></returns>
        public FileData? Resolve(uint address)
        {
            return DataList.First(x => x.AddressRange.Contains(address));
        }

        /// <summary>
        /// Resolve the <see cref="FileData"/> by virtual address range.
        /// </summary>
        /// <param name="range">Virtual address range</param>
        /// <returns></returns>
        public FileData? Resolve(ValueRange range)
        {
            return DataList.First(x => x.AddressRange.Contains(range));
        }

        /// <summary>
        /// Resolve the index of a file given a virtual address.
        /// </summary>
        /// <param name="address">Virtual address</param>
        /// <returns></returns>
        public int ResolveIndex(uint address)
        {
            return Resolve(address)!.Index;
        }

        /// <summary>
        /// Resolve the index of a file given a virtual address range.
        /// </summary>
        /// <param name="range">Virtual address range</param>
        /// <returns></returns>
        public int ResolveIndex(ValueRange range)
        {
            return Resolve(range)!.Index;
        }

        /// <summary>
        /// Resolve the <see cref="FileData"/> given an exact virtual start address.
        /// </summary>
        /// <param name="start">Virtual start address</param>
        /// <returns></returns>
        public FileData? ResolveExact(uint start)
        {
            return DataList.First(x => x.AddressRange.Start == start);
        }

        /// <summary>
        /// Resolve the index of a file given an exact virtual start address.
        /// </summary>
        /// <param name="start">Virtual start address</param>
        /// <returns>File index</returns>
        public int ResolveExactIndex(uint start)
        {
            return ResolveExact(start)!.Index;
        }

        /// <summary>
        /// Get left-most virtual address range boundary value.
        /// </summary>
        /// <param name="index">File index</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public uint GetBoundaryLeft(int index)
        {
            if (index == 0)
            {
                return 0;
            }
            var prev = DataList[index - 1];
            if (prev.Storage == StorageType.Unused)
            {
                throw new ArgumentException($"File[{index}]: Cannot get left-most boundary if previous file has no virtual address range.", "index");
            }
            return Alignment.Align(prev.AddressRange.End);
        }

        /// <summary>
        /// Get right-most virtual address range boundary value.
        /// </summary>
        /// <param name="index">File index</param>
        /// <returns></returns>
        public uint GetBoundaryRight(int index)
        {
            const uint Max = (uint)int.MaxValue + 1;
            if (index == (DataList.Length - 1))
            {
                return Max;
            }
            var next = DataList[index + 1];
            if (next.Storage == StorageType.Unused)
            {
                return Max;
            }
            return next.AddressRange.Start;
        }

        /// <summary>
        /// Get the available address range for a given file by index.
        /// </summary>
        /// <param name="index">File index</param>
        /// <param name="direction">Direction to search for address range boundaries.</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public ValueRange GetAvailableAddressRange(int index, AddressDirection direction = AddressDirection.Right) => direction switch
        {
            AddressDirection.Left => new ValueRange(GetBoundaryLeft(index), DataList[index].AddressRange.End),
            AddressDirection.Right => new ValueRange(DataList[index].AddressRange.Start, GetBoundaryRight(index)),
            AddressDirection.LeftAndRight => new ValueRange(GetBoundaryLeft(index), GetBoundaryRight(index)),
            _ => throw new NotSupportedException(),
        };

        /// <summary>
        /// Get an acceptable resized virtual address range for the given file.
        /// </summary>
        /// <param name="index">File index</param>
        /// <param name="length">Resized address range length</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public ValueRange GetResizedAddressRange(int index, uint length)
        {
            var available = GetAvailableAddressRange(index, AddressDirection.Right);
            if (available.Length < length)
            {
                throw new ArgumentOutOfRangeException("length", $"File[{index}]: Provided length is too large for available virtual address range: {index}");
            }
            return ValueRange.WithLength(available.Start, length);
        }

        /// <summary>
        /// Replace the file data at the given index, and resize the address range appropriately.
        /// </summary>
        /// <param name="index">File index</param>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public FileData ResizeWithData(int index, byte[] data)
        {
            if (!AlignmentHelper.IsAlignedLenient((uint)data.Length))
            {
                throw new ArgumentException($"File[{index}]: New data length should be 0x10-byte aligned.", "data");
            }
            var cached = DataList[index];
            var range = GetResizedAddressRange(index, (uint)data.Length);
            cached.Update(range, data);
            return cached;
        }

        /// <summary>
        /// Shrink the virtual address space of the deleted file at the given index.
        /// </summary>
        /// <param name="index">File index</param>
        /// <exception cref="ArgumentException"></exception>
        public void Shrink(int index, AddressDirection direction)
        {
            var cached = DataList[index];
            if (cached.Storage != StorageType.Deleted)
            {
                throw new ArgumentException($"File[{index}]: Must be deleted to shrink address range.", "index");
            }

            var available = GetAvailableAddressRange(index, AddressDirection.LeftAndRight);
            if (available.Length < 0x10)
            {
                throw new ArgumentException($"File[{index}]: Available address range is too small to shrink to length of 0x10.", "index");
            }

            // NOTE: Currently does not shift to left-most boundary.
            var range = direction switch
            {
                AddressDirection.Left => ValueRange.WithLength(available.Start, 0x10),
                AddressDirection.Right => ValueRange.WithLength(checked(available.End - 0x10), 0x10),
                _ => throw new NotSupportedException(),
            };
            cached.UseAddressRange(range);
        }

        /// <summary>
        /// Build a <see cref="RomFile"/>.
        /// </summary>
        /// <param name="compressedFiles">Compressed file data</param>
        /// <returns></returns>
        public RomFile Build(byte[][] compressedFiles)
        {
            var buffer = new byte[Rom.Buffer.Length];
            var files = new VirtualFile[DataList.Length];
            int offset = 0;

            for (int i = 0; i < files.Length; i++)
            {
                ref readonly var original = ref Rom.Files[i];
                var cached = DataList[i];
                var compressed = compressedFiles[i];

                ReadOnlySpan<byte> source;
                if (cached.Storage == StorageType.Compressed && compressed != null)
                {
                    // Handle modified compressed files.
                    files[i] = cached.ToFileEntry(in original, (uint)offset, (uint)compressed.Length);
                    source = compressed;
                }
                else
                {
                    // Handle all other files.
                    files[i] = cached.ToFileEntry(in original, (uint)offset);
                    source = GetReadOnlySpanForROM(i);
                }

                // If non-empty data, copy to output ROM buffer.
                if (0 < source.Length)
                {
                    var dest = buffer.AsSpan(offset, source.Length);
                    source.CopyTo(dest);
                    offset += source.Length;
                }
            }

            return new RomFile(buffer, files, Rom.TableIndex);
        }
    }
}
