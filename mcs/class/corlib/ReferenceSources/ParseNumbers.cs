//
// ParseNumbers.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2015 Xamarin Inc (http://www.xamarin.com)
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

using System.Globalization;
using System.Text;

namespace System {
   
    static class ParseNumbers
    {
        internal const int PrintAsI1=0x40;
        internal const int PrintAsI2=0x80;
//        internal const int PrintAsI4=0x100;
        internal const int TreatAsUnsigned=0x200;
        internal const int TreatAsI1=0x400;
        internal const int TreatAsI2=0x800;
        internal const int IsTight=0x1000;
        internal const int NoSpace=0x2000;

		public static int StringToInt (string value, int fromBase, int flags)
		{
			unsafe {
				return StringToInt (value, fromBase, flags, null);
			}
		}

		public unsafe static int StringToInt (string value, int fromBase, int flags, int* parsePos)
		{
			if ((flags & (IsTight | NoSpace)) == 0)
				throw new NotImplementedException (flags.ToString ());
			
			if (value == null)
				return 0;

			int chars = 0;
			uint result = 0;
			int digitValue;

			int len = value.Length;
			bool negative = false;

			if (len == 0) {
				// Mimic broken .net behaviour
				throw new ArgumentOutOfRangeException ("Empty string");
			}

			int i = parsePos == null ? 0 : *parsePos;

			//Check for a sign
			if (value [i] == '-') {
				if (fromBase != 10)
					throw new ArgumentException ("String cannot contain a minus sign if the base is not 10.");

				if ((flags & TreatAsUnsigned) != 0)
					throw new OverflowException ("Negative number");

				negative = true;
				i++;
			} else if (value [i] == '+') {
				i++;
			}

			if (fromBase == 16 && i + 1 < len && value [i] =='0' && (value [i + 1] == 'x' || value [i + 1] == 'X')) {
				i += 2;
			}

			uint max_value;
			if ((flags & TreatAsI1) != 0) {
				max_value = Byte.MaxValue;
			} else if ((flags & TreatAsI2) != 0) {
				max_value = UInt16.MaxValue;
			} else {
				max_value = UInt32.MaxValue;
			}

			while (i < len) {
				char c = value [i];
				if (Char.IsNumber (c)) {
					digitValue = c - '0';
				} else if (Char.IsLetter (c)) {
					digitValue = Char.ToLowerInvariant (c) - 'a' + 10;
				} else {
					if (i == 0)
						throw new FormatException ("Could not find any parsable digits.");
					
					if ((flags & IsTight) != 0)
						throw new FormatException ("Additional unparsable characters are at the end of the string.");
					
					break;
				}

				if (digitValue >= fromBase) {
					if (chars > 0) {
						throw new FormatException ("Additional unparsable characters are at the end of the string.");
					}

					throw new FormatException ("Could not find any parsable digits.");
				}

				long res = fromBase * result + digitValue;
				if (res > max_value)
					throw new OverflowException ();
					
				result = (uint)res;
				chars++;
				++i;
			}

			if (chars == 0)
				throw new FormatException ("Could not find any parsable digits.");

			if (parsePos != null)
				*parsePos = i;

			return negative ? -(int)result : (int)result;
		}        

		public static string LongToString (long value, int toBase, int width, char paddingChar, int flags)
        {
			if (value == 0)
				return "0";
			if (toBase == 10)
				return value.ToString ();

			byte[] val = BitConverter.GetBytes (value);

			switch (toBase) {
			case 2:
				return ConvertToBase2 (val).ToString ();
			case 8:
				return ConvertToBase8 (val).ToString ();
			case 16:
				return ConvertToBase16 (val).ToString ();
			default:
				throw new NotImplementedException ();
			}
        }

 		public static long StringToLong (string value, int fromBase, int flags)
		{
			unsafe {
				return StringToLong (value, fromBase, flags, null);
			}
		}

		// Value from which a new base 16 digit can cause an overflow.
		const ulong base16MaxOverflowFreeValue = ulong.MaxValue / (16 * 16);

		// From ulong we can only cast to positive long.
		// As |long.MinValue| > |long.MaxValue| we need to do this to avoid an overflow.
		const ulong longMinValue = ((ulong) long.MaxValue) + (ulong) -(long.MinValue + long.MaxValue);

		public unsafe static long StringToLong (string value, int fromBase, int flags, int* parsePos)
		{
			if ((flags & (IsTight | NoSpace)) == 0)
				throw new NotImplementedException (flags.ToString ());

			if (value == null)
				return 0;

			int chars = 0;
			ulong fromBaseULong = (ulong) fromBase;
			ulong digitValue = 0;
			ulong result = 0;

			int len = value.Length;
			bool negative = false;
			bool treatAsUnsigned = (flags & ParseNumbers.TreatAsUnsigned) != 0;

			if (len == 0) {
				// Mimic broken .net behaviour
				throw new ArgumentOutOfRangeException ("Empty string");
			}

			int i = parsePos == null ? 0 : *parsePos;

			//Check for a sign
			if (value [i] == '-') {
				if (fromBase != 10)
					throw new ArgumentException ("String cannot contain a minus sign if the base is not 10.");

				if (treatAsUnsigned)
					throw new OverflowException ("Negative number");

				negative = true;
				i++;
			} else if (value [i] == '+') {
				i++;
			}

			if (fromBase == 16 && i + 1 < len && value [i] =='0' && (value [i + 1] == 'x' || value [i + 1] == 'X')) {
				i += 2;
			}

			while (i < len) {
				char c = value[i];
				if (Char.IsNumber (c)) {
					digitValue = (ulong) (c - '0');
				} else if (Char.IsLetter (c)) {
					digitValue = (ulong) (Char.ToLowerInvariant (c) - 'a' + 10);
				} else {
					if (i == 0)
						throw new FormatException ("Could not find any parsable digits.");

					if ((flags & IsTight) != 0)
						throw new FormatException ("Additional unparsable characters are at the end of the string.");

					break;
				}

				if (digitValue >= fromBaseULong) {
					if (chars > 0) {
						throw new FormatException ("Additional unparsable "
							+ "characters are at the end of the string.");
					} else {
						throw new FormatException ("Could not find any parsable"
							+ " digits.");
					}
				}

				if (result <= base16MaxOverflowFreeValue) {
					result = result * (ulong) fromBaseULong + digitValue;
				} else {
					// decompose 64 bit operation into 32 bit operations so we can check for overflows
					ulong a = (result >> 32) * fromBaseULong;
					ulong b = (result & uint.MaxValue) * fromBaseULong + digitValue;
					if (((b >> 32) + a) > uint.MaxValue)
						throw new OverflowException ();

					result = (a << 32) + b;
				}

				chars++;
				++i;
			}

			if (chars == 0)
				throw new FormatException ("Could not find any parsable digits.");

			if (parsePos != null)
				*parsePos = i;

			if (treatAsUnsigned)
				return (long) result;

			if (!negative) {
				if (fromBase == 10 && result > ((ulong) long.MaxValue))
					throw new OverflowException ();

				return (long)result;
			}

			if (result <= (ulong) long.MaxValue)
				return -((long) result);

			if (result > longMinValue)
				throw new OverflowException ();

			// Avoids overflow of -result when result > long.MaxValue
			return long.MinValue + (long) (longMinValue - result);
		}

		public static string IntToString (int value, int toBase, int width, char paddingChar, int flags)
 		{
			StringBuilder sb;

			if (value == 0) {
				if (width <= 0)
					return "0";

				sb = new StringBuilder ("0", width);
			} else if (toBase == 10)
				sb = new StringBuilder (value.ToString ());
			else {
				byte[] val;
				if ((flags & PrintAsI1) != 0) {
					val = BitConverter.GetBytes ((byte) value);
				} else if ((flags & PrintAsI2) != 0) {
					val = BitConverter.GetBytes ((short) value);
				} else {
					val = BitConverter.GetBytes (value);
				}

				switch (toBase) {
				case 2:
					sb = ConvertToBase2 (val);
					break;
				case 8:
					sb = ConvertToBase8 (val);
					break;
				case 16:
					sb = ConvertToBase16 (val);
					break;
				default:
					throw new NotImplementedException ();
				}
			}

			var padding = width - sb.Length;
			while (padding > 0) {
				sb.Insert (0, paddingChar);
				--padding;
			}

			return sb.ToString ();
 		}

		static void EndianSwap (ref byte[] value)
		{
			byte[] buf = new byte[value.Length];
			for (int i = 0; i < value.Length; i++)
				buf[i] = value[value.Length-1-i];
			value = buf;
		}

		static StringBuilder ConvertToBase2 (byte[] value)
		{
			if (!BitConverter.IsLittleEndian)
				EndianSwap (ref value);
			StringBuilder sb = new StringBuilder ();
			for (int i = value.Length - 1; i >= 0; i--) {
				byte b = value [i];
				for (int j = 0; j < 8; j++) {
					if ((b & 0x80) == 0x80) {
						sb.Append ('1');
					}
					else {
						if (sb.Length > 0)
							sb.Append ('0');
					}
					b <<= 1;
				}
			}
			return sb;
		}

		static StringBuilder ConvertToBase8 (byte[] value)
		{
			ulong l = 0;
			switch (value.Length) {
			case 1:
				l = (ulong) value [0];
				break;
			case 2:
				l = (ulong) BitConverter.ToUInt16 (value, 0);
				break;
			case 4:
				l = (ulong) BitConverter.ToUInt32 (value, 0);
				break;
			case 8:
				l = BitConverter.ToUInt64 (value, 0);
				break;
			default:
				throw new ArgumentException ("value");
			}

			StringBuilder sb = new StringBuilder ();
			for (int i = 21; i >= 0; i--) {
				// 3 bits at the time
				char val = (char) ((l >> i * 3) & 0x7);
				if ((val != 0) || (sb.Length > 0)) {
					val += '0';
					sb.Append (val);
				}
			}
			return sb;
		}

		static StringBuilder ConvertToBase16 (byte[] value)
		{
			if (!BitConverter.IsLittleEndian)
				EndianSwap (ref value);
			StringBuilder sb = new StringBuilder ();
			for (int i = value.Length - 1; i >= 0; i--) {
				char high = (char)((value[i] >> 4) & 0x0f);
				if ((high != 0) || (sb.Length > 0)) {
					if (high < 10) 
						high += '0';
					else {
						high -= (char) 10;
						high += 'a';
					}
					sb.Append (high);
				}

				char low = (char)(value[i] & 0x0f);
				if ((low != 0) || (sb.Length > 0)) {
					if (low < 10)
						low += '0';
					else {
						low -= (char) 10;
						low += 'a';
					}
					sb.Append (low);
				}
			}
			return sb;
		}

    }
}
