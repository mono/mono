//
// System.UInt64.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Globalization;
using System.Threading;

namespace System
{
	[Serializable]
	[CLSCompliant (false)]
	public struct UInt64 : IComparable, IFormattable, IConvertible
	{
		public const ulong MaxValue = 0xffffffffffffffff;
		public const ulong MinValue = 0;

		internal ulong value;

		public int CompareTo (object value)
		{
			if (value == null)
				return 1;

			if (!(value is System.UInt64))
				throw new ArgumentException (Locale.GetText ("Value is not a System.UInt64."));

			if (this.value == (ulong) value)
				return 0;

			if (this.value < (ulong) value)
				return -1;

			return 1;
		}

		public override bool Equals (object obj)
		{
			if (!(obj is System.UInt64))
				return false;

			return ((ulong) obj) == value;
		}

		public override int GetHashCode ()
		{
			return (int)(value & 0xffffffff) ^ (int)(value >> 32);
		}

		[CLSCompliant (false)]
		public static ulong Parse (string s)
		{
			return Parse (s, NumberStyles.Integer, null);
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

		[CLSCompliant (false)]
		public static ulong Parse (string s, NumberStyles style, IFormatProvider provider)
		{
			if (s == null)
				throw new ArgumentNullException ("s");

			if (s.Length == 0)
				throw new FormatException (Locale.GetText ("Input string was not in the correct format."));

			NumberFormatInfo nfi;
			if (provider != null) {
				Type typeNFI = typeof (NumberFormatInfo);
				nfi = (NumberFormatInfo) provider.GetFormat (typeNFI);
			}
			else
				nfi = Thread.CurrentThread.CurrentCulture.NumberFormat;

			Int32.CheckStyle (style);

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
				pos = Int32.JumpOverWhite (pos, s, true);

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
					pos = Int32.JumpOverWhite (pos, s, true);

				if (s.Substring (pos, nfi.NegativeSign.Length) == nfi.NegativeSign)
					throw new FormatException (Locale.GetText ("Input string was not in the correct format."));
				if (s.Substring (pos, nfi.PositiveSign.Length) == nfi.PositiveSign)
					throw new FormatException (Locale.GetText ("Input string was not in the correct format."));
			}

			if (AllowLeadingSign && !foundSign) {
				// Sign + Currency
				Int32.FindSign (ref pos, s, nfi, ref foundSign, ref negative);
				if (foundSign) {
					if (AllowLeadingWhite)
						pos = Int32.JumpOverWhite (pos, s, true);
					if (AllowCurrencySymbol) {
						Int32.FindCurrency (ref pos, s, nfi,
								    ref foundCurrency);
						if (foundCurrency && AllowLeadingWhite)
							pos = Int32.JumpOverWhite (pos, s, true);
					}
				}
			}

			if (AllowCurrencySymbol && !foundCurrency) {
				// Currency + sign
				Int32.FindCurrency (ref pos, s, nfi, ref foundCurrency);
				if (foundCurrency) {
					if (AllowLeadingWhite)
						pos = Int32.JumpOverWhite (pos, s, true);
					if (foundCurrency) {
						if (!foundSign && AllowLeadingSign) {
							Int32.FindSign (ref pos, s, nfi, ref foundSign,
									ref negative);
							if (foundSign && AllowLeadingWhite)
								pos = Int32.JumpOverWhite (pos, s, true);
						}
					}
				}
			}

			ulong number = 0;
			int nDigits = 0;
			bool decimalPointFound = false;
			ulong digitValue;
			char hexDigit;

			// Number stuff
			// Just the same as Int32, but this one adds instead of substract
			do {

				if (!Int32.ValidDigit (s [pos], AllowHexSpecifier)) {
					if (AllowThousands && Int32.FindOther (ref pos, s, nfi.NumberGroupSeparator))
						continue;
					else
						if (!decimalPointFound && AllowDecimalPoint && 
						    Int32.FindOther (ref pos, s, nfi.NumberDecimalSeparator)) {
							decimalPointFound = true;
							continue;
						}
					break;
				}
				else if (AllowHexSpecifier) {
					nDigits++;
					hexDigit = s [pos++];
					if (Char.IsDigit (hexDigit))
						digitValue = (ulong) (hexDigit - '0');
					else if (Char.IsLower (hexDigit))
						digitValue = (ulong) (hexDigit - 'a' + 10);
					else
						digitValue = (ulong) (hexDigit - 'A' + 10);

					number = checked (number * 16 + digitValue);
				}
				else if (decimalPointFound) {
					nDigits++;
					// Allows decimal point as long as it's only 
					// followed by zeroes.
					if (s [pos++] != '0')
						throw new OverflowException (Locale.GetText ("Value too large or too small."));
				}
				else {
					nDigits++;

					try {
						number = checked (number * 10 + (ulong) (s [pos++] - '0'));
					}
					catch (OverflowException) {
						throw new OverflowException (Locale.GetText ("Value too large or too small."));
					}
				}
			} while (pos < s.Length);

			// Post number stuff
			if (nDigits == 0)
				throw new FormatException (Locale.GetText ("Input string was not in the correct format."));

			if (AllowTrailingSign && !foundSign) {
				// Sign + Currency
				Int32.FindSign (ref pos, s, nfi, ref foundSign, ref negative);
				if (foundSign) {
					if (AllowTrailingWhite)
						pos = Int32.JumpOverWhite (pos, s, true);
					if (AllowCurrencySymbol)
						Int32.FindCurrency (ref pos, s, nfi, ref foundCurrency);
				}
			}

			if (AllowCurrencySymbol && !foundCurrency) {
				// Currency + sign
				Int32.FindCurrency (ref pos, s, nfi, ref foundCurrency);
				if (foundCurrency) {
					if (AllowTrailingWhite)
						pos = Int32.JumpOverWhite (pos, s, true);
					if (!foundSign && AllowTrailingSign)
						Int32.FindSign (ref pos, s, nfi, ref foundSign, ref negative);
				}
			}

			if (AllowTrailingWhite && pos < s.Length)
				pos = Int32.JumpOverWhite (pos, s, false);

			if (foundOpenParentheses) {
				if (pos >= s.Length || s [pos++] != ')')
					throw new FormatException (Locale.GetText
						("Input string was not in the correct format."));
				if (AllowTrailingWhite && pos < s.Length)
					pos = Int32.JumpOverWhite (pos, s, false);
			}

			if (pos < s.Length && s [pos] != '\u0000')
				throw new FormatException (Locale.GetText ("Input string was not in the correct format."));

			if (negative)
				throw new OverflowException (Locale.GetText ("Value too large or too small."));

			return number;
		}

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

			if (format == null)
				format = "G";

			return IntegerFormatter.NumberToString (format, nfi, value);
		}

		// =========== IConvertible Methods =========== //
		public TypeCode GetTypeCode ()
		{
			return TypeCode.UInt64;
		}

		bool IConvertible.ToBoolean (IFormatProvider provider)
		{
			return System.Convert.ToBoolean (value);
		}

		byte IConvertible.ToByte (IFormatProvider provider)
		{
			return System.Convert.ToByte (value);
		}

		char IConvertible.ToChar (IFormatProvider provider)
		{
			return System.Convert.ToChar (value);
		}

		DateTime IConvertible.ToDateTime (IFormatProvider provider)
		{
			return System.Convert.ToDateTime (value);
		}

		decimal IConvertible.ToDecimal (IFormatProvider provider)
		{
			return System.Convert.ToDecimal (value);
		}

		double IConvertible.ToDouble (IFormatProvider provider)
		{
			return System.Convert.ToDouble (value);
		}

		short IConvertible.ToInt16 (IFormatProvider provider)
		{
			return System.Convert.ToInt16 (value);
		}

		int IConvertible.ToInt32 (IFormatProvider provider)
		{
			return System.Convert.ToInt32 (value);
		}

		long IConvertible.ToInt64 (IFormatProvider provider)
		{
			return System.Convert.ToInt64 (value);
		}

		[CLSCompliant (false)]
		sbyte IConvertible.ToSByte(IFormatProvider provider)
		{
			return System.Convert.ToSByte (value);
		}

		float IConvertible.ToSingle (IFormatProvider provider)
		{
			return System.Convert.ToSingle (value);
		}

		object IConvertible.ToType (Type conversionType, IFormatProvider provider)
		{
			return System.Convert.ToType (value, conversionType, provider);
		}

		[CLSCompliant (false)]
		ushort IConvertible.ToUInt16 (IFormatProvider provider)
		{
			return System.Convert.ToUInt16 (value);
		}

		[CLSCompliant (false)]
		uint IConvertible.ToUInt32 (IFormatProvider provider)
		{
			return System.Convert.ToUInt32 (value);
		}

		[CLSCompliant (false)]
		ulong IConvertible.ToUInt64 (IFormatProvider provider)
		{
			return value;
		}
	}
}
