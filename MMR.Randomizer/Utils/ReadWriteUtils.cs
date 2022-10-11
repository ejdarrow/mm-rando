using MMR.Randomizer.Models.Rom;
using MMR.Rom;
using System;
using System.Buffers.Binary;
using System.IO;
using System.Runtime.CompilerServices;

namespace MMR.Randomizer.Utils
{

    public static class ReadWriteUtils
    {

        public static void WriteFileAddr(int[] Addr, byte[] data, Span<byte> file)
        {
            for (int i = 0; i < Addr.Length; i++)
            {
                var offset = Addr[i];
                data.CopyTo(file.Slice(offset, data.Length));
            }
        }

        public static void WriteROMAddr(int[] Addr, byte[] data)
        {
            for (int i = 0; i < Addr.Length; i++)
            {
                int var = (int)(Addr[i] & 0xF0000000) >> 28;
                int rAddr = Addr[i] & 0xFFFFFFF;
                byte[] rdata = data;
                if (var == 1)
                {
                    rdata[0] += 0xA;
                    rdata[1] -= 0x70;
                }
                var span = RomData.Files.GetSpanAt((uint)rAddr);
                rdata.CopyTo(span);
            }
        }

        public static void WriteToROM(int Addr, byte val)
        {
            var span = RomData.Files.GetSpanAt((uint)Addr);
            span[0] = val;
        }

        public static void WriteToROM(int Addr, ushort val)
        {
            var span = RomData.Files.GetSpanAt((uint)Addr);
            BinaryPrimitives.WriteUInt16BigEndian(span, val);
        }

        public static void WriteToROM(int Addr, uint val)
        {
            var span = RomData.Files.GetSpanAt((uint)Addr);
            BinaryPrimitives.WriteUInt32BigEndian(span, val);
        }

        public static void WriteToROM(int Addr, ReadOnlySpan<byte> val)
        {
            var span = RomData.Files.GetSpanAt((uint)Addr);
            val.CopyTo(span);
        }

        public static void Arr_Insert(byte[] src, int start, int len, byte[] dest, int addr)
        {
            for (int i = 0; i < len; i++)
            {
                dest[addr + i] = src[start + i];
            }
        }

        public static uint Byteswap32(uint val)
        {
            return ((val & 0xFF) << 24) | ((val & 0xFF00) << 8) | ((val & 0xFF0000) >> 8) | ((val & 0xFF000000) >> 24);
        }

        public static ushort Byteswap16(ushort val)
        {
            return (ushort)(((val & 0xFF) << 8) | ((val & 0xFF00) >> 8));
        }

        public static uint Arr_ReadU32(ReadOnlySpan<byte> span, int start)
        {
            return BinaryPrimitives.ReadUInt32BigEndian(span.Slice(start));
        }

        public static int Arr_ReadS32(ReadOnlySpan<byte> span, int start)
        {
            return BinaryPrimitives.ReadInt32BigEndian(span.Slice(start));
        }

        public static ushort Arr_ReadU16(ReadOnlySpan<byte> span, int start)
        {
            return BinaryPrimitives.ReadUInt16BigEndian(span.Slice(start));
        }

        public static short Arr_ReadS16(ReadOnlySpan<byte> span, int start)
        {
            return BinaryPrimitives.ReadInt16BigEndian(span.Slice(start));
        }

        public static void Arr_WriteU32(Span<byte> span, int start, uint value)
        {
            BinaryPrimitives.WriteUInt32BigEndian(span.Slice(start), value);
        }

        public static void Arr_WriteU16(Span<byte> span, int start, ushort value)
        {
            BinaryPrimitives.WriteUInt16BigEndian(span.Slice(start), value);
        }

        public static uint ReadU32(BinaryReader ROM)
        {
            return Byteswap32(ROM.ReadUInt32());
        }

        public static int ReadS32(BinaryReader ROM)
        {
            return (int)ReadU32(ROM);
        }

        public static ushort ReadU16(BinaryReader ROM)
        {
            return Byteswap16(ROM.ReadUInt16());
        }

        public static void WriteU32(BinaryWriter ROM, uint val)
        {
            ROM.Write(Byteswap32(val));
        }

        public static void WriteU16(BinaryWriter ROM, ushort val)
        {
            ROM.Write(Byteswap16(val));
        }

        public static ushort ReadU16(int address)
        {
            var span = RomData.Files.GetReadOnlySpanAt((uint)address);
            return BinaryPrimitives.ReadUInt16BigEndian(span);
        }

        public static uint ReadU32(int address)
        {
            var span = RomData.Files.GetReadOnlySpanAt((uint)address);
            return BinaryPrimitives.ReadUInt32BigEndian(span);
        }

        public static byte ReadU8(ReadOnlySpan<byte> span)
        {
            return span[0];
        }

        public static byte ReadU8(ReadOnlySpan<byte> span, int start)
        {
            return span[start];
        }

        public static short ReadS16(ReadOnlySpan<byte> span)
        {
            return BinaryPrimitives.ReadInt16BigEndian(span);
        }

        public static short ReadS16(ReadOnlySpan<byte> span, int start)
        {
            return BinaryPrimitives.ReadInt16BigEndian(span.Slice(start));
        }

        public static int ReadS32(ReadOnlySpan<byte> span)
        {
            return BinaryPrimitives.ReadInt32BigEndian(span);
        }

        public static int ReadS32(ReadOnlySpan<byte> span, int start)
        {
            return BinaryPrimitives.ReadInt32BigEndian(span.Slice(start));
        }

        public static ushort ReadU16(ReadOnlySpan<byte> span)
        {
            return BinaryPrimitives.ReadUInt16BigEndian(span);
        }

        public static ushort ReadU16(ReadOnlySpan<byte> span, int start)
        {
            return BinaryPrimitives.ReadUInt16BigEndian(span.Slice(start));
        }

        public static uint ReadU32(ReadOnlySpan<byte> span)
        {
            return BinaryPrimitives.ReadUInt32BigEndian(span);
        }

        public static uint ReadU32(ReadOnlySpan<byte> span, int start)
        {
            return BinaryPrimitives.ReadUInt32BigEndian(span.Slice(start));
        }

        public static void WriteU8(Span<byte> span, byte value)
        {
            span[0] = value;
        }

        public static void WriteU8(Span<byte> span, int start, byte value)
        {
            span[start] = value;
        }

        public static void WriteU16(Span<byte> span, ushort value)
        {
            BinaryPrimitives.WriteUInt16BigEndian(span, value);
        }

        public static void WriteU16(Span<byte> span, int start, ushort value)
        {
            BinaryPrimitives.WriteUInt16BigEndian(span.Slice(start), value);
        }

        public static void WriteU32(Span<byte> span, uint value)
        {
            BinaryPrimitives.WriteUInt32BigEndian(span, value);
        }

        public static void WriteU32(Span<byte> span, int start, uint value)
        {
            BinaryPrimitives.WriteUInt32BigEndian(span.Slice(start), value);
        }

        public static void WriteU64(Span<byte> span, ulong value)
        {
            BinaryPrimitives.WriteUInt64BigEndian(span, value);
        }

        public static void WriteS16(Span<byte> span, short value)
        {
            BinaryPrimitives.WriteInt16BigEndian(span, value);
        }

        public static void WriteS32(Span<byte> span, int value)
        {
            BinaryPrimitives.WriteInt32BigEndian(span, value);
        }

        public static void WriteS32(Span<byte> span, int start, int value)
        {
            BinaryPrimitives.WriteInt32BigEndian(span.Slice(start), value);
        }

        public static ValueRange ReadValueRange(ReadOnlySpan<byte> span)
        {
            return ValueRange.Read(span);
        }

        public static void WriteValueRange(Span<byte> span, ValueRange range)
        {
            range.Write(span);
        }

        public static byte[] Concat(ReadOnlySpan<byte> a, ReadOnlySpan<byte> b)
        {
            var dest = new byte[checked(a.Length + b.Length)];
            var span = dest.AsSpan();
            a.CopyTo(span);
            b.CopyTo(span.Slice(a.Length));
            return dest;
        }

        /// <summary>
        /// Copy source bytes into an equal or smaller sized destination buffer.
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="src"></param>
        public static void Copy(Span<byte> dest, ReadOnlySpan<byte> src)
        {
            src.Slice(0, dest.Length).CopyTo(dest);
        }

        /// <summary>
        /// Write source bytes into an equal or larger sized destination buffer.
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="src"></param>
        public static void Write(Span<byte> dest, ReadOnlySpan<byte> src)
        {
            src.CopyTo(dest);
        }

        public static void WriteExact(Span<byte> dest, ReadOnlySpan<byte> src)
        {
            if (dest.Length != src.Length)
            {
                throw new ArgumentException("Destination buffer length must match source buffer length.");
            }
            src.CopyTo(dest);
        }

        public static byte Read(int address)
        {
            var span = RomData.Files.GetReadOnlySpanAt((uint)address);
            return span[0];
        }

        public static byte[] ReadBytes(int address, uint count)
        {
            var span = RomData.Files.GetReadOnlySpanAt((uint)address, count);
            return span.ToArray();
        }

        /// <summary>
        /// Copy bytes from a source array to a dest array of a specific length.
        /// </summary>
        /// <param name="src">Source array</param>
        /// <param name="length">Dest length</param>
        /// <returns>Dest bytes</returns>
        public static byte[] CopyBytes(byte[] src, uint length)
        {
            var dest = new byte[length];
            var amount = Math.Min(src.Length, dest.Length);
            for (var i = 0; i < amount; i++)
                dest[i] = src[i];
            return dest;
        }

        /// <summary>
        /// Write 16-bit <see cref="ushort"/> value to ROM as big-endian.
        /// </summary>
        /// <param name="addr">VROM address</param>
        /// <param name="value">Value</param>
        public static void WriteU16ToROM(int addr, ushort value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            WriteToROM(addr, bytes);
        }

        /// <summary>
        /// Write 32-bit <see cref="uint"/> value to ROM as big-endian.
        /// </summary>
        /// <param name="addr">VROM address</param>
        /// <param name="value">Value</param>
        public static void WriteU32ToROM(int addr, uint value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            WriteToROM(addr, bytes);
        }

        /// <summary>
        /// Write 64-bit <see cref="ulong"/> value to ROM as big-endian.
        /// </summary>
        /// <param name="addr">VROM address</param>
        /// <param name="value">Value</param>
        public static void WriteU64ToROM(int addr, ulong value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            WriteToROM(addr, bytes);
        }

        /// <summary>
        /// VRAM load address of <c>code</c> file.
        /// </summary>
        const uint CodeRAMStart = 0x800A5AC0;

        /// <summary>
        /// Write a <c>NOP</c> instruction to the <c>code</c> file.
        /// </summary>
        /// <param name="vram">VRAM address within loaded <c>code</c> file.</param>
        public static void WriteCodeNOP(uint vram)
        {
            WriteCodeUInt32(vram, 0);
        }

        /// <summary>
        /// Get hi and lo values for a <c>lui</c>/<c>addiu</c> instruction pair.
        /// </summary>
        /// <param name="value">Full value</param>
        /// <returns></returns>
        public static (ushort, ushort) GetMipsSignedHiLo(uint value)
        {
            ushort hi = (ushort)(value >> 16);
            ushort lo = (ushort)(value & 0xFFFF);
            if (0x8000 <= lo)
            {
                return ((ushort)(hi + 1), lo);
            }
            return (hi, lo);
        }

        /// <summary>
        /// Write value for a <c>lui</c>/<c>addiu</c> instruction pair.
        /// </summary>
        /// <param name="address">Address of contiguous instructions</param>
        /// <param name="value">Full value</param>
        public static void WriteCodeSignedHiLo(uint address, uint value)
        {
            WriteCodeSignedHiLo(address, address + 4, value);
        }

        /// <summary>
        /// Write value for a <c>lui</c>/<c>addiu</c> instruction pair.
        /// </summary>
        /// <param name="hiAddress">Address of <c>lui</c> instruction</param>
        /// <param name="loAddress">Address of <c>addiu</c> instruction</param>
        /// <param name="value">Full value</param>
        public static void WriteCodeSignedHiLo(uint hiAddress, uint loAddress, uint value)
        {
            var span = RomData.Files.GetSpan(FileIndex.code.ToInt());
            var hiSpan = span.Slice((int)(hiAddress - CodeRAMStart), 4);
            var loSpan = span.Slice((int)(loAddress - CodeRAMStart), 4);
            WriteMipsSignedHiLo(hiSpan, loSpan, value);
        }

        /// <summary>
        /// Write value for a <c>lui</c>/<c>addiu</c> instruction pair.
        /// </summary>
        /// <param name="hiSpan">Hi instruction span</param>
        /// <param name="loSpan">Lo instruction span</param>
        /// <param name="value">Full value</param>
        public static void WriteMipsSignedHiLo(Span<byte> hiSpan, Span<byte> loSpan, uint value)
        {
            var (hi, lo) = GetMipsSignedHiLo(value);
            BinaryPrimitives.WriteUInt16BigEndian(hiSpan.Slice(2, 2), hi);
            BinaryPrimitives.WriteUInt16BigEndian(loSpan.Slice(2, 2), lo);
        }

        /// <summary>
        /// Write a 32-bit <see cref="uint"/> value to the <c>code</c> file.
        /// </summary>
        /// <param name="vram">VRAM address within loaded <c>code</c> file.</param>
        /// <param name="value">Value to write.</param>
        public static void WriteCodeUInt32(uint vram, uint value)
        {
            var offset = vram - CodeRAMStart;
            var span = RomData.Files.GetSpan(FileIndex.code.ToInt());
            var slice = span.Slice((int)offset, 4);
            WriteUInt32(slice, value);
        }

        /// <summary>
        /// Write a 32-bit <see cref="uint"/> value to a <see cref="Span{byte}"/> in big-endian order.
        /// </summary>
        /// <param name="span">Span to write to.</param>
        /// <param name="value">Value to write.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt32(Span<byte> span, uint value)
        {
            span[0] = (byte)((value >> 24) & 0xFF);
            span[1] = (byte)((value >> 16) & 0xFF);
            span[2] = (byte)((value >> 8) & 0xFF);
            span[3] = (byte)(value & 0xFF);
        }
    }

}
