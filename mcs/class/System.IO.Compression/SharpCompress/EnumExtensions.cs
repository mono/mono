using System;

namespace SharpCompress
{
    internal static class EnumExtensions
    {
        public static bool HasFlag(this Enum enumRef, Enum flag)
        {
            long value = Convert.ToInt64(enumRef);
            long flagVal = Convert.ToInt64(flag);

            return (value & flagVal) == flagVal;
        }
    }
}