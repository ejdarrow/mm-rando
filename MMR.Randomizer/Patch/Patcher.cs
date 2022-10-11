using Be.IO;
using MMR.Common.Extensions;
using MMR.Rom;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using VCDiff.Decoders;
using VCDiff.Encoders;
using VCDiff.Includes;

namespace MMR.Randomizer.Patch
{
    public static class Patcher
    {
        /// <summary>
        /// Patch file magic number ("MMRP").
        /// </summary>
        public const uint PatchMagic = 0x4D4D5250;

        private static readonly byte[] key;
        private static readonly byte[] iv;

        static Patcher()
        {
            var bigInt = new BigInteger(typeof(Patcher).Assembly.ManifestModule.ModuleVersionId.ToByteArray());
            var random = new Random((int)(bigInt & int.MaxValue));
            var buffer = new byte[16];
            random.NextBytes(buffer);
            key = buffer.ToArray();
            random.NextBytes(buffer);
            iv = buffer.ToArray();
        }

        /// <summary>
        /// Apply a patch entry.
        /// </summary>
        /// <param name="header">Patch entry header.</param>
        /// <param name="data">Patch entry data.</param>
        /// <param name="fileTable">File table.</param>
        /// <exception cref="NotSupportedException"></exception>
        static void ApplyPatchEntry(PatchHeader header, byte[] data, FileTable fileTable)
        {
            var index = checked((int)header.Index);
            var file = fileTable[index];

            if (header.Command == PatchCommand.MetaOnly)
            {
                file.ApplyMetaInfo(header.AddressRange, header.Storage);
            }
            else if (header.Command == PatchCommand.ExistingData)
            {
                var (original, _) = fileTable.LoadFromRom(index);
                var patched = VcDiffDecodeManaged(original, data);
                fileTable.ResizeWithData(index, patched);

                file.ApplyMetaInfo(header.AddressRange, header.Storage);
            }
            else if (header.Command == PatchCommand.NewData)
            {
                file.ApplyMetaInfoWithData(header.AddressRange, header.Storage, data);
            }
            else
            {
                throw new NotSupportedException($"Unknown {typeof(PatchCommand).Name}: {(byte)header.Command}");
            }
        }

        /// <summary>
        /// Apply encrypted patch data from file at given path to the ROM.
        /// </summary>
        /// <param name="filePath">Patch file path.</param>
        /// <param name="fileTable"></param>
        /// <returns><see cref="SHA256"/> hash of the patch.</returns>
        public static byte[] ApplyPatchEncrypted(string filePath, FileTable fileTable)
        {
            using var outStream = File.OpenRead(filePath);
            return ApplyPatchEncrypted(outStream, fileTable);
        }

        /// <summary>
        /// Apply encrypted patch data from given <see cref="Stream"/> to the ROM.
        /// </summary>
        /// <param name="inStream">Input stream.</param>
        /// <param name="fileTable"></param>
        /// <returns><see cref="SHA256"/> hash of the patch.</returns>
        public static byte[] ApplyPatchEncrypted(Stream inStream, FileTable fileTable)
        {
            var aes = Aes.Create();
            using (var cryptoStream = new CryptoStream(inStream, aes.CreateDecryptor(key, iv), CryptoStreamMode.Read))
            {
                return ApplyPatch(cryptoStream, fileTable);
            }
        }

        /// <summary>
        /// Apply patch data from given <see cref="Stream"/> to the ROM.
        /// </summary>
        /// <param name="inStream">Input stream.</param>
        /// <param name="fileTable"></param>
        /// <returns><see cref="SHA256"/> hash of the patch.</returns>
        public static byte[] ApplyPatch(Stream inStream, FileTable fileTable)
        {
            try
            {
                var hashAlg = new SHA256Managed();
                using (var hashStream = new CryptoStream(inStream, hashAlg, CryptoStreamMode.Read))
                using (var decompressStream = new GZipStream(hashStream, CompressionMode.Decompress))
                using (var memoryStream = new MemoryStream())
                {
                    // Fully decompress into MemoryStream so that we can access Position to check for end of Stream.
                    decompressStream.CopyTo(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    using var reader = new BeBinaryReader(memoryStream);

                    // Validate patch magic.
                    var magic = reader.ReadUInt32();
                    ValidateMagic(magic);

                    Span<byte> headerBytes = stackalloc byte[PatchHeader.Size];
                    while (reader.BaseStream.Position != reader.BaseStream.Length)
                    {
                        // Read header bytes into stack buffer to prevent allocation.
                        reader.ReadExact(headerBytes);
                        var header = PatchHeader.Read(headerBytes);
                        var data = reader.ReadBytes(header.Length);
                        ApplyPatchEntry(header, data, fileTable);
                    }
                }
                return hashAlg.Hash;
            }
            catch
            {
                throw new IOException("Failed to apply patch. Patch may be invalid.");
            }
        }

        /// <summary>
        /// Create hash of patch data from current ROM state.
        /// </summary>
        /// <param name="fileTable"></param>
        /// <returns><see cref="SHA256"/> hash of the patch.</returns>
        public static byte[] CreatePatch(FileTable fileTable)
        {
            return CreatePatch(Stream.Null, fileTable);
        }

        /// <summary>
        /// Create encrypted patch data from current ROM state and write to a file.
        /// </summary>
        /// <param name="filePath">Output file path.</param>
        /// <param name="fileTable"></param>
        /// <returns><see cref="SHA256"/> hash of the patch.</returns>
        public static byte[] CreatePatchEncrypted(string filePath, FileTable fileTable)
        {
            using var outStream = File.Open(filePath, FileMode.Create);
            return CreatePatchEncrypted(outStream, fileTable);
        }

        /// <summary>
        /// Create encrypted patch data from current ROM state and write to <see cref="Stream"/>.
        /// </summary>
        /// <param name="outStream">Output stream.</param>
        /// <param name="fileTable"></param>
        /// <returns></returns>
        public static byte[] CreatePatchEncrypted(Stream outStream, FileTable fileTable)
        {
            var aes = Aes.Create();
            using (var cryptoStream = new CryptoStream(outStream, aes.CreateEncryptor(key, iv), CryptoStreamMode.Write))
            {
                return CreatePatch(cryptoStream, fileTable);
            }
        }

        /// <summary>
        /// Create patch data from current ROM state and write to <see cref="Stream"/>.
        /// </summary>
        /// <param name="outStream">Output stream.</param>
        /// <param name="fileTable"></param>
        /// <returns><see cref="SHA256"/> hash of the patch.</returns>
        public static byte[] CreatePatch(Stream outStream, FileTable fileTable)
        {
            var hashAlg = new SHA256Managed();
            using (var hashStream = new CryptoStream(outStream, hashAlg, CryptoStreamMode.Write))
            using (var compressStream = new GZipStream(hashStream, CompressionMode.Compress))
            using (var writer = new BeBinaryWriter(compressStream))
            {
                // Write magic value.
                writer.WriteUInt32(PatchMagic);

                Span<byte> headerBytes = stackalloc byte[PatchHeader.Size];
                for (var fileIndex = 0; fileIndex < fileTable.Length; fileIndex++)
                {
                    var file = fileTable[fileIndex];
                    ref readonly var orig = ref fileTable.Rom[fileIndex];
                    var index = checked((ushort)fileIndex);

                    if (file.Modified && file.Storage.ContainsData() && orig.Storage.ContainsData())
                    {
                        var (original, compressed) = fileTable.LoadFromRom(fileIndex);
                        var diff = VcDiffEncodeManaged(original, file.GetData());

                        // Create header for patching existing file data.
                        var header = PatchHeader.CreateExistingData(index, diff.Length, file.AddressRange, file.Storage);
                        header.Write(headerBytes);

                        // Write header bytes and diff bytes.
                        writer.Write(headerBytes);
                        writer.Write(diff);
                    }
                    else if (file.Modified && file.Storage.ContainsData())
                    {
                        var span = file.ToReadOnlySpan();

                        // Create header for writing new file data.
                        var header = PatchHeader.CreateNewData(index, span.Length, file.AddressRange, file.Storage);
                        header.Write(headerBytes);

                        // Write header bytes and file bytes.
                        writer.Write(headerBytes);
                        writer.Write(span);
                    }
                    else if (fileTable.IsMetaModified(fileIndex))
                    {
                        // Create header for modifying file meta data.
                        var header = PatchHeader.CreateMetaOnly(index, file.AddressRange, file.Storage);
                        header.Write(headerBytes);

                        // Write header bytes.
                        writer.Write(headerBytes);
                    }
                }
            }
            return hashAlg.Hash;
        }

        /// <summary>
        /// Validate magic value and throw a <see cref="PatchMagicException"/> if invalid.
        /// </summary>
        /// <param name="magic">Magic value</param>
        static void ValidateMagic(uint magic)
        {
            if (magic != PatchMagic)
            {
                throw new PatchMagicException(magic);
            }
        }

        /// <summary>
        /// Perform VCDiff decode (apply a diff).
        /// </summary>
        /// <param name="original">Original file data.</param>
        /// <param name="patch">Diff data.</param>
        /// <returns>Modified file data.</returns>
        static byte[] VcDiffDecodeManaged(byte[] original, byte[] patch)
        {
            using var output = new MemoryStream();
            using var dict = new MemoryStream(original);
            using var target = new MemoryStream(patch);

            // Decode using patch data.
            var decoder = new VcDecoder(dict, target, output);
            if (decoder.Decode(out var written) != VCDiffResult.SUCCESS)
            {
                throw new Exception("VCDiff decode failed.");
            }
            return output.ToArray();
        }

        /// <summary>
        /// Perform VCDiff encode (create a diff).
        /// </summary>
        /// <param name="original">Original file data.</param>
        /// <param name="modified">Modified file data.</param>
        /// <returns>Diff data.</returns>
        static byte[] VcDiffEncodeManaged(byte[] original, byte[] modified)
        {
            using var output = new MemoryStream();
            using var dict = new MemoryStream(original);
            using var target = new MemoryStream(modified);

            // Encode and write patch data.
            var encoder = new VcEncoder(dict, target, output);
            if (encoder.Encode(false) != VCDiffResult.SUCCESS)
            {
                throw new Exception("VCDiff encode failed.");
            }
            return output.ToArray();
        }
    }
}
