//
// System.BitConverter.cs
//
// Author:
//   Matt Kimball (matt@kimball.net)
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

namespace System
{
	public sealed class BitConverter
	{
		public static readonly bool IsLittleEndian = AmILittleEndian ();

		private BitConverter ()
		{
		}

		static bool AmILittleEndian ()
		{
			byte[] one = GetBytes ((int) 1);
			return (one [0] == 1);
		}

		public static long DoubleToInt64Bits (double value)
		{
			return ToInt64 (GetBytes (value), 0);
		}

		public static double Int64BitsToDouble (long value)
		{
			return ToDouble (GetBytes (value), 0);
		}

		unsafe static byte[] GetBytes (byte *ptr, int count)
		{
			byte [] ret = new byte [count];

			for (int i = 0; i < count; i++) {
				ret [i] = ptr [i];
			}

			return ret;
		}

		unsafe public static byte[] GetBytes (bool value)
		{
			return GetBytes ((byte *) &value, 1);
		}

		unsafe public static byte[] GetBytes (char value)
		{
			return GetBytes ((byte *) &value, 2);
		}

		unsafe public static byte[] GetBytes (short value)
		{
			return GetBytes ((byte *) &value, 2);
		}

		unsafe public static byte[] GetBytes (int value)
		{
			return GetBytes ((byte *) &value, 4);
		}

		unsafe public static byte[] GetBytes (long value)
		{
			return GetBytes ((byte *) &value, 8);
		}

		[CLSCompliant (false)]
		unsafe public static byte[] GetBytes (ushort value)
		{
			return GetBytes ((byte *) &value, 2);
		}

		[CLSCompliant (false)]
		unsafe public static byte[] GetBytes (uint value)
		{
			return GetBytes ((byte *) &value, 4);
		}

		[CLSCompliant (false)]
		unsafe public static byte[] GetBytes (ulong value)
		{
			return GetBytes ((byte *) &value, 8);
		}

		unsafe public static byte[] GetBytes (float value)
		{
			return GetBytes ((byte *) &value, 4);
		}

		unsafe public static byte[] GetBytes (double value)
		{
			return GetBytes ((byte *) &value, 8);
		}

		unsafe static void PutBytes (byte *dst, byte[] src, int start_index, int count)
		{
			if (src == null) {
				throw new ArgumentNullException ("value"); // gets called from methods with value params
			}

			if (start_index < 0)
				throw new ArgumentOutOfRangeException ("startIndex < 0");

			// avoid integer overflow (with large pos/neg start_index values)
			if (src.Length - count < start_index) {
				throw new ArgumentOutOfRangeException (Locale.GetText (
					"Value is too big to return the requested type."), "startIndex");
			}

			for (int i = 0; i < count; i++) {
				dst[i] = src[i + start_index];
			}
		}

		unsafe public static bool ToBoolean (byte[] value, int startIndex)
		{
			if (value == null) 
				throw new ArgumentNullException ("value");

			if (startIndex < 0)
				throw new ArgumentOutOfRangeException ("startIndex < 0");

			// avoid integer overflow (with large pos/neg start_index values)
			if (value.Length - 1 < startIndex) 
				throw new ArgumentOutOfRangeException (Locale.GetText (
					"Value is too big to return the requested type."), "startIndex");

			if (value [startIndex] != 0)
				return true;
			
			return false;
		}

		unsafe public static char ToChar (byte[] value, int startIndex)
		{
			char ret;

			PutBytes ((byte *) &ret, value, startIndex, 2);

			return ret;
		}

		unsafe public static short ToInt16 (byte[] value, int startIndex)
		{
			short ret;

			PutBytes ((byte *) &ret, value, startIndex, 2);

			return ret;
		}

		unsafe public static int ToInt32 (byte[] value, int startIndex)
		{
			int ret;

			PutBytes ((byte *) &ret, value, startIndex, 4);

			return ret;
		}

		unsafe public static long ToInt64 (byte[] value, int startIndex)
		{
			long ret;

			PutBytes ((byte *) &ret, value, startIndex, 8);

			return ret;
		}

		[CLSCompliant (false)]
		unsafe public static ushort ToUInt16 (byte[] value, int startIndex)
		{
			ushort ret;

			PutBytes ((byte *) &ret, value, startIndex, 2);

			return ret;
		}

		[CLSCompliant (false)]
		unsafe public static uint ToUInt32 (byte[] value, int startIndex)
		{
			uint ret;

			PutBytes ((byte *) &ret, value, startIndex, 4);

			return ret;
		}

		[CLSCompliant (false)]
		unsafe public static ulong ToUInt64 (byte[] value, int startIndex)
		{
			ulong ret;

			PutBytes ((byte *) &ret, value, startIndex, 8);

			return ret;
		}

		unsafe public static float ToSingle (byte[] value, int startIndex)
		{
			float ret;

			PutBytes ((byte *) &ret, value, startIndex, 4);

			return ret;
		}

		unsafe public static double ToDouble (byte[] value, int startIndex)
		{
			double ret;

			PutBytes ((byte *) &ret, value, startIndex, 8);

			return ret;
		}

		public static string ToString (byte[] value)
		{
			if (value == null) {
				throw new ArgumentNullException ("value");
			}

			return ToString (value, 0, value.Length);
		}

		public static string ToString (byte[] value, int startIndex)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			if (startIndex < 0 || startIndex > value.Length - 1) 
				throw new ArgumentOutOfRangeException ("startIndex");

			return ToString (value, startIndex, value.Length - startIndex);
		}

		public static string ToString (byte[] value, int startIndex, int length)
		{
			if (value == null) {
				throw new ArgumentNullException ("value");
			}

			// The 4th and last clause (start_index >= value.Length)
			// was added as a small fix to a very obscure bug.
			// It makes a small difference when start_index is
			// outside the range and length==0. 
			if (startIndex < 0 || length < 0 || startIndex >= value.Length) {
				// special (but valid) case (e.g. new byte [0])
				if ((startIndex == 0) && (value.Length == 0))
					return String.Empty;
				throw new ArgumentOutOfRangeException ("startIndex");
			}
			// note: re-ordered to avoid possible integer overflow
			if (startIndex > value.Length - length)
				throw new ArgumentException ("startIndex + length > value.Length");

			string ret = "";
			int end = startIndex + length;

			for (int i = startIndex; i < end; i++) {
				if (i > startIndex)
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
