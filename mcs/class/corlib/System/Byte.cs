//
// System.Byte.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
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

using System.Globalization;

namespace System
{
	[Serializable]
	public struct Byte : IFormattable, IConvertible,
#if NET_2_0
		IComparable, IComparable<Byte>
#else
		IComparable
#endif
	{
		public const byte MinValue = 0;
		public const byte MaxValue = 255;

		internal byte m_value;

		public int CompareTo (object value)
		{
			if (value == null)
				return 1;

			if (!(value is System.Byte))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Byte."));

			byte xv = (byte) value;

			if (m_value == xv)
				return 0;
			if (m_value > xv)
				return 1;
			else
				return -1;
		}

		public override bool Equals (object obj)
		{
			if (!(obj is System.Byte))
				return false;

			return ((byte) obj) == m_value;
		}

		public override int GetHashCode ()
		{
			return m_value;
		}

#if NET_2_0
		public int CompareTo (byte value)
		{
			if (m_value == value)
				return 0;
			if (m_value > value)
				return 1;
			else
				return -1;
		}

		public bool Equals (byte value)
		{
			return value == m_value;
		}
#endif

		internal static bool Parse (string s, bool tryParse, out byte result)
		{
			byte val = 0;
			int len;
			int i;
			bool digits_seen = false;
			bool negative = false;

			result = 0;

			if (s == null)
				if (tryParse)
					return false;
				else
					throw new ArgumentNullException ("s");

			len = s.Length;

			// look for the first non-whitespace character
			char c;
			for (i = 0; i < len; i++){
				c = s [i];
				if (!Char.IsWhiteSpace (c))
					break;
			}

			// if it's all whitespace, then throw exception
			if (i == len)
				if (tryParse)
					return false;
				else
					throw new FormatException ();

			// look for the optional '+' sign
			if (s [i] == '+')
				i++;
			else if (s [i] == '-') {
				negative = true;
				i++;
			}

			// we should just have numerals followed by whitespace now
			for (; i < len; i++){
				c = s [i];

				if (c >= '0' && c <= '9'){
					// shift left and accumulate every time we find a numeral
					byte d = (byte) (c - '0');

					val = checked ((byte) (val * 10 + d));
					digits_seen = true;
				} else {
					// after the last numeral, only whitespace is allowed
					if (Char.IsWhiteSpace (c)){
						for (i++; i < len; i++){
							if (!Char.IsWhiteSpace (s [i]))
								if (tryParse)
									return false;
								else
									throw new FormatException ();
						}
						break;
					} else
						if (tryParse)
							return false;
						else
							throw new FormatException ();
				}
			}

			// -0 is legal but other negative values are not
			if (negative && (val > 0)) {
				if (tryParse)
					return false;
				else
					throw new OverflowException (
					    Locale.GetText ("Negative number"));
			}

			// if all we had was a '+' sign, then throw exception
			if (!digits_seen)
				if (tryParse)
					return false;
				else
					throw new FormatException ();

		    result = val;
			return true;
		}

		public static byte Parse (string s, IFormatProvider provider)
		{
			return Parse (s, NumberStyles.Integer, provider);
		}

		public static byte Parse (string s, NumberStyles style)
		{
			return Parse (s, style, null);
		}

		public static byte Parse (string s, NumberStyles style, IFormatProvider provider)
		{
			uint tmpResult = UInt32.Parse (s, style, provider);
			if (tmpResult > Byte.MaxValue || tmpResult < Byte.MinValue)
				throw new OverflowException (Locale.GetText ("Value too large or too small."));

			return (byte) tmpResult;
		}

		public static byte Parse (string s) {
			byte res;

			Parse (s, false, out res);

			return res;
		}

#if NET_2_0
		public static bool TryParse (string s, out byte result) {
			try {
				return Parse (s, true, out result);
			}
			catch (Exception) {
				result = 0;
				return false;
			}
		}

		public static bool TryParse (string s, NumberStyles style, IFormatProvider provider, out byte result) {
			try {
				uint tmpResult;

				result = 0;
				if (!UInt32.TryParse (s, style, provider, out tmpResult))
					return false;
				if (tmpResult > Byte.MaxValue || tmpResult < Byte.MinValue)
					return false;
				result = (byte)tmpResult;
				return true;
			}
			catch (Exception) {
				result = 0;
				return false;
			}
		}
#endif

		public override string ToString ()
		{
			return ToString (null, null);
		}

		public string ToString (string format)
		{
			return ToString (format, null);
		}

		public string ToString (IFormatProvider provider)
		{
			return ToString (null, provider);
		}

		public string ToString (string format, IFormatProvider provider)
		{
			NumberFormatInfo nfi = NumberFormatInfo.GetInstance (provider);

			// null or empty ("")
			if ((format == null) || (format.Length == 0))
				format = "G";

			return IntegerFormatter.NumberToString (format, nfi, m_value);
		}

		// =========== IConvertible Methods =========== //
		public TypeCode GetTypeCode ()
		{
			return TypeCode.Byte;
		}

		object IConvertible.ToType (Type conversionType, IFormatProvider provider)
		{
			return System.Convert.ToType (m_value, conversionType, provider);
		}

		bool IConvertible.ToBoolean (IFormatProvider provider)
		{
			return System.Convert.ToBoolean (m_value);
		}

		byte IConvertible.ToByte (IFormatProvider provider)
		{
			return m_value;
		}

		char IConvertible.ToChar (IFormatProvider provider)
		{
			return System.Convert.ToChar (m_value);
		}

		DateTime IConvertible.ToDateTime (IFormatProvider provider)
		{
			throw new InvalidCastException ();
		}

		decimal IConvertible.ToDecimal (IFormatProvider provider)
		{
			return System.Convert.ToDecimal (m_value);
		}

		double IConvertible.ToDouble (IFormatProvider provider)
		{
			return System.Convert.ToDouble (m_value);
		}

		short IConvertible.ToInt16 (IFormatProvider provider)
		{
			return System.Convert.ToInt16 (m_value);
		}

		int IConvertible.ToInt32 (IFormatProvider provider)
		{
			return System.Convert.ToInt32 (m_value);
		}

		long IConvertible.ToInt64 (IFormatProvider provider)
		{
			return System.Convert.ToInt64 (m_value);
		}

		sbyte IConvertible.ToSByte (IFormatProvider provider)
		{
			return System.Convert.ToSByte (m_value);
		}

		float IConvertible.ToSingle (IFormatProvider provider)
		{
			return System.Convert.ToSingle (m_value);
		}

/*
		string IConvertible.ToString (IFormatProvider provider)
		{
			return ToString("G", provider);
		}
*/

		ushort IConvertible.ToUInt16 (IFormatProvider provider)
		{
			return System.Convert.ToUInt16 (m_value);
		}

		uint IConvertible.ToUInt32 (IFormatProvider provider)
		{
			return System.Convert.ToUInt32 (m_value);
		}

		ulong IConvertible.ToUInt64 (IFormatProvider provider)
		{
			return System.Convert.ToUInt64 (m_value);
		}
	}
}
