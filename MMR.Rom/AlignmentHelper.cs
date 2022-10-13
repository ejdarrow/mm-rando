namespace MMR.Rom
{
    public static class AlignmentHelper
    {
        /// <summary>
        /// Align upwards to nearest <c>0x10</c> boundary.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static uint AlignLenient(uint value)
        {
            if (value % 0x10 != 0)
                return (value + 0x10) & 0xFFFFFFF0;
            return value;
        }

        /// <summary>
        /// Whether or not this value is aligned on a <c>0x10</c> boundary.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsAlignedLenient(uint value) => value % 0x10 == 0;

        /// <summary>
        /// Align upwards to nearest <c>0x1000</c> boundary.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static uint AlignStrict(uint value)
        {
            if (value % 0x1000 != 0)
                return (value + 0x1000) & 0xFFFFF000;
            return value;
        }

        /// <summary>
        /// Whether or not this value is aligned on a <c>0x1000</c> boundary.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsAlignedStrict(uint value) => value % 0x1000 == 0;
    }
}
