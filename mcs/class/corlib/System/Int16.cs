//
// System.Int16.cs
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

namespace System {
	
	[Serializable]
	[System.Runtime.InteropServices.ComVisible (true)]
	public struct Int16 : IFormattable, IConvertible, IComparable, IComparable<Int16>, IEquatable <Int16>
	{

		public const short MaxValue =  32767;
		public const short MinValue = -32768;
		
		internal short m_value;

		public int CompareTo (object value)
		{
			if (value == null)
				return 1;

			if (!(value is System.Int16))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Int16"));

			short xv = (short) value;
			if (m_value == xv)
				return 0;
			if (m_value > xv)
				return 1;
			else
				return -1;
		}

		public override bool Equals (object obj)
		{
			if (!(obj is System.Int16))
				return false;

			return ((short) obj) == m_value;
		}

		public override int GetHashCode ()
		{
			return m_value;
		}

		public int CompareTo (short value)
		{
			if (m_value == value)
				return 0;
			if (m_value > value)
				return 1;
			else
				return -1;
		}

		public bool Equals (short obj)
		{
			return obj == m_value;
		}

		internal static bool Parse (string s, bool tryParse, out short result, out Exception exc)
		{
			short val = 0;
			int len;
			int i, sign = 1;
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
			for (i = 0; i < len; i++){
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
			else if (c == '-'){
				sign = -1;
				i++;
			}
			
			for (; i < len; i++){
				c = s [i];
				if (c >= '0' && c <= '9'){
					byte d = (byte) (c - '0');
						
					if (val > (MaxValue/10))
						goto overflow;
					
					if (val == (MaxValue/10)){
						if ((d > (MaxValue % 10)) && (sign == 1 || (d > ((MaxValue % 10) + 1))))
							goto overflow;
						if (sign == -1)
							val = (short) ((val * sign * 10) - d);
						else
							val = (short) ((val * 10) + d);

						if (Int32.ProcessTrailingWhitespace (tryParse, s, i + 1, ref exc)){
							result = val;
							return true;
						}
						goto overflow;
					} else 
						val = (short) (val * 10 + d);
					
					
					digits_seen = true;
				} else if (!Int32.ProcessTrailingWhitespace (tryParse, s, i, ref exc))
					return false;
					
			}
			if (!digits_seen) {
				if (!tryParse)
					exc = Int32.GetFormatException ();
				return false;
			}
			
			if (sign == -1)
				result = (short) (val * sign);
			else
				result = val;

			return true;

		overflow:
			if (!tryParse)
				exc = new OverflowException ("Value is too large");
			return false;
		}

		public static short Parse (string s, IFormatProvider provider)
		{
			return Parse (s, NumberStyles.Integer, provider);
		}

		public static short Parse (string s, NumberStyles style)
		{
			return Parse (s, style, null);
		}

		public static short Parse (string s, NumberStyles style, IFormatProvider provider)
		{
			int tmpResult = Int32.Parse (s, style, provider);
			if ((style & NumberStyles.AllowHexSpecifier) != 0) {
				if (tmpResult >= 0 && tmpResult <= ushort.MaxValue)
					return (short) tmpResult;
			} else if (tmpResult <= MaxValue && tmpResult >= MinValue) {
				return (short) tmpResult;
			}

			throw new OverflowException ("Value too large or too small.");
		}

		public static short Parse (string s) 
		{
			Exception exc;
			short res;

			if (!Parse (s, false, out res, out exc))
				throw exc;

			return res;
		}

		public static bool TryParse (string s, out short result) 
		{
			Exception exc;
			if (!Parse (s, true, out result, out exc)) {
				result = 0;
				return false;
			}

			return true;
		}

		public static bool TryParse (string s, NumberStyles style, IFormatProvider provider, out short result) 
		{
			int tmpResult;
			result = 0;
				
			if (!Int32.TryParse (s, style, provider, out tmpResult))
				return false;
			
			if (tmpResult > Int16.MaxValue || tmpResult < Int16.MinValue)
				return false;
				
			result = (short)tmpResult;
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
			return NumberFormatter.NumberToString(format, m_value, provider);
		}

		// =========== IConvertible Methods =========== //

		public TypeCode GetTypeCode ()
		{
			return TypeCode.Int16;
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
