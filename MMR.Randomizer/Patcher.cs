﻿using System;
using System.Linq;
using System.Numerics;

namespace MMR.Randomizer
{
    public static class Patcher
    {
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
        /// Apply a patch file.
        /// </summary>
        /// <param name="filePath">File path</param>
        /// <returns></returns>
        public static byte[] Apply(string filePath)
        {
            return Patch.Patcher.ApplyPatchEncrypted(filePath, RomData.Files, key, iv);
        }

        /// <summary>
        /// Create the hash generated by patch data.
        /// </summary>
        /// <returns></returns>
        public static byte[] Create()
        {
            return Patch.Patcher.CreatePatch(RomData.Files);
        }

        /// <summary>
        /// Create a patch file.
        /// </summary>
        /// <param name="filePath">File path</param>
        /// <returns></returns>
        public static byte[] Create(string filePath)
        {
            return Patch.Patcher.CreatePatchEncrypted(filePath, RomData.Files, key, iv);
        }
    }
}
