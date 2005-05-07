//
// System.NumberFormatter.cs
//
// Author:
//   Kazuki Oikawa (kazuki@panicode.com)
//

using System.Collections;
using System.Globalization;
using System.Text;

namespace System
{
	class NumberFormatter
	{
		static char[] digitLowerTable = {'0','1','2','3','4','5','6','7','8','9','a','b','c','d','e','f'};
		static char[] digitUpperTable = {'0','1','2','3','4','5','6','7','8','9','A','B','C','D','E','F'};

		#region NumberToString
		public static string NumberToString (string format, sbyte value, NumberFormatInfo nfi)
		{
			return NumberToString (format, new NumberStore (value), nfi);
		}
		public static string NumberToString (string format, byte value, NumberFormatInfo nfi)
		{
			return NumberToString (format, new NumberStore (value), nfi);
		}
		public static string NumberToString (string format, ushort value, NumberFormatInfo nfi)
		{
			return NumberToString (format, new NumberStore (value), nfi);
		}
		public static string NumberToString (string format, short value, NumberFormatInfo nfi)
		{
			return NumberToString (format, new NumberStore (value), nfi);
		}
		public static string NumberToString (string format, uint value, NumberFormatInfo nfi)
		{
			return NumberToString (format, new NumberStore (value), nfi);
		}
		public static string NumberToString (string format, int value, NumberFormatInfo nfi)
		{
			return NumberToString (format, new NumberStore (value), nfi);
		}
		public static string NumberToString (string format, ulong value, NumberFormatInfo nfi)
		{
			return NumberToString (format, new NumberStore (value), nfi);
		}
		public static string NumberToString (string format, long value, NumberFormatInfo nfi)
		{
			return NumberToString (format, new NumberStore (value), nfi);
		}
		public static string NumberToString (string format, float value, NumberFormatInfo nfi)
		{
			return NumberToString (format, new NumberStore (value), nfi);
		}
		public static string NumberToString (string format, double value, NumberFormatInfo nfi)
		{
			return NumberToString (format, new NumberStore (value), nfi);
		}
		public static string NumberToString (string format, NumberStore ns, NumberFormatInfo nfi)
		{
			if (ns.IsNaN) {
				return nfi.NaNSymbol;
			}
			if (ns.IsInfinity) {
				if (ns.Positive)
					return nfi.PositiveInfinitySymbol;
				else
					return nfi.NegativeInfinitySymbol;
			}

			char specifier;
			int precision;
			bool custom;

			if (format == null || format.Length == 0)
				format = "G";

			if (nfi == null)
				nfi = NumberFormatInfo.GetInstance (null);
			
			if (!ParseBasicFormat (format, out specifier, out precision, out custom))
				throw new FormatException (Locale.GetText ("The specified format '" + format + "' is invalid"));
			
			if (custom){
				if (ns.IsFloatingSource)
					ns.RoundEffectiveDigits (ns.DefaultPrecision);
				return FormatCustom (format, ns, nfi);
			}

			if (ns.IsFloatingSource) {
				switch(specifier) {
				case 'p':
				case 'P':
				case 'c':
				case 'C':
				case 'f':
				case 'F':
				case 'N':
				case 'n':
					ns.RoundEffectiveDigits (ns.DefaultPrecision);
					break;
				case 'g':
				case 'G':
					if (precision <= 0)
						ns.RoundEffectiveDigits (ns.DefaultPrecision);
					else
						ns.RoundEffectiveDigits (precision);
					break;
				case 'r':
				case 'R':
					ns.RoundEffectiveDigits (ns.DefaultMaxPrecision);
					break;
				default:
					if (precision > ns.DefaultPrecision)
						ns.RoundEffectiveDigits (precision + 1);
					else
						ns.RoundEffectiveDigits (ns.DefaultPrecision + 1);
					break;
				}
			}

			switch(specifier) {
			case 'c':
			case 'C':
				return FormatCurrency (ns, precision, nfi);
			case 'd':
			case 'D':
				return FormatDecimal (ns, precision, nfi);
			case 'e':
			case 'E':
				return FormatExponential (ns, precision, nfi, specifier == 'E');
			case 'f': 
			case 'F':
				return FormatFixedPoint (ns, precision, nfi);
			case 'g':
			case 'G':
				return FormatGeneral (ns, precision, nfi, specifier == 'G');
			case 'n':
			case 'N':
				return FormatNumber (ns, precision, nfi);
			case 'p':
			case 'P':
				return FormatPercent (ns, precision, nfi);
			case 'r':
			case 'R':
				if (ns.IsFloatingSource) {
					return FormatGeneral (ns, ns.DefaultPrecision, nfi, true);
				} else {
					throw new FormatException (Locale.GetText ("The specified format cannot be used in this instance"));
				}
			case 'x': 
			case 'X': return FormatHexadecimal (ns, precision, nfi, specifier == 'X');
			default: 
				throw new FormatException (Locale.GetText ("The specified format '" + format + "' is invalid"));
			}	
		}
		#endregion

		#region BasicParser
		private static bool ParseBasicFormat (string format, out char specifier, out int precision, out bool custom)
		{		 		 
			precision = -1;
			specifier = '\0';
			custom = false;
			
			if (format.Length < 1)
				return false;
			
			specifier = format [0];

			if (Char.IsLetter (specifier)) {
				if (format.Length == 1)
					return true;

				bool flag = true;
				precision = 0;
				for (int i = 1; i < format.Length; i++) {
					char c = format [i];
					if (char.IsDigit (c)) {
						precision = precision * 10 + (c - '0');
						if (precision > 99) {
							flag = false;
							break;
						}
					}
					else {
						flag = false;
						break;
					}
				}
				if (flag)
					return true;
			}

			custom = true;
			return true;
		}	

		#endregion

		#region Helpers
		private static void ZeroTrimEnd (StringBuilder sb)
		{
			ZeroTrimEnd (sb, false);
		}
		private static void ZeroTrimEnd (StringBuilder sb, bool canEmpty)
		{
			int len = 0;
			for (int i = sb.Length - 1; (canEmpty ? i >= 0 : i > 0); i --) {
				if (sb [i] != '0')
					break;
				len ++;
			}

			if (len > 0)
				sb.Remove (sb.Length - len, len);
		}
		#endregion

		#region Basic
		internal static string FormatCurrency (NumberStore ns, int precision, NumberFormatInfo nfi)
		{
			precision = (precision >= 0 ? precision : nfi.CurrencyDecimalDigits);
			ns.RoundDecimal (precision);
			StringBuilder sb = new StringBuilder (ns.IntegerDigits * 2 + precision * 2 + 16);
			bool needNegativeSign = !ns.Positive && !ns.ZeroOnly;

			if (!needNegativeSign) {
				switch (nfi.CurrencyPositivePattern) {
				case 0:
					sb.Append (nfi.CurrencySymbol);
					break;
				case 2:
					sb.Append (nfi.CurrencySymbol);
					sb.Append (' ');
					break;
				}
			} else {
				switch (nfi.CurrencyNegativePattern) {
				case 0:
					sb.Append ('(');
					sb.Append (nfi.CurrencySymbol);
					break;
				case 1:
					sb.Append (nfi.NegativeSign);
					sb.Append (nfi.CurrencySymbol);
					break;
				case 2:
					sb.Append (nfi.CurrencySymbol);
					sb.Append (nfi.NegativeSign);
					break;
				case 3:
					sb.Append (nfi.CurrencySymbol);
					break;
				case 4:
					sb.Append ('(');
					break;
				case 5:
					sb.Append (nfi.NegativeSign);
					break;
				case 8:
					sb.Append (nfi.NegativeSign);
					break;
				case 9:
					sb.Append (nfi.NegativeSign);
					sb.Append (nfi.CurrencySymbol);
					sb.Append (' ');					
					break;
				case 11:
					sb.Append (nfi.CurrencySymbol);
					sb.Append (' ');
					break;
				case 12:
					sb.Append (nfi.CurrencySymbol);
					sb.Append (' ');
					sb.Append (nfi.NegativeSign);					
					break;
				case 14:
					sb.Append ('(');
					sb.Append (nfi.CurrencySymbol);
					sb.Append (' ');
					break;
				case 15:
					sb.Append ('(');
					break;
				}
			}

			ns.AppendIntegerStringWithGroupSeparator (sb, nfi.CurrencyGroupSizes, nfi.CurrencyGroupSeparator);

			if (precision > 0)
			{
				sb.Append (nfi.CurrencyDecimalSeparator);
				ns.AppendDecimalString (precision, sb);
			}

			if (!needNegativeSign) {
				switch (nfi.CurrencyPositivePattern) {
				case 1:
					sb.Append (nfi.CurrencySymbol);
					break;
				case 3:
					sb.Append (' ');
					sb.Append (nfi.CurrencySymbol);
					break;
				}
			} else {
				switch (nfi.CurrencyNegativePattern) {
				case 0:
					sb.Append (')');
					break;
				case 3:
					sb.Append (nfi.NegativeSign);
					break;
				case 4:
					sb.Append (nfi.CurrencySymbol);
					sb.Append (')');
					break;
				case 5:
					sb.Append (nfi.CurrencySymbol);
					break;
				case 6:
					sb.Append (nfi.NegativeSign);
					sb.Append (nfi.CurrencySymbol);
					break;
				case 7:
					sb.Append (nfi.CurrencySymbol);
					sb.Append (nfi.NegativeSign);
					break;
				case 8:
					sb.Append (' ');
					sb.Append (nfi.CurrencySymbol);
					break;
				case 10:
					sb.Append (' ');
					sb.Append (nfi.CurrencySymbol);
					sb.Append (nfi.NegativeSign);
					break;
				case 11:
					sb.Append (nfi.NegativeSign);
					break;
				case 13:
					sb.Append (nfi.NegativeSign);
					sb.Append (' ');
					sb.Append (nfi.CurrencySymbol);
					break;
				case 14:
					sb.Append (')');
					break;
				case 15:
					sb.Append (' ');
					sb.Append (nfi.CurrencySymbol);
					sb.Append (')');
					break;
				}
			}

			return sb.ToString ();
		}
		internal static string FormatDecimal (NumberStore ns, int precision, NumberFormatInfo nfi)
		{
			if (ns.IsFloatingSource)
				throw new FormatException ();

			precision = precision > 0 ? precision : 1;
			precision = ns.IntegerDigits > precision ? ns.IntegerDigits : precision;

			StringBuilder sb = new StringBuilder (precision + nfi.NegativeSign.Length);

			if (!ns.Positive && !ns.CheckZeroOnlyInteger ()) {
				sb.Append (nfi.NegativeSign);
			}

			ns.AppendIntegerString (precision, sb);

			return sb.ToString ();
		}
		internal static string FormatFixedPoint (NumberStore ns, int precision, NumberFormatInfo nfi)
		{
			precision = precision >= 0 ? precision : nfi.NumberDecimalDigits;
			ns.RoundDecimal (precision);

			StringBuilder cb = new StringBuilder (ns.IntegerDigits + precision + nfi.NumberDecimalSeparator.Length);

			if (!ns.Positive && !ns.ZeroOnly)
				cb.Append (nfi.NegativeSign);

			ns.AppendIntegerString (ns.IntegerDigits > 0 ? ns.IntegerDigits : 1, cb);

			if (precision > 0) {
				cb.Append (nfi.NumberDecimalSeparator);
				ns.AppendDecimalString (precision, cb);
			}

			return cb.ToString ();
		}

		internal static string FormatGeneral (NumberStore ns)
		{
			return FormatGeneral (ns, -1, NumberFormatInfo.CurrentInfo, true);
		}
		internal static string FormatGeneral (NumberStore ns, IFormatProvider provider)
		{
			return FormatGeneral (ns, -1, NumberFormatInfo.GetInstance (provider), true);
		}
		private static string FormatGeneral (NumberStore ns, int precision, NumberFormatInfo nfi, bool roundtrip)
		{
			if (ns.ZeroOnly)
				return "0";

			precision = precision > 0 ? precision : ns.DefaultPrecision;

			int exponent = 0;
			bool expMode = (ns.IntegerDigits > precision || ns.DecimalPointPosition <= -4);
			if (expMode) {
				while (!(ns.DecimalPointPosition == 1 && ns.GetChar (0) != '0')) {
					if (ns.DecimalPointPosition > 1) {
						ns.Divide10 (1);
						exponent ++;
					} else {
						ns.Multiply10 (1);
						exponent --;
					}
				}
			}

			precision = precision < ns.DefaultPrecision + 2 ? (precision < 17 ? precision : 17) : ns.DefaultPrecision + 2;
			StringBuilder cb = new StringBuilder (ns.IntegerDigits + precision + 16);
			if (expMode) {
				if (ns.RoundDecimal (precision - 1)) {
					ns.Divide10 (1);
					exponent ++;
				}
			} else if (!roundtrip) {
				ns.RoundDecimal (precision);
			}

			if (!ns.Positive) {
				cb.Append (nfi.NegativeSign);
			}

			ns.AppendIntegerString (ns.IntegerDigits > 0 ? ns.IntegerDigits : 1, cb);

			if (ns.DecimalDigits > 0) {
				cb.Append (nfi.NumberDecimalSeparator);
				ns.AppendDecimalString (ns.DecimalDigits, cb);
			}

			if (expMode) {
				if (roundtrip)
					cb.Append ('E');
				else
					cb.Append ('e');

				if (exponent >= 0)
					cb.Append (nfi.PositiveSign);
				else {
					cb.Append (nfi.NegativeSign);
					exponent = -exponent;
				}

				if (exponent == 0) {
					cb.Append ('0', 2);
				} else if (exponent < 10) {
					cb.Append ('0');
					cb.Append (digitLowerTable [exponent]);
				} else if (exponent < 100) {
					cb.Append (digitLowerTable [exponent / 10 % 10]);
					cb.Append (digitLowerTable [exponent % 10]);
				} else if (exponent < 1000) {
					cb.Append (digitLowerTable [exponent / 100 % 10]);
					cb.Append (digitLowerTable [exponent / 10 % 10]);
					cb.Append (digitLowerTable [exponent % 10]);
				}
			}

			return cb.ToString ();
		}
		internal static string FormatNumber (NumberStore ns, int precision, NumberFormatInfo nfi)
		{
			precision = (precision >= 0 ? precision : nfi.NumberDecimalDigits);
			StringBuilder sb = new StringBuilder(ns.IntegerDigits * 3 + precision);

			ns.RoundDecimal (precision);
			bool needNegativeSign = (!ns.Positive && !ns.ZeroOnly);

			if (needNegativeSign) {
				switch (nfi.NumberNegativePattern) {
				case 0:
					sb.Append ('(');
					break;
				case 1:
					sb.Append (nfi.NegativeSign);
					break;
				case 2:
					sb.Append (nfi.NegativeSign);
					sb.Append (' ');
					break;
				}
			}

			ns.AppendIntegerStringWithGroupSeparator (sb, nfi.NumberGroupSizes, nfi.NumberGroupSeparator);

			if (precision > 0) {
				sb.Append (nfi.NumberDecimalSeparator);
				ns.AppendDecimalString (precision, sb);
			}

			if (needNegativeSign) {
				switch (nfi.NumberNegativePattern) {
				case 0:
					sb.Append (')');
					break;
				case 3:
					sb.Append (nfi.NegativeSign);
					break;
				case 4:
					sb.Append (' ');
					sb.Append (nfi.NegativeSign);
					break;
				}
			}

			return sb.ToString ();
		}
		internal static string FormatPercent (NumberStore ns, int precision, NumberFormatInfo nfi)
		{
			precision = (precision >= 0 ? precision : nfi.PercentDecimalDigits);
			ns.Multiply10 (2);
			ns.RoundDecimal (precision);
			bool needNegativeSign = (!ns.Positive && !ns.ZeroOnly);

			StringBuilder sb = new StringBuilder(ns.IntegerDigits * 2 + precision + 16);

			if (!needNegativeSign) {
				if (nfi.PercentPositivePattern == 2) {
					sb.Append (nfi.PercentSymbol);
				}
			} else {
				switch (nfi.PercentNegativePattern) {
				case 0:
					sb.Append (nfi.NegativeSign);
					break;
				case 1:
					sb.Append (nfi.NegativeSign);
					break;
				case 2:
					sb.Append (nfi.NegativeSign);
					sb.Append (nfi.PercentSymbol);
					break;
				}
			}

			ns.AppendIntegerStringWithGroupSeparator (sb, nfi.PercentGroupSizes, nfi.PercentGroupSeparator);
			
			if (precision > 0) {
				sb.Append (nfi.PercentDecimalSeparator);
				ns.AppendDecimalString (precision, sb);
			}

			if (!needNegativeSign) {
				switch (nfi.PercentPositivePattern) {
				case 0:
					sb.Append (' ');
					sb.Append (nfi.PercentSymbol);
					break;
				case 1:
					sb.Append (nfi.PercentSymbol);
					break;
				}
			} else {
				switch (nfi.PercentNegativePattern) {
				case 0:
					sb.Append (' ');
					sb.Append (nfi.PercentSymbol);
					break;
				case 1:
					sb.Append (nfi.PercentSymbol);
					break;
				}
			}

			return sb.ToString ();
		}
		internal static string FormatHexadecimal (NumberStore ns, int precision, NumberFormatInfo nfi, bool upper)
		{
			if (ns.IsFloatingSource)
				throw new FormatException ();

			int intSize = ns.DefaultByteSize;
			ulong value = 0;
			for (int i = 0; i < ns.IntegerDigits; i++) {
				value *= 10;
				value += ns.GetDigitByte (i);
			}

			if (!ns.Positive) {
				value = (ulong)(Math.Pow (2, intSize * 8)) - value;
			}

			char[] digits = (upper ? digitUpperTable : digitLowerTable);
			CharBuffer sb = new CharBuffer (16 + precision + 1);

			while (value > 0) {
				sb.InsertToFront (digits [value % 16]);
				value >>= 4;
			}

			if (sb.Length == 0)
				sb.InsertToFront ('0');

			if (sb.Length < precision)
				sb.InsertToFront ('0', precision - sb.Length);

			return sb.ToString ();
		}
		internal static string FormatExponential (NumberStore ns, int precision, NumberFormatInfo nfi, bool upper)
		{
			if (precision < 0)
				precision = 6;

			if (ns.ZeroOnly) {
				StringBuilder sb = new StringBuilder (precision + nfi.PositiveSign.Length + 6);
				sb.Append ('0');
				if (precision > 0) {
					sb.Append ('.');
					sb.Append ('0', precision);
				}

				if (upper)
					sb.Append ('E');
				else
					sb.Append ('e');

				sb.Append (nfi.PositiveSign);
				sb.Append ('0', 3);
				
				return sb.ToString ();
			}

			int exponent = 0;
			while (!(ns.DecimalPointPosition == 1 && ns.GetChar (0) != '0')) {
				if (ns.DecimalPointPosition > 1) {
					ns.Divide10 (1);
					exponent ++;
				} else {
					ns.Multiply10 (1);
					exponent --;
				}
			}

			if (ns.RoundDecimal (precision)) {
				ns.Divide10 (1);
				exponent ++;
			}

			StringBuilder cb = new StringBuilder (ns.DecimalDigits + 1 + 8);

			if (!ns.Positive) {
				cb.Append (nfi.NegativeSign);
			}

			ns.AppendIntegerString (ns.IntegerDigits > 0 ? ns.IntegerDigits : 1, cb);

			if (precision > 0) {
				cb.Append (nfi.NumberDecimalSeparator);
				ns.AppendDecimalString (precision, cb);
			}

			if (upper)
				cb.Append ('E');
			else
				cb.Append ('e');

			if (exponent >= 0)
				cb.Append (nfi.PositiveSign);
			else {
				cb.Append (nfi.NegativeSign);
				exponent = -exponent;
			}

			if (exponent == 0) {
				cb.Append ('0', 3);
			} else if (exponent < 10) {
				cb.Append ('0', 2);
				cb.Append (digitLowerTable [exponent]);
			} else if (exponent < 100) {
				cb.Append ('0', 1);
				cb.Append (digitLowerTable [exponent / 10 % 10]);
				cb.Append (digitLowerTable [exponent % 10]);
			} else if (exponent < 1000) {
				cb.Append (digitLowerTable [exponent / 100 % 10]);
				cb.Append (digitLowerTable [exponent / 10 % 10]);
				cb.Append (digitLowerTable [exponent % 10]);
			/*} else { // exponent range is 0`}324
				int pos = cb.Length;
				int count = 3;
				while (exponent > 0 || --count > 0) {
					cb.Insert (pos, digitLowerTable [exponent % 10]);
					exponent /= 10;
				}*/
			}

			return cb.ToString ();
		}
		#endregion

		#region Custom
		internal static string FormatCustom (string format, NumberStore ns, NumberFormatInfo nfi)
		{
			bool p = ns.Positive;
			int offset = 0;
			int length = 0;
			CustomInfo.GetActiveSection (format,ref p, ns.ZeroOnly, ref offset, ref length);
			if (length == 0) {
				return ns.Positive ? "" : nfi.NegativeSign;
			}
			ns.Positive = p;

			CustomInfo info = CustomInfo.Parse (format, offset, length, nfi);
#if false
			Console.WriteLine("Format : {0}",format);
			Console.WriteLine("DecimalDigits : {0}",info.DecimalDigits);
			Console.WriteLine("DecimalPointPos : {0}",info.DecimalPointPos);
			Console.WriteLine("DecimalTailSharpDigits : {0}",info.DecimalTailSharpDigits);
			Console.WriteLine("IntegerDigits : {0}",info.IntegerDigits);
			Console.WriteLine("IntegerHeadSharpDigits : {0}",info.IntegerHeadSharpDigits);
			Console.WriteLine("IntegerHeadPos : {0}",info.IntegerHeadPos);
			Console.WriteLine("UseExponent : {0}",info.UseExponent);
			Console.WriteLine("ExponentDigits : {0}",info.ExponentDigits);
			Console.WriteLine("ExponentTailSharpDigits : {0}",info.ExponentTailSharpDigits);
			Console.WriteLine("ExponentNegativeSignOnly : {0}",info.ExponentNegativeSignOnly);
			Console.WriteLine("DividePlaces : {0}",info.DividePlaces);
			Console.WriteLine("Percents : {0}",info.Percents);
			Console.WriteLine("Permilles : {0}",info.Permilles);
#endif
			StringBuilder sb_int = new StringBuilder(info.IntegerDigits * 2);
			StringBuilder sb_dec = new StringBuilder(info.DecimalDigits * 2);
			StringBuilder sb_exp = (info.UseExponent ? new StringBuilder(info.ExponentDigits * 2) : null);

			int diff = 0;
			if (info.Percents > 0) {
				ns.Multiply10 (2 * info.Percents);
			}
			if (info.Permilles > 0) {
				ns.Multiply10 (3 * info.Permilles);
			}
			if (info.DividePlaces > 0) {
				ns.Divide10 (info.DividePlaces);
			}

			bool expPositive = true;
			if (info.UseExponent && (info.DecimalDigits > 0 || info.IntegerDigits > 0)) {
				if (!ns.ZeroOnly) {
					while (true) {
						while (ns.IntegerDigits > info.IntegerDigits) {
							ns.Divide10 (1);
							diff--;
							if (ns.IntegerDigits == 1 && ns.GetChar (0) == '0')
								break;
						}
						while (ns.IntegerDigits < info.IntegerDigits || (ns.IntegerDigits == info.IntegerDigits && ns.GetChar (0) == '0')) {
							ns.Multiply10 (1);
							diff++;
						}

						if (!ns.RoundDecimal (info.DecimalDigits))
							break;
					}
				}

				expPositive = diff <= 0;
				NumberStore.AppendIntegerStringFromUInt32 (sb_exp, (uint)(diff >= 0 ? diff : -diff));
			} else {
				ns.RoundDecimal (info.DecimalDigits);
				if (ns.ZeroOnly)
					ns.Positive = true;
			}

			if (info.IntegerDigits != 0 || !ns.CheckZeroOnlyInteger ()) {
				ns.AppendIntegerString (ns.IntegerDigits, sb_int);
			}
			/* if (sb_int.Length > info.IntegerDigits) {
				int len = 0;
				while (sb_int.Length > info.IntegerDigits && len < sb_int.Length) {
					if (sb_int [len] == '0')
						len ++;
					else
						break;
				}
				sb_int.Remove (0, len);
			} */

			ns.AppendDecimalString (ns.DecimalDigits, sb_dec);

			if (info.UseExponent) {
				if (info.DecimalDigits <= 0 && info.IntegerDigits <= 0)
					ns.Positive = true;

				/*if (sb_int.Length < info.IntegerDigits)
					sb_int.Insert (0, "0", info.IntegerDigits - sb_int.Length);*/

				while (sb_exp.Length < info.ExponentDigits - info.ExponentTailSharpDigits)
					sb_exp.Insert (0, '0');

				if (expPositive && !info.ExponentNegativeSignOnly)
					sb_exp.Insert (0, nfi.PositiveSign);
				else if(!expPositive)
					sb_exp.Insert (0, nfi.NegativeSign);
			} else {
				if (sb_int.Length < info.IntegerDigits - info.IntegerHeadSharpDigits)
					sb_int.Insert (0, "0", info.IntegerDigits - info.IntegerHeadSharpDigits - sb_int.Length);
				if (info.IntegerDigits == info.IntegerHeadSharpDigits && NumberStore.IsZeroOnly (sb_int))
					sb_int.Remove (0, sb_int.Length);
			}

			ZeroTrimEnd (sb_dec, true);
			while (sb_dec.Length < info.DecimalDigits - info.DecimalTailSharpDigits)
				sb_dec.Append ('0');
			if (sb_dec.Length > info.DecimalDigits)
				sb_dec.Remove (info.DecimalDigits, sb_dec.Length - info.DecimalDigits);

			return info.Format (format, offset, length, nfi, ns.Positive, sb_int, sb_dec, sb_exp);
		}

		private class CustomInfo
		{
			public bool UseGroup = false;
			public int DecimalDigits = 0;
			public int DecimalPointPos = -1;
			public int DecimalTailSharpDigits = 0;
			public int IntegerDigits = 0;
			public int IntegerHeadSharpDigits = 0;
			public int IntegerHeadPos = 0;
			public bool UseExponent = false;
			public int ExponentDigits = 0;
			public int ExponentTailSharpDigits = 0;
			public bool ExponentNegativeSignOnly = true;
			public int DividePlaces = 0;
			public int Percents = 0;
			public int Permilles = 0;

			public static void GetActiveSection (string format, ref bool positive, bool zero, ref int offset, ref int length)
			{
				int[] lens = new int [3];
				int index = 0;
				int lastPos = 0;
				char literal = '\0';
				for (int i = 0; i < format.Length; i++) {
					char c = format [i];

					if (c == literal || (literal == '\0' && (c == '\"' || c == '\''))) {
						if (literal == '\0')
							literal = c;
						else
							literal = '\0';
						continue;
					}
					
					if (literal == '\0' && format [i] == ';' && (i == 0 || format [i - 1] != '\\')) {
						lens [index ++] = i - lastPos;
						lastPos = i + 1;
						if (index == 3)
							break;
					}
				}

				if (index == 0) {
					offset = 0;
					length = format.Length;
					return;
				}
				if (index == 1) {
					if (positive || zero) {
						offset = 0;
						length = lens [0];
						return;
					}
					if (lens [0] + 1 < format.Length) {
						positive = true;
						offset = lens [0] + 1;
						length = format.Length - offset;
						return;
					} else {
						offset = 0;
						length = lens [0];
						return;
					}
				}
				if (index == 2) {
					if (zero) {
						offset = lens [0] + lens [1] + 2;
						length = format.Length - offset;
						return;
					}
					if (positive) {
						offset = 0;
						length = lens [0];
						return;
					}
					if (lens [1] > 0) {
						positive = true;
						offset = lens [0] + 1;
						length = lens [1];
						return;
					} else {
						offset = 0;
						length = lens [0];
						return;
					}
				}
				if (index == 3) {
					if (zero) {
						offset = lens [0] + lens [1] + 2;
						length = lens [2];
						return;
					}
					if (positive) {
						offset = 0;
						length = lens [0];
						return;
					}
					if (lens [1] > 0) {
						positive = true;
						offset = lens [0] + 1;
						length = lens [1];
						return;
					} else {
						offset = 0;
						length = lens [0];
						return;
					}
				}

				throw new ArgumentException ();
			}

			public static CustomInfo Parse (string format, int offset, int length, NumberFormatInfo nfi)
			{
				char literal = '\0';
				bool integerArea = true;
				bool decimalArea = false;
				bool exponentArea = false;
				bool sharpContinues = true;

				CustomInfo info = new CustomInfo ();
				int groupSeparatorCounter = 0;

				for (int i = offset; i - offset < length; i++) {
					char c = format [i];

					if (c == literal && c != '\0') {
						literal = '\0';
						continue;
					}
					if (literal != '\0')
						continue;

					if (exponentArea && (c != '\0' && c != '0' && c != '#')) {
						exponentArea = false;
						integerArea = (info.DecimalPointPos < 0);
						decimalArea = !integerArea;
						i--;
						continue;
					}

					switch (c) {
					case '\\':
						i ++;
						continue;
					case '\'':
					case '\"':
						if (c == '\"' || c == '\'') {
							literal = c;
						}
						continue;
					case '#':
						if (sharpContinues && integerArea)
							info.IntegerHeadSharpDigits ++;
						else if (decimalArea)
							info.DecimalTailSharpDigits ++;
						else if (exponentArea)
							info.ExponentTailSharpDigits ++;

						goto case '0';
					case '0':
						if (c != '#') {
							sharpContinues = false;
							if (decimalArea)
								info.DecimalTailSharpDigits = 0;
							else if (exponentArea)
								info.ExponentTailSharpDigits = 0;
						}
						if (info.IntegerHeadPos == -1)
							info.IntegerHeadPos = i;

						if (integerArea) {
							info.IntegerDigits ++;
							if (groupSeparatorCounter > 0)
								info.UseGroup = true;
							groupSeparatorCounter = 0;
						} else if (decimalArea) {
							info.DecimalDigits ++;
						} else if (exponentArea) {
							info.ExponentDigits ++;
						}
						break;
					case 'e':
					case 'E':
						if (info.UseExponent)
							break;

						info.UseExponent = true;
						integerArea = false;
						decimalArea = false;
						exponentArea = true;
						if (i + 1 - offset < length) {
							char nc = format [i + 1];
							if (nc == '+')
								info.ExponentNegativeSignOnly = false;
							if (nc == '+' || nc == '-') {
								i ++;
							} else if (nc != '0' && nc != '#') {
								info.UseExponent = false;
								if (info.DecimalPointPos < 0)
									integerArea = true;
							}
							c = '\0';
						}
						
						break;
					case '.':
						integerArea = false;
						decimalArea = true;
						exponentArea = false;
						if (info.DecimalPointPos == -1)
							info.DecimalPointPos = i;
						break;
					case '%':
						info.Percents++;
						break;
					case '\u2030':
						info.Permilles++;
						break;
					case ',':
						if (integerArea && info.IntegerDigits > 0)
							groupSeparatorCounter ++;
						break;
					default:
						break;
					}
				}

				if (info.ExponentDigits == 0)
					info.UseExponent = false;
				else
					info.IntegerHeadSharpDigits = 0;

				if (info.DecimalDigits == 0)
					info.DecimalPointPos = -1;

				info.DividePlaces += groupSeparatorCounter * 3;

				return info;
			}

			public string Format (string format, int offset, int length, NumberFormatInfo nfi, bool positive, StringBuilder sb_int, StringBuilder sb_dec, StringBuilder sb_exp)
			{
				StringBuilder sb = new StringBuilder ();
				char literal = '\0';
				bool integerArea = true;
				bool decimalArea = false;
				int  intSharpCounter = 0;
				int sb_int_index = 0;
				int sb_dec_index = 0;

				int[] groups = nfi.NumberGroupSizes;
				string groupSeparator = nfi.NumberGroupSeparator;
				int intLen = 0, total = 0, groupIndex = 0, counter = 0, groupSize = 0, fraction = 0;
				if (UseGroup && groups.Length > 0) {
					intLen = sb_int.Length;
					for (int i = 0; i < groups.Length; i++) {
						total += groups [i];
						if (total <= intLen)
							groupIndex = i;
					}
					groupSize = groups [groupIndex];
					fraction = intLen > total ? intLen - total : 0;
					if (groupSize == 0) {
						while (groupIndex >= 0 && groups [groupIndex] == 0)
							groupIndex --;
						
						groupSize = fraction > 0 ? fraction : groups [groupIndex];
					}
					if (fraction == 0) {
						counter = groupSize;
					} else {
						groupIndex += fraction / groupSize;
						counter = fraction % groupSize;
						if (counter == 0)
							counter = groupSize;
						else
							groupIndex ++;
					}
				} else {
					UseGroup = false;
				}

				for (int i = offset; i - offset < length; i++) {
					char c = format [i];

					if (c == literal && c != '\0') {
						literal = '\0';
						continue;
					}
					if (literal != '\0') {
						sb.Append (c);
						continue;
					}

					switch (c) {
					case '\\':
						i ++;
						if (i - offset < length)
							sb.Append (format [i]);
						continue;
					case '\'':
					case '\"':
						if (c == '\"' || c == '\'') {
							literal = c;
						}
						continue;
					case '#':
						goto case '0';
					case '0':
						if (integerArea) {
							intSharpCounter++;
							if (IntegerDigits - intSharpCounter < sb_int.Length + sb_int_index || c == '0')
								while (IntegerDigits - intSharpCounter + sb_int_index < sb_int.Length) {
									sb.Append (sb_int[ sb_int_index++]);
									if (UseGroup && --intLen > 0 && --counter == 0) {
										sb.Append (groupSeparator);
										if (--groupIndex < groups.Length && groupIndex >= 0)
											groupSize = groups [groupIndex];
										counter = groupSize;
									}
								}
							break;
						} else if (decimalArea) {
							if (sb_dec_index < sb_dec.Length)
								sb.Append (sb_dec [sb_dec_index++]);
							break;
						}

						sb.Append (c);
						break;
					case 'e':
					case 'E':
						if (sb_exp == null || !UseExponent) {
							sb.Append (c);
							break;
						}

						bool flag1 = true;
						bool flag2 = false;
						
						int q;
						for (q = i + 1; q - offset < length; q++) {
							if (format [q] == '0') {
								flag2 = true;
								continue;
							}
							if (q == i + 1 && (format [q] == '+' || format [q] == '-')) {
								continue;
							}
							if (!flag2)
								flag1 = false;
							break;
						}

						if (flag1) {
							i = q - 1;
							integerArea = (DecimalPointPos < 0);
							decimalArea = !integerArea;

							sb.Append (c);
							sb.Append (sb_exp);
							sb_exp = null;
						} else
							sb.Append (c);

						break;
					case '.':
						if (DecimalPointPos == i) {
							if (DecimalDigits > 0) {
								while (sb_int_index < sb_int.Length)
									sb.Append (sb_int [sb_int_index++]);
							}
							if (sb_dec.Length > 0)
								sb.Append (nfi.NumberDecimalSeparator);
						}
						integerArea = false;
						decimalArea = true;
						break;
					case ',':
						break;
					case '%':
						sb.Append (nfi.PercentSymbol);
						break;
					case '\u2030':
						sb.Append (nfi.PerMilleSymbol);
						break;
					default:
						sb.Append (c);
						break;
					}
				}

				if (!positive)
					sb.Insert (0, nfi.NegativeSign);

				return sb.ToString ();
			}
		}

		#endregion

		#region Internal structures
		internal struct NumberStore
		{
			bool _NaN;
			bool _infinity;
			bool _positive;
			int  _decPointPos;
			int  _defPrecision;
			int  _defMaxPrecision;
			int  _defByteSize;

			byte[] _digits;

			static uint [] IntList = new uint [] {
				1,
				10,
				100,
				1000,
				10000,
				100000,
				1000000,
				10000000,
				100000000,
				1000000000,
			};

			static ulong [] ULongList = new ulong [] {
				1,
				10,
				100,
				1000,
				10000,
				100000,
				1000000,
				10000000,
				100000000,
				1000000000,
				10000000000,
				100000000000,
				1000000000000,
				10000000000000,
				100000000000000,
				1000000000000000,
				10000000000000000,
				100000000000000000,
				1000000000000000000,
				10000000000000000000,
			};

			#region Constructors
			public NumberStore (long value)
			{
				_infinity = _NaN = false;
				_defByteSize = 8;
				_defMaxPrecision = _defPrecision = 19;
				_positive = value >= 0;

				if (value == 0) {
					_digits = new byte []{0};
					_decPointPos = 1;
					return;
				}
				
				ulong v = (ulong)(_positive ? value : -value);

				int i = 18, j = 0;

				if (v < 10)
					i = 0;
				else if (v < 100)
					i = 1;
				else if (v < 1000)
					i = 2;
				else if (v < 10000)
					i = 3;
				else if (v < 100000)
					i = 4;
				else if (v < 1000000)
					i = 5;
				else if (v < 10000000)
					i = 6;
				else if (v < 100000000)
					i = 7;
				else if (v < 1000000000)
					i = 8;
				else if (v < 10000000000)
					i = 9;
				else if (v < 100000000000)
					i = 10;
				else if (v < 1000000000000)
					i = 11;
				else if (v < 10000000000000)
					i = 12;
				else if (v < 100000000000000)
					i = 13;
				else if (v < 1000000000000000)
					i = 14;
				else if (v < 10000000000000000)
					i = 15;
				else if (v < 100000000000000000)
					i = 16;
				else if (v < 1000000000000000000)
					i = 17;
				else
					i = 18;

				_digits = new byte [i + 1];
				do {
					ulong n = v / ULongList [i];
					_digits [j++] = (byte)n;
					v -= ULongList [i--] * n;
				} while (i >= 0);

				_decPointPos = _digits.Length;
			}
			public NumberStore (int value)
			{
				_infinity = _NaN = false;
				_defByteSize = 4;
				_defMaxPrecision = _defPrecision = 10;
				_positive = value >= 0;

				if (value == 0) {
					_digits = new byte []{0};
					_decPointPos = 1;
					return;
				}
				
				uint v = (uint)(_positive ? value : -value);

				int i = 9, j = 0;

				if (v < 10)
					i = 0;
				else if (v < 100)
					i = 1;
				else if (v < 1000)
					i = 2;
				else if (v < 10000)
					i = 3;
				else if (v < 100000)
					i = 4;
				else if (v < 1000000)
					i = 5;
				else if (v < 10000000)
					i = 6;
				else if (v < 100000000)
					i = 7;
				else if (v < 1000000000)
					i = 8;
				else
					i = 9;

				_digits = new byte [i + 1];
				do {
					uint n = v / IntList [i];
					_digits [j++] = (byte)n;
					v -= IntList [i--] * n;
				} while (i >= 0);

				_decPointPos = _digits.Length;
			}
			public NumberStore (short value) : this ((int)value)
			{
				_defByteSize = 2;
				_defMaxPrecision = _defPrecision = 5;
			}
			public NumberStore (sbyte value) : this ((int)value)
			{
				_defByteSize = 1;
				_defMaxPrecision = _defPrecision = 3;
			}

			public NumberStore (ulong value)
			{
				_infinity = _NaN = false;
				_defByteSize = 8;
				_defMaxPrecision = _defPrecision = 20;
				_positive = true;

				if (value == 0) {
					_digits = new byte []{0};
					_decPointPos = 1;
					return;
				}

				int i = 19, j = 0;

				if (value < 10)
					i = 0;
				else if (value < 100)
					i = 1;
				else if (value < 1000)
					i = 2;
				else if (value < 10000)
					i = 3;
				else if (value < 100000)
					i = 4;
				else if (value < 1000000)
					i = 5;
				else if (value < 10000000)
					i = 6;
				else if (value < 100000000)
					i = 7;
				else if (value < 1000000000)
					i = 8;
				else if (value < 10000000000)
					i = 9;
				else if (value < 100000000000)
					i = 10;
				else if (value < 1000000000000)
					i = 11;
				else if (value < 10000000000000)
					i = 12;
				else if (value < 100000000000000)
					i = 13;
				else if (value < 1000000000000000)
					i = 14;
				else if (value < 10000000000000000)
					i = 15;
				else if (value < 100000000000000000)
					i = 16;
				else if (value < 1000000000000000000)
					i = 17;
				else if (value < 10000000000000000000)
					i = 18;
				else
					i = 19;

				_digits = new byte [i + 1];
				do {
					ulong n = value / ULongList [i];
					_digits [j++] = (byte)n;
					value -= ULongList [i--] * n;
				} while (i >= 0);

				_decPointPos = _digits.Length;
			}
			public NumberStore (uint value)
			{
				_infinity = _NaN = false;
				_positive = true;
				_defByteSize = 4;
				_defMaxPrecision = _defPrecision = 10;

				if (value == 0) {
					_digits = new byte []{0};
					_decPointPos = 1;
					return;
				}
				
				int i = 9, j = 0;

				if (value < 10)
					i = 0;
				else if (value < 100)
					i = 1;
				else if (value < 1000)
					i = 2;
				else if (value < 10000)
					i = 3;
				else if (value < 100000)
					i = 4;
				else if (value < 1000000)
					i = 5;
				else if (value < 10000000)
					i = 6;
				else if (value < 100000000)
					i = 7;
				else if (value < 1000000000)
					i = 8;
				else
					i = 9;

				_digits = new byte [i + 1];
				do {
					uint n = value / IntList [i];
					_digits [j++] = (byte)n;
					value -= IntList [i--] * n;
				} while (i >= 0);

				_decPointPos = _digits.Length;
			}
			public NumberStore (ushort value) : this ((uint)value)
			{
				_defByteSize = 2;
				_defMaxPrecision = _defPrecision = 5;
			}
			public NumberStore (byte value) : this ((uint)value)
			{
				_defByteSize = 1;
				_defMaxPrecision = _defPrecision = 3;
			}

			public NumberStore(double value)
			{
				_digits = null;
				_defByteSize = 64;
				_defPrecision = 15;
				_defMaxPrecision = _defPrecision + 2;

				if (double.IsNaN (value) || double.IsInfinity (value)) {
					_NaN = double.IsNaN (value);
					_infinity = double.IsInfinity (value);
					_positive = value > 0;
					_decPointPos = 0;
					return;
				} else {
					_NaN = _infinity = false;
				}

				long bits = BitConverter.DoubleToInt64Bits (value);
				_positive = (bits >= 0);
				int e = (int) ((bits >> 52) & 0x7ffL);
				long m = bits & 0xfffffffffffffL;

				if (e == 0 && m == 0) {
					_decPointPos = 1;
					_digits = new byte []{0};
					_positive = true;
					return;
				}

				if (e == 0) {
					e ++;
				} else if (e != 0) {
					m |= (1L << 52);
				}

				e -= 1075;

				int nsize = 0;
				while ((m & 1) == 0) {
					m >>= 1;
					e ++;
					nsize ++;
				}

				long mt = m;
				int length = 1;
				byte[] temp = new byte [56];
				for (int i = temp.Length - 1; i >= 0; i--, length++) {
					temp [i] = (byte)(mt % 10);
					mt /= 10;
					if (mt == 0)
						break;
				}

				_decPointPos = temp.Length - 1;

				if (e >= 0) {
					for (int i = 0; i < e; i++) {
						if (MultiplyBy (ref temp, ref length, 2)) {
							_decPointPos ++;
						}
					}
				} else {
					for (int i = 0; i < -e; i++) {
						if (MultiplyBy (ref temp, ref length, 5)) {
							_decPointPos ++;
						}
					}
					_decPointPos += e;
				}

				int ulvc = 1;
				ulong ulv = 0;
				for (int i = 0; i < temp.Length; i++)
					if (temp [i] != 0) {
						_decPointPos -= i - 1;
						_digits = new byte [temp.Length - i];
						for (int q = i; q < temp.Length; q++) {
							_digits [q - i] = temp [q];
							if (ulvc < 20) {
								ulv = ulv * 10 + temp [q];
								ulvc ++;
							}
						}
						break;
					}

				RoundEffectiveDigits (17, true, true);
			}
			public NumberStore(float value)
			{
				_digits = null;
				_defByteSize = 32;
				_defPrecision = 7;
				_defMaxPrecision = _defPrecision + 2;

				if (float.IsNaN (value) || float.IsInfinity (value)) {
					_NaN = float.IsNaN (value);
					_infinity = float.IsInfinity (value);
					_positive = value > 0;
					_decPointPos = 0;
					return;
				} else
					_infinity = _NaN = false;

				long bits = BitConverter.DoubleToInt64Bits (value);
				_positive = (bits >= 0);
				int e = (int) ((bits >> 52) & 0x7ffL);
				long m = bits & 0xfffffffffffffL;

				if (e == 0 && m == 0) {
					_decPointPos = 1;
					_digits = new byte []{0};
					_positive = true;
					return;
				}

				if (e == 0) {
					e ++;
				} else if (e != 0) {
					m |= (1L << 52);
				}

				e -= 1075;

				int nsize = 0;
				while ((m & 1) == 0) {
					m >>= 1;
					e ++;
					nsize ++;
				}

				long mt = m;
				int length = 1;
				byte[] temp = new byte [26];
				for (int i = temp.Length - 1; i >= 0; i--, length++) {
					temp [i] = (byte)(mt % 10);
					mt /= 10;
					if (mt == 0)
						break;
				}

				_decPointPos = temp.Length - 1;

				if (e >= 0) {
					for (int i = 0; i < e; i++) {
						if (MultiplyBy (ref temp, ref length, 2)) {
							_decPointPos ++;
						}
					}
				} else {
					for (int i = 0; i < -e; i++) {
						if (MultiplyBy (ref temp, ref length, 5)) {
							_decPointPos ++;
						}
					}
					_decPointPos += e;
				}

				int ulvc = 1;
				ulong ulv = 0;
				for (int i = 0; i < temp.Length; i++)
					if (temp [i] != 0) {
						_decPointPos -= i - 1;
						_digits = new byte [temp.Length - i];
						for (int q = i; q < temp.Length; q++) {
							_digits [q - i] = temp [q];
							if (ulvc < 20) {
								ulv = ulv * 10 + temp [q];
								ulvc ++;
							}
						}
						break;
					}

				RoundEffectiveDigits (9, true, true);
			}

			internal bool MultiplyBy (ref byte[] buffer,ref int length, int amount)
			{
				int mod = 0;
				int ret;
				int start = buffer.Length - length - 1;
				if (start < 0) start = 0;

				for (int i = buffer.Length - 1; i > start; i--) {
					ret = buffer [i] * amount + mod;
					mod = ret / 10;
					buffer [i] = (byte)(ret % 10);
				}

				if (mod != 0) {
					length = buffer.Length - start;

					if (start == 0) {
						buffer [0] = (byte)mod;
						Array.Copy (buffer, 0, buffer, 1, buffer.Length - 1);
						buffer [0] = 0;
						return true;
					}
					else {
						buffer [start] = (byte)mod;
					}
				}

				return false;
			}
			#endregion

			#region Public Property
			public bool IsNaN 
			{
				get { return _NaN; }
			}
			public bool IsInfinity {
				get { return _infinity; }
			}
			public int DecimalPointPosition {
				get { return _decPointPos; }
			}
			public bool Positive {
				get { return _positive; }
				set { _positive = value;}
			}
			public int DefaultPrecision {
				get { return _defPrecision; }
			}
			public int DefaultMaxPrecision {
				get { return _defMaxPrecision; }
			}
			public int DefaultByteSize {
				get { return _defByteSize; }
			}
			public bool HasDecimal {
				get { return _digits.Length > _decPointPos; }
			}
			public int IntegerDigits {
				get { return _decPointPos > 0 ? _decPointPos : 1; }
			}
			public int DecimalDigits {
				get { return HasDecimal ? _digits.Length - _decPointPos : 0; }
			}
			public bool IsFloatingSource {
				get { return _defPrecision == 15 || _defPrecision == 7; }
			}
			public bool ZeroOnly {
				get {
					for (int i = 0; i < _digits.Length; i++)
						if (_digits [i] != 0)
							return false;
					return true;
				}
			}
			#endregion

			#region Public Method

			#region Round
			public bool RoundPos (int pos)
			{
				return RoundPos (pos, true);
			}
			public bool RoundPos (int pos, bool carryFive)
			{
				bool carry = false;

				if (_decPointPos <= 0)
					pos = pos - _decPointPos - 1;

				if (pos >= _digits.Length)
					return false;

				if (pos < 0) {
					_digits = new byte [1];
					_digits [0] = 0;
					_decPointPos = 1;
					_positive = true;
					return false;
				}

				for (int i = pos; i >= 0; i--) {
					RoundHelper (i, carryFive, ref carry);
					if (!carry)
						break;
				}

				if (carry) {
					byte[] temp = new byte [_digits.Length + 1];
					_digits.CopyTo (temp, 1);
					temp [0] = 1;
					_digits = temp;
					_decPointPos ++;
					pos ++;
				}

				for (int i = pos; i < _digits.Length; i++)
					_digits [i] = 0;
				TrimDecimalEndZeros ();

				return carry;
			}
			public bool RoundDecimal (int decimals)
			{
				return RoundDecimal (decimals, true);
			}
			public bool RoundDecimal (int decimals, bool carryFive)
			{
				bool carry = false;

				decimals += _decPointPos;

				if (!HasDecimal || decimals >= _digits.Length)
					return false;

				if (decimals < 0) {
					_digits = new byte [1];
					_digits [0] = 0;
					_decPointPos = 1;
					_positive = true;
					return false;
				}

				for (int i = decimals; i >= 0; i--) {
					RoundHelper (i, carryFive, ref carry);
					if (!carry)
						break;
				}

				if (carry) {
					byte[] temp = new byte [_digits.Length + 1];
					_digits.CopyTo (temp, 1);
					temp [0] = 1;
					_digits = temp;
					_decPointPos ++;
					decimals ++;
				}

				for (int i = decimals; i < _digits.Length; i++)
					_digits [i] = 0;
				TrimDecimalEndZeros ();

				return carry;
			}
			void RoundHelper (int index, bool carryFive, ref bool carry)
			{
				if (carry) {
					if (_digits [index] == 9) {
						carry = true;
						_digits [index] = 0;
					} else {
						carry = false;
						_digits [index] ++;
					}
				} else if (_digits [index] >= (carryFive ? 5 : 6)) {
					carry = true;
				}
			}
			public bool RoundEffectiveDigits (int digits)
			{
				return RoundEffectiveDigits (digits, true, true);
			}
			public bool RoundEffectiveDigits (int digits, bool carryFive, bool carryEven)
			{
				bool carry = false;

				if (digits >= _digits.Length || digits < 0)
					return false;

				if (digits + 1 < _digits.Length && _digits [digits + 1] == 5 && _digits [digits] % 2 == (carryEven ? 0 : 1))
					carryFive = false;

				for (int i = digits; i >= 0; i--) {
					RoundHelper (i, carryFive, ref carry);
					if (!carry)
						break;
				}

				if (carry) {
					byte[] temp = new byte [_digits.Length + 1];
					_digits.CopyTo (temp, 1);
					temp [0] = 1;
					_digits = temp;
					_decPointPos ++;
					digits ++;
				}

				for (int i = digits; i < _digits.Length; i++)
					_digits [i] = 0;
				TrimDecimalEndZeros ();

				return carry;
			}
			#endregion

			#region Trim
			public void TrimDecimalEndZeros ()
			{
				int len = 0;
				for (int i = _digits.Length - 1; i >= 0; i --) {
					if (_digits [i] != 0)
						break;
					len ++;
				}

				if (len > 0) {
					byte[] temp = new byte [_digits.Length - len];
					Array.Copy (_digits, 0, temp, 0, temp.Length);
					_digits = temp;
				}
			}
			public void TrimIntegerStartZeros ()
			{
				if (_decPointPos < 0 && _decPointPos >= _digits.Length)
					return;

				int len = 0;
				for (int i = 0; i < _decPointPos && i < _digits.Length; i++) {
					if (_digits [i] != 0)
						break;
					len ++;
				}

				if (len == _decPointPos)
					len --;

				if (len == _digits.Length) {
					_digits = new byte [1];
					_digits [0] = 0;
					_decPointPos = 1;
					_positive = true;
				} else if (len > 0) {
					byte[] temp = new byte [_digits.Length - len];
					Array.Copy (_digits, len, temp, 0, temp.Length);
					_digits = temp;
					_decPointPos -= len;
				}
			}

			#endregion

			#region Integer
			public void AppendIntegerString (int minLength, StringBuilder cb)
			{
				if (IntegerDigits == 0) {
					cb.Append ('0', minLength);
					return;
				}
				if (_decPointPos <= 0) {
					cb.Append ('0', minLength);
					return;
				}

				if (_decPointPos < minLength)
					cb.Append ('0', minLength - _decPointPos);

				for (int i = 0; i < _decPointPos; i++) {
					if (i < _digits.Length)
						cb.Append ((char)('0' + _digits [i]));
					else
						cb.Append ('0');
				}
			}
			public void AppendIntegerStringWithGroupSeparator (StringBuilder sb, int[] groups, string groupSeparator)
			{
				if (_decPointPos <= 0) {
					sb.Append ('0');
					return;
				}

				int intLen = IntegerDigits;
				int total = 0;
				int groupIndex = 0;
				for (int i = 0; i < groups.Length; i++) {
					total += groups [i];
					if (total <= intLen)
						groupIndex = i;
				}

				if (groups.Length > 0 && total > 0) {
					int counter;
					int groupSize = groups [groupIndex];
					int fraction = intLen > total ? intLen - total : 0;
					if (groupSize == 0) {
						while (groupIndex >= 0 && groups [groupIndex] == 0)
							groupIndex --;
						
						groupSize = fraction > 0 ? fraction : groups [groupIndex];
					}
					if (fraction == 0) {
						counter = groupSize;
					} else {
						groupIndex += fraction / groupSize;
						counter = fraction % groupSize;
						if (counter == 0)
							counter = groupSize;
						else
							groupIndex ++;
					}
					
					for (int i = 0; i < _decPointPos; i++) {
						if (i < _digits.Length) {
							sb.Append ((char)('0' + _digits [i]));
						} else {
							sb.Append ('0');
						}

						if (i < intLen - 1 && --counter == 0) {
							sb.Append (groupSeparator);
							if (--groupIndex < groups.Length && groupIndex >= 0)
								groupSize = groups [groupIndex];
							counter = groupSize;
						}
					}
				} else {
					for (int i = 0; i < _decPointPos; i++) {
						if (i < _digits.Length) {
							sb.Append ((char)('0' + _digits [i]));
						} else {
							sb.Append ('0');
						}
					}
				}
			}
			#endregion

			#region Decimal
			public string GetDecimalString (int precision)
			{
				if (!HasDecimal)
					return new string ('0', precision);

				StringBuilder sb = new StringBuilder (precision);
				for (int i = _decPointPos; i < _digits.Length && i < precision + _decPointPos; i++) {
					if (i >= 0)
						sb.Append ((char)('0' + _digits [i]));
					else
						sb.Append ('0');
				}
				if (sb.Length < precision)
					sb.Append ('0', precision - sb.Length);
				else if (sb.Length > precision)
					sb.Remove (0, precision);
				return sb.ToString ();
			}

			public void AppendDecimalString (int precision, StringBuilder cb)
			{
				if (!HasDecimal) {
					cb.Append ('0', precision);
					return;
				}

				int i = _decPointPos;
				for (; i < _digits.Length && i < precision + _decPointPos; i++) {
					if (i >= 0)
						cb.Append ((char)('0' + _digits [i]));
					else
						cb.Append ('0');
				}

				i -= _decPointPos;
				if (i < precision)
					cb.Append ('0', precision - i);
			}
			#endregion

			#region others
			public bool CheckZeroOnlyInteger ()
			{
				for (int i = 0; i < _decPointPos && i < _digits.Length; i++) {
					if (_digits [i] != 0)
						return false;
				}
				return true;
			}
			public void Multiply10 (int count)
			{
				if (count <= 0)
					return;

				_decPointPos += count;

				TrimIntegerStartZeros ();
			}
			public void Divide10 (int count)
			{
				if (count <= 0)
					return;

				_decPointPos -= count;
			}
			public override string ToString()
			{
				StringBuilder sb = new StringBuilder ();
				AppendIntegerString (IntegerDigits, sb);
				if (HasDecimal) {
					sb.Append ('.');
					AppendDecimalString (DecimalDigits, sb);
				}
				return sb.ToString ();
			}
			public char GetChar (int pos)
			{
				if (_decPointPos <= 0)
					pos += _decPointPos - 1;
				
				if (pos < 0 || pos >= _digits.Length)
					return '0';
				else
					return (char)('0' + _digits [pos]);
			}
			public byte GetDigitByte (int pos)
			{
				if (_decPointPos <= 0)
					pos += _decPointPos - 1;
				
				if (pos < 0 || pos >= _digits.Length)
					return 0;
				else
					return _digits [pos];
			}
			public NumberStore GetClone ()
			{
				NumberStore ns = new NumberStore ();

				ns._decPointPos = this._decPointPos;
				ns._defMaxPrecision = this._defMaxPrecision;
				ns._defPrecision = this._defPrecision;
				ns._digits = (byte[])this._digits.Clone ();
				ns._infinity = this._infinity;
				ns._NaN = this._NaN;
				ns._positive = this._positive;

				return ns;
			}
			public int GetDecimalPointPos ()
			{
				return _decPointPos;
			}
			public void SetDecimalPointPos (int dp)
			{
				_decPointPos = dp;
			}
			#endregion

			#endregion

			#region Public Static Method
			public static bool IsZeroOnly (StringBuilder sb)
			{
				for (int i = 0; i < sb.Length; i++)
					if (char.IsDigit (sb [i]) && sb [i] != '0')
						return false;
				return true;
			}
			public static void AppendIntegerStringFromUInt32 (StringBuilder sb, uint v)
			{
				if (v < 0)
					throw new ArgumentException ();

				int i = 9;

				if (v >= 1000000000)
					i = 9;
				else if (v >= 100000000)
					i = 8;
				else if (v >= 10000000)
					i = 7;
				else if (v >= 1000000)
					i = 6;
				else if (v >= 100000)
					i = 5;
				else if (v >= 10000)
					i = 4;
				else if (v >= 1000)
					i = 3;
				else if (v >= 100)
					i = 2;
				else if (v >= 10)
					i = 1;
				else
					i = 0;
				do {
					uint n = v / IntList [i];
					sb.Append (NumberFormatter.digitLowerTable [n]);
					v -= IntList [i--] * n;
				} while (i >= 0);
			}
			#endregion
		}
		internal struct CharBuffer
		{
			int offset;
			char[] buffer;

			public CharBuffer (int capacity)
			{
				buffer = new char [capacity];
				offset = capacity;
			}

			void AllocateBuffer (int size)
			{
				size = size > buffer.Length * 2 ? size : buffer.Length * 2;
				char[] newBuffer = new char [size];
				offset += size - buffer.Length;
				Array.Copy (buffer, 0, newBuffer, size - buffer.Length, buffer.Length);
				buffer = newBuffer;
			}

			void CheckInsert (int length)
			{
				if (offset - length < 0) {
					AllocateBuffer (buffer.Length + length - offset);
				}
			}

			public void InsertToFront (char c)
			{
				CheckInsert (1);
				buffer [--offset] = c;
			}

			public void InsertToFront (char c, int repeat)
			{
				CheckInsert (repeat);
				while (repeat-- > 0) {
					buffer [--offset] = c;
				}
			}

			public char this [int index] 
			{
				get {
					return buffer [offset + index];
				}
			}

			public override string ToString()
			{
				if (offset == buffer.Length)
					return "";
				
				return new string (buffer, offset, buffer.Length - offset);
			}

			public int Length {
				get { return buffer.Length - offset; }
			}
		}
		#endregion
	}
}