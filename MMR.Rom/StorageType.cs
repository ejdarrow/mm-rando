namespace MMR.Rom
{
    public static class StorageTypeExtensions
    {
        /// <summary>
        /// Whether or not this <see cref="StorageType"/> may contain physical data.
        /// </summary>
        /// <param name="storageType"></param>
        /// <returns></returns>
        public static bool ContainsData(this StorageType storageType)
        {
            return storageType == StorageType.Plain || storageType == StorageType.Compressed;
        }
    }

    /// <summary>
    /// Storage type of file data.
    /// </summary>
    public enum StorageType : byte
    {
        /// <summary>
        /// Data is non-compressed.
        /// </summary>
        Plain,

        /// <summary>
        /// Data is <c>Yaz0</c>-compressed.
        /// </summary>
        Compressed,

        /// <summary>
        /// Data was assigned virtual address space, but has no physical representation.
        /// </summary>
        Deleted,

        /// <summary>
        /// Data is not present because file row is unused (all values are <c>0</c>).
        /// </summary>
        Unused,
    }
}
