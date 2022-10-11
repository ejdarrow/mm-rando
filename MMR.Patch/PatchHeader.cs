using MMR.Rom;
using System;
using System.Buffers.Binary;

namespace MMR.Patch
{
    /// <summary>
    /// Header for file entries in patch data.
    /// </summary>
    public readonly struct PatchHeader
    {
        /// <summary>
        /// Size of serialized <see cref="PatchHeader"/>.
        /// </summary>
        public const int Size = 0x10;

        /// <summary>
        /// Virtual address range of file.
        /// </summary>
        public readonly ValueRange AddressRange { get; }

        /// <summary>
        /// Command for this entry.
        /// </summary>
        public readonly PatchCommand Command { get; }

        /// <summary>
        /// Storage type of file.
        /// </summary>
        public readonly StorageType Storage { get; }

        /// <summary>
        /// Index of file.
        /// </summary>
        public readonly ushort Index { get; }

        /// <summary>
        /// Length of data.
        /// </summary>
        public readonly int Length { get; }

        public PatchHeader(ushort index, ValueRange addressRange, int length, PatchCommand command, StorageType storage)
        {
            Index = index;
            AddressRange = addressRange;
            Length = length;
            Command = command;
            Storage = storage;
        }

        /// <summary>
        /// Create a <see cref="PatchHeader"/> for updating meta-information of a file.
        /// </summary>
        /// <param name="index">File index.</param>
        /// <param name="addressRange">File virtual address range.</param>
        /// <param name="storage">File storage type.</param>
        /// <returns></returns>
        public static PatchHeader CreateMetaOnly(ushort index, ValueRange addressRange, StorageType storage)
        {
            return new PatchHeader(index, addressRange, 0, PatchCommand.MetaOnly, storage);
        }

        /// <summary>
        /// Create a <see cref="PatchHeader"/> for patching an existing file.
        /// </summary>
        /// <param name="index">File index.</param>
        /// <param name="length">Patch data length.</param>
        /// <param name="addressRange">File virtual address range.</param>
        /// <param name="storage">File storage type.</param>
        /// <returns></returns>
        public static PatchHeader CreateExistingData(ushort index, int length, ValueRange addressRange, StorageType storage)
        {
            return new PatchHeader(index, addressRange, length, PatchCommand.ExistingData, storage);
        }

        /// <summary>
        /// Create a <see cref="PatchHeader"/> for adding a new file.
        /// </summary>
        /// <param name="index">File index.</param>
        /// <param name="length">File data length.</param>
        /// <param name="addressRange">File virtual address range.</param>
        /// <param name="storage">File storage type.</param>
        /// <returns></returns>
        public static PatchHeader CreateNewData(ushort index, int length, ValueRange addressRange, StorageType storage)
        {
            return new PatchHeader(index, addressRange, length, PatchCommand.NewData, storage);
        }

        /// <summary>
        /// Read a <see cref="PatchHeader"/> from a <see cref="ReadOnlySpan{byte}"/>.
        /// </summary>
        /// <param name="data">Source to read.</param>
        /// <returns></returns>
        public static PatchHeader Read(ReadOnlySpan<byte> data)
        {
            var start = BinaryPrimitives.ReadUInt32BigEndian(data);
            var end = BinaryPrimitives.ReadUInt32BigEndian(data.Slice(0x4));
            var length = BinaryPrimitives.ReadInt32BigEndian(data.Slice(0x8));
            var index = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(0xC));
            var command = data[0xE];
            var storage = data[0xF];
            return new PatchHeader(index, new ValueRange(start, end), length, (PatchCommand)command, (StorageType)storage);
        }

        /// <summary>
        /// Write this <see cref="PatchHeader"/> to a <see cref="Span{byte}"/>.
        /// </summary>
        /// <param name="data">Destination to write.</param>
        public readonly void Write(Span<byte> data)
        {
            BinaryPrimitives.WriteUInt32BigEndian(data, AddressRange.Start);
            BinaryPrimitives.WriteUInt32BigEndian(data.Slice(0x4), AddressRange.End);
            BinaryPrimitives.WriteInt32BigEndian(data.Slice(0x8), Length);
            BinaryPrimitives.WriteUInt16BigEndian(data.Slice(0xC), Index);
            data[0xE] = (byte)Command;
            data[0xF] = (byte)Storage;
        }
    }
}
