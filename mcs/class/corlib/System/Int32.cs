//
// System.Int32.cs
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

namespace System {
	
	[Serializable]
	[System.Runtime.InteropServices.ComVisible (true)]
	public struct Int32 : IFormattable, IConvertible, IComparable, IComparable<Int32>, IEquatable <Int32>
	{

		public const int MaxValue = 0x7fffffff;
		public const int MinValue = -2147483648;
		
		// This field is looked up by name in the runtime
		internal int m_value;

		public int CompareTo (object value)
		{
			if (value == null)
				return 1;
			
			if (!(value is System.Int32))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Int32"));

			int xv = (int) value;
			if (m_value == xv)
				return 0;
			if (m_value > xv)
				return 1;
			else
				return -1;
		}

		public override bool Equals (object obj)
		{
			if (!(obj is System.Int32))
				return false;

			return ((int) obj) == m_value;
		}

		public override int GetHashCode ()
		{
			return m_value;
		}

		public int CompareTo (int value)
		{
			if (m_value == value)
				return 0;
			if (m_value > value)
				return 1;
			else
				return -1;
		}

		public bool Equals (int obj)
		{
			return obj == m_value;
		}

		internal static bool ProcessTrailingWhitespace (bool tryParse, string s, int position, ref Exception exc)
		{
			int len = s.Length;
			
			for (int i = position; i < len; i++){
				char c = s [i];
				
				if (c != 0 && !Char.IsWhiteSpace (c)){
					if (!tryParse)
						exc = GetFormatException ();
					return false;
				}
			}
			return true;
		}

		internal static bool Parse (string s, bool tryParse, out int result, out Exception exc)
		{
			int val = 0;
			int len;
			int i, sign = 1;
			bool digits_seen = false;

			result = 0;
			exc = null;
			NumberFormatInfo nfi = Thread.CurrentThread.CurrentCulture.NumberFormat;

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
					exc = GetFormatException ();
				return false;
			}

			var ps_length = nfi.PositiveSign.Length;
			var ns_length = nfi.NegativeSign.Length;
			if (len > ps_length && string.CompareOrdinalUnchecked (s, i, ns_length, nfi.PositiveSign, 0, ps_length) == 0)
				i += ps_length;
			else if (len > ns_length && string.CompareOrdinalUnchecked (s, i, ns_length, nfi.NegativeSign, 0, ns_length) == 0) {
				sign = -1;
				i += ns_length;
			}
			
			for (; i < len; i++){
				c = s [i];

				if (c == '\0') {
					i = len;
					continue;
				}
				
				if (c >= '0' && c <= '9'){
					byte d = (byte) (c - '0');
						
					if (val > (MaxValue/10))
						goto overflow;
					
					if (val == (MaxValue/10)){
						if ((d > (MaxValue % 10)) && (sign == 1 || (d > ((MaxValue % 10) + 1))))
							goto overflow;
						if (sign == -1)
							val = (val * sign * 10) - d;
						else
							val = (val * 10) + d;

						if (ProcessTrailingWhitespace (tryParse, s, i + 1, ref exc)){
							result = val;
							return true;
						}
						goto overflow;
					} else 
						val = val * 10 + d;
					
					digits_seen = true;
				} else if (!ProcessTrailingWhitespace (tryParse, s, i, ref exc))
					return false;
			}
			if (!digits_seen) {
				if (!tryParse)
					exc = GetFormatException ();
				return false;
			}

			if (sign == -1)
				result = val * sign;
			else
				result = val;

			return true;

		overflow:
			if (!tryParse)
				exc = new OverflowException ("Value is too large");
			return false;
		}

		public static int Parse (string s, IFormatProvider provider)
		{
			return Parse (s, NumberStyles.Integer, provider);
		}

		public static int Parse (string s, NumberStyles style)
		{
			return Parse (s, style, null);
		}

		internal static bool CheckStyle (NumberStyles style, bool tryParse, ref Exception exc)
		{
			if ((style & NumberStyles.AllowHexSpecifier) != 0) {
				NumberStyles ne = style ^ NumberStyles.AllowHexSpecifier;
				if ((ne & NumberStyles.AllowLeadingWhite) != 0)
					ne ^= NumberStyles.AllowLeadingWhite;
				if ((ne & NumberStyles.AllowTrailingWhite) != 0)
					ne ^= NumberStyles.AllowTrailingWhite;
				if (ne != 0) {
					if (!tryParse)
						exc = new ArgumentException (
							"With AllowHexSpecifier only " + 
							"AllowLeadingWhite and AllowTrailingWhite " + 
							"are permitted.");
					return false;
				}
			} else if ((uint) style > (uint) NumberStyles.Any){
				if (!tryParse)
					exc = new ArgumentException ("Not a valid number style");
				return false;
			}

			return true;
		}
		
		internal static bool JumpOverWhite (ref int pos, string s, bool reportError, bool tryParse, ref Exception exc)
		{
			while (pos < s.Length && Char.IsWhiteSpace (s [pos]))
				pos++;

			if (reportError && pos >= s.Length) {
				if (!tryParse)
					exc = GetFormatException ();
				return false;
			}

			return true;
		}

		internal static void FindSign (ref int pos, string s, NumberFormatInfo nfi, 
				      ref bool foundSign, ref bool negative)
		{
			if ((pos + nfi.NegativeSign.Length) <= s.Length &&
				s.IndexOfOrdinalUnchecked (nfi.NegativeSign, pos, nfi.NegativeSign.Length) == pos) {
				negative = true;
				foundSign = true;
				pos += nfi.NegativeSign.Length;
			} else if ((pos + nfi.PositiveSign.Length) <= s.Length &&
				s.IndexOfOrdinalUnchecked (nfi.PositiveSign, pos, nfi.PositiveSign.Length) == pos) {
				negative = false;
				pos += nfi.PositiveSign.Length;
				foundSign = true;
			} 
		}

		internal static void FindCurrency (ref int pos,
						 string s, 
						 NumberFormatInfo nfi,
						 ref bool foundCurrency)
		{
			if ((pos + nfi.CurrencySymbol.Length) <= s.Length &&
			     s.Substring (pos, nfi.CurrencySymbol.Length) == nfi.CurrencySymbol) {
				foundCurrency = true;
				pos += nfi.CurrencySymbol.Length;
			} 
		}

		internal static bool FindExponent (ref int pos, string s, ref int exponent, bool tryParse, ref Exception exc)
		{
				exponent = 0;

				if (pos >= s.Length || (s [pos] != 'e' && s[pos] != 'E')) {
					exc = null;
					return false;
				}

				var i = pos + 1;
				if (i == s.Length) {
					exc = tryParse ? null : GetFormatException ();
					return true;
				}

				// negative exponent not valid for Int32
				if (s [i] == '-') {
					exc = tryParse ? null : new OverflowException ("Value too large or too small.");
					return true;
				}

				if (s [i] == '+' && ++i == s.Length) {
					exc = tryParse ? null : GetFormatException ();
					return true;
				}

				long exp = 0; // temp long value
				for (; i < s.Length; i++) {
					if (!Char.IsDigit (s [i]))  {
						exc = tryParse ? null : GetFormatException ();
						return true;
					}

					// Reduce the risk of throwing an overflow exc
					exp = checked (exp * 10 - (int) (s [i] - '0'));
					if (exp < Int32.MinValue || exp > Int32.MaxValue) {
						exc = tryParse ? null : new OverflowException ("Value too large or too small.");
						return true;
					}
				}

				// exp value saved as negative
				exp = -exp;

				exc = null;
				exponent = (int)exp;
				pos = i;
				return true;
		}

		internal static bool FindOther (ref int pos,
					      string s, 
					      string other)
		{
			if ((pos + other.Length) <= s.Length &&
			     s.Substring (pos, other.Length) == other) {
				pos += other.Length;
				return true;
			} 

			return false;
		}

		internal static bool ValidDigit (char e, bool allowHex)
		{
			if (allowHex)
				return Char.IsDigit (e) || (e >= 'A' && e <= 'F') || (e >= 'a' && e <= 'f');

			return Char.IsDigit (e);
		}
		
		internal static Exception GetFormatException ()
		{
			return new FormatException ("Input string was not in the correct format");
		}
		
		internal static bool Parse (string s, NumberStyles style, IFormatProvider fp, bool tryParse, out int result, out Exception exc)
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
					exc = GetFormatException ();
				return false;
			}

			NumberFormatInfo nfi = null;
			if (fp != null) {
				Type typeNFI = typeof (System.Globalization.NumberFormatInfo);
				nfi = (NumberFormatInfo) fp.GetFormat (typeNFI);
			}
			if (nfi == null)
				nfi = Thread.CurrentThread.CurrentCulture.NumberFormat;

			if (!CheckStyle (style, tryParse, ref exc))
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

			if (AllowLeadingWhite && !JumpOverWhite (ref pos, s, true, tryParse, ref exc))
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
				if (AllowLeadingWhite && !JumpOverWhite (ref pos, s, true, tryParse, ref exc))
					return false;

				if (s.Substring (pos, nfi.NegativeSign.Length) == nfi.NegativeSign) {
					if (!tryParse)
						exc = GetFormatException ();
					return false;
				}
				
				if (s.Substring (pos, nfi.PositiveSign.Length) == nfi.PositiveSign) {
					if (!tryParse)
						exc = GetFormatException ();
					return false;
				}
			}

			if (AllowLeadingSign && !foundSign) {
				// Sign + Currency
				FindSign (ref pos, s, nfi, ref foundSign, ref negative);
				if (foundSign) {
					if (AllowLeadingWhite && !JumpOverWhite (ref pos, s, true, tryParse, ref exc))
						return false;
					if (AllowCurrencySymbol) {
						FindCurrency (ref pos, s, nfi,
							      ref foundCurrency);
						if (foundCurrency && AllowLeadingWhite &&
								!JumpOverWhite (ref pos, s, true, tryParse, ref exc))
							return false;
					}
				}
			}
			
			if (AllowCurrencySymbol && !foundCurrency) {
				// Currency + sign
				FindCurrency (ref pos, s, nfi, ref foundCurrency);
				if (foundCurrency) {
					if (AllowLeadingWhite && !JumpOverWhite (ref pos, s, true, tryParse, ref exc))
						return false;
					if (foundCurrency) {
						if (!foundSign && AllowLeadingSign) {
							FindSign (ref pos, s, nfi, ref foundSign,
								  ref negative);
							if (foundSign && AllowLeadingWhite &&
									!JumpOverWhite (ref pos, s, true, tryParse, ref exc))
								return false;
						}
					}
				}
			}

			int number = 0;
			int nDigits = 0;
			int decimalPointPos = -1;
			int digitValue;
			char hexDigit;
				
			// Number stuff
			while (pos < s.Length) {

				if (!ValidDigit (s [pos], AllowHexSpecifier)) {
					if (AllowThousands &&
					    (FindOther (ref pos, s, nfi.NumberGroupSeparator)
						|| FindOther (ref pos, s, nfi.CurrencyGroupSeparator)))
					    continue;

					if (AllowDecimalPoint && decimalPointPos < 0 &&
					    (FindOther (ref pos, s, nfi.NumberDecimalSeparator)
						|| FindOther (ref pos, s, nfi.CurrencyDecimalSeparator))) {
						decimalPointPos = nDigits;
					    continue;
					}

					break;
				}

				nDigits++;

				if (AllowHexSpecifier) {
					hexDigit = s [pos++];
					if (Char.IsDigit (hexDigit))
						digitValue = (int) (hexDigit - '0');
					else if (Char.IsLower (hexDigit))
						digitValue = (int) (hexDigit - 'a' + 10);
					else
						digitValue = (int) (hexDigit - 'A' + 10);

					uint unumber = (uint)number;
					if (tryParse){
						if ((unumber & 0xf0000000) != 0)
							return false;
						
						number = (int) (unumber * 16u + (uint) digitValue);
					} else {
						number = (int)checked (unumber * 16u + (uint)digitValue);
					}

					continue;
				}

				try {
					// Calculations done as negative
					// (abs (MinValue) > abs (MaxValue))
					number = checked (number * 10 - (int) (s [pos++] - '0'));
				} catch (OverflowException) {
					if (!tryParse)
						exc = new OverflowException ("Value too large or too small.");
					return false;
				}
			}

			// Post number stuff
			if (nDigits == 0) {
				if (!tryParse)
					exc = GetFormatException ();
				return false;
			}

			int exponent = 0;
			if (AllowExponent)
				if (FindExponent (ref pos, s, ref exponent, tryParse, ref exc) && exc != null)
					return false;

			if (AllowTrailingSign && !foundSign) {
				// Sign + Currency
				FindSign (ref pos, s, nfi, ref foundSign, ref negative);
				if (foundSign && pos < s.Length) {
					if (AllowTrailingWhite && !JumpOverWhite (ref pos, s, true, tryParse, ref exc))
						return false;
				}
			}
			
			if (AllowCurrencySymbol && !foundCurrency) {
				if (AllowTrailingWhite && pos < s.Length && !JumpOverWhite (ref pos, s, false, tryParse, ref exc))
					return false;
				
				// Currency + sign
				FindCurrency (ref pos, s, nfi, ref foundCurrency);
				if (foundCurrency  && pos < s.Length) {
					if (AllowTrailingWhite  && !JumpOverWhite (ref pos, s, true, tryParse, ref exc))
						return false;
					if (!foundSign && AllowTrailingSign)
						FindSign (ref pos, s, nfi, ref foundSign,
							  ref negative);
				}
			}
			
			if (AllowTrailingWhite && pos < s.Length && !JumpOverWhite (ref pos, s, false, tryParse, ref exc))
				return false;

			if (foundOpenParentheses) {
				if (pos >= s.Length || s [pos++] != ')') {
					if (!tryParse)
						exc = GetFormatException ();
					return false;
				}
				if (AllowTrailingWhite && pos < s.Length && !JumpOverWhite (ref pos, s, false, tryParse, ref exc))
					return false;
			}

			if (pos < s.Length && s [pos] != '\u0000') {
				if (!tryParse)
					exc = GetFormatException ();
				return false;
			}
			
			if (!negative && !AllowHexSpecifier){
				if (tryParse){
					long lval = -((long)number);

					if (lval < MinValue || lval > MaxValue)
						return false;
					number = (int) lval;
				} else
					number = checked (-number);
			}

			if (decimalPointPos >= 0)
				exponent = exponent - nDigits + decimalPointPos;
			
			if (exponent < 0) {
				//
				// Any non-zero values after decimal point are not allowed
				//
				int remainder;
				number = Math.DivRem (number, (int) Math.Pow (10, -exponent), out remainder);
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

				number = (int)res;
			}
			
			result = number;
			return true;
		}

		public static int Parse (string s) 
		{
			Exception exc;
			int res;

			if (!Parse (s, false, out res, out exc))
				throw exc;

			return res;
		}

		public static int Parse (string s, NumberStyles style, IFormatProvider provider) 
		{
			Exception exc;
			int res;

			if (!Parse (s, style, provider, false, out res, out exc))
				throw exc;

			return res;
		}

		public static bool TryParse (string s, out int result) 
		{
			Exception exc;
			if (!Parse (s, true, out result, out exc)) {
				result = 0;
				return false;
			}

			return true;
		}

		public static bool TryParse (string s, NumberStyles style, IFormatProvider provider, out int result) 
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
			return TypeCode.Int32;
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
			return m_value;
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
