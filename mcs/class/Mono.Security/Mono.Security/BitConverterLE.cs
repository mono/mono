//
// Mono.Security.BitConverterLE.cs
//  Like System.BitConverter but always little endian
//
// Author:
//   Bernie Solomon
//

using System;

namespace Mono.Security
{
	internal sealed class BitConverterLE
	{
		private BitConverterLE ()
		{
		}

		unsafe private static byte[] GetUShortBytes (byte *bytes)
		{
			if (BitConverter.IsLittleEndian)
				return new byte [] { bytes [0], bytes [1] };
			else
				return new byte [] { bytes [1], bytes [0] };
		}

		unsafe private static byte[] GetUIntBytes (byte *bytes)
		{
			if (BitConverter.IsLittleEndian)
				return new byte [] { bytes [0], bytes [1], bytes [2], bytes [3] };
			else
				return new byte [] { bytes [3], bytes [2], bytes [1], bytes [0] };
		}

		unsafe private static byte[] GetULongBytes (byte *bytes)
		{
			if (BitConverter.IsLittleEndian)
				return new byte [] { bytes [0], bytes [1], bytes [2], bytes [3],
						     bytes [4], bytes [5], bytes [6], bytes [7] };
			else
				return new byte [] { bytes [7], bytes [6], bytes [5], bytes [4],
						     bytes [3], bytes [2], bytes [1], bytes [0] };
		}

		unsafe internal static byte[] GetBytes (bool value)
		{
			return new byte [] { value ? (byte)1 : (byte)0 };
		}

		unsafe internal static byte[] GetBytes (char value)
		{
			return GetUShortBytes ((byte *) &value);
		}

		unsafe internal static byte[] GetBytes (short value)
		{
			return GetUShortBytes ((byte *) &value);
		}

		unsafe internal static byte[] GetBytes (int value)
		{
			return GetUIntBytes ((byte *) &value);
		}

		unsafe internal static byte[] GetBytes (long value)
		{
			return GetULongBytes ((byte *) &value);
		}

		unsafe internal static byte[] GetBytes (ushort value)
		{
			return GetUShortBytes ((byte *) &value);
		}

		unsafe internal static byte[] GetBytes (uint value)
		{
			return GetUIntBytes ((byte *) &value);
		}

		unsafe internal static byte[] GetBytes (ulong value)
		{
			return GetULongBytes ((byte *) &value);
		}

		unsafe internal static byte[] GetBytes (float value)
		{
			return GetUIntBytes ((byte *) &value);
		}

		unsafe internal static byte[] GetBytes (double value)
		{
			return GetULongBytes ((byte *) &value);
		}

		unsafe private static void UShortFromBytes (byte *dst, byte[] src, int startIndex)
		{
			if (BitConverter.IsLittleEndian) {
				dst [0] = src [startIndex];
				dst [1] = src [startIndex + 1];
			} else {
				dst [0] = src [startIndex + 1];
				dst [1] = src [startIndex];
			}
		}

		unsafe private static void UIntFromBytes (byte *dst, byte[] src, int startIndex)
		{
			if (BitConverter.IsLittleEndian) {
				dst [0] = src [startIndex];
				dst [1] = src [startIndex + 1];
				dst [2] = src [startIndex + 2];
				dst [3] = src [startIndex + 3];
			} else {
				dst [0] = src [startIndex + 3];
				dst [1] = src [startIndex + 2];
				dst [2] = src [startIndex + 1];
				dst [3] = src [startIndex];
			}
		}

		unsafe private static void ULongFromBytes (byte *dst, byte[] src, int startIndex)
		{
			if (BitConverter.IsLittleEndian) {
				for (int i = 0; i < 8; ++i)
					dst [i] = src [startIndex + i];
			} else {
				for (int i = 0; i < 8; ++i)
					dst [i] = src [startIndex + (7 - i)];
			}
		}

		unsafe internal static bool ToBoolean (byte[] value, int startIndex)
		{
			return value [startIndex] != 0;
		}

		unsafe internal static char ToChar (byte[] value, int startIndex)
		{
			char ret;

			UShortFromBytes ((byte *) &ret, value, startIndex);

			return ret;
		}

		unsafe internal static short ToInt16 (byte[] value, int startIndex)
		{
			short ret;

			UShortFromBytes ((byte *) &ret, value, startIndex);

			return ret;
		}

		unsafe internal static int ToInt32 (byte[] value, int startIndex)
		{
			int ret;

			UIntFromBytes ((byte *) &ret, value, startIndex);

			return ret;
		}

		unsafe internal static long ToInt64 (byte[] value, int startIndex)
		{
			long ret;

			ULongFromBytes ((byte *) &ret, value, startIndex);

			return ret;
		}

		unsafe internal static ushort ToUInt16 (byte[] value, int startIndex)
		{
			ushort ret;

			UShortFromBytes ((byte *) &ret, value, startIndex);

			return ret;
		}

		unsafe internal static uint ToUInt32 (byte[] value, int startIndex)
		{
			uint ret;

			UIntFromBytes ((byte *) &ret, value, startIndex);

			return ret;
		}

		unsafe internal static ulong ToUInt64 (byte[] value, int startIndex)
		{
			ulong ret;

			ULongFromBytes ((byte *) &ret, value, startIndex);

			return ret;
		}

		unsafe internal static float ToSingle (byte[] value, int startIndex)
		{
			float ret;

			UIntFromBytes ((byte *) &ret, value, startIndex);

			return ret;
		}

		unsafe internal static double ToDouble (byte[] value, int startIndex)
		{
			double ret;

			ULongFromBytes ((byte *) &ret, value, startIndex);

			return ret;
		}
	}
}
