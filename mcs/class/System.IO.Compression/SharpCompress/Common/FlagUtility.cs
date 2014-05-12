using System;

namespace SharpCompress.Common
{
    internal static class FlagUtility
    {
        /// <summary>
        /// Returns true if the flag is set on the specified bit field.
        /// Currently only works with 32-bit bitfields. 
        /// </summary>
        /// <typeparam name="T">Enumeration with Flags attribute</typeparam>
        /// <param name="bitField">Flagged variable</param>
        /// <param name="flag">Flag to test</param>
        /// <returns></returns>
        public static bool HasFlag<T>(long bitField, T flag)
            where T : struct
        {
            return HasFlag(bitField, flag);
        }

        /// <summary>
        /// Returns true if the flag is set on the specified bit field.
        /// Currently only works with 32-bit bitfields. 
        /// </summary>
        /// <typeparam name="T">Enumeration with Flags attribute</typeparam>
        /// <param name="bitField">Flagged variable</param>
        /// <param name="flag">Flag to test</param>
        /// <returns></returns>
        public static bool HasFlag<T>(ulong bitField, T flag)
            where T : struct
        {
            return HasFlag(bitField, flag);
        }

        /// <summary>
        /// Returns true if the flag is set on the specified bit field.
        /// Currently only works with 32-bit bitfields. 
        /// </summary>
        /// <param name="bitField">Flagged variable</param>
        /// <param name="flag">Flag to test</param>
        /// <returns></returns>
        public static bool HasFlag(ulong bitField, ulong flag)
        {
            return ((bitField & flag) == flag);
        }

        public static bool HasFlag(short bitField, short flag)
        {
            return ((bitField & flag) == flag);
        }

#if PORTABLE
    /// <summary>
    /// Generically checks enums in a Windows Phone 7 enivronment
    /// </summary>
    /// <param name="enumVal"></param>
    /// <param name="flag"></param>
    /// <returns></returns>
        public static bool HasFlag(this Enum enumVal, Enum flag)
        {
            if (enumVal.GetHashCode() > 0) //GetHashCode returns the enum value. But it's something very crazy if not set beforehand
            {
                ulong num = Convert.ToUInt64(flag.GetHashCode());
                return ((Convert.ToUInt64(enumVal.GetHashCode()) & num) == num);
            }
            else
            {
                return false;
            }
        }
#endif

        /// <summary>
        /// Returns true if the flag is set on the specified bit field.
        /// Currently only works with 32-bit bitfields. 
        /// </summary>
        /// <typeparam name="T">Enumeration with Flags attribute</typeparam>
        /// <param name="bitField">Flagged variable</param>
        /// <param name="flag">Flag to test</param>
        /// <returns></returns>
        public static bool HasFlag<T>(T bitField, T flag)
            where T : struct
        {
            return HasFlag(Convert.ToInt64(bitField), Convert.ToInt64(flag));
        }

        /// <summary>
        /// Returns true if the flag is set on the specified bit field.
        /// Currently only works with 32-bit bitfields. 
        /// </summary>
        /// <param name="bitField">Flagged variable</param>
        /// <param name="flag">Flag to test</param>
        /// <returns></returns>
        public static bool HasFlag(long bitField, long flag)
        {
            return ((bitField & flag) == flag);
        }


        /// <summary>
        /// Sets a bit-field to either on or off for the specified flag.
        /// </summary>
        /// <param name="bitField">Flagged variable</param>
        /// <param name="flag">Flag to change</param>
        /// <param name="on">bool</param>
        /// <returns>The flagged variable with the flag changed</returns>
        public static long SetFlag(long bitField, long flag, bool @on)
        {
            if (@on)
            {
                return bitField | flag;
            }
            return bitField & (~flag);
        }

        /// <summary>
        /// Sets a bit-field to either on or off for the specified flag.
        /// </summary>
        /// <typeparam name="T">Enumeration with Flags attribute</typeparam>
        /// <param name="bitField">Flagged variable</param>
        /// <param name="flag">Flag to change</param>
        /// <param name="on">bool</param>
        /// <returns>The flagged variable with the flag changed</returns>
        public static long SetFlag<T>(T bitField, T flag, bool @on)
            where T : struct
        {
            return SetFlag(Convert.ToInt64(bitField), Convert.ToInt64(flag), @on);
        }
    }
}