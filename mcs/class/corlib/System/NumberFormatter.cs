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
			return NumberToString (format, NumberStore.CreateInstance (value), nfi);
		}
		public static string NumberToString (string format, byte value, NumberFormatInfo nfi)
		{
			return NumberToString (format, NumberStore.CreateInstance (value), nfi);
		}
		public static string NumberToString (string format, ushort value, NumberFormatInfo nfi)
		{
			return NumberToString (format, NumberStore.CreateInstance (value), nfi);
		}
		public static string NumberToString (string format, short value, NumberFormatInfo nfi)
		{
			return NumberToString (format, NumberStore.CreateInstance (value), nfi);
		}
		public static string NumberToString (string format, uint value, NumberFormatInfo nfi)
		{
			return NumberToString (format, NumberStore.CreateInstance (value), nfi);
		}
		public static string NumberToString (string format, int value, NumberFormatInfo nfi)
		{
			return NumberToString (format, NumberStore.CreateInstance (value), nfi);
		}
		public static string NumberToString (string format, ulong value, NumberFormatInfo nfi)
		{
			return NumberToString (format, NumberStore.CreateInstance (value), nfi);
		}
		public static string NumberToString (string format, long value, NumberFormatInfo nfi)
		{
			return NumberToString (format, NumberStore.CreateInstance (value), nfi);
		}
		public static string NumberToString (string format, float value, NumberFormatInfo nfi)
		{
			return NumberToString (format, NumberStore.CreateInstance (value), nfi);
		}
		public static string NumberToString (string format, double value, NumberFormatInfo nfi)
		{
			return NumberToString (format, NumberStore.CreateInstance (value), nfi);
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

			if (format == null || format == "")
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
					return FormatGeneral (ns, ns.DefaultMaxPrecision, nfi, true);
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
			
			int length = format.Length;
			if (length < 1)
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
			int[] groups = nfi.CurrencyGroupSizes;
			string groupSeparator = nfi.CurrencyGroupSeparator;
			precision = (precision >= 0 ? precision : nfi.CurrencyDecimalDigits);
			StringBuilder sb = new StringBuilder();

			ns.RoundDecimal (precision);
			if (precision > 0) {
				sb.Append (nfi.CurrencyDecimalSeparator);
				sb.Append (ns.GetDecimalString (precision));
			}

			string intPart = ns.GetIntegerString (ns.IntegerDigits > 0 ? ns.IntegerDigits : 1);
			int index = intPart.Length - 1;
			int counter = 0;
			int groupIndex = 0;
			int groupSize = (groups.Length > 0 ? groups [groupIndex++] : 0);
			while (index >= 0) {
				sb.Insert (0, intPart [index --]);
				counter ++;

				if (index >= 0 && groupSize > 0 && counter % groupSize == 0) {
					sb.Insert (0, groupSeparator);
					groupSize = (groupIndex < groups.Length ? groups [groupIndex++] : groupSize);
					counter = 0;
				}
			}

			if (ns.Positive || NumberStore.IsZeroOnly (sb)) {
				switch (nfi.CurrencyPositivePattern) {
				case 0:
					sb.Insert (0, nfi.CurrencySymbol);
					break;
				case 1:
					sb.Append (nfi.CurrencySymbol);
					break;
				case 2:
					sb.Insert (0, " ");
					sb.Insert (0, nfi.CurrencySymbol);
					break;
				case 3:
					sb.Append (" ");
					sb.Append (nfi.CurrencySymbol);
					break;
				}
			} else {
				switch (nfi.CurrencyNegativePattern) {
				case 0:
					sb.Insert (0, nfi.CurrencySymbol);
					sb.Insert (0, '(');
					sb.Append (')');
					break;
				case 1:
					sb.Insert (0, nfi.CurrencySymbol);
					sb.Insert (0, nfi.NegativeSign);
					break;
				case 2:
					sb.Insert (0, nfi.NegativeSign);
					sb.Insert (0, nfi.CurrencySymbol);
					break;
				case 3:
					sb.Insert (0, nfi.CurrencySymbol);
					sb.Append (nfi.NegativeSign);
					break;
				case 4:
					sb.Insert (0, '(');
					sb.Append (nfi.CurrencySymbol);
					sb.Append (')');
					break;
				case 5:
					sb.Insert (0, nfi.NegativeSign);
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
					sb.Insert (0, nfi.NegativeSign);
					sb.Append (' ');
					sb.Append (nfi.CurrencySymbol);
					break;
				case 9:
					sb.Insert (0, ' ');
					sb.Insert (0, nfi.CurrencySymbol);
					sb.Insert (0, nfi.NegativeSign);
					break;
				case 10:
					sb.Append (' ');
					sb.Append (nfi.CurrencySymbol);
					sb.Append (nfi.NegativeSign);
					break;
				case 11:
					sb.Insert (0, ' ');
					sb.Insert (0, nfi.CurrencySymbol);
					sb.Append (nfi.NegativeSign);
					break;
				case 12:
					sb.Insert (0, nfi.NegativeSign);
					sb.Insert (0, ' ');
					sb.Insert (0, nfi.CurrencySymbol);
					break;
				case 13:
					sb.Append (nfi.NegativeSign);
					sb.Append (' ');
					sb.Append (nfi.CurrencySymbol);
					break;
				case 14:
					sb.Insert (0, ' ');
					sb.Insert (0, nfi.CurrencySymbol);
					sb.Insert (0, '(');
					sb.Append (')');
					break;
				case 15:
					sb.Insert (0, '(');
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
			StringBuilder sb = new StringBuilder();
			sb.Append (ns.GetIntegerString (ns.IntegerDigits > precision ? ns.IntegerDigits : precision));

			if (!ns.Positive && !NumberStore.IsZeroOnly (sb))
				sb.Insert (0, nfi.NegativeSign);

			return sb.ToString ();
		}
		internal static string FormatFixedPoint (NumberStore ns, int precision, NumberFormatInfo nfi)
		{
			precision = precision >= 0 ? precision : nfi.NumberDecimalDigits;
			ns.RoundDecimal (precision);

			StringBuilder sb = new StringBuilder();
			sb.Append (ns.GetIntegerString (ns.IntegerDigits > 0 ? ns.IntegerDigits : 1));

			if (precision > 0) {
				sb.Append (nfi.NumberDecimalSeparator);
				sb.Append (ns.GetDecimalString (precision));
			}

			if (!ns.Positive && !NumberStore.IsZeroOnly (sb))
				sb.Insert (0, nfi.NegativeSign);

			return sb.ToString ();
		}

		internal static string FormatGeneral (NumberStore ns, int precision, NumberFormatInfo nfi, bool upper)
		{
			if (ns.ZeroOnly)
				return "0";

			precision = precision > 0 ? precision : ns.DefaultPrecision;
			StringBuilder sb = new StringBuilder();

			int preExponent = 0;
			NumberStore prens = ns.GetClone ();
			while (!(prens.DecimalPointPosition == 1 && prens.GetChar (0) != '0')) {
				if (prens.DecimalPointPosition > 1) {
					prens.Divide10 (1);
					preExponent ++;
				} else {
					prens.Multiply10 (1);
					preExponent --;
				}
			}

			bool fixedPointMode = preExponent > -5 && preExponent < precision;

			precision = precision < ns.DefaultPrecision + 2 ? precision : ns.DefaultPrecision + 2;
			precision = precision < 17 ? precision : 17;
			if (fixedPointMode) {
				ns.RoundDecimal (precision);
			} else {
				ns = prens;
				if (ns.RoundDecimal (precision - 1)) {
					ns.Divide10 (1);
					preExponent ++;
				}
			}

			if (!ns.Positive) {
				sb.Append (nfi.NegativeSign);
			}
			sb.Append (ns.GetIntegerString (ns.IntegerDigits > 0 ? ns.IntegerDigits : 1));
			if (ns.DecimalDigits > 0) {
				sb.Append (nfi.NumberDecimalSeparator);
				sb.Append (ns.GetDecimalString (ns.DecimalDigits));
			}

			if (!fixedPointMode) {
				if (upper)
					sb.Append ('E');
				else
					sb.Append ('e');

				if (preExponent >= 0)
					sb.Append (nfi.PositiveSign);
				else {
					sb.Append (nfi.NegativeSign);
					preExponent = -preExponent;
				}

				if (preExponent < 10)
					sb.Append ('0');

				int pos = sb.Length;
				while (preExponent > 0) {
					sb.Insert (pos, digitLowerTable [preExponent % 10]);
					preExponent /= 10;
				}
			}

			return sb.ToString ();
		}
		internal static string FormatNumber (NumberStore ns, int precision, NumberFormatInfo nfi)
		{
			int[] groups = nfi.NumberGroupSizes;
			string groupSeparator = nfi.NumberGroupSeparator;
			precision = (precision >= 0 ? precision : nfi.NumberDecimalDigits);
			StringBuilder sb = new StringBuilder();

			ns.RoundDecimal (precision);
			if (precision > 0) {
				sb.Append (nfi.NumberDecimalSeparator);
				sb.Append (ns.GetDecimalString (precision));
			}

			string intPart = ns.GetIntegerString (ns.IntegerDigits > 0 ? ns.IntegerDigits : 1);
			int index = intPart.Length - 1;
			int counter = 0;
			int groupIndex = 0;
			int groupSize = (groups.Length > 0 ? groups [groupIndex++] : 0);
			while (index >= 0) {
				sb.Insert (0, intPart [index --]);
				counter ++;

				if (index >= 0 && groupSize > 0 && counter % groupSize == 0) {
					sb.Insert (0, groupSeparator);
					groupSize = (groupIndex < groups.Length ? groups [groupIndex++] : groupSize);
					counter = 0;
				}
			}

			if (!ns.Positive && !NumberStore.IsZeroOnly (sb)) {
				switch (nfi.NumberNegativePattern) {
				case 0:
					sb.Insert (0, '(');
					sb.Append (')');
					break;
				case 1:
					sb.Insert (0, nfi.NegativeSign);
					break;
				case 2:
					sb.Insert (0, ' ');
					sb.Insert (0, nfi.NegativeSign);
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
			int[] groups = nfi.PercentGroupSizes;
			string groupSeparator = nfi.PercentGroupSeparator;
			precision = (precision >= 0 ? precision : nfi.PercentDecimalDigits);
			StringBuilder sb = new StringBuilder();

			ns.Multiply10 (2);

			ns.RoundDecimal (precision);
			if (precision > 0) {
				sb.Append (nfi.PercentDecimalSeparator);
				sb.Append (ns.GetDecimalString (precision));
			}

			string intPart = ns.GetIntegerString (ns.IntegerDigits > 0 ? ns.IntegerDigits : 1);
			int index = intPart.Length - 1;
			int counter = 0;
			int groupIndex = 0;
			int groupSize = (groups.Length > 0 ? groups [groupIndex++] : 0);
			while (index >= 0) {
				sb.Insert (0, intPart [index --]);
				counter ++;

				if (index >= 0 && groupSize > 0 && counter % groupSize == 0) {
					sb.Insert (0, groupSeparator);
					groupSize = (groupIndex < groups.Length ? groups [groupIndex++] : groupSize);
					counter = 0;
				}
			}

			if (ns.Positive || NumberStore.IsZeroOnly (sb)) {
				switch (nfi.PercentPositivePattern) {
				case 0:
					sb.Append (' ');
					sb.Append (nfi.PercentSymbol);
					break;
				case 1:
					sb.Append (nfi.PercentSymbol);
					break;
				case 2:
					sb.Insert (0, nfi.PercentSymbol);
					break;
				}
			} else {
				switch (nfi.PercentNegativePattern) {
				case 0:
					sb.Append (' ');
					sb.Append (nfi.PercentSymbol);
					sb.Insert (0, nfi.NegativeSign);
					break;
				case 1:
					sb.Append (nfi.PercentSymbol);
					sb.Insert (0, nfi.NegativeSign);
					break;
				case 2:
					sb.Insert (0, nfi.PercentSymbol);
					sb.Insert (0, nfi.NegativeSign);
					break;
				}
			}

			return sb.ToString ();
		}
		internal static string FormatHexadecimal (NumberStore ns, int precision, NumberFormatInfo nfi, bool upper)
		{
			if (ns.IsFloatingSource)
				throw new FormatException ();

			StringBuilder sb = new StringBuilder();

			int intSize = ns.DefaultByteSize;
			ulong value = ulong.Parse (ns.GetIntegerString (ns.IntegerDigits > 0 ? ns.IntegerDigits : 1));

			if (!ns.Positive) {
				value = (ulong)(Math.Pow (2, intSize * 8)) - value;
			}

			char[] digits = (upper ? digitUpperTable : digitLowerTable);

			while (value > 0) {
				sb.Insert (0, digits [value % 16]);
				value >>= 4;
			}

			if (sb.Length == 0)
				sb.Append ('0');

			if (sb.Length < precision)
				sb.Insert (0, "0", precision - sb.Length);

			return sb.ToString ();
		}
		internal static string FormatExponential (NumberStore ns, int precision, NumberFormatInfo nfi, bool upper)
		{
			if (precision < 0)
				precision = 6;
			string decimalPart = (precision > 0 ? string.Concat(".", new string ('0', precision)) : "");
			return FormatCustom (string.Concat ("0", decimalPart , (upper ? "E": "e"), "+000"), ns, nfi);
		}
		#endregion

		#region Custom
		internal static string FormatCustom (string format, NumberStore ns, NumberFormatInfo nfi)
		{
			bool p = ns.Positive;
			format = CustomInfo.GetActiveSection (format,ref p, ns.ZeroOnly);
			if (format == "") {
				return ns.Positive ? "" : nfi.NegativeSign;
			}
			ns.Positive = p;

			CustomInfo info = CustomInfo.Parse (format, nfi);
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
			StringBuilder sb_int = new StringBuilder();
			StringBuilder sb_dec = new StringBuilder();
			StringBuilder sb_exp = new StringBuilder();

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

				int exp = diff >= 0 ? diff : -diff;
				expPositive = -diff >= 0;
				while (exp > 0) {
					sb_exp.Insert (0, digitLowerTable [exp % 10]);
					exp /= 10;
				}
			} else {
				ns.RoundDecimal (info.DecimalDigits);
				if (ns.ZeroOnly)
					ns.Positive = true;
			}

			sb_int.Append (ns.GetIntegerString (ns.IntegerDigits));
			if (sb_int.Length > info.IntegerDigits) {
				int len = 0;
				while (sb_int.Length > info.IntegerDigits && len < sb_int.Length) {
					if (sb_int [len] == '0')
						len ++;
					else
						break;
				}
				sb_int.Remove (0, len);
			}
					
			sb_dec.Append (ns.GetDecimalString (ns.DecimalDigits));

			if (info.UseExponent) {
				if (info.DecimalDigits <= 0 && info.IntegerDigits <= 0)
					ns.Positive = true;

				if (sb_int.Length < info.IntegerDigits)
					sb_int.Insert (0, "0", info.IntegerDigits - sb_int.Length);

				while (sb_exp.Length < info.ExponentDigits - info.ExponentTailSharpDigits)
					sb_exp.Insert (0, "0");

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

			return info.Format (format, nfi, ns.Positive, sb_int.ToString (), sb_dec.ToString (), sb_exp.ToString ());
		}

		private class CustomInfo
		{
			public bool UseGroup = false;
			public int DecimalDigits = 0;
			public int DecimalPointPos = -1;
			public int DecimalTailSharpDigits = 0;
			public int IntegerDigits = 0;
			public int IntegerHeadSharpDigits = 0;
			public int IntegerHeadPos = -1;
			public bool UseExponent = false;
			public int ExponentDigits = 0;
			public int ExponentTailSharpDigits = 0;
			public bool ExponentNegativeSignOnly = true;
			public int DividePlaces = 0;
			public int Percents = 0;
			public int Permilles = 0;

			public static string GetActiveSection (string format, ref bool positive, bool zero)
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

				if (index == 0)
					return format;
				if (index == 1) {
					if (positive || zero)
						return format.Substring (0, lens [0]);
					if (lens [0] + 1 < format.Length) {
						positive = true;
						return format.Substring (lens [0] + 1);
					} else
						return format.Substring (0, lens [0]);
				}
				if (index == 2) {
					if (zero)
						return format.Substring (lens [0] + lens [1] + 2);
					if (positive)
						return format.Substring (0, lens [0]);
					if (lens [1] > 0) {
						positive = true;
						return format.Substring (lens [0] + 1, lens [1]);
					} else
						return format.Substring (0, lens [0]);
				}
				if (index == 3) {
					if (zero)
						return format.Substring (lens [0] + lens [1] + 2, lens [2]);
					if (positive)
						return format.Substring (0, lens [0]);
					if (lens [1] > 0) {
						positive = true;
						return format.Substring (lens [0] + 1, lens [1]);
					} else
						return format.Substring (0, lens [0]);
				}

				throw new ArgumentException ();
			}

			public static CustomInfo Parse (string format, NumberFormatInfo nfi)
			{
				char literal = '\0';
				bool integerArea = true;
				bool decimalArea = false;
				bool exponentArea = false;
				bool sharpContinues = true;

				CustomInfo info = new CustomInfo ();
				int groupSeparatorCounter = 0;

				for (int i = 0; i < format.Length; i++) {
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
						if (i + 1 < format.Length) {
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

			public string Format (string format, NumberFormatInfo nfi, bool positive, string sb_int, string sb_dec, string sb_exp)
			{
				StringBuilder sb = new StringBuilder ();
				char literal = '\0';
				bool integerArea = true;
				bool decimalArea = false;
				int  intSharpCounter = 0;
				int sb_int_index = 0;
				int sb_dec_index = 0;

				int[] groups = GetFormattedGroupSizes (sb_int.Length, nfi);
				int groupIndex = 0;
				int groupSize = (groups.Length > 0 ? groups [groupIndex++] : -1);
				int int_counter = 0;

				for (int i = 0; i < format.Length; i++) {
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
						if (i < format.Length)
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
									int_counter ++;
									if (UseGroup && groupSize > 0 && int_counter % groupSize == 0 && sb_int_index < sb_int.Length) {
										sb.Append (nfi.NumberGroupSeparator);
										groupSize = (groupIndex < groups.Length ? groups [groupIndex++] : groupSize);
										int_counter = 0;
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
						for (q = i + 1; q < format.Length; q++) {
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
			private int[] GetFormattedGroupSizes (int intLen, NumberFormatInfo nfi)
			{
				int[] sizes = new int [intLen];

				int index = 0;
				int counter = 0;
				int[] groups = nfi.NumberGroupSizes;
				int groupIndex = 0;
				int groupSize = (groups.Length > 0 ? groups [groupIndex++] : 0);
				for (int i = 0; i < intLen; i++) {
					counter ++;
					if (groupSize > 0 && counter % groupSize == 0) {
						sizes [index++] = groupSize;
						groupSize = (groupIndex < groups.Length ? groups [groupIndex++] : groupSize);
						counter = 0;
					}
				}

				if (counter > 0) {
					sizes [index++] = counter;
				}

				int[] temp = new int[index];
				Array.Copy (sizes, 0, temp, 0, index);
				Array.Reverse (temp);

				return temp;
			}
		}

		#endregion

		#region Internal Class
		internal class NumberStore
		{
			protected bool _NaN;
			protected bool _infinity;
			protected bool _positive;
			protected int  _decPointPos;
			protected int  _defPrecision;
			protected int  _defMaxPrecision;
			protected int  _defByteSize;

			protected byte[] _digits;

			#region Create
			public static DoubleStore CreateInstance (double value)
			{
				return new DoubleStore (value);
			}
			public static SingleStore CreateInstance (float value)
			{
				return new SingleStore (value);
			}
			public static IntegerStore CreateInstance (long value)
			{
				return new IntegerStore (value);
			}
			public static IntegerStore CreateInstance (ulong value)
			{
				return new IntegerStore (value);
			}
			public static IntegerStore CreateInstance (int value)
			{
				return new IntegerStore (value);
			}
			public static IntegerStore CreateInstance (uint value)
			{
				return new IntegerStore (value);
			}
			public static IntegerStore CreateInstance (short value)
			{
				return new IntegerStore (value);
			}
			public static IntegerStore CreateInstance (ushort value)
			{
				return new IntegerStore (value);
			}
			public static IntegerStore CreateInstance (byte value)
			{
				return new IntegerStore (value);
			}
			public static IntegerStore CreateInstance (sbyte value)
			{
				return new IntegerStore (value);
			}
			#endregion

			#region Public Property
			public virtual bool IsNaN 
			{
				get { return _NaN; }
			}
			public virtual bool IsInfinity {
				get { return _infinity; }
			}
			public virtual int DecimalPointPosition {
				get { return _decPointPos; }
			}
			public virtual bool Positive {
				get { return _positive; }
				set { _positive = value;}
			}
			public virtual int DefaultPrecision {
				get { return _defPrecision; }
			}
			public virtual int DefaultMaxPrecision {
				get { return _defMaxPrecision; }
			}
			public virtual int DefaultByteSize {
				get { return _defByteSize; }
			}
			public virtual bool HasDecimal {
				get { return _digits.Length > _decPointPos; }
			}
			public virtual int IntegerDigits {
				get { return _decPointPos > 0 ? _decPointPos : 1; }
			}
			public virtual int DecimalDigits {
				get { return HasDecimal ? _digits.Length - _decPointPos : 0; }
			}
			public virtual bool IsFloatingSource {
				get { return _defPrecision == 15 || _defPrecision == 7; }
			}
			public virtual bool ZeroOnly {
				get {
					for (int i = 0; i < _digits.Length; i++)
						if (_digits [i] != 0)
							return false;
					return true;
				}
			}
			#endregion

			#region Public Method
			public virtual bool RoundPos (int pos)
			{
				return RoundPos (pos, true);
			}
			public virtual bool RoundPos (int pos, bool carryFive)
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
			public virtual bool RoundDecimal (int decimals)
			{
				return RoundDecimal (decimals, true);
			}
			public virtual bool RoundDecimal (int decimals, bool carryFive)
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
			protected virtual void RoundHelper (int index, bool carryFive, ref bool carry)
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
			public virtual bool RoundEffectiveDigits (int digits)
			{
				return RoundEffectiveDigits (digits, true, true);
			}
			public virtual bool RoundEffectiveDigits (int digits, bool carryFive, bool carryEven)
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
			public virtual void TrimDecimalEndZeros ()
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
			public virtual void TrimIntegerStartZeros ()
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

			public virtual string GetIntegerString (int minLength)
			{
				if (IntegerDigits == 0)
					return new string ('0', minLength);

				StringBuilder sb = new StringBuilder (IntegerDigits > minLength ? IntegerDigits : minLength);
				for (int i = 0; i < _decPointPos; i++) {
					if (i < _digits.Length)
						sb.Append ((char)('0' + _digits [i]));
					else
						sb.Append ('0');
				}
				if (sb.Length < minLength)
					sb.Insert (0, "0", minLength - sb.Length);
				return sb.ToString ();
			}
			public virtual string GetDecimalString (int precision)
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
			public virtual void Multiply10 (int count)
			{
				if (count <= 0)
					return;

				_decPointPos += count;

				TrimIntegerStartZeros ();
			}
			public virtual void Divide10 (int count)
			{
				if (count <= 0)
					return;

				_decPointPos -= count;
			}
			public override string ToString()
			{
				StringBuilder sb = new StringBuilder ();
				sb.Append (GetIntegerString (IntegerDigits));
				if (HasDecimal) {
					sb.Append (".");
					sb.Append (GetDecimalString (DecimalDigits));
				}
				return sb.ToString ();
			}
			public virtual char GetChar (int pos)
			{
				if (_decPointPos <= 0)
					pos += _decPointPos - 1;
				
				if (pos < 0 || pos >= _digits.Length)
					return '0';
				else
					return (char)('0' + _digits [pos]);
			}
			public virtual NumberStore GetClone ()
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
			#endregion

			#region Public Static Method
			public static bool IsZeroOnly (StringBuilder sb)
			{
				for (int i = 0; i < sb.Length; i++)
					if (char.IsDigit (sb [i]) && sb [i] != '0')
						return false;
				return true;
			}
			#endregion
		}

		internal class IntegerStore : NumberStore
		{
			public IntegerStore (long value)
			{
				_positive = value >= 0;
				ulong v = (ulong)(_positive ? value : -value);

				byte[] temp = new byte [30];
				int i = temp.Length - 1;

				while (v > 0) {
					temp [i--] = (byte)(v % 10);
					v /= 10;
				}

				if (temp.Length - i - 1 > 0) {
					_digits = new byte [temp.Length - i - 1];
					Array.Copy (temp, i + 1, _digits, 0, _digits.Length);
				} else {
					_digits = new byte [1];
				}

				_defByteSize = 8;
				_defMaxPrecision = _defPrecision = 19;
				_decPointPos = _digits.Length;
			}
			public IntegerStore (int value) : this ((long)value)
			{
				_defByteSize = 4;
				_defMaxPrecision = _defPrecision = 10;
			}
			public IntegerStore (short value) : this ((long)value)
			{
				_defByteSize = 2;
				_defMaxPrecision = _defPrecision = 5;
			}
			public IntegerStore (sbyte value) : this ((long)value)
			{
				_defByteSize = 1;
				_defMaxPrecision = _defPrecision = 3;
			}

			public IntegerStore (ulong value)
			{
				_positive = true;

				byte[] temp = new byte [30];
				int i = temp.Length - 1;

				while (value > 0) {
					temp [i--] = (byte)(value % 10);
					value /= 10;
				}

				if (temp.Length - i - 1 > 0) {
					_digits = new byte [temp.Length - i - 1];
					Array.Copy (temp, i + 1, _digits, 0, _digits.Length);
				} else {
					_digits = new byte [1];
				}

				_defByteSize = 8;
				_defMaxPrecision = _defPrecision = 20;
				_decPointPos = _digits.Length;
			}
			public IntegerStore (uint value) : this ((ulong)value)
			{
				_defByteSize = 4;
				_defMaxPrecision = _defPrecision = 10;
			}
			public IntegerStore (ushort value) : this ((ulong)value)
			{
				_defByteSize = 2;
				_defMaxPrecision = _defPrecision = 5;
			}
			public IntegerStore (byte value) : this ((ulong)value)
			{
				_defByteSize = 1;
				_defMaxPrecision = _defPrecision = 3;
			}
		}
		internal class DoubleStore : NumberStore
		{
			public DoubleStore(double value)
			{
				_defPrecision = 15;
				_defMaxPrecision = _defPrecision + 2;

				if (double.IsNaN (value) || double.IsInfinity (value)) {
					_NaN = double.IsNaN (value);
					_infinity = double.IsInfinity (value);
					_positive = value > 0;
					return;
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

				bool flag = true;
				if (e == 0) {
					flag = false;
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
		}

		internal class SingleStore : NumberStore
		{
			public SingleStore(float value)
			{
				_defPrecision = 7;
				_defMaxPrecision = _defPrecision + 2;

				if (float.IsNaN (value) || float.IsInfinity (value))
					throw new ArgumentException ();

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
		}

		#endregion
	}

	#region Wrapper
	class IntegerFormatter
	{
		#region NumberToString
		public static string NumberToString (string format, NumberFormatInfo nfi, short value)
		{
			return NumberFormatter.NumberToString (format, value, nfi);
		}

		public static string NumberToString (string format, NumberFormatInfo nfi, int value)
		{
			return NumberFormatter.NumberToString (format, value, nfi);
		}

		public static string NumberToString (string format, NumberFormatInfo nfi, long value)
		{
			return NumberFormatter.NumberToString (format, value, nfi);
		}

		public static string NumberToString (string format, NumberFormatInfo nfi, sbyte value)
		{
			return NumberFormatter.NumberToString (format, value, nfi);
		}

		public static string NumberToString (string format, NumberFormatInfo nfi, byte value)
		{
			return NumberFormatter.NumberToString (format, value, nfi);
		}		

		public static string NumberToString (string format, NumberFormatInfo nfi, ushort value)
		{
			return NumberFormatter.NumberToString (format, value, nfi);
		}

		public static string NumberToString (string format, NumberFormatInfo nfi, uint value)
		{
			return NumberFormatter.NumberToString (format, value, nfi);
		}

		public static string NumberToString (string format, NumberFormatInfo nfi, ulong value)
		{
			return NumberFormatter.NumberToString (format, value, nfi);
		}
		#endregion

		#region Wrapper
		internal static string FormatDecimal (long value, int precision, int maxLength)
		{
			return NumberFormatter.FormatDecimal (NumberFormatter.NumberStore.CreateInstance (value), precision, System.Globalization.CultureInfo.CurrentCulture.NumberFormat);
		}
		internal static string FormatGeneral (long value, int precision, NumberFormatInfo nfi, bool upper) 
		{
			return NumberFormatter.FormatGeneral (NumberFormatter.NumberStore.CreateInstance (value), precision, System.Globalization.CultureInfo.CurrentCulture.NumberFormat, upper);
		}
		
		internal static string FormatGeneral (long value, int precision, NumberFormatInfo nfi, bool upper, int maxLength) 
		{
			return NumberFormatter.FormatGeneral (NumberFormatter.NumberStore.CreateInstance (value), precision, System.Globalization.CultureInfo.CurrentCulture.NumberFormat, upper);
		}
		#endregion
	}

	class FloatingPointFormatter
	{
		public FloatingPointFormatter (double p, double p10, int dec_len, int dec_len_min, double p2, double p102, int dec_len2, int dec_len_min2)
		{

		}

		public string GetStringFrom (string format, NumberFormatInfo nfi, float value)
		{
			if (nfi == null) nfi = CultureInfo.CurrentCulture.NumberFormat;
			return NumberFormatter.NumberToString (format, value, nfi);
		}

		public string GetStringFrom (string format, NumberFormatInfo nfi, double value)
		{
			if (nfi == null) nfi = CultureInfo.CurrentCulture.NumberFormat;
			return NumberFormatter.NumberToString (format, value, nfi);
		}
	}
	#endregion
}