//
// System.Int32.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Globalization;
using System.Threading;

namespace System {
	
	[Serializable]
	public struct Int32 : IComparable, IFormattable, IConvertible {

		public const int MaxValue = 0x7fffffff;
		public const int MinValue = -2147483648;
		
		internal int value;

		public int CompareTo (object v)
		{
			if (v == null)
				return 1;
			
			if (!(v is System.Int32))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Int32"));

			int xv = (int) v;
			if (value == xv)
				return 0;
			if (value > xv)
				return 1;
			else
				return -1;
		}

		public override bool Equals (object o)
		{
			if (!(o is System.Int32))
				return false;

			return ((int) o) == value;
		}

		public override int GetHashCode ()
		{
			return value;
		}

		public static int Parse (string s)
		{
			int val = 0;
			int len;
			int i, sign = 1;
			bool digits_seen = false;
			
			if (s == null)
				throw new ArgumentNullException (Locale.GetText ("s is null"));

			len = s.Length;

			char c;
			for (i = 0; i < len; i++){
				c = s [i];
				if (!Char.IsWhiteSpace (c))
					break;
			}
			
			if (i == len)
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
								throw new FormatException ();
						}
						break;
					} else
						throw new FormatException ();
				}
			}
			if (!digits_seen)
				throw new FormatException ();
			
			return val;
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
		
		public static int Parse (string s, NumberStyles style, IFormatProvider fp)
		{
			if (s == null)
				throw new ArgumentNullException ();

			if (s.Length == 0)
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
			bool AllowExponent = (style & NumberStyles.AllowExponent) != 0;
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
					throw new FormatException ("Input string was not in the correct " +
								   "format.");
				if (s.Substring (pos, nfi.PositiveSign.Length) == nfi.PositiveSign)
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

					number = checked (number * 16 - digitValue);
				}
				else if (decimalPointFound) {
					nDigits++;
					// Allows decimal point as long as it's only 
					// followed by zeroes.
					if (s [pos++] != '0')
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
						throw new OverflowException ("Value too large or too " +
									     "small.");
					}
				}
			} while (pos < s.Length);

			// Post number stuff
			if (nDigits == 0)
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
					throw new FormatException ("Input string was not in the correct " +
								   "format.");
				if (AllowTrailingWhite && pos < s.Length)
					pos = JumpOverWhite (pos, s, false);
			}

			if (pos < s.Length && s [pos] != '\u0000')
				throw new FormatException ("Input string was not in the correct format.");

			
			if (!negative)
				number = -number;

			return number;
		}

		public override string ToString ()
		{
			return IntegerFormatter.FormatGeneral(value, 0, null, true);
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
			
			if ( format == null )
				format = "G";

			return IntegerFormatter.NumberToString (format, nfi, value);
		}

		// =========== IConvertible Methods =========== //

		public TypeCode GetTypeCode ()
		{
			return TypeCode.Int32;
		}
		
		bool IConvertible.ToBoolean  (IFormatProvider provider)
		{
			return System.Convert.ToBoolean (value);
		}
		byte IConvertible.ToByte     (IFormatProvider provider)
		{
			return System.Convert.ToByte (value);
		}
		char IConvertible.ToChar     (IFormatProvider provider)
		{
			return System.Convert.ToChar (value);
		}
		DateTime IConvertible.ToDateTime (IFormatProvider provider)
		{
			return System.Convert.ToDateTime (value);
		}
		decimal IConvertible.ToDecimal  (IFormatProvider provider)
		{
			return System.Convert.ToDecimal (value);
		}
		double IConvertible.ToDouble   (IFormatProvider provider)
		{
			return System.Convert.ToDouble (value);
		}
		short IConvertible.ToInt16    (IFormatProvider provider)
		{
			return System.Convert.ToInt16 (value);
		}
		int IConvertible.ToInt32    (IFormatProvider provider)
		{
			return value;
		}
		long IConvertible.ToInt64    (IFormatProvider provider)
		{
			return System.Convert.ToInt64 (value);
		}
    		[CLSCompliant(false)]
		sbyte IConvertible.ToSByte    (IFormatProvider provider)
		{
			return System.Convert.ToSByte (value);
		}
		float IConvertible.ToSingle   (IFormatProvider provider)
		{
			return System.Convert.ToSingle (value);
		}

		object IConvertible.ToType     (Type conversionType, IFormatProvider provider)
		{
			return System.Convert.ToType (value, conversionType, provider);
		}
		
    		[CLSCompliant(false)]
		ushort IConvertible.ToUInt16   (IFormatProvider provider)
		{
			return System.Convert.ToUInt16 (value);
		}
    		[CLSCompliant(false)]
		uint IConvertible.ToUInt32   (IFormatProvider provider)
		{
			return System.Convert.ToUInt32 (value);
		}
    		[CLSCompliant(false)]
		ulong IConvertible.ToUInt64   (IFormatProvider provider)
		{
			return System.Convert.ToUInt64 (value);
		}
	}
}
