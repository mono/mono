//
// System.BitConverter
//
// Author:
//   Matt Kimball (matt@kimball.net)
//

using System;

namespace System {
	public class BitConverter {

		static bool AmILittleEndian()
		{
			byte[] one = GetBytes((int)1);
			return (one[0] == 1);
		}

		public static readonly bool IsLittleEndian = AmILittleEndian ();

		public static long DoubleToInt64Bits(double value)
		{
			return ToInt64(GetBytes(value), 0);
		}

		public static double Int64BitsToDouble(long value)
		{
			return ToDouble(GetBytes(value), 0);
		}

		unsafe static byte[] GetBytes(byte *ptr, int count)
		{
			byte[] ret = new byte[count];

			for (int i = 0; i < count; i++) {
				ret[i] = ptr[i];
			}

			return ret;
		}

		unsafe public static byte[] GetBytes(bool value)
		{
			return GetBytes((byte *)&value, 1);
		}

		unsafe public static byte[] GetBytes(char value)
		{
			return GetBytes((byte *)&value, 2);
		}

		unsafe public static byte[] GetBytes(short value)
		{
			return GetBytes((byte *)&value, 2);
		}

		unsafe public static byte[] GetBytes(int value)
		{
			return GetBytes((byte *)&value, 4);
		}

		unsafe public static byte[] GetBytes(long value)
		{
			return GetBytes((byte *)&value, 8);
		}

		[CLSCompliant(false)]
		unsafe public static byte[] GetBytes(ushort value)
		{
			return GetBytes((byte *)&value, 2);
		}

		[CLSCompliant(false)]
		unsafe public static byte[] GetBytes(uint value)
		{
			return GetBytes((byte *)&value, 4);
		}

		[CLSCompliant(false)]
		unsafe public static byte[] GetBytes(ulong value)
		{
			return GetBytes((byte *)&value, 8);
		}

		unsafe public static byte[] GetBytes(float value)
		{
			return GetBytes((byte *)&value, 4);
		}

		unsafe public static byte[] GetBytes(double value)
		{
			return GetBytes((byte *)&value, 8);
		}

		unsafe static void PutBytes(byte *dst, byte[] src, int start_index, int count)
		{
			if (src == null) {
				throw new ArgumentNullException();
			}

			if (src.Length < start_index + count) {
				// LAMESPEC:
				// the docs say it should be ArgumentOutOfRangeException, but
				// the mscorlib throws an ArgumentException.
				throw new ArgumentException();
			}

			for (int i = 0; i < count; i++) {
				dst[i] = src[i + start_index];
			}
		}

		unsafe public static bool ToBoolean(byte[] value, int start_index)
		{
			bool ret;

			PutBytes((byte *)&ret, value, start_index, 1);

			return ret;
		}

		unsafe public static char ToChar(byte[] value, int start_index)
		{
			char ret;

			PutBytes((byte *)&ret, value, start_index, 2);

			return ret;
		}

		unsafe public static short ToInt16(byte[] value, int start_index)
		{
			short ret;

			PutBytes((byte *)&ret, value, start_index, 2);

			return ret;
		}

		unsafe public static int ToInt32(byte[] value, int start_index)
		{
			int ret;

			PutBytes((byte *)&ret, value, start_index, 4);

			return ret;
		}

		unsafe public static long ToInt64(byte[] value, int start_index)
		{
			long ret;

			PutBytes((byte *)&ret, value, start_index, 8);

			return ret;
		}

		[CLSCompliant(false)]
		unsafe public static ushort ToUInt16(byte[] value, int start_index)
		{
			ushort ret;

			PutBytes((byte *)&ret, value, start_index, 2);

			return ret;
		}

		[CLSCompliant(false)]
		unsafe public static uint ToUInt32(byte[] value, int start_index)
		{
			uint ret;

			PutBytes((byte *)&ret, value, start_index, 4);

			return ret;
		}

		[CLSCompliant(false)]
		unsafe public static ulong ToUInt64(byte[] value, int start_index)
		{
			ulong ret;

			PutBytes((byte *)&ret, value, start_index, 8);

			return ret;
		}

		unsafe public static float ToSingle(byte[] value, int start_index)
		{
			float ret;

			PutBytes((byte *)&ret, value, start_index, 4);

			return ret;
		}

		unsafe public static double ToDouble(byte[] value, int start_index)
		{
			double ret;

			PutBytes((byte *)&ret, value, start_index, 8);

			return ret;
		}

		public static string ToString(byte[] value)
		{
			if (value == null) {
				throw new ArgumentNullException();
			}

			return ToString(value, 0, value.Length);
		}

		public static string ToString(byte[] value, int start_index)
		{
			if (value == null) {
				throw new ArgumentNullException();
			}

			return ToString(value, start_index, value.Length - start_index);
		}

		public static string ToString(byte[] value, int start_index, int length)
		{
			if (value == null) {
				throw new ArgumentNullException();
			}

			// The 4th and last clause (start_index >= value.Length)
			// was added as a small fix to a very obscure bug.
			// It makes a small difference when start_index is
			// outside the range and length==0. 
			if (start_index < 0 || length < 0 || start_index + length > value.Length || start_index >= value.Length) {
				throw new ArgumentOutOfRangeException();
			}

			string ret = "";
			int end = start_index + length;

			for (int i = start_index; i < end; i++) {
				if (i > start_index)
					ret = ret + '-';
				
				char high = (char)((value[i] >> 4) & 0x0f);
				char low = (char)(value[i] & 0x0f);

				if (high < 10) 
					high += '0';
				else {
					high -= (char) 10;
					high += 'A';
				}

				if (low < 10)
					low += '0';
				else {
					low -= (char) 10;
					low += 'A';
				}

				ret = ret + high + low;
			}

			return ret;
		}
	}
}
