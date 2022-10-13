using MMR.Randomizer.Models.Rom;
using MMR.Rom;
using System;

namespace MMR.Randomizer.Utils
{

    public static class ObjUtils
    {
        const int OBJECT_TABLE = 0xC58C80;
        public static int GetObjSize(int obj)
        {
            var range = GetObjectAddressRange(obj);
            return (int)range.Length;
        }

        public static ValueRange GetObjectAddressRange(int objectIndex)
        {
            var offset = objectIndex * 8;
            var span = RomData.Files.GetReadOnlySpan(FileIndex.code.ToInt(), OBJECT_TABLE);
            return ReadWriteUtils.ReadValueRange(span.Slice(offset));
        }

        public static int GetObjectFileIndex(int objectIndex)
        {
            var range = GetObjectAddressRange(objectIndex);
            return RomData.Files.Rom.FindExact(range.Start).Value;
        }

        public static ReadOnlySpan<byte> GetObjectReadOnlySpan(int objectIndex)
        {
            var fileIndex = GetObjectFileIndex(objectIndex);
            return RomData.Files.GetReadOnlySpan(fileIndex);
        }

        public static Span<byte> GetObjectData(int objectIndex)
        {
            var fileIndex = GetObjectFileIndex(objectIndex);
            return RomData.Files.GetSpan(fileIndex);
        }

        public static void InsertIndexedObj(int index, int replace, params byte[][] obj)
        {
            InsertObj(obj[index], replace);
        }

        /// <summary>
        /// Whether or not to use legacy length check for determining if object file should be relocated to end of file table.
        /// </summary>
        const bool ShouldUseLegacyInsertion = false;

        public static (int, bool) InsertObj(byte[] obj, int objectIndex)
        {
            var offset = objectIndex * 8;
            var table = RomData.Files.GetSpan(FileIndex.code.ToInt(), OBJECT_TABLE);
            var entry = table.Slice(offset);
            var range = ReadWriteUtils.ReadValueRange(entry);
            var fileIndex = RomData.Files.Rom.FindExact(range.Start).Value;
            var available = RomData.Files.GetAvailableAddressRange(fileIndex);

            if (ShouldUseLegacyInsertion
                ? obj.Length <= RomData.Files.GetAddressRange(fileIndex).Length
                : obj.Length <= available.Length)
            {
                // If available virtual address space, just resize.
                RomData.Files.ResizeWithData(fileIndex, obj);
                var file = RomData.Files.GetCached(fileIndex);
                ReadWriteUtils.WriteValueRange(table.Slice(offset), file.AddressRange);
                return (fileIndex, false);
            }
            else
            {
                // Otherwise, relocate object data to appended file.
                RomData.Files.Delete(fileIndex);
                var file = RomData.Files.Append(obj, true);
                ReadWriteUtils.WriteValueRange(table.Slice(offset), file.AddressRange);
                return (file.Index, true);
            }
        }
    }

}
