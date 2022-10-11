using System;

namespace MMR.Rom
{
    public class FileData
    {
        /// <summary>
        /// Data buffer.
        /// </summary>
        byte[] Data;

        /// <summary>
        /// Whether or not this data has been accessed in a mutable state.
        /// </summary>
        /// <remarks>
        /// Should only be <see langword="true"/> when paired with a <see cref="StorageType"/> that may contain data,
        /// and if so must be <see langword="true"/> if the original <see cref="StorageType"/> could not contain data.
        /// </remarks>
        public bool Modified { get; private set; } = false;

        /// <summary>
        /// File index.
        /// </summary>
        public readonly int Index;

        /// <summary>
        /// File data length.
        /// </summary>
        public int Length => Data.Length;

        /// <summary>
        /// Virtual address range of file data.
        /// </summary>
        public ValueRange AddressRange { get; private set; }

        /// <summary>
        /// Storage type for physical data.
        /// </summary>
        public StorageType Storage { get; private set; }

        /// <summary>
        /// Whether or not this file has a data buffer.
        /// </summary>
        public bool HasData => Data != null;

        internal FileData(int index, in VirtualFile file)
        {
            AddressRange = file.VirtualRange;
            Index = index;
            Storage = file.Storage;
        }

        public FileData(ValueRange addressRange, int index)
        {
            AddressRange = addressRange;
            Index = index;
            Storage = StorageType.Deleted;
            Data = new byte[0];
        }

        public FileData(ValueRange addressRange, byte[] data, int index, bool compress, bool modified = false)
        {
            if (addressRange.Length != data.Length)
            {
                throw NewDataLengthMismatchException(index, addressRange.Length, data.Length);
            }
            AddressRange = addressRange;
            Data = data;
            Index = index;
            Storage = compress ? StorageType.Compressed : StorageType.Plain;
            Modified = modified;
        }

        /// <summary>
        /// Apply a given virtual address range and <see cref="StorageType"/> to this file.
        /// </summary>
        /// <param name="addressRange"></param>
        /// <param name="storage"></param>
        /// <remarks>Only intended to be used by patcher code.</remarks>
        public void ApplyMetaInfo(ValueRange addressRange, StorageType storage)
        {
            AddressRange = addressRange;
            if (storage == Storage)
                return;
            else if (storage == StorageType.Deleted)
                Delete();
            else if (storage == StorageType.Unused)
                throw new InvalidOperationException($"Applying {StorageType.Unused} is currently not supported.");
            else
                Storage = storage;
        }

        /// <summary>
        /// Apply a given virtual address range and <see cref="StorageType"/> to this file, along with data.
        /// </summary>
        /// <param name="addressRange"></param>
        /// <param name="storage"></param>
        /// <param name="data"></param>
        /// <remarks>Only intended to be used by patcher code.</remarks>
        public void ApplyMetaInfoWithData(ValueRange addressRange, StorageType storage, byte[] data)
        {
            if (!storage.ContainsData() && 0 < data.Length)
            {
                throw new InvalidOperationException($"Applying data with {typeof(StorageType)} which may not contain physical data: {storage}");
            }
            ApplyMetaInfo(addressRange, storage);
            Data = data;
            Modified = true;
        }

        /// <summary>
        /// Get the underlying data buffer.
        /// </summary>
        /// <param name="modified">Whether or not to mark data as modified.</param>
        /// <returns></returns>
        /// <remarks>DO NOT USE unless necessary for compatibility with external APIs.</remarks>
        public byte[] GetData(bool modified = false)
        {
            Modified = Modified || modified;
            return Data;
        }

        /// <summary>
        /// Create a <see cref="VirtualFile"/> representation of this file.
        /// </summary>
        /// <param name="original">Original file row</param>
        /// <param name="offset">Current physical offset</param>
        /// <param name="compressedLength">Optional length of data when compressed</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public VirtualFile ToFileEntry(in VirtualFile original, uint offset, uint? compressedLength = null) => Storage switch
        {
            StorageType.Plain => new VirtualFile(AddressRange, (offset, 0)),
            StorageType.Compressed => new VirtualFile(AddressRange, ValueRange.WithLength(offset, compressedLength.GetValueOrDefault(original.GetPhysicalRange().Length))),
            StorageType.Deleted => new VirtualFile(AddressRange, (uint.MaxValue, uint.MaxValue)),
            StorageType.Unused => new VirtualFile(0, 0, 0, 0),
            _ => throw new NotSupportedException(),
        };

        /// <summary>
        /// Get an immutable view via <see cref="ReadOnlySpan{T}"/>.
        /// </summary>
        /// <returns></returns>
        public ReadOnlySpan<byte> ToReadOnlySpan()
        {
            return new ReadOnlySpan<byte>(Data);
        }

        /// <summary>
        /// Get a <see cref="ReadOnlySpan{T}"/> given a virtual start address.
        /// </summary>
        /// <param name="start">Virtual start address</param>
        /// <returns></returns>
        public ReadOnlySpan<byte> ToReadOnlySpan(uint start)
        {
            return ToSpan_Internal(start);
        }

        /// <summary>
        /// Get a <see cref="ReadOnlySpan{T}"/> given a virtual address range.
        /// </summary>
        /// <param name="range">Virtual address range</param>
        /// <returns></returns>
        public ReadOnlySpan<byte> ToReadOnlySpan(ValueRange range)
        {
            return ToSpan_Internal(range);
        }

        /// <summary>
        /// Get a mutable view via <see cref="Span{T}"/>, and mark data as modified.
        /// </summary>
        /// <returns></returns>
        public Span<byte> ToSpan()
        {
            Modified = true;
            return new Span<byte>(Data);
        }

        /// <summary>
        /// Get a <see cref="Span{T}"/> given a virtual start address.
        /// </summary>
        /// <param name="start">Virtual start address</param>
        /// <returns></returns>
        public Span<byte> ToSpan(uint start)
        {
            Modified = true;
            return ToSpan_Internal(start);
        }

        /// <summary>
        /// Get a <see cref="Span{T}"/> given a virtual address range.
        /// </summary>
        /// <param name="range">Virtual address range</param>
        /// <returns></returns>
        public Span<byte> ToSpan(ValueRange range)
        {
            Modified = true;
            return ToSpan_Internal(range);
        }

        /// <summary>
        /// Get a <see cref="Span{T}"/> given a virtual start address.
        /// </summary>
        /// <param name="start">Virtual start address</param>
        /// <returns></returns>
        /// <remarks>Does not mark file as modified.</remarks>
        internal Span<byte> ToSpan_Internal(uint start)
        {
            var offset = AddressRange.OffsetOf(start);
            return new Span<byte>(Data).Slice(checked((int)offset));
        }

        /// <summary>
        /// Get a <see cref="Span{T}"/> given a virtual address range.
        /// </summary>
        /// <param name="range">Virtual address range</param>
        /// <returns></returns>
        /// <remarks>Does not mark file as modified.</remarks>
        internal Span<byte> ToSpan_Internal(ValueRange range)
        {
            var offset = AddressRange.OffsetOf(range);
            return new Span<byte>(Data, checked((int)offset), checked((int)range.Length));
        }

        /// <summary>
        /// Remove data and mark file as deleted.
        /// </summary>
        internal void Delete()
        {
            Data = new byte[0];
            Storage = StorageType.Deleted;
            Modified = false;
        }

        /// <summary>
        /// Resize data buffer to match address range length.
        /// </summary>
        internal void MatchBufferWithLength()
        {
            if (Data.Length == AddressRange.Length)
                return;
            var data = new byte[AddressRange.Length];
            var copylen = Math.Min(Data.Length, data.Length);
            Data.AsSpan(0, copylen).CopyTo(data.AsSpan(0, copylen));
            Data = data;
            Modified = true;
        }

        /// <summary>
        /// Update with a new address range and data.
        /// </summary>
        /// <param name="addressRange"></param>
        /// <param name="data"></param>
        internal void Update(ValueRange addressRange, byte[] data)
        {
            if (addressRange.Length != data.Length)
            {
                throw NewDataLengthMismatchException(Index, addressRange.Length, data.Length);
            }
            AddressRange = addressRange;
            Data = data;
            Modified = true;
        }

        /// <summary>
        /// Update with original file data, and thus does not mark as modified.
        /// </summary>
        /// <param name="data"></param>
        internal void UpdateWithOriginal(byte[] data)
        {
            if (data.Length != AddressRange.Length)
            {
                throw NewDataLengthMismatchException(Index, AddressRange.Length, data.Length);
            }
            Data = data;
        }

        /// <summary>
        /// Replace the address range.
        /// </summary>
        /// <param name="range"></param>
        /// <remarks>Will resize the buffer if address range length is changed.</remarks>
        internal void UseAddressRange(ValueRange range)
        {
            AddressRange = range;
            if (Data != null && Storage != StorageType.Deleted)
            {
                MatchBufferWithLength();
            }
        }

        static Exception NewDataLengthMismatchException(int index, uint available, int actual) =>
            new Exception($"File [{index}] data length must match address range length: {actual} != {available}");
    }
}
