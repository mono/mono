//
// System.SByte.cs
//
// Author:
// Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc. http://www.ximian.com
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
	[CLSCompliant(false)]
	[Serializable]
	public struct SByte : IFormattable, IConvertible,
#if NET_2_0
		IComparable, IComparable<SByte>
#else
		IComparable
#endif
	{
		public const sbyte MinValue = -128;
		public const sbyte MaxValue = 127;

		internal sbyte m_value;

		public int CompareTo (object v)
		{
			if (v == null)
				return 1;

			if (!(v is System.SByte))
				throw new ArgumentException (Locale.GetText ("Value is not a System.SByte."));

			sbyte xv = (sbyte) v;
			if (m_value == xv)
				return 0;
			if (m_value > xv)
				return 1;
			else
				return -1;
		}

		public override bool Equals (object o)
		{
			if (!(o is System.SByte))
				return false;

			return ((sbyte) o) == m_value;
		}

		public override int GetHashCode ()
		{
			return m_value;
		}

#if NET_2_0
		public int CompareTo (sbyte value)
		{
			if (m_value == value)
				return 0;
			if (m_value > value)
				return 1;
			else
				return -1;
		}

		public bool Equals (sbyte value)
		{
			return value == m_value;
		}
#endif

		internal static bool Parse (string s, bool tryParse, out sbyte result)
		{
			int ival = 0;
			int len;
			int i;
			bool neg = false;
			bool digits_seen = false;

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

			c = s [i];
			if (c == '+')
				i++;
			else if (c == '-') {
				neg = true;
				i++;
			}

			for (; i < len; i++) {
				c = s [i];

				if (c >= '0' && c <= '9') {
					ival = checked (ival * 10 - (int) (c - '0'));
					digits_seen = true;
				} else {
					if (Char.IsWhiteSpace (c)) {
						for (i++; i < len; i++) {
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
			if (!digits_seen)
				if (tryParse)
					return false;
				else
					throw new FormatException ();

			ival = neg ? ival : -ival;
			if (ival < SByte.MinValue || ival > SByte.MaxValue)
				if (tryParse)
					return false;
				else
					throw new OverflowException ();

			result = (sbyte)ival;
			return true;
		}

		[CLSCompliant(false)]
		public static sbyte Parse (string s, IFormatProvider provider)
		{
			return Parse (s, NumberStyles.Integer, provider);
		}

		[CLSCompliant(false)]
		public static sbyte Parse (string s, NumberStyles style)
		{
			return Parse (s, style, null);
		}

		[CLSCompliant(false)]
		public static sbyte Parse (string s, NumberStyles style, IFormatProvider provider)
		{
			int tmpResult = Int32.Parse (s, style, provider);
			if (tmpResult > SByte.MaxValue || tmpResult < SByte.MinValue)
				throw new OverflowException (Locale.GetText ("Value too large or too small."));

			return (sbyte) tmpResult;
		}

		[CLSCompliant(false)]
		public static sbyte Parse (string s) {
			sbyte res;

			Parse (s, false, out res);

			return res;
		}

#if NET_2_0
		[CLSCompliant(false)]
		public static bool TryParse (string s, out sbyte result) {
			try {
				return Parse (s, true, out result);
			}
			catch (Exception) {
				result = 0;
				return false;
			}
		}

		[CLSCompliant(false)]
		public static bool TryParse (string s, NumberStyles style, IFormatProvider provider, out sbyte result) {
			try {
				int tmpResult;

				result = 0;
				if (!Int32.TryParse (s, style, provider, out tmpResult))
					return false;
				if (tmpResult > SByte.MaxValue || tmpResult < SByte.MinValue)
					return false;
				result = (sbyte)tmpResult;
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

		// =========== ICovnertible Methods =========== //
		public TypeCode GetTypeCode ()
		{
			return TypeCode.SByte;
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
			return m_value;
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
