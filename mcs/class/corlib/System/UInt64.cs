//
// System.UInt64.cs
//
// Authors:
//   Miguel de Icaza (miguel@ximian.com)
//   Marek Safar (marek.safar@gmail.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
// Copyright (C) 2012 Xamarin Inc (http://www.xamarin.com)
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
using System.Threading;

namespace System
{
	[Serializable]
	[CLSCompliant (false)]
	[System.Runtime.InteropServices.ComVisible (true)]
	public struct UInt64 : IFormattable, IConvertible, IComparable, IComparable<UInt64>, IEquatable <UInt64>
	{
		public const ulong MaxValue = 0xffffffffffffffff;
		public const ulong MinValue = 0;

		internal ulong m_value;

		public int CompareTo (object value)
		{
			if (value == null)
				return 1;

			if (!(value is System.UInt64))
				throw new ArgumentException (Locale.GetText ("Value is not a System.UInt64."));

			ulong int64 = (ulong) value;

			if (m_value == int64)
				return 0;

			return (m_value < int64) ? -1 : 1;
		}

		public override bool Equals (object obj)
		{
			if (!(obj is System.UInt64))
				return false;

			return ((ulong) obj) == m_value;
		}

		public override int GetHashCode ()
		{
			return (int)(m_value & 0xffffffff) ^ (int)(m_value >> 32);
		}

		public int CompareTo (ulong value)
		{
			if (m_value == value)
				return 0;
			if (m_value > value)
				return 1;
			else
				return -1;
		}

		public bool Equals (ulong obj)
		{
			return obj == m_value;
		}

		[CLSCompliant (false)]
		public static ulong Parse (string s)
		{
			Exception exc;
			ulong result;

			if (!Parse (s, false, out result, out exc))
				throw exc;

			return result;
		}

		internal static bool Parse (string s, bool tryParse, out ulong result, out Exception exc)
		{
			ulong val = 0;
			int len;
			int i;
			bool digits_seen = false;
			bool has_negative_sign = false;

			exc = null;
			result = 0;

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

			if (s [i] == '+')
				i++;
			else if (s [i] == '-') {
				i++;
				has_negative_sign = true;
			}

			// Actual number stuff
			for (; i < len; i++) {
				c = s [i];

				if (c >= '0' && c <= '9') {
					uint d = (uint) (c - '0');

					if (val > MaxValue / 10 || (val == MaxValue / 10 && d > MaxValue % 10)) {
						if (!tryParse)
							exc = new OverflowException ("Value is too large.");
						return false;
					}

					val = (val * 10) + d;
					digits_seen = true;
				} else if (!Int32.ProcessTrailingWhitespace (tryParse, s, i, ref exc))
					return false;
			}

			if (!digits_seen) {
				if (!tryParse)
					exc = Int32.GetFormatException ();
				return false;
			}

			if (has_negative_sign && val > 0) {
				if (!tryParse)
					exc = new OverflowException ("Negative number.");
				return false;
			}

			result = val;
			return true;
		}

		[CLSCompliant (false)]
		public static ulong Parse (string s, IFormatProvider provider)
		{
			return Parse (s, NumberStyles.Integer, provider);
		}

		[CLSCompliant (false)]
		public static ulong Parse (string s, NumberStyles style)
		{
			return Parse (s, style, null);
		}

		internal static bool Parse (string s, NumberStyles style, IFormatProvider provider, bool tryParse, out ulong result, out Exception exc)
		{
			result = 0;
			exc = null;

			if (s == null) {
				if (!tryParse)
					exc = new ArgumentNullException ("s");
				return false;
			}

			if (s.Length == 0) {
				if (!tryParse)
					exc = Int32.GetFormatException ();
				return false;
			}

			NumberFormatInfo nfi = null;
			if (provider != null) {
				Type typeNFI = typeof (NumberFormatInfo);
				nfi = (NumberFormatInfo) provider.GetFormat (typeNFI);
			}
			if (nfi == null)
				nfi = Thread.CurrentThread.CurrentCulture.NumberFormat;

			if (!Int32.CheckStyle (style, tryParse, ref exc))
				return false;

			bool AllowCurrencySymbol = (style & NumberStyles.AllowCurrencySymbol) != 0;
			bool AllowHexSpecifier = (style & NumberStyles.AllowHexSpecifier) != 0;
			bool AllowThousands = (style & NumberStyles.AllowThousands) != 0;
			bool AllowDecimalPoint = (style & NumberStyles.AllowDecimalPoint) != 0;
			bool AllowParentheses = (style & NumberStyles.AllowParentheses) != 0;
			bool AllowTrailingSign = (style & NumberStyles.AllowTrailingSign) != 0;
			bool AllowLeadingSign = (style & NumberStyles.AllowLeadingSign) != 0;
			bool AllowTrailingWhite = (style & NumberStyles.AllowTrailingWhite) != 0;
			bool AllowLeadingWhite = (style & NumberStyles.AllowLeadingWhite) != 0;
			bool AllowExponent = (style & NumberStyles.AllowExponent) != 0;

			int pos = 0;

			if (AllowLeadingWhite && !Int32.JumpOverWhite (ref pos, s, true, tryParse, ref exc))
				return false;

			bool foundOpenParentheses = false;
			bool negative = false;
			bool foundSign = false;
			bool foundCurrency = false;

			// Pre-number stuff
			if (AllowParentheses && s [pos] == '(') {
				foundOpenParentheses = true;
				foundSign = true;
				negative = true; // MS always make the number negative when there parentheses
						 // even when NumberFormatInfo.NumberNegativePattern != 0!!!
				pos++;
				if (AllowLeadingWhite && !Int32.JumpOverWhite (ref pos, s, true, tryParse, ref exc))
					return false;

				if (s.Substring (pos, nfi.NegativeSign.Length) == nfi.NegativeSign) {
					if (!tryParse)
						exc = Int32.GetFormatException ();
					return false;
				}
				
				if (s.Substring (pos, nfi.PositiveSign.Length) == nfi.PositiveSign) {
					if (!tryParse)
						exc = Int32.GetFormatException ();
					return false;
				}
			}

			if (AllowLeadingSign && !foundSign) {
				// Sign + Currency
				Int32.FindSign (ref pos, s, nfi, ref foundSign, ref negative);
				if (foundSign) {
					if (AllowLeadingWhite && !Int32.JumpOverWhite (ref pos, s, true, tryParse, ref exc))
						return false;
					if (AllowCurrencySymbol) {
						Int32.FindCurrency (ref pos, s, nfi,
								    ref foundCurrency);
						if (foundCurrency && AllowLeadingWhite &&
								!Int32.JumpOverWhite (ref pos, s, true, tryParse, ref exc))
							return false;
					}
				}
			}

			if (AllowCurrencySymbol && !foundCurrency) {
				// Currency + sign
				Int32.FindCurrency (ref pos, s, nfi, ref foundCurrency);
				if (foundCurrency) {
					if (AllowLeadingWhite && !Int32.JumpOverWhite (ref pos, s, true, tryParse, ref exc))
						return false;
					if (foundCurrency) {
						if (!foundSign && AllowLeadingSign) {
							Int32.FindSign (ref pos, s, nfi, ref foundSign,
									ref negative);
							if (foundSign && AllowLeadingWhite &&
									!Int32.JumpOverWhite (ref pos, s, true, tryParse, ref exc))
								return false;
						}
					}
				}
			}

			ulong number = 0;
			int nDigits = 0;
			int decimalPointPos = -1;
			ulong digitValue;
			char hexDigit;

			// Number stuff
			// Just the same as Int32, but this one adds instead of substract
			do {

				if (!Int32.ValidDigit (s [pos], AllowHexSpecifier)) {
					if (AllowThousands &&
					    (Int32.FindOther (ref pos, s, nfi.NumberGroupSeparator)
						|| Int32.FindOther (ref pos, s, nfi.CurrencyGroupSeparator)))
						continue;
					
					if (AllowDecimalPoint && decimalPointPos < 0 &&
					    (Int32.FindOther (ref pos, s, nfi.NumberDecimalSeparator)
						|| Int32.FindOther (ref pos, s, nfi.CurrencyDecimalSeparator))) {
							decimalPointPos = nDigits;
							continue;
						}

					break;
				}

				nDigits++;

				if (AllowHexSpecifier) {
					hexDigit = s [pos++];
					if (Char.IsDigit (hexDigit))
						digitValue = (ulong) (hexDigit - '0');
					else if (Char.IsLower (hexDigit))
						digitValue = (ulong) (hexDigit - 'a' + 10);
					else
						digitValue = (ulong) (hexDigit - 'A' + 10);

					if (tryParse){
						// Any number above 32 will do 
						bool can_overflow = number > 0xffff;

						number = number * 16 + digitValue;

						if (can_overflow && number < 16)
							return false;
					} else
						number = checked (number * 16 + digitValue);

					continue;
				}

				try {
					number = checked (number * 10 + (ulong) (s [pos++] - '0'));
				} catch (OverflowException) {
					if (!tryParse)
						exc = new OverflowException (Locale.GetText ("Value too large or too small."));
					return false;
				}
			} while (pos < s.Length);

			// Post number stuff
			if (nDigits == 0) {
				if (!tryParse)
					exc = Int32.GetFormatException ();
				return false;
			}

			int exponent = 0;
			if (AllowExponent)
				if (Int32.FindExponent (ref pos, s, ref exponent, tryParse, ref exc) && exc != null)
					return false;

			if (AllowTrailingSign && !foundSign) {
				// Sign + Currency
				Int32.FindSign (ref pos, s, nfi, ref foundSign, ref negative);
				if (foundSign && pos < s.Length) {
					if (AllowTrailingWhite && !Int32.JumpOverWhite (ref pos, s, true, tryParse, ref exc))
						return false;
				}
			}

			if (AllowCurrencySymbol && !foundCurrency) {
				if (AllowTrailingWhite && pos < s.Length && !Int32.JumpOverWhite (ref pos, s, false, tryParse, ref exc))
					return false;
				
				// Currency + sign
				Int32.FindCurrency (ref pos, s, nfi, ref foundCurrency);
				if (foundCurrency && pos < s.Length) {
					if (AllowTrailingWhite && !Int32.JumpOverWhite (ref pos, s, true, tryParse, ref exc))
						return false;
					if (!foundSign && AllowTrailingSign)
						Int32.FindSign (ref pos, s, nfi, ref foundSign,
								ref negative);
				}
			}

			if (AllowTrailingWhite && pos < s.Length && !Int32.JumpOverWhite (ref pos, s, false, tryParse, ref exc))
				return false;

			if (foundOpenParentheses) {
				if (pos >= s.Length || s [pos++] != ')') {
					if (!tryParse)
						exc = Int32.GetFormatException ();
					return false;
				}
				if (AllowTrailingWhite && pos < s.Length && !Int32.JumpOverWhite (ref pos, s, false, tryParse, ref exc))
					return false;
			}

			if (pos < s.Length && s [pos] != '\u0000') {
				if (!tryParse)
					exc = Int32.GetFormatException ();
				return false;
			}

			// -0 is legal but other negative values are not
			if (negative && (number > 0)) {
				if (!tryParse)
					exc = new OverflowException (
					    Locale.GetText ("Negative number"));
				return false;
			}

			if (decimalPointPos >= 0)
				exponent = exponent - nDigits + decimalPointPos;
			
			if (exponent < 0) {
				//
				// Any non-zero values after decimal point are not allowed
				//
				long remainder;
				number = (ulong) Math.DivRem ((long) number, (long) Math.Pow (10, -exponent), out remainder);
				if (remainder != 0) {
					if (!tryParse)
						exc = new OverflowException ("Value too large or too small.");
					return false;
				}
			} else if (exponent > 0) {
				//
				// result *= 10^exponent
				//
				// Reduce the risk of throwing an overflow exc
				//
				double res = checked (Math.Pow (10, exponent) * number);
				if (res < MinValue || res > MaxValue) {
					if (!tryParse)
						exc = new OverflowException ("Value too large or too small.");
					return false;
				}

				number = (ulong)res;
			}

			result = number;
			return true;
		}

		[CLSCompliant (false)]
		public static ulong Parse (string s, NumberStyles style, IFormatProvider provider) 
		{
			Exception exc;
			ulong res;

			if (!Parse (s, style, provider, false, out res, out exc))
				throw exc;

			return res;
		}


		[CLSCompliant (false)]
		public static bool TryParse (string s, out ulong result) 
		{
			Exception exc;
			if (!Parse (s, true, out result, out exc)) {
				result = 0;
				return false;
			}

			return true;
		}

		[CLSCompliant (false)]
		public static bool TryParse (string s, NumberStyles style, IFormatProvider provider, out ulong result) 
		{
			Exception exc;
			if (!Parse (s, style, provider, true, out result, out exc)) {
				result = 0;
				return false;
			}

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

		// =========== IConvertible Methods =========== //
		public TypeCode GetTypeCode ()
		{
			return TypeCode.UInt64;
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

		sbyte IConvertible.ToSByte(IFormatProvider provider)
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
			return m_value;
		}
	}
}
