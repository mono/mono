//
// System.UInt16.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004 Novell (http://www.novell.com)
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
	[CLSCompliant (false)]
	public struct UInt16 : IFormattable, IConvertible,
#if NET_2_0
		IComparable, IComparable<UInt16>
#else
		IComparable
#endif
	{
		public const ushort MaxValue = 0xffff;
		public const ushort MinValue = 0;

		internal ushort m_value;

		public int CompareTo (object value)
		{
			if (value == null)
				return 1;

			if(!(value is System.UInt16))
				throw new ArgumentException (Locale.GetText ("Value is not a System.UInt16."));

			return this.m_value - ((ushort) value);
		}

		public override bool Equals (object obj)
		{
			if (!(obj is System.UInt16))
				return false;

			return ((ushort) obj) == m_value;
		}

		public override int GetHashCode ()
		{
			return m_value;
		}

#if NET_2_0
		public int CompareTo (ushort value)
		{
			if (m_value == value)
				return 0;
			if (m_value > value)
				return 1;
			else
				return -1;
		}

		public bool Equals (ushort value)
		{
			return value == m_value;
		}
#endif

		internal static bool Parse (string s, bool tryParse, out ushort result)
		{
			ushort val = 0;
			int len;
			int i;
			bool digits_seen = false;
			bool has_negative_sign = false;

			result = 0;

			if (s == null)
				if (tryParse)
					return false;
				else
					throw new ArgumentNullException ("s");

			len = s.Length;

			char c;
			for (i = 0; i < len; i++) {
				c = s [i];
				if (!Char.IsWhiteSpace (c))
					break;
			}

			if (i == len)
				if (tryParse)
					return false;
				else
					throw new FormatException ();

			if (s [i] == '+')
				i++;
			else
				if (s[i] == '-') {
					i++;
					has_negative_sign = true;
				}

			for (; i < len; i++) {
				c = s [i];

				if (c >= '0' && c <= '9') {
					ushort d = (ushort) (c - '0');

					val = checked ((ushort) (val * 10 + d));
					digits_seen = true;
				}
				else {
					if (Char.IsWhiteSpace (c)) {
						for (i++; i < len; i++) {
							if (!Char.IsWhiteSpace (s [i]))
								if (tryParse)
									return false;
								else
									throw new FormatException ();
						}
						break;
					}
					else
						if (tryParse)
							return false;
						else
							throw new FormatException ();
				}
			}
			if (!digits_seen)
				if (tryParse)
					return false;
				else
					throw new FormatException ();

			// -0 is legal but other negative values are not
			if (has_negative_sign && (val > 0)) {
				if (tryParse)
					return false;
				else
					throw new OverflowException (
					    Locale.GetText ("Negative number"));
			}

			result = val;
			return true;
		}

		[CLSCompliant (false)]
		public static ushort Parse (string s, IFormatProvider provider)
		{
			return Parse (s, NumberStyles.Integer, provider);
		}

		[CLSCompliant (false)]
		public static ushort Parse (string s, NumberStyles style)
		{
			return Parse (s, style, null);
		}

		[CLSCompliant (false)]
		public static ushort Parse (string s, NumberStyles style, IFormatProvider provider)
		{
			uint tmpResult = UInt32.Parse (s, style, provider);
			if (tmpResult > UInt16.MaxValue || tmpResult < UInt16.MinValue)
				throw new OverflowException (Locale.GetText ("Value too large or too small."));

			return (ushort) tmpResult;
		}

		[CLSCompliant(false)]
		public static ushort Parse (string s) {
			ushort res;

			Parse (s, false, out res);

			return res;
		}

#if NET_2_0
		[CLSCompliant(false)]
		public static bool TryParse (string s, out ushort result) {
			try {
				return Parse (s, true, out result);
			}
			catch (Exception) {
				result = 0;
				return false;
			}
		}

		[CLSCompliant(false)]
		public static bool TryParse (string s, NumberStyles style, IFormatProvider provider, out ushort result) {
			try {
				uint tmpResult;

				if (!UInt32.TryParse (s, style, provider, out tmpResult))
					return false;
				if (tmpResult > UInt16.MaxValue || tmpResult < UInt16.MinValue)
					return false;
				result = (ushort)tmpResult;
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

		public string ToString (IFormatProvider provider)
		{
			return ToString (null, provider);
		}

		public string ToString (string format)
		{
			return ToString (format, null);
		}

		public string ToString (string format, IFormatProvider provider)
		{
			NumberFormatInfo nfi = NumberFormatInfo.GetInstance (provider);

			// use "G" when format is null or String.Empty
			if ((format == null) || (format.Length == 0))
				format = "G";
			
			return IntegerFormatter.NumberToString (format, nfi, m_value);
		}

		// =========== IConvertible Methods =========== //
		public TypeCode GetTypeCode ()
		{
			return TypeCode.UInt16;
		}

		bool IConvertible.ToBoolean (IFormatProvider provider)
		{
			return System.Convert.ToBoolean (m_value);
		}

		byte IConvertible.ToByte (IFormatProvider provider)
		{
			return System.Convert.ToByte (m_value);
		}

		char IConvertible.ToChar (IFormatProvider provider)
		{
			return System.Convert.ToChar (m_value);
		}

		DateTime IConvertible.ToDateTime (IFormatProvider provider)
		{
			return System.Convert.ToDateTime (m_value);
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

		object IConvertible.ToType (Type conversionType, IFormatProvider provider)
		{
			return System.Convert.ToType (m_value, conversionType, provider);
		}

		ushort IConvertible.ToUInt16 (IFormatProvider provider)
		{
			return m_value;
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
