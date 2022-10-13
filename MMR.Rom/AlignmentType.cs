using System;

namespace MMR.Rom
{
    public static class AlignmentTypeExtensions
    {
        public static uint Align(this AlignmentType type, uint value)
        {
            return type switch
            {
                AlignmentType.Lenient => AlignmentHelper.AlignLenient(value),
                AlignmentType.Strict => AlignmentHelper.AlignStrict(value),
                _ => throw new NotSupportedException(),
            };
        }

        public static bool IsAligned(this AlignmentType type, uint value)
        {
            return type switch
            {
                AlignmentType.Lenient => AlignmentHelper.IsAlignedLenient(value),
                AlignmentType.Strict => AlignmentHelper.IsAlignedStrict(value),
                _ => throw new NotSupportedException(),
            };
        }
    }

    public enum AlignmentType
    {
        Lenient,
        Strict,
    }
}
