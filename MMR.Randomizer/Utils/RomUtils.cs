using Be.IO;
using MMR.Randomizer.Models.Rom;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Threading;
using System.Numerics;
using MMR.Rom;

namespace MMR.Randomizer.Utils
{
    using Yaz = Yaz.Yaz;

    public static class RomUtils
    {
        const int FILE_TABLE = 0x1A500;
        const int SIGNATURE_ADDRESS = 0x1A4D0;
        public static void SetStrings(byte[] hack, string ver, string setting)
        {
            ResourceUtils.ApplyHack(hack);
            int veraddr = 0xC44E30;
            int settingaddr = 0xC44E70;
            string verstring = $"MM Rando {ver}\x00";
            string settingstring = $"{setting}\x00";

            var code = RomData.Files.GetCached(FileIndex.code.ToInt());

            // Write version string.
            {
                var buffer = Encoding.ASCII.GetBytes(verstring);
                var range = ValueRange.WithLength((uint)veraddr, (uint)buffer.Length);
                var dest = code.ToSpan(range);
                buffer.CopyTo(dest);
            }

            // Write setting string.
            {
                var buffer = Encoding.ASCII.GetBytes(settingstring);
                var range = ValueRange.WithLength((uint)settingaddr, (uint)buffer.Length);
                var dest = code.ToSpan(range);
                buffer.CopyTo(dest);
            }
        }

        public static int AddNewFile(byte[] content)
        {
            var file = RomData.Files.Append(content);
            return (int)file.AddressRange.Start;
        }

        public static int AddrToFile(int RAddr)
        {
            return RomData.Files.ResolveIndex((uint)RAddr);
        }

        public static List<byte[]> GetFilesFromArchive(int fileIndex)
        {
            var data = RomData.Files.GetReadOnlySpan(fileIndex);
            var headerLength = ReadWriteUtils.Arr_ReadS32(data, 0);
            var pointer = headerLength;
            var files = new List<byte[]>();
            for (var i = 4; i < headerLength; i += 4)
            {
                var nextFileOffset = headerLength + ReadWriteUtils.Arr_ReadS32(data, i);
                var fileLength = nextFileOffset - pointer;
                // Copy file data.
                var dest = data.Slice(pointer, fileLength).ToArray();
                pointer += fileLength;
                var decompressed = Yaz.Decode(dest);
                files.Add(decompressed);
            }
            return files;
        }

        public static int GetFileIndexForWriting(int rAddr)
        {
            int index = AddrToFile(rAddr);
            return index;
        }

        public static int ByteswapROM(string filename)
        {
            using (BinaryReader ROM = new BinaryReader(File.OpenRead(filename)))
            {
                if (ROM.BaseStream.Length % 4 != 0)
                {
                    return -1;
                }

                byte[] buffer = new byte[4];
                ROM.Read(buffer, 0, 4);
                // very hacky
                ROM.BaseStream.Seek(0, 0);
                if (buffer[0] == 0x80)
                {
                    return 1;
                }
                else if (buffer[1] == 0x80)
                {
                    using (BinaryWriter newROM = new BinaryWriter(File.Open(filename + ".z64", FileMode.Create)))
                    {
                        while (ROM.BaseStream.Position < ROM.BaseStream.Length)
                        {
                            newROM.Write(ReadWriteUtils.Byteswap16(ReadWriteUtils.ReadU16(ROM)));
                        }
                    }
                    return 0;
                }
                else if (buffer[3] == 0x80)
                {
                    using (BinaryWriter newROM = new BinaryWriter(File.Open(filename + ".z64", FileMode.Create)))
                    {
                        while (ROM.BaseStream.Position < ROM.BaseStream.Length)
                        {
                            newROM.Write(ReadWriteUtils.Byteswap32(ReadWriteUtils.ReadU32(ROM)));
                        }
                    }
                    return 0;
                }
            }
            return -1;
        }

        public static void WriteROM(string fileName, ReadOnlySpan<byte> ROM)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(fileName, FileMode.Create)))
            {
                writer.Write(ROM);
            }
        }

        public static byte[][] CompressMMFiles(FileTable files)
        {
            /// Re-Compressing the files back into a compressed rom is the most expensive job during seed creation.
            /// To speed up, we compress files in parallel with a sorted list to reduce idle threads at the end.

            var startTime = DateTime.Now;
            var results = new byte[files.Length][];

            // sorting the list with .Where().ToList() => OrderByDescending().ToList only takes (~ 0.400 miliseconds) on Isghj's computer
            var sortedCompressibleFiles = files.GetFilesToCompress();
            sortedCompressibleFiles = sortedCompressibleFiles.OrderByDescending(file => file.Length).ToList();

            // Debug.WriteLine($" sort the list with Sort() : [{(DateTime.Now).Subtract(startTime).TotalMilliseconds} (ms)]");

            // lower priority so that the rando can't lock a badly scheduled CPU by using 100%
            var previousThreadPriority = Thread.CurrentThread.Priority;
            Thread.CurrentThread.Priority = ThreadPriority.Lowest;
            // yaz0 encode all of the files for the rom
            Parallel.ForEach(sortedCompressibleFiles.AsParallel().AsOrdered(), file =>
            {
                //var yazTime = DateTime.Now;
                results[file.Index] = Yaz.EncodeAndCopy(file.ToReadOnlySpan());
                //Debug.WriteLine($" size: [{file.Data.Length}] time to complete compression : [{(DateTime.Now).Subtract(yazTime).TotalMilliseconds} (ms)]");
            });
            // this thread is borrowed, we don't want it to always be the lowest priority, return to previous state
            Thread.CurrentThread.Priority = previousThreadPriority;

            Debug.WriteLine($" compress all files time : [{(DateTime.Now).Subtract(startTime).TotalMilliseconds} (ms)]");

            return results;
        }

        public static RomFile BuildROM()
        {
            var compressed = CompressMMFiles(RomData.Files);
            var rom = RomData.Files.Build(compressed);
            SequenceUtils.UpdateBankInstrumentPointers(rom);
            rom.WriteFileTable();
            SignROM(rom);
            rom.UpdateCRC();
            return rom;
        }

        private static void SignROM(RomFile rom)
        {
            var values = new List<string>
            {
                "MajoraRando",
                DateTime.UtcNow.ToString("yy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                "\x01\x02" // versionCode
            };
            var signature = string.Join('\x00', values);
            var bytes = Encoding.Latin1.GetBytes(signature);
            var range = ValueRange.WithLength(SIGNATURE_ADDRESS, 0x30);
            bytes.CopyTo(rom.GetSpanAt(range));
        }

        public static void ReadFileTable(string filepath)
        {
            RomData.Files = new FileTable(RomFile.From(filepath, FILE_TABLE));
        }

        public static bool CheckOldCRC(BinaryReader ROM)
        {
            ROM.BaseStream.Seek(16, 0);
            uint CRC1 = ReadWriteUtils.ReadU32(ROM);
            uint CRC2 = ReadWriteUtils.ReadU32(ROM);
            return (CRC1 == 0x5354631C) && (CRC2 == 0x03A2DEF0);
        }

        public static bool ValidateROM(string FileName)
        {
            bool res = false;
            using (BinaryReader ROM = new BinaryReader(File.OpenRead(FileName)))
            {
                if (ROM.BaseStream.Length == 0x2000000)
                {
                    res = CheckOldCRC(ROM);
                }
            }
            return res;
        }

        /// <summary>
        /// Append a <see cref="MMFile"/> without a static virtual address to the end of the list.
        /// </summary>
        /// <param name="data">File data</param>
        /// <param name="isCompressed">Is file compressed</param>
        /// <returns>File index</returns>
        public static int AppendFile(byte[] data, bool isCompressed = false)
        {
            var cached = RomData.Files.Append(data, isCompressed);
            return cached.Index;
        }
    }

}
