//
// System.Int32.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
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
using System.Threading;

namespace System {
	
	[Serializable]
	public struct Int32 : IFormattable, IConvertible,
#if NET_2_0
		IComparable, IComparable<Int32>
#else
		IComparable
#endif
	{

		public const int MaxValue = 0x7fffffff;
		public const int MinValue = -2147483648;
		
		// This field is looked up by name in the runtime
		internal int m_value;

		public int CompareTo (object v)
		{
			if (v == null)
				return 1;
			
			if (!(v is System.Int32))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Int32"));

			int xv = (int) v;
			if (m_value == xv)
				return 0;
			if (m_value > xv)
				return 1;
			else
				return -1;
		}

		public override bool Equals (object o)
		{
			if (!(o is System.Int32))
				return false;

			return ((int) o) == m_value;
		}

		public override int GetHashCode ()
		{
			return m_value;
		}

#if NET_2_0
		public int CompareTo (int value)
		{
			if (m_value == value)
				return 0;
			if (m_value > value)
				return 1;
			else
				return -1;
		}

		public bool Equals (int value)
		{
			return value == m_value;
		}
#endif

		internal static bool Parse (string s, bool tryParse, out int result)
		{
			int val = 0;
			int len;
			int i, sign = 1;
			bool digits_seen = false;

			result = 0;

			if (s == null)
				if (tryParse)
					return false;
				else
					throw new ArgumentNullException ("s");

			len = s.Length;

			char c;
			for (i = 0; i < len; i++){
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
			else if (c == '-'){
				sign = -1;
				i++;
			}
			
			for (; i < len; i++){
				c = s [i];

				if (c == '\0') {
					i = len;
					continue;
				}
				
				if (c >= '0' && c <= '9'){
					val = checked (val * 10 + (c - '0') * sign);
					digits_seen = true;
				} else {
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
			if (!digits_seen)
				if (tryParse)
					return false;
				else
					throw new FormatException ();

			result = val;

			return true;
		}

		public static int Parse (string s, IFormatProvider fp)
		{
			return Parse (s, NumberStyles.Integer, fp);
		}

		public static int Parse (string s, NumberStyles style)
		{
			return Parse (s, style, null);
		}

		internal static void CheckStyle (NumberStyles style)
		{
			if ((style & NumberStyles.AllowHexSpecifier) != 0) {
				NumberStyles ne = style ^ NumberStyles.AllowHexSpecifier;
				if ((ne & NumberStyles.AllowLeadingWhite) != 0)
					ne ^= NumberStyles.AllowLeadingWhite;
				if ((ne & NumberStyles.AllowTrailingWhite) != 0)
					ne ^= NumberStyles.AllowTrailingWhite;
				if (ne != 0)
					throw new ArgumentException (
						"With AllowHexSpecifier only " + 
						"AllowLeadingWhite and AllowTrailingWhite " + 
						"are permitted.");
			}
		}
		
		internal static int JumpOverWhite (int pos, string s, bool excp)
		{
			while (pos < s.Length && Char.IsWhiteSpace (s [pos]))
				pos++;

			if (excp && pos >= s.Length)
				throw new FormatException ("Input string was not in the correct format.");

			return pos;
		}

		internal static void FindSign (ref int pos, string s, NumberFormatInfo nfi, 
				      ref bool foundSign, ref bool negative)
		{
			if ((pos + nfi.NegativeSign.Length) <= s.Length &&
			     s.Substring (pos, nfi.NegativeSign.Length) == nfi.NegativeSign) {
				negative = true;
				foundSign = true;
				pos += nfi.NegativeSign.Length;
			} 
			else if ((pos + nfi.PositiveSign.Length) < s.Length &&
			     s.Substring (pos, nfi.PositiveSign.Length) == nfi.PositiveSign) {
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
		
		internal static bool Parse (string s, NumberStyles style, IFormatProvider fp, bool tryParse, out int result)
		{
			result = 0;

			if (s == null)
				if (tryParse)
					return false;
				else
					throw new ArgumentNullException ();

			if (s.Length == 0)
				if (tryParse)
					return false;
				else
					throw new FormatException ("Input string was not " + 
											   "in the correct format.");

			NumberFormatInfo nfi;
			if (fp != null) {
				Type typeNFI = typeof (System.Globalization.NumberFormatInfo);
				nfi = (NumberFormatInfo) fp.GetFormat (typeNFI);
			}
			else
				nfi = Thread.CurrentThread.CurrentCulture.NumberFormat;

			CheckStyle (style);

			bool AllowCurrencySymbol = (style & NumberStyles.AllowCurrencySymbol) != 0;
			bool AllowHexSpecifier = (style & NumberStyles.AllowHexSpecifier) != 0;
			bool AllowThousands = (style & NumberStyles.AllowThousands) != 0;
			bool AllowDecimalPoint = (style & NumberStyles.AllowDecimalPoint) != 0;
			bool AllowParentheses = (style & NumberStyles.AllowParentheses) != 0;
			bool AllowTrailingSign = (style & NumberStyles.AllowTrailingSign) != 0;
			bool AllowLeadingSign = (style & NumberStyles.AllowLeadingSign) != 0;
			bool AllowTrailingWhite = (style & NumberStyles.AllowTrailingWhite) != 0;
			bool AllowLeadingWhite = (style & NumberStyles.AllowLeadingWhite) != 0;

			int pos = 0;

			if (AllowLeadingWhite)
				pos = JumpOverWhite (pos, s, true);

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
				if (AllowLeadingWhite)
					pos = JumpOverWhite (pos, s, true);

				if (s.Substring (pos, nfi.NegativeSign.Length) == nfi.NegativeSign)
					if (tryParse)
						return false;
					else
						throw new FormatException ("Input string was not in the correct " +
												   "format.");
				if (s.Substring (pos, nfi.PositiveSign.Length) == nfi.PositiveSign)
					if (tryParse)
						return false;
					else
						throw new FormatException ("Input string was not in the correct " +
												   "format.");
			}

			if (AllowLeadingSign && !foundSign) {
				// Sign + Currency
				FindSign (ref pos, s, nfi, ref foundSign, ref negative);
				if (foundSign) {
					if (AllowLeadingWhite)
						pos = JumpOverWhite (pos, s, true);
					if (AllowCurrencySymbol) {
						FindCurrency (ref pos, s, nfi,
							      ref foundCurrency);
						if (foundCurrency && AllowLeadingWhite)
							pos = JumpOverWhite (pos, s, true);
					}
				}
			}
			
			if (AllowCurrencySymbol && !foundCurrency) {
				// Currency + sign
				FindCurrency (ref pos, s, nfi, ref foundCurrency);
				if (foundCurrency) {
					if (AllowLeadingWhite)
						pos = JumpOverWhite (pos, s, true);
					if (foundCurrency) {
						if (!foundSign && AllowLeadingSign) {
							FindSign (ref pos, s, nfi, ref foundSign,
								  ref negative);
							if (foundSign && AllowLeadingWhite)
								pos = JumpOverWhite (pos, s, true);
						}
					}
				}
			}
			
			int number = 0;
			int nDigits = 0;
			bool decimalPointFound = false;
			int digitValue;
			char hexDigit;
				
			// Number stuff
			do {

				if (!ValidDigit (s [pos], AllowHexSpecifier)) {
					if (AllowThousands &&
					    FindOther (ref pos, s, nfi.NumberGroupSeparator))
					    continue;
					else
					if (!decimalPointFound && AllowDecimalPoint &&
					    FindOther (ref pos, s, nfi.NumberDecimalSeparator)) {
					    decimalPointFound = true;
					    continue;
					}

					break;
				}
				else if (AllowHexSpecifier) {
					nDigits++;
					hexDigit = s [pos++];
					if (Char.IsDigit (hexDigit))
						digitValue = (int) (hexDigit - '0');
					else if (Char.IsLower (hexDigit))
						digitValue = (int) (hexDigit - 'a' + 10);
					else
						digitValue = (int) (hexDigit - 'A' + 10);

					uint unumber = (uint)number;
					number = (int)checked (unumber * 16u + (uint)digitValue);
				}
				else if (decimalPointFound) {
					nDigits++;
					// Allows decimal point as long as it's only 
					// followed by zeroes.
					if (s [pos++] != '0')
						if (tryParse)
							return false;
						else
							throw new OverflowException ("Value too large or too " +
														 "small.");
				}
				else {
					nDigits++;

					try {
						// Calculations done as negative
						// (abs (MinValue) > abs (MaxValue))
						number = checked (
							number * 10 - 
							(int) (s [pos++] - '0')
							);
					} catch (OverflowException) {
						if (tryParse)
							return false;
						else
							throw new OverflowException ("Value too large or too " +
														 "small.");
					}
				}
			} while (pos < s.Length);

			// Post number stuff
			if (nDigits == 0)
				if (tryParse)
					return false;
				else
					throw new FormatException ("Input string was not in the correct format.");

			if (AllowTrailingSign && !foundSign) {
				// Sign + Currency
				FindSign (ref pos, s, nfi, ref foundSign, ref negative);
				if (foundSign) {
					if (AllowTrailingWhite)
						pos = JumpOverWhite (pos, s, true);
					if (AllowCurrencySymbol)
						FindCurrency (ref pos, s, nfi,
							      ref foundCurrency);
				}
			}
			
			if (AllowCurrencySymbol && !foundCurrency) {
				// Currency + sign
				FindCurrency (ref pos, s, nfi, ref foundCurrency);
				if (foundCurrency) {
					if (AllowTrailingWhite)
						pos = JumpOverWhite (pos, s, true);
					if (!foundSign && AllowTrailingSign)
						FindSign (ref pos, s, nfi, ref foundSign,
							  ref negative);
				}
			}
			
			if (AllowTrailingWhite && pos < s.Length)
				pos = JumpOverWhite (pos, s, false);

			if (foundOpenParentheses) {
				if (pos >= s.Length || s [pos++] != ')')
					if (tryParse)
						return false;
					else
						throw new FormatException ("Input string was not in the correct " +
												   "format.");
				if (AllowTrailingWhite && pos < s.Length)
					pos = JumpOverWhite (pos, s, false);
			}

			if (pos < s.Length && s [pos] != '\u0000')
				if (tryParse)
					return false;
				else
					throw new FormatException ("Input string was not in the correct format.");

			
			if (!negative && !AllowHexSpecifier)
				number = -number;

			result = number;

			return true;
		}

		public static int Parse (string s) {
			int res;

			Parse (s, false, out res);

			return res;
		}

		public static int Parse (string s, NumberStyles style, IFormatProvider fp) {
			int res;

			Parse (s, style, fp, false, out res);

			return res;
		}

#if NET_2_0
		public static bool TryParse (string s, out int result) {
			try {
				return Parse (s, true, out result);
			}
			catch (Exception) {
				result = 0;
				return false;
			}
		}

		public static bool TryParse (string s, NumberStyles style, IFormatProvider provider, out int result) {
			try {
				return Parse (s, style, provider, true, out result);
			}
			catch (Exception) {
				result = 0;
				return false;
			}
		}
#endif

		public override string ToString ()
		{
			return IntegerFormatter.FormatGeneral(m_value, 0, null, true);
		}

		public string ToString (IFormatProvider fp)
		{
			return ToString (null, fp);
		}

		public string ToString (string format)
		{
			return ToString (format, null);
		}

		public string ToString (string format, IFormatProvider fp )
		{
			NumberFormatInfo nfi = NumberFormatInfo.GetInstance( fp );
			
			// use "G" when format is null or String.Empty
			if ((format == null) || (format.Length == 0))
				format = "G";

			return IntegerFormatter.NumberToString (format, nfi, m_value);
		}

		// =========== IConvertible Methods =========== //

		public TypeCode GetTypeCode ()
		{
			return TypeCode.Int32;
		}
		
		bool IConvertible.ToBoolean  (IFormatProvider provider)
		{
			return System.Convert.ToBoolean (m_value);
		}
		byte IConvertible.ToByte     (IFormatProvider provider)
		{
			return System.Convert.ToByte (m_value);
		}
		char IConvertible.ToChar     (IFormatProvider provider)
		{
			return System.Convert.ToChar (m_value);
		}
		DateTime IConvertible.ToDateTime (IFormatProvider provider)
		{
			return System.Convert.ToDateTime (m_value);
		}
		decimal IConvertible.ToDecimal  (IFormatProvider provider)
		{
			return System.Convert.ToDecimal (m_value);
		}
		double IConvertible.ToDouble   (IFormatProvider provider)
		{
			return System.Convert.ToDouble (m_value);
		}
		short IConvertible.ToInt16    (IFormatProvider provider)
		{
			return System.Convert.ToInt16 (m_value);
		}
		int IConvertible.ToInt32    (IFormatProvider provider)
		{
			return m_value;
		}
		long IConvertible.ToInt64    (IFormatProvider provider)
		{
			return System.Convert.ToInt64 (m_value);
		}

		sbyte IConvertible.ToSByte    (IFormatProvider provider)
		{
			return System.Convert.ToSByte (m_value);
		}
		float IConvertible.ToSingle   (IFormatProvider provider)
		{
			return System.Convert.ToSingle (m_value);
		}

		object IConvertible.ToType     (Type conversionType, IFormatProvider provider)
		{
			return System.Convert.ToType (m_value, conversionType, provider);
		}
		
		ushort IConvertible.ToUInt16   (IFormatProvider provider)
		{
			return System.Convert.ToUInt16 (m_value);
		}

		uint IConvertible.ToUInt32   (IFormatProvider provider)
		{
			return System.Convert.ToUInt32 (m_value);
		}
		ulong IConvertible.ToUInt64   (IFormatProvider provider)
		{
			return System.Convert.ToUInt64 (m_value);
		}
	}
}
