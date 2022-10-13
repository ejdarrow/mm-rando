using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MMR.Rom
{
    public class RomFile
    {
        /// <summary>
        /// File offset of Majora's Mask (US) <c>dmadata</c> file.
        /// </summary>
        public const int MajoraTableOffset = 0x1A500;

        /// <summary>
        /// Expected length of ROM file (32 MiB).
        /// </summary>
        public const int ExpectedLength = 0x2000000;

        /// <summary>
        /// Underlying buffer containing ROM data.
        /// </summary>
        public readonly Memory<byte> Buffer;

        /// <summary>
        /// Parsed <see cref="VirtualFile"/> list.
        /// </summary>
        public readonly VirtualFile[] Files;

        /// <summary>
        /// File index of <see cref="VirtualFile"/> table.
        /// </summary>
        public readonly int TableIndex;

        /// <summary>
        /// Get a <see langword="ref"/> <see langword="readonly"/> reference to the <see cref="VirtualFile"/> at the given index.
        /// </summary>
        /// <param name="index">File index</param>
        /// <returns></returns>
        public ref readonly VirtualFile this[int index] => ref Files[index];

        public RomFile(Memory<byte> buffer, VirtualFile[] files, int tableIndex)
        {
            Buffer = buffer;
            Files = files;
            TableIndex = tableIndex;
        }

        /// <summary>
        /// Get a <see langword="ref"/> <see langword="readonly"/> reference to the <see cref="VirtualFile"/> at the given index.
        /// </summary>
        /// <param name="index">File index</param>
        /// <returns></returns>
        public ref readonly VirtualFile At(int index)
        {
            return ref Files[index];
        }

        /// <summary>
        /// Clone contents into a new <see cref="RomFile"/>.
        /// </summary>
        /// <returns></returns>
        public RomFile Clone()
        {
            return new RomFile(Buffer.ToArray(), Files.ToArray(), TableIndex);
        }

        /// <summary>
        /// Find the first <see cref="VirtualFile"/> with the exact given virtual start address.
        /// </summary>
        /// <param name="start">Virtual start address</param>
        /// <returns>File index</returns>
        public int? FindExact(uint start)
        {
            for (int i = 0; i < Files.Length; i++)
            {
                if (Files[i].VirtualStart == start)
                    return i;
            }
            return null;
        }

        /// <summary>
        /// Check whether or not a file exists at the given file index.
        /// </summary>
        /// <param name="index">File index</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public bool HasFile(int index)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            return index < Files.Length;
        }

        /// <summary>
        /// Get a slice of the ROM data for a specific <see cref="VirtualFile"/>.
        /// </summary>
        /// <param name="file">File used to get slice.</param>
        /// <returns></returns>
        public ReadOnlySpan<byte> GetReadOnlySpan(in VirtualFile file)
        {
            return GetSpan(in file);
        }

        /// <summary>
        /// Get a <see cref="ReadOnlySpan{T}"/> for the given file index.
        /// </summary>
        /// <param name="fileIndex">File index</param>
        /// <returns></returns>
        public ReadOnlySpan<byte> GetReadOnlySpan(int fileIndex)
        {
            return GetSpan(in Files[fileIndex]);
        }

        /// <summary>
        /// Get a <see cref="ReadOnlySpan{T}"/> for the given file index, with data offset.
        /// </summary>
        /// <param name="fileIndex">File index</param>
        /// <param name="start">Data offset</param>
        /// <returns></returns>
        public ReadOnlySpan<byte> GetReadOnlySpan(int fileIndex, int start)
        {
            return GetSpan(in Files[fileIndex]).Slice(start);
        }

        /// <summary>
        /// Get a <see cref="ReadOnlySpan{T}"/> for a given file index, with data offset and length.
        /// </summary>
        /// <param name="fileIndex">File index</param>
        /// <param name="start">Data offset</param>
        /// <param name="length">Data length</param>
        /// <returns></returns>
        public ReadOnlySpan<byte> GetReadOnlySpan(int fileIndex, int start, int length)
        {
            return GetSpan(in Files[fileIndex]).Slice(start, length);
        }

        /// <summary>
        /// Get a <see cref="ReadOnlySpan{T}"/> at the given virtual address range.
        /// </summary>
        /// <param name="range">Virtual address range</param>
        /// <returns></returns>
        public ReadOnlySpan<byte> GetReadOnlySpanAt(ValueRange range)
        {
            return GetSpanAt(range);
        }

        /// <summary>
        /// Get a mutable slice of the ROM data for a specific <see cref="VirtualFile"/>.
        /// </summary>
        /// <param name="file">File used to get slice.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public Span<byte> GetSpan(in VirtualFile file)
        {
            if (!file.Storage.ContainsData())
            {
                throw new ArgumentException($"File must contain physical data to get span: {file}", "file");
            }
            var range = file.ToRange();
            return Buffer.Span.Slice((int)range.Start, (int)range.Length);
        }

        /// <summary>
        /// Get a <see cref="Span{T}"/> for a given file index.
        /// </summary>
        /// <param name="fileIndex">File index</param>
        /// <returns></returns>
        public Span<byte> GetSpan(int fileIndex)
        {
            return GetSpan(in Files[fileIndex]);
        }

        /// <summary>
        /// Get a <see cref="Span{T}"/> for a given file index, with data offset.
        /// </summary>
        /// <param name="fileIndex">File index</param>
        /// <param name="start">Data offset</param>
        /// <returns></returns>
        public Span<byte> GetSpan(int fileIndex, int start)
        {
            return GetSpan(in Files[fileIndex]).Slice(start);
        }

        /// <summary>
        /// Get a <see cref="Span{T}"/> for a given file index, with data offset and length.
        /// </summary>
        /// <param name="fileIndex">File index</param>
        /// <param name="start">Data offset</param>
        /// <param name="length">Data length</param>
        /// <returns></returns>
        public Span<byte> GetSpan(int fileIndex, int start, int length)
        {
            return GetSpan(in Files[fileIndex]).Slice(start, length);
        }

        /// <summary>
        /// Get a <see cref="Span{T}"/> at the given virtual address range.
        /// </summary>
        /// <param name="range">Virtual address range</param>
        /// <returns></returns>
        public Span<byte> GetSpanAt(ValueRange range)
        {
            var result = Resolve(range);
            var (index, offset) = result!.Value;
            return GetSpan(index, (int)offset, (int)range.Length);
        }

        /// <summary>
        /// Resolve the file index and data offset for a given virtual ROM address.
        /// </summary>
        /// <param name="vrom">Virtual ROM address</param>
        /// <returns></returns>
        public (int, uint)? Resolve(uint vrom)
        {
            for (var i = 0; i < Files.Length; i++)
            {
                ref readonly var file = ref Files[i];
                if (file.Storage.ContainsData() && file.VirtualRange.Contains(vrom))
                    return (i, vrom - file.VirtualStart);
            }

            return null;
        }

        /// <summary>
        /// Resolve the file index and data offset for a given virtual ROM address range.
        /// </summary>
        /// <param name="range">Virtual ROM address range</param>
        /// <returns></returns>
        public (int, uint)? Resolve(ValueRange range)
        {
            for (var i = 0; i < Files.Length; i++)
            {
                ref readonly var file = ref Files[i];
                if (file.Storage.ContainsData() && file.VirtualRange.Contains(range))
                    return (i, range.Start - file.VirtualStart);
            }

            return null;
        }

        /// <summary>
        /// Write the virtual file table to ROM.
        /// </summary>
        public void WriteFileTable()
        {
            var region = GetSpan(Files[TableIndex]);
            for (int i = 0; i < Files.Length; i++)
            {
                ref var row = ref Files[i];
                row.Write(region.Slice(i * 0x10, 0x10));
            }
        }

        /// <summary>
        /// Update the CRC values in the header of the ROM data.
        /// </summary>
        public void UpdateCRC()
        {
            var crc = CRC(Buffer.Span);
            BinaryPrimitives.WriteUInt32BigEndian(Buffer.Span.Slice(0x10, 4), crc.Item1);
            BinaryPrimitives.WriteUInt32BigEndian(Buffer.Span.Slice(0x14, 4), crc.Item2);
        }

        /// <summary>
        /// Generate the CRC values for the given ROM data.
        /// </summary>
        /// <param name="rom">ROM data</param>
        /// <returns></returns>
        public static (uint, uint) CRC(ReadOnlySpan<byte> rom)
        {
            // Reference: http://n64dev.org/n64crc.html
            uint crc0, crc1;
            uint seed = 0xDF26F436;
            uint t1, t2, t3, t4, t5, t6, r, d;
            t1 = t2 = t3 = t4 = t5 = t6 = seed;
            for (int i = 0x1000; i < 0x101000; i += 4)
            {
                d = BinaryPrimitives.ReadUInt32BigEndian(rom.Slice(i, 4));
                if ((t6 + d) < t6) { t4++; }
                t6 += d;
                t3 ^= d;
                r = (d << (byte)(d & 0x1F)) | (d >> (byte)(32 - (d & 0x1F)));
                t5 += r;
                if (t2 < d)
                {
                    t2 ^= (t6 ^ d);
                }
                else
                {
                    t2 ^= r;
                }
                t1 += BinaryPrimitives.ReadUInt32BigEndian(rom.Slice(0x750 + (i & 0xFF))) ^ d;
            }
            crc0 = t6 ^ t4 ^ t3;
            crc1 = t5 ^ t2 ^ t1;
            return (crc0, crc1);
        }

        /// <summary>
        /// Read a <see cref="RomFile"/> from a given file path.
        /// </summary>
        /// <param name="filepath">Path to ROM file.</param>
        /// <returns></returns>
        public static RomFile From(string filepath, int offset = MajoraTableOffset)
        {
            using (var stream = File.OpenRead(filepath))
            {
                return From(stream, offset);
            }
        }

        /// <summary>
        /// Read a <see cref="RomFile"/> from a given <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">Stream to read from.</param>
        /// <param name="offset">Offset of <c>dmadata</c> file table</param>
        /// <returns></returns>
        public static RomFile From(Stream stream, int offset = MajoraTableOffset)
        {
            var buffer = new byte[ExpectedLength];
            var amount = stream.Read(buffer);
            if (amount != buffer.Length)
            {
                throw new Exception("ROM must be exactly 32 MiB in size.");
            }
            var (files, tableIndex) = ReadFileTable(buffer, offset);
            return new RomFile(buffer, files, tableIndex);
        }

        /// <summary>
        /// Read <see cref="VirtualFile"/> table (dmadata) from ROM data.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset">Table offset.</param>
        /// <returns>Tuple containing <see cref="VirtualFile"/> entries in table, and <c>dmadata</c> file index.</returns>
        static (VirtualFile[], int) ReadFileTable(ReadOnlySpan<byte> buffer, int offset)
        {
            int end = buffer.Length;
            int tableIndex = -1;
            var list = new List<VirtualFile>();
            for (var index = offset; index < end; index += 0x10)
            {
                var span = buffer.Slice(index, 0x10);
                var vfile = VirtualFile.Read(span);
                if (vfile.VirtualStart == (uint)offset)
                {
                    end = (int)vfile.VirtualEnd;
                    tableIndex = (index - offset) / 0x10;
                }
                list.Add(vfile);
            }
            return (list.ToArray(), tableIndex);
        }
    }
}
