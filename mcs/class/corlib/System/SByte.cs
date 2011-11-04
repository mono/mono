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
	[System.Runtime.InteropServices.ComVisible (true)]
	public struct SByte : IFormattable, IConvertible, IComparable, IComparable<SByte>, IEquatable <SByte>
	{
		public const sbyte MinValue = -128;
		public const sbyte MaxValue = 127;

		internal sbyte m_value;

		public int CompareTo (object obj)
		{
			if (obj == null)
				return 1;

			if (!(obj is System.SByte))
				throw new ArgumentException (Locale.GetText ("Value is not a System.SByte."));

			sbyte xv = (sbyte) obj;
			if (m_value == xv)
				return 0;
			if (m_value > xv)
				return 1;
			else
				return -1;
		}

		public override bool Equals (object obj)
		{
			if (!(obj is System.SByte))
				return false;

			return ((sbyte) obj) == m_value;
		}

		public override int GetHashCode ()
		{
			return m_value;
		}

		public int CompareTo (sbyte value)
		{
			if (m_value == value)
				return 0;
			if (m_value > value)
				return 1;
			else
				return -1;
		}

		public bool Equals (sbyte obj)
		{
			return obj == m_value;
		}

		internal static bool Parse (string s, bool tryParse, out sbyte result, out Exception exc)
		{
			int ival = 0;
			int len;
			int i;
			bool neg = false;
			bool digits_seen = false;

			result = 0;
			exc = null;

			if (s == null) {
				if (!tryParse)
					exc = new ArgumentNullException ("s");
				return false;
			}

			len = s.Length;

			char c;
			for (i = 0; i < len; i++) {
				c = s [i];
				if (!Char.IsWhiteSpace (c))
					break;
			}

			if (i == len) {
				if (!tryParse)
					exc = Int32.GetFormatException ();
				return false;
			}

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
					if (tryParse){
						int intval = ival * 10 - (int) (c - '0');

						if (intval < MinValue)
							return false;
						ival = (sbyte) intval;
					} else
						ival = checked (ival * 10 - (int) (c - '0'));
					digits_seen = true;
				} else {
					if (Char.IsWhiteSpace (c)) {
						for (i++; i < len; i++) {
							if (!Char.IsWhiteSpace (s [i])) {
								if (!tryParse)
									exc = Int32.GetFormatException ();
								return false;
							}
						}
						break;
					} else {
						if (!tryParse)
							exc = Int32.GetFormatException ();
						return false;
					}
				}
			}
			if (!digits_seen) {
				if (!tryParse)
					exc = Int32.GetFormatException ();
				return false;
			}

			ival = neg ? ival : -ival;
			if (ival < SByte.MinValue || ival > SByte.MaxValue) {
				if (!tryParse)
					exc = new OverflowException ();
				return false;
			}

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
			if ((style & NumberStyles.AllowHexSpecifier) != 0) {
				if (tmpResult >= 0 && tmpResult <= byte.MaxValue)
					return (sbyte) tmpResult;
			} else if (tmpResult <= MaxValue && tmpResult >= MinValue) {
				return (sbyte) tmpResult;
			}
			
			throw new OverflowException (Locale.GetText ("Value too large or too small."));
		}

		[CLSCompliant(false)]
		public static sbyte Parse (string s) 
		{
			Exception exc;
			sbyte res;

			if (!Parse (s, false, out res, out exc))
				throw exc;

			return res;
		}

		[CLSCompliant(false)]
		public static bool TryParse (string s, out sbyte result) 
		{
			Exception exc;
			if (!Parse (s, true, out result, out exc)) {
				result = 0;
				return false;
			}

			return true;
		}

		[CLSCompliant(false)]
		public static bool TryParse (string s, NumberStyles style, IFormatProvider provider, out sbyte result) 
		{
			int tmpResult;
			result = 0;

			if (!Int32.TryParse (s, style, provider, out tmpResult))
				return false;
			if (tmpResult > SByte.MaxValue || tmpResult < SByte.MinValue)
				return false;
				
			result = (sbyte)tmpResult;
			return true;
		}

		public override string ToString ()
		{
			return NumberFormatter.NumberToString (m_value, null);
		}

		public string ToString (IFormatProvider provider)
		{
			return NumberFormatter.NumberToString (m_value, provider);
		}

		public string ToString (string format)
		{
			return ToString (format, null);
		}

		public string ToString (string format, IFormatProvider provider)
		{
			return NumberFormatter.NumberToString (format, m_value, provider);
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

		object IConvertible.ToType (Type targetType, IFormatProvider provider)
		{
			if (targetType == null)
				throw new ArgumentNullException ("targetType");
			return System.Convert.ToType (m_value, targetType, provider, false);
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
