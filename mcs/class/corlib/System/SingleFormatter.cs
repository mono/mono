//
// System.SingleFormatter.cs
//
// Author:
//   Pedro Martinez Juliá  <yoros@wanadoo.es>
//
// Copyright (C) 2003 Pedro Martínez Juliá <yoros@wanadoo.es>
//

using System;
using System.Text;
using System.Collections;
using System.Globalization;

using System.Runtime.InteropServices;

namespace System {

	internal sealed class SingleFormatter {

		public static string NumberToString (string format,
				NumberFormatInfo nfi, float value) {
			if (format == null)
				format = "G";
			if (nfi == null)
				nfi = NumberFormatInfo.CurrentInfo;
			char specifier;
			int precision;
			if (!SingleFormatter.ParseFormat(
					format, out specifier, out precision)) {
				throw new FormatException(Locale.GetText(
					"The specified format is invalid"));
			}
			switch (specifier) {
			case 'C':
				return FormatCurrency(nfi, value, precision);
			case 'D':
				throw new FormatException(Locale.GetText(
					"The specified format is invalid"));
			case 'E':
				return FormatExponential(nfi, value, precision);
			case 'F':
				return FormatFixedPoint(nfi, value, precision);
			case 'G':
				return FormatGeneral(nfi, value, precision);
			case 'N':
				return FormatNumber(nfi, value, precision);
			case 'P':
				return FormatPercent(nfi, value, precision);
			case 'R':
				return FormatReversible(nfi, value, precision);
			case 'X':
				throw new FormatException(Locale.GetText(
					"The specified format is invalid"));
			default:
				throw new FormatException(Locale.GetText(
					"The specified format is invalid"));
			}
		}

		private static bool ParseFormat (string format,
				out char specifier, out int precision) {
			specifier = '\0';
			precision = -1;
			switch (format.Length) {
			case 1:
				specifier = Char.ToUpper(format[0]);
				return true;
			case 2:
				if (Char.IsLetter(format[0]) && Char.IsDigit(format[1])) {
					specifier = Char.ToUpper(format[0]);
					precision = Convert.ToInt32(format[1] - '0');
					return true;
				}
				break;
			case 3:
				if (Char.IsLetter(format[0]) && Char.IsDigit(format[1])
						&& Char.IsDigit(format[2])) {
					specifier = Char.ToUpper(format[0]);
					precision = Convert.ToInt32(format.Substring(1, 2));
					return true;
				}
				break;
			}
			return false;
		}

		private static void Normalize (float value, out long mantissa,
				out int exponent) {
			mantissa = 0;
			exponent = 0;
			if (value == 0.0 ||
				Single.IsInfinity(value) ||
				Single.IsNaN(value)) {
				return;
			}
			value = Math.Abs(value);
			float p = 1000000.0f;
			float p10 = 10000000.0f;
			if (value > p10) {
				while (value > p10) {
					value /= 10;
					exponent++;
				}
			}
			else if (value < p) {
				while (value < p) {
					value *= 10;
					exponent--;
				}
			}
			mantissa = (long) Math.Round(value);
		}

		private static char[] Digits =
			{'0', '1', '2', '3', '4', '5', '6', '7', '8', '9'};

		private static string FormatCurrency (NumberFormatInfo nfi,
				float value, int precision) {
			StringBuilder sb = new StringBuilder();
			if (Single.IsNaN(value)) {
				sb.Append(nfi.NaNSymbol);
			}
			else if (Single.IsInfinity(value)) {
				sb.Append(nfi.PositiveInfinitySymbol);
			}
			else {
				int decimals = (precision >= 0) ?
					precision : nfi.CurrencyDecimalDigits;
				long mantissa;
				int exponent;
				Normalize(value, out mantissa, out exponent);
				if (exponent >= 0) {
					while (decimals > 0) {
						sb.Append("0");
						decimals--;
					}
				}
				else {
					while (exponent < 0) {
						if (exponent > -(decimals+1)) {
							sb.Insert(0, Digits[mantissa % 10]);
						}
						mantissa /= 10;
						exponent++;
					}
				}
				sb.Insert(0, nfi.NumberDecimalSeparator);
				if (mantissa == 0) {
					sb.Insert(0, "0");
				}
				else {
					int i = 0;
					while (exponent > 0) {
						int fin = nfi.NumberGroupSizes[i];
						for (int j = 0; (j < fin) && (exponent > 0);
								j++, exponent--) {
							sb.Insert(0, "0");
						}
						sb.Insert(0, nfi.NumberGroupSeparator);
						if (i < nfi.NumberGroupSizes.Length - 1)
							i++;
					}
					while (mantissa != 0) {
						int fin = nfi.NumberGroupSizes[i];
						for (int j = 0; (j < fin) && (mantissa != 0); j++) {
							sb.Insert(0, Digits[mantissa % 10]);
							mantissa /= 10;
						}
						if (mantissa != 0)
							sb.Insert(0, nfi.NumberGroupSeparator);
						if (i < nfi.NumberGroupSizes.Length - 1)
							i++;
					}
				}
			}
			string numb = sb.ToString();
			if (value < 0) {
				switch (nfi.CurrencyNegativePattern) {
				case 0:
					return "(" + nfi.CurrencySymbol + numb + ")";
				case 1:
					return nfi.NegativeSign + nfi.CurrencySymbol + numb;
				case 2:
					return nfi.CurrencySymbol + nfi.NegativeSign + numb;
				case 3:
					return nfi.CurrencySymbol + numb + nfi.NegativeSign;
				case 4:
					return "(" + numb + nfi.CurrencySymbol + ")";
				case 5:
					return nfi.NegativeSign + numb + nfi.CurrencySymbol;
				case 6:
					return numb + nfi.NegativeSign + nfi.CurrencySymbol;
				case 7:
					return numb + nfi.CurrencySymbol + nfi.NegativeSign;
				case 8:
					return nfi.NegativeSign + numb + " " + nfi.CurrencySymbol;
				case 9:
					return nfi.NegativeSign + nfi.CurrencySymbol + " " + numb;
				case 10:
					return numb + " " + nfi.CurrencySymbol + nfi.NegativeSign;
				case 11:
					return nfi.CurrencySymbol + " " + numb + nfi.NegativeSign;
				case 12:
					return nfi.CurrencySymbol + " " + nfi.NegativeSign + numb;
				case 13:
					return numb + nfi.NegativeSign + " " + nfi.CurrencySymbol;
				case 14:
					return "(" + nfi.CurrencySymbol + " " + numb + ")";
				case 15:
					return "(" + numb + " " + nfi.CurrencySymbol + ")";
				default:
					throw new ArgumentException(Locale.GetText(
						"Invalid CurrencyNegativePattern"));
				}
			}
			else {
				switch (nfi.CurrencyPositivePattern) {
				case 0:
					return nfi.CurrencySymbol + numb ;
				case 1:
					return numb + nfi.CurrencySymbol;
				case 2:
					return nfi.CurrencySymbol + " " + numb;
				case 3:
					return numb + " " + nfi.CurrencySymbol;
				default:
					throw new ArgumentException(Locale.GetText(
						"invalid CurrencyPositivePattern"));
				}
			}
		}

		[MonoTODO]
		private static string FormatExponential (NumberFormatInfo nfi,
				float value, int precision) {
			StringBuilder sb = new StringBuilder();
			if (value == 0.0) {
				sb.Append("0");
			}
			else if (Single.IsNaN(value)) {
				sb.Append(nfi.NaNSymbol);
			}
			else if (Single.IsPositiveInfinity(value)) {
				sb.Append(nfi.PositiveInfinitySymbol);
			}
			else if (Single.IsNegativeInfinity(value)) {
				sb.Append(nfi.NegativeInfinitySymbol);
			}
			else {
				int decimals = (precision >= 0) ?
					precision : nfi.NumberDecimalDigits;
				long mantissa;
				int exponent;
				Normalize(value, out mantissa, out exponent);
				bool not_null = false;
				for (int i = 0; i < 6; i++) {
					if ((not_null == false) && ((mantissa % 10) != 0)) {
						not_null = true;
					}
					if (not_null) {
						sb.Insert(0,Digits[mantissa % 10]);
					}
					mantissa /= 10;
					exponent++;
				}
				if (sb.Length == 0) {
					sb.Insert(0, "0");
				}
				sb.Insert(0,
					Digits[mantissa % 10] + nfi.NumberDecimalSeparator);
				if (exponent > 0) {
					sb.Append("E" + nfi.PositiveSign);
				}
				else {
					sb.Append("E" + nfi.NegativeSign);
				}
				sb.Append(Math.Abs(exponent).ToString());
				if (value < 0.0) {
					sb.Insert(0, nfi.NegativeSign);
				}
			}
			return sb.ToString();
		}

		private static string FormatFixedPoint (NumberFormatInfo nfi,
				float value, int precision) {
			StringBuilder sb = new StringBuilder();
			if (Single.IsNaN(value)) {
				sb.Append(nfi.NaNSymbol);
			}
			else if (Single.IsPositiveInfinity(value)) {
				sb.Append(nfi.PositiveInfinitySymbol);
			}
			else if (Single.IsNegativeInfinity(value)) {
				sb.Append(nfi.NegativeInfinitySymbol);
			}
			else {
				int decimals = (precision >= 0) ?
					precision : nfi.NumberDecimalDigits;
				long mantissa;
				int exponent;
				Normalize(value, out mantissa, out exponent);
				if (exponent >= 0) {
					while (decimals > 0) {
						sb.Append("0");
						decimals--;
					}
				}
				else {
					while (exponent < 0) {
						if (exponent > -(decimals+1)) {
							sb.Insert(0, Digits[mantissa % 10]);
						}
						mantissa /= 10;
						exponent++;
					}
				}
				sb.Insert(0, nfi.NumberDecimalSeparator);
				if (mantissa == 0) {
					sb.Insert(0, "0");
				}
				else {
					while (exponent > 0) {
						sb.Insert(0, "0");
						exponent--;
					}
					while (mantissa != 0) {
						sb.Insert(0, Digits[mantissa % 10]);
						mantissa /= 10;
					}
				}
				if (value < 0.0) {
					sb.Insert(0, nfi.NegativeSign);
				}
			}
			return sb.ToString();
		}

		private static string FormatGeneral (NumberFormatInfo nfi,
				float value, int precision) {
			StringBuilder sb = new StringBuilder();
			if (value == 0.0) {
				sb.Append("0");
			}
			else if (Single.IsNaN(value)) {
				sb.Append(nfi.NaNSymbol);
			}
			else if (Single.IsPositiveInfinity(value)) {
				sb.Append(nfi.PositiveInfinitySymbol);
			}
			else if (Single.IsNegativeInfinity(value)) {
				sb.Append(nfi.NegativeInfinitySymbol);
			}
			else {
				int decimals = (precision >= 0) ?
					precision : nfi.NumberDecimalDigits;
				long mantissa;
				int exponent;
				Normalize(value, out mantissa, out exponent);
				if (exponent > -14 && exponent <= 0) {
					bool not_null = false;
					while (exponent < 0) {
						if ((not_null == false) && ((mantissa % 10) != 0)) {
							not_null = true;
						}
						if (not_null) {
							sb.Insert(0,Digits[mantissa % 10]);
						}
						mantissa /= 10;
						exponent++;
					}
					if (sb.Length != 0) {
						sb.Insert(0, nfi.NumberDecimalSeparator);
					}
					if (mantissa == 0) {
						sb.Insert(0, "0");
					}
					else {
						while (mantissa > 0) {
							sb.Insert(0, Digits[mantissa % 10]);
							mantissa /= 10;
						}
					}
				}
				else {
					bool not_null = false;
					for (int i = 0; i < 6; i++) {
						if ((not_null == false) && ((mantissa % 10) != 0)) {
							not_null = true;
						}
						if (not_null) {
							sb.Insert(0,Digits[mantissa % 10]);
						}
						mantissa /= 10;
						exponent++;
					}
					sb.Insert(0,
						Digits[mantissa % 10] + nfi.NumberDecimalSeparator);
					if (exponent > 0) {
						sb.Append("E" + nfi.PositiveSign);
					}
					else {
						sb.Append("E" + nfi.NegativeSign);
					}
					sb.Append(Math.Abs(exponent).ToString());
				}
				if (value < 0.0) {
					sb.Insert(0, nfi.NegativeSign);
				}
			}
			return sb.ToString();
		}

		private static string FormatNumber (NumberFormatInfo nfi,
				float value, int precision) {
			StringBuilder sb = new StringBuilder();
			if (Single.IsNaN(value)) {
				sb.Append(nfi.NaNSymbol);
			}
			else if (Single.IsInfinity(value)) {
				sb.Append(nfi.PositiveInfinitySymbol);
			}
			else {
				int decimals = (precision >= 0) ?
					precision : nfi.NumberDecimalDigits;
				long mantissa;
				int exponent;
				Normalize(value, out mantissa, out exponent);
				if (exponent >= 0) {
					while (decimals > 0) {
						sb.Append("0");
						decimals--;
					}
				}
				else {
					while (exponent < 0) {
						if (exponent > -(decimals+1)) {
							sb.Insert(0, Digits[mantissa % 10]);
						}
						mantissa /= 10;
						exponent++;
					}
				}
				sb.Insert(0, nfi.NumberDecimalSeparator);
				if (mantissa == 0) {
					sb.Insert(0, "0");
				}
				else {
					int i = 0;
					while (exponent > 0) {
						int fin = nfi.NumberGroupSizes[i];
						for (int j = 0; (j < fin) && (exponent > 0);
								j++, exponent--) {
							sb.Insert(0, "0");
						}
						sb.Insert(0, nfi.NumberGroupSeparator);
						if (i < nfi.NumberGroupSizes.Length - 1)
							i++;
					}
					while (mantissa != 0) {
						int fin = nfi.NumberGroupSizes[i];
						for (int j = 0; (j < fin) && (mantissa != 0); j++) {
							sb.Insert(0, Digits[mantissa % 10]);
							mantissa /= 10;
						}
						if (mantissa != 0)
							sb.Insert(0, nfi.NumberGroupSeparator);
						if (i < nfi.NumberGroupSizes.Length - 1)
							i++;
					}
				}
			}
			string numb = sb.ToString();
			if (value < 0) {
				switch (nfi.NumberNegativePattern) {
				case 0:
					return "(" + numb + ")";
				case 1:
					return nfi.NegativeSign + numb;
				case 2:
					return nfi.NegativeSign + " " + numb;
				case 3:
					return numb + nfi.NegativeSign;
				case 4:
					return numb + " " + nfi.NegativeSign;
				default:
					throw new ArgumentException(Locale.GetText(
						"Invalid NumberNegativePattern"));
				}
			}
			return numb;
		}

		[MonoTODO]
		private static string FormatPercent (NumberFormatInfo nfi,
				float value, int precision) {
			StringBuilder sb = new StringBuilder();
			if (Single.IsNaN(value)) {
				sb.Append(nfi.NaNSymbol);
			}
			else if (Single.IsInfinity(value)) {
				sb.Append(nfi.PositiveInfinitySymbol);
			}
			else {
				int decimals = (precision >= 0) ?
					precision : nfi.PercentDecimalDigits;
				long mantissa;
				int exponent;
				Normalize(value, out mantissa, out exponent);
				exponent += 2;
				if (exponent >= 0) {
					while (decimals > 0) {
						sb.Append("0");
						decimals--;
					}
				}
				else {
					while (exponent < 0) {
						if (exponent > -(decimals+1)) {
							sb.Insert(0, Digits[mantissa % 10]);
						}
						mantissa /= 10;
						exponent++;
					}
				}
				sb.Insert(0, nfi.NumberDecimalSeparator);
				if (mantissa == 0) {
					sb.Insert(0, "0");
				}
				else {
					int i = 0;
					while (exponent > 0) {
						int fin = nfi.NumberGroupSizes[i];
						for (int j = 0; (j < fin) && (exponent > 0);
								j++, exponent--) {
							sb.Insert(0, "0");
						}
						sb.Insert(0, nfi.NumberGroupSeparator);
						if (i < nfi.NumberGroupSizes.Length - 1)
							i++;
					}
					while (mantissa != 0) {
						int fin = nfi.NumberGroupSizes[i];
						for (int j = 0; (j < fin) && (mantissa != 0); j++) {
							sb.Insert(0, Digits[mantissa % 10]);
							mantissa /= 10;
						}
						if (mantissa != 0)
							sb.Insert(0, nfi.NumberGroupSeparator);
						if (i < nfi.NumberGroupSizes.Length - 1)
							i++;
					}
				}
			}
			string numb = sb.ToString();
			if (value < 0) {
				switch (nfi.PercentNegativePattern) {
				case 0:
					return nfi.NegativeSign + numb + " " + nfi.PercentSymbol;
				case 1:
					return nfi.NegativeSign + numb + nfi.PercentSymbol;
				case 2:
					return nfi.NegativeSign + nfi.PercentSymbol + numb;
				default:
					throw new ArgumentException(Locale.GetText(
						"Invalid PercentNegativePattern"));
				}
			}
			else {
				switch (nfi.PercentPositivePattern) {
				case 0:
					return numb + " " + nfi.PercentSymbol;
				case 1:
					return numb + nfi.PercentSymbol;
				case 2:
					return nfi.PercentSymbol + numb;
				default:
					throw new ArgumentException(Locale.GetText(
						"invalid PercehtPositivePattern"));
				}
			}
		}

		[MonoTODO]
		private static string FormatReversible (NumberFormatInfo nfi,
				float value, int precision) {
			return FormatGeneral(nfi, value, precision);
		}

	}

}
