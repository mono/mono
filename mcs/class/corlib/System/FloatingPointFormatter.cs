//
// System.FloatingPointFormatter.cs
//
// Authors:
//	Pedro Martinez Julia <yoros@wanadoo.es>
//	Jon Skeet <skeet@pobox.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2003 Pedro Martíez Juliá <yoros@wanadoo.es>
// Copyright (C) 2004 Jon Skeet
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

using System;
using System.Text;
using System.Collections;
using System.Globalization;


namespace System {

	internal class FloatingPointFormatter {

		struct Format {
			public double p;
			public double p10;
			public int dec_len;
			public int dec_len_min;
		}
		
		Format format1;
		Format format2;

		public FloatingPointFormatter
			(double p, double p10, int dec_len, int dec_len_min,
			 double p2, double p102, int dec_len2, int dec_len_min2) {
			 
			format1.p = p;
			format1.p10 = p10;
			format1.dec_len = dec_len;
			format1.dec_len_min = dec_len_min;
			
			format2.p = p2;
			format2.p10 = p102;
			format2.dec_len = dec_len2;
			format2.dec_len_min = dec_len_min2;
		}

		public string GetStringFrom
				(string format, NumberFormatInfo nfi, double value) {
				
			if (format == null || format == "") {
				format = "G";
			}
			if (nfi == null) {
				nfi = NumberFormatInfo.CurrentInfo;
			}
			if (Double.IsNaN(value)) {
				return nfi.NaNSymbol;
			}
			if (Double.IsNegativeInfinity(value)) {
				return nfi.NegativeInfinitySymbol;
			}
			if (Double.IsPositiveInfinity(value)) {
				return nfi.PositiveInfinitySymbol;
			}
			
			char specifier;
			int precision;
			if (!ParseFormat(format, out specifier, out precision)) {
				return FormatCustom (format1, value, nfi, format);
			}
			
			Format formatData = format1;//(precision > format1.dec_len+1) ? format2 : format1;
			
			switch (specifier) {
			case 'C':
				return FormatCurrency (formatData, value, nfi, precision);
			case 'D':
				throw new FormatException(Locale.GetText(
					"The specified format is invalid") + ": " + format);
			case 'E':
				formatData = (precision > format1.dec_len) ? format2 : format1;
				return FormatExponential (formatData, value, nfi, precision, format[0]);
			case 'F':
				return FormatFixedPoint (formatData, value, nfi, precision);
			case 'G':
				return FormatGeneral (formatData, value, nfi, precision);
			case 'N':
				return FormatNumber (formatData, value, nfi, precision);
			case 'P':
				return FormatPercent (formatData, value, nfi, precision);
			case 'R':
				return FormatReversible (value, nfi, precision);
			case 'X':
				throw new FormatException(Locale.GetText(
					"The specified format is invalid") + ": " + format);
			default:
				throw new FormatException(Locale.GetText(
					"The specified format is invalid") + ": " + format);
			}
		}

		private bool ParseFormat (string format,
				out char specifier, out int precision) {
			specifier = '\0';
			precision = format2.dec_len;
			
			// FIXME: Math.Round is used and the max is 15.
			
			if (precision > 15)
				precision = 15;
				
			switch (format.Length) {
			case 1:
				specifier = Char.ToUpperInvariant(format[0]);
				precision = -1;
				return true;
			case 2:
				if (Char.IsLetter(format[0]) && Char.IsDigit(format[1])) {
					specifier = Char.ToUpperInvariant(format[0]);
					precision = Convert.ToInt32(format[1] - '0');
					return true;
				}
				break;
			case 3:
				if (Char.IsLetter(format[0]) && Char.IsDigit(format[1])
						&& Char.IsDigit(format[2])) {
					specifier = Char.ToUpperInvariant(format[0]);
					precision = Convert.ToInt32(format.Substring(1, 2));
					return true;
				}
				break;
			}
			return false;
		}

		// Math.Round use banker's rounding while this is not what must
		// be used for string formatting (see bug #60111)
		// http://bugzilla.ximian.com/show_bug.cgi?id=60111

		// FIXME: should be moved out of here post Mono 1.0
		private double Round (double value) 
		{
			double int_part = Math.Floor (value);
			double dec_part = value - int_part;
			if (dec_part >= 0.5) {
				int_part++;
			}
			return int_part;
		}
		
		// FIXME: should be moved out of here post Mono 1.0
		private double Round (double value, int digits) 
		{
			if (digits == 0)
				return Round (value);
			double p = Math.Pow (10, digits);
			double int_part = Math.Floor (value);
			double dec_part = value - int_part;
			dec_part *= 1000000000000000L;
			dec_part = Math.Floor (dec_part);
			dec_part /= (1000000000000000L / p);
			dec_part = Round (dec_part);
			dec_part /= p;
			return int_part + dec_part;
		}

		private void Normalize (Format formatData, double value, int precision,
				out long mantissa, out int exponent) {
			mantissa = 0;
			exponent = 0;
			if (value == 0.0 ||
				Double.IsInfinity(value) ||
				Double.IsNaN(value)) {
				return;
			}
			value = Math.Abs(value);
			if (precision <= (formatData.dec_len) && precision >= 0) {
				value = Round (value, precision);
			}
			
			if (value == 0.0 ||
				Double.IsInfinity(value) ||
				Double.IsNaN(value)) {
				return;
			}
			
			if (value >= formatData.p10) {
				while (value >= formatData.p10) {
					value /= 10;
					exponent++;
				}
			}
			else if (value < formatData.p) {
				while (value < formatData.p) {
					value *= 10;
					exponent--;
				}
			}
			mantissa = (long) Round(value);
		}

		private string FormatCurrency (Format formatData, double value,
				NumberFormatInfo nfi, int precision) {
				
			precision = (precision >= 0) ? precision : nfi.CurrencyDecimalDigits;
			string numb = FormatNumberInternal (formatData, value, nfi, precision);
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

		private string FormatExponential (Format formatData, double value, NumberFormatInfo nfi,
				int precision, char exp_char) {
			StringBuilder sb = new StringBuilder();
			precision = (precision >= 0) ? precision : 6;
			int decimals = precision;
			long mantissa;
			int exponent;
			Normalize (formatData, value, precision, out mantissa, out exponent);
			if (formatData.dec_len > precision) {
				double aux = mantissa;
				for (int i = 0; i < formatData.dec_len - precision; i++) {
					aux /= 10;
				}
				mantissa = (long) Round(aux);
				for (int i = 0; i < formatData.dec_len - precision; i++) {
					mantissa *= 10;
				}
			}
			bool not_null = false;
			if (mantissa != 0.0) {
				for (int i = 0; i < formatData.dec_len || mantissa >= 10; i++) {
					if ((not_null == false) && ((mantissa % 10) != 0)) {
						not_null = true;
					}
					if (not_null) {
						sb.Insert(0, (char)('0' + (mantissa % 10)));
						precision--;
					}
					mantissa /= 10;
					exponent++;
				}
			}
			if (decimals == 0) {
				sb = new StringBuilder();
				sb.Append((char)('0' + (mantissa % 10)));
			}
			else {
				while (precision > 0) {
					sb.Append('0');
					precision--;
				}
				if (sb.Length == 0) {
					sb.Insert(0, "0");
				}
				sb.Insert (0, (char)('0' + (mantissa % 10)) +
						nfi.NumberDecimalSeparator);
			}
			if (exponent >= 0) {
				sb.Append(exp_char + nfi.PositiveSign);
			}
			else {
				sb.Append(exp_char + nfi.NegativeSign);
			}
			sb.Append(Math.Abs(exponent).ToString("000"));
			if (value < 0.0) {
				sb.Insert(0, nfi.NegativeSign);
			}
			return sb.ToString();
		}

		private string FormatFixedPoint (Format formatData, double value,
				NumberFormatInfo nfi, int precision) {
			StringBuilder sb = new StringBuilder();
			precision = (precision >= 0) ? precision : nfi.NumberDecimalDigits;
			int decimals = precision;
			long mantissa;
			int exponent;
			Normalize (formatData, value, precision, out mantissa, out exponent);
			if (exponent >= 0) {
				while (decimals > 0) {
					sb.Append("0");
					decimals--;
				}
			}
			else {
				int decimal_limit = -(decimals + 1);
				while (exponent < 0) {
					if (exponent > decimal_limit) {
						sb.Insert(0, (char)('0' + (mantissa % 10)));
					}
					mantissa /= 10;
					exponent++;
					decimals--;
				}
				if (decimals > 0) {
					sb.Append ('0', decimals);
					decimals = 0;
				}
			}
			if (precision != 0) {
				sb.Insert(0, nfi.NumberDecimalSeparator);
			}
			if (mantissa == 0) {
				sb.Insert(0, "0");
			}
			else {
				while (exponent > 0) {
					sb.Insert(0, "0");
					exponent--;
				}
				while (mantissa != 0) {
					sb.Insert(0, (char)('0' + (mantissa % 10)));
					mantissa /= 10;
				}
			}
			if (value < 0.0) {
				sb.Insert(0, nfi.NegativeSign);
			}
			return sb.ToString();
		}

		private string FormatGeneral (Format formatData, double value,
				NumberFormatInfo nfi, int precision) {
			StringBuilder sb = new StringBuilder();
			if (value == 0.0) {
				sb.Append("0");
			}
			else {
				precision = (precision > 0) ?
					precision : formatData.dec_len+1;
					
				long mantissa;
				int exponent;
				Normalize (formatData, value, precision, out mantissa, out exponent);
				if (precision > 0) {
					double dmant = mantissa;
					for (int i = 0; i < formatData.dec_len - precision + 1; i++) {
						dmant /= 10;
					}
					mantissa = (long) Round (dmant);
					for (int i = 0; i < formatData.dec_len - precision + 1; i++) {
						mantissa *= 10;
					}
				}
				
				/* Calculate the exponent we would get using the scientific notation */
				int snExponent = exponent;
				long snMantissa = mantissa;
			
				while (snMantissa >= 10) {
					snMantissa /= 10;
					snExponent++;
				}
				
				if (snExponent > -5 && snExponent < precision) {
					bool not_null = false;
					while (exponent < 0) {
						if ((not_null == false) && ((mantissa % 10) != 0)) {
							not_null = true;
						}
						if (not_null) {
							sb.Insert(0, (char)('0' + (mantissa % 10)));
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
							sb.Insert(0, (char)('0' + (mantissa % 10)));
							mantissa /= 10;
						}
					}
				}
				else {
					bool not_null = false;
					while (mantissa >= 10) {
						if ((not_null == false) && ((mantissa % 10) != 0)) {
							not_null = true;
						}
						if (not_null) {
							sb.Insert (0, (char)('0' + (mantissa % 10)));
						}
						mantissa /= 10;
						exponent++;
					}
					if (sb.Length != 0)
					  sb.Insert(0, nfi.NumberDecimalSeparator);
					
					sb.Insert(0, (char)('0' + (mantissa % 10)) );

					if (exponent > 0) {
						sb.Append("E" + nfi.PositiveSign);
					}
					else {
						sb.Append("E" + nfi.NegativeSign);
					}
					sb.Append(Math.Abs(exponent).ToString("00"));
				}
				if (value < 0.0) {
					sb.Insert(0, nfi.NegativeSign);
				}
			}
			return sb.ToString();
		}

		private string FormatNumber (Format formatData, double value, NumberFormatInfo nfi, int precision) {
		
			precision = (precision >= 0) ? precision : nfi.NumberDecimalDigits;
			string numb = FormatNumberInternal (formatData, value, nfi, precision);
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

		private string FormatPercent (Format formatData, double value, NumberFormatInfo nfi,
				int precision) {

			precision = (precision >= 0) ? precision : nfi.PercentDecimalDigits;
			string numb = FormatNumberInternal (formatData, value*100, nfi, precision);
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

		private string FormatNumberInternal (Format formatData, double value, NumberFormatInfo nfi, int precision) 
		{
			StringBuilder sb = new StringBuilder();
			int decimals = precision;
			long mantissa;
			int exponent;
			Normalize (formatData, value, precision, out mantissa, out exponent);
			if (exponent >= 0) {
				while (decimals > 0) {
					sb.Append("0");
					decimals--;
				}
			}
			else {
				int decimal_limit = -(decimals + 1);
				while (exponent < 0) {
					if (exponent > decimal_limit) {
						sb.Insert(0, (char)('0' + (mantissa % 10)));
					}
					mantissa /= 10;
					exponent++;
					decimals--;
				}
				if (decimals > 0) {
					sb.Append ('0', decimals);
					decimals = 0;
				}
			}
			if (precision != 0) {
				sb.Insert(0, nfi.NumberDecimalSeparator);
			}
			if (mantissa == 0) {
				sb.Insert(0, "0");
			}
			else {
				int groupIndex = 0;
				int groupPos = 0;
				int groupSize = nfi.NumberGroupSizes[0];
				if (groupSize == 0) groupSize = int.MaxValue;
				
				while (exponent > 0 || mantissa != 0) {
					
					if (groupPos == groupSize) {
						sb.Insert (0, nfi.NumberGroupSeparator);
						groupPos = 0;
						if (groupIndex < nfi.NumberGroupSizes.Length - 1) {
							groupIndex++;
							groupSize = nfi.NumberGroupSizes[groupIndex];
							if (groupSize == 0) groupSize = int.MaxValue;
						}
					}
					
					if (exponent > 0) {
						sb.Insert (0, "0");
						exponent--;
					}
					else {
						sb.Insert(0, (char)('0' + (mantissa % 10)));
						mantissa /= 10;
					}
					
					groupPos++;
				}
			}
			return sb.ToString();
		}

		// from http://www.yoda.arachsys.com/csharp/floatingpoint.html
		// used with permission from original author
		private string FormatReversible (double value, NumberFormatInfo nfi, int precision)
		{
			// Translate the double into sign, exponent and mantissa.
			long bits = BitConverter.DoubleToInt64Bits (value);
			bool negative = ((bits >> 63) == -1);
			int exponent = (int) ((bits >> 52) & 0x7ffL);
			long mantissa = bits & 0xfffffffffffffL;

			// Subnormal numbers; exponent is effectively one higher,
			// but there's no extra normalisation bit in the mantissa
			if (exponent == 0) {
				exponent++;
			}
			// Normal numbers; leave exponent as it is but add extra
			// bit to the front of the mantissa
			else {
				mantissa = mantissa | (1L<<52);
			}
        
			// Bias the exponent. It's actually biased by 1023, but we're
			// treating the mantissa as m.0 rather than 0.m, so we need
			// to subtract another 52 from it.
			exponent -= 1075;
        
			if (mantissa == 0) {
				return "0";
			}
        
			// Normalize
			while((mantissa & 1) == 0) {
				//  i.e., Mantissa is even
				mantissa >>= 1;
				exponent++;
			}
        
			// Construct a new decimal expansion with the mantissa
        		ArbitraryDecimal ad = new ArbitraryDecimal (mantissa);
        
			// If the exponent is less than 0, we need to repeatedly
			// divide by 2 - which is the equivalent of multiplying
			// by 5 and dividing by 10.
			if (exponent < 0) {
				for (int i=0; i < -exponent; i++)
					ad.MultiplyBy (5);
				ad.Shift (-exponent);
			}
			// Otherwise, we need to repeatedly multiply by 2
			else {
				for (int i=0; i < exponent; i++)
					ad.MultiplyBy(2);
			}
        
			// Finally, return the string with an appropriate sign
			if (negative)
				return nfi.NegativeSign + ad.ToString (nfi);
			else
				return ad.ToString (nfi);
		}

		private string FormatCustom (Format formatData, double value,
				NumberFormatInfo nfi, string format) {
			int first_semicolon, second_semicolon, third_semicolon;
			first_semicolon = format.IndexOf(';');
			second_semicolon = format.IndexOf(';', first_semicolon + 1);
			if (second_semicolon < 0) {
				if (first_semicolon == -1) {
					if (value < 0.0) {
						string result = FormatCustomParser (formatData, value, nfi, format);
						if (result == "0") {
							return "0";
						}
						if (result.Length > 0) {
							result = nfi.NegativeSign + result;
						}
						return result;
					}
					return FormatCustomParser (formatData, value, nfi, format);
					
				}
				if (value < 0.0) {
					return FormatCustomParser
						(formatData, value, nfi, format.Substring(first_semicolon + 1));
				}
				return FormatCustomParser (formatData, value, nfi,
						format.Substring(0, first_semicolon - 1));
			}
			if (value > 0.0) {
				return FormatCustomParser (formatData, value, nfi,
						format.Substring(0, first_semicolon - 1));
			}
			else if (value < 0.0) {
				return FormatCustomParser (formatData, value, nfi,
						format.Substring (first_semicolon + 1,
							second_semicolon - first_semicolon - 1));
			}
			third_semicolon = second_semicolon < 0 ?  - 1 : format.IndexOf (';', second_semicolon + 1);
			if (third_semicolon < 0)
				return FormatCustomParser (formatData, value,
					nfi, format.Substring(second_semicolon + 1));
			else
				return FormatCustomParser (formatData, value,
					nfi, format.Substring(second_semicolon + 1, third_semicolon - second_semicolon - 1));
		}

		private struct Flags {
			public int NumberOfColons;
			public bool Groupping;
			public bool Percent;
			public bool Permille;
			public int DotPos;
			public int ExpPos;
			public int FirstFormatPos;
			public int IntegralLength;
			public int DecimalLength;
			public int ExponentialLength;
		}

		private Flags AnalizeFormat (string format) {
			Flags f = new Flags();
			f.NumberOfColons = 0;
			f.DotPos = -1;
			f.ExpPos = -1;
			f.Groupping = false;
			f.Percent = false;
			f.FirstFormatPos = -1;
			int aux = 0, i = 0, count = 0;
			bool inQuote = false;
			foreach (char c in format) {
				if (c == '\'') {
					if (inQuote)
						inQuote = false;
					else
						inQuote = true;
					i++;
					continue;
				} else if (inQuote) {
					i++;
					continue;
				}

				switch (c) {
				case ',':
					aux++;
					break;
				case '0':
				case '#':
					if (f.FirstFormatPos < 0) {
						f.FirstFormatPos = i;
					}
					if (aux > 0) {
						f.Groupping = true;
						aux = 0;
					}
					if (count < 15)
						count++;
					break;
				case '.':
					if (f.DotPos >= 0)
						break; // ignore
					f.DotPos = i;
					f.IntegralLength = count;
					count = 0;
					if (aux > 0) {
						f.NumberOfColons = aux;
						aux = 0;
					}
					break;
				case '%':
					f.Percent = true;
					break;
				case '\u2030':
					f.Permille = true;
					break;
				case 'e':
				case 'E':
					f.DecimalLength = count;
					count = 0;
					f.ExpPos = i;
					break;
				}
				i++;
			}
			if (inQuote)
				throw new FormatException ("Literal in format string is not correctly terminated.");
			if (aux > 0) {
				f.NumberOfColons = aux;
			}
			if (f.DecimalLength > 0) {
				f.ExponentialLength = count;
			}
			else {
				f.DecimalLength = count;
			}
			return f;
		}

		private string FormatCustomParser (Format formatData, double value,
				NumberFormatInfo nfi, string format) {
			long mantissa;
			int exponent;
			Flags f = AnalizeFormat(format);
			if (f.FirstFormatPos < 0) {
				return format;
			}
			if (((f.Percent) || (f.Permille) || (f.NumberOfColons > 0)) && (f.ExpPos < 0)) {
				int len = f.DecimalLength;
				int exp = 0;
				if (f.Percent) {
					len += 2;
					exp += 2;
				}
				else if (f.Permille) {
					len += 3;
					exp += 3;
				}
				if (f.NumberOfColons > 0) {
					len -= (3 * f.NumberOfColons);
					exp -= 3 * f.NumberOfColons;
				}
				if (len < 0) {
					len = 0;
				}
				value = Round(value, len);
				Normalize (formatData, value, 15, out mantissa, out exponent);
				exponent += exp;
			}
			else {
				value = Round(value, f.DecimalLength);
				Normalize (formatData, value, 15, out mantissa, out exponent);
			}
			StringBuilder sb = new StringBuilder();
			if (f.ExpPos > 0) {
				StringBuilder sb_decimal = new StringBuilder();
				while (mantissa > 0) {
					sb_decimal.Insert(0, (char)('0' + (mantissa % 10)));
					mantissa /= 10;
					exponent++;
				}
				exponent--;
				int k;
				for (k = sb_decimal.Length - 1;
					k >= 0 && sb_decimal[k] == '0'; k--);
				sb_decimal.Remove(k + 1, sb_decimal.Length - k - 1);
				for (int i = f.DotPos - 2; i >= 0; i--) {
					char c = format[i];
					if (i > 0 && format[i-1] == '\\') {
						sb.Insert(0, c);
						i -= 2;
						continue;
					}
					switch (c) {
					case ',':
					case '#':
						break;
					case '0':
						sb.Insert(0, '0');
						break;
					default:
						sb.Insert(0, c);
						break;
					}
				}
				sb.Append(sb_decimal[0]);
				sb.Append(nfi.NumberDecimalSeparator);
				for (int j = 1, i = f.DotPos + 1; i < f.ExpPos; i++) {
					char c = format[i];
					switch (c) {
					case '\\':
						sb.Append(format[++i]);
						break;
					case '0':
						if (j >= sb_decimal.Length) {
							sb.Append('0');
							break;
						}
						goto case '#';
					case '#':
						if (j < sb_decimal.Length) {
							if ((i == f.ExpPos - 1) &&
									(j < sb_decimal.Length - 1)) {
								int a = sb_decimal[j] - '0';
								int b = sb_decimal[j+1] - '0';
								if (((b == 5) && ((a % 2) == 0)) || (b > 5)) {
									a++;
								}
								sb.Append((char)('0' + (a % 10)));
							}
							else {
								sb.Append(sb_decimal[j++]);
							}
						}
						break;
					default:
						sb.Append(c);
						break;
					}
				}
				sb.Append(format[f.ExpPos]);
				if (exponent < 0) {
					sb.Append('-');
				}
				int fin, inicio;
				inicio = f.ExpPos + 1;
				if (format[inicio] == '-') {
					inicio++;
				}
				else if (format[inicio] == '+') {
					if (exponent >= 0) {
						sb.Append('+');
					}
					inicio++;
				}
				fin = inicio;
				while (fin < format.Length && format[fin++] == '0');
				StringBuilder sb_exponent = new StringBuilder();
				exponent = Math.Abs(exponent);
				while (exponent > 0) {
					sb_exponent.Insert(0, (char)('0' + (exponent % 10)));
					exponent /= 10;
				}
				while (sb_exponent.Length < (fin - inicio)) {
					sb_exponent.Insert(0, '0');
				}
				sb.Append(sb_exponent.ToString());
				for (int i = fin; i < format.Length; i++) {
					sb.Append(format[i]);
				}
				return sb.ToString();
			}
			else {
				f.ExpPos = format.Length;
			}
			if (f.DotPos < 0) {
				while (exponent < 0) {
					mantissa = (long) Round((double)mantissa / 10);
					exponent++;
				}
				f.DotPos = format.Length;
			}
			else {
				StringBuilder sb_decimal = new StringBuilder();
				while (exponent < 0) {
					sb_decimal.Insert(0, (char)('0' + (mantissa % 10)));
					mantissa /= 10;
					exponent++;
				}
				int k;
				for (k = sb_decimal.Length - 1;
					k >= 0 && sb_decimal[k] == '0'; k--);
				sb_decimal.Remove(k + 1, sb_decimal.Length - k - 1);
				if (sb_decimal.Length > 0) {
					sb.Append(nfi.NumberDecimalSeparator);
				}
				else if (format[f.DotPos + 1] == '0') {
					sb.Append(nfi.NumberDecimalSeparator);
				}
				bool terminado = false;
				for (int j = 0, i = f.DotPos + 1; i < f.ExpPos; i++) {
					if (format[i] == '0' || format[i] == '#') {
						if (j < sb_decimal.Length) {
							sb.Append(sb_decimal[j++]);
						}
						else if (format[i] == '0' && !terminado) {
							sb.Append('0');
						}
						else if (format[i] == '#' && !terminado) {
							terminado = true;
						}
					}
					else if (format[i] == '\\') {
						sb.Append(format[++i]);
					}
					else if (format [i] == '%')
						sb.Append (nfi.PercentSymbol);
					else if (format [i] == '\u2030')
						sb.Append (nfi.PerMilleSymbol);
					else if (format [i] == '\'') {
						int l = ++i;
						while (i < format.Length) {
							if (format [i] == '\'')
								break;
							i++;
						}
						sb.Insert (0, format.Substring (l, i - l));
					}
					else {
						sb.Append(format[i]);
					}
				}
			}
			int gro = 0;
			for (int i = f.DotPos - 1; i >= f.FirstFormatPos; i--) {
				if (format[i] == '#' || format[i] == '0') {
					if (exponent > 0 || mantissa > 0 || format[i] == '0') {
						if (f.Groupping && gro == nfi.NumberGroupSizes[0]) {
							sb.Insert(0, nfi.NumberGroupSeparator);
							gro = 0;
						}
						gro++;
						if (exponent > 0) {
							sb.Insert(0, '0');
							exponent--;
						}
						else if (mantissa > 0) {
							sb.Insert(0, (char)('0' + (mantissa % 10)));
							mantissa /= 10;
						}
						else if (format[i] == '0') {
							sb.Insert(0, '0');
						}
					}
				}
				else if (format [i] == '%')
					sb.Insert (0, nfi.PercentSymbol);
				else if (format [i] == '\u2030')
					sb.Insert (0, nfi.PerMilleSymbol);
				else if (format [i] == '\'') {
					int l = i;
					while (i >= 0) {
						if (format [i] == '\'')
							break;
						i--;
					}
					sb.Insert (0, format.Substring (i, l - i));
				}
				else if (format[i] != ',') {
					sb.Insert(0, format[i]);
				}
				else if (i > 0 && format[i-1] == '\\') {
					sb.Insert(0, format[i]);
					i -= 2;
				}
			}
			while (exponent > 0) {
				if (f.Groupping && gro == nfi.NumberGroupSizes[0]) {
					sb.Insert(0, nfi.NumberGroupSeparator);
					gro = 0;
				}
				gro++;
				sb.Insert(0, '0');
				exponent--;
			}
			while (mantissa > 0) {
				if (f.Groupping && gro == nfi.NumberGroupSizes[0]) {
					sb.Insert(0, nfi.NumberGroupSeparator);
					gro = 0;
				}
				gro++;
				sb.Insert(0, (char)('0' + (mantissa % 10)));
				mantissa /= 10;
			}
			for (int i = f.FirstFormatPos - 1; i >= 0; i--) {
				if (format [i] == '%')
					sb.Insert (0, nfi.PercentSymbol);
				else if (format [i] == '\u2030')
					sb.Insert (0, nfi.PerMilleSymbol);
				else if (format [i] == '\'') {
					int l = i;
					while (i >= 0) {
						if (format [i] == '\'')
							break;
						i--;
					}
					sb.Insert (0, format.Substring (i, l - i));
				}
				else if (format [i] != '.')
					sb.Insert(0, format[i]);
			}
			return sb.ToString();
		}

	}

	// from http://www.yoda.arachsys.com/csharp/floatingpoint.html
	// used with permission from original author
	internal class ArbitraryDecimal {
		/// <summary>Digits in the decimal expansion, one byte per digit
		byte[] digits;
		/// <summary> 
		/// How many digits are *after* the decimal point
		/// </summary>
		int decimalPoint=0;

		/// <summary> 
		/// Constructs an arbitrary decimal expansion from the given long.
		/// The long must not be negative.
		/// </summary>
		internal ArbitraryDecimal (long x)
		{
			string tmp = x.ToString (CultureInfo.InvariantCulture);
			digits = new byte [tmp.Length];
			for (int i=0; i < tmp.Length; i++)
				digits[i] = (byte) (tmp[i] - '0');
			Normalize ();
		}
        
		/// <summary>
		/// Multiplies the current expansion by the given amount, which should
		/// only be 2 or 5.
		/// </summary>
		internal void MultiplyBy (int amount)
		{
			byte[] result = new byte [digits.Length+1];
			for (int i=digits.Length-1; i >= 0; i--) {
				int resultDigit = digits [i] * amount + result [i+1];
				result [i] = (byte)(resultDigit / 10);
				result [i+1] = (byte)(resultDigit % 10);
			}
			if (result [0] != 0) {
				digits = result;
			}
			else {
				Array.Copy (result, 1, digits, 0, digits.Length);
			}
			Normalize ();
		}
        
		/// <summary>
		/// Shifts the decimal point; a negative value makes
		/// the decimal expansion bigger (as fewer digits come after the
		/// decimal place) and a positive value makes the decimal
		/// expansion smaller.
		/// </summary>
		internal void Shift (int amount)
		{
			decimalPoint += amount;
		}

		/// <summary>
		/// Removes leading/trailing zeroes from the expansion.
		/// </summary>
		internal void Normalize ()
		{
			int first;
			for (first=0; first < digits.Length; first++) {
				if (digits [first] != 0)
					break;
			}

			int last;
			for (last = digits.Length - 1; last >= 0; last--) {
				if (digits [last] != 0)
					break;
			}
            
			if ((first == 0) && (last == digits.Length - 1))
				return;
            
			byte[] tmp = new byte [last-first+1];
			for (int i=0; i < tmp.Length; i++)
				tmp [i] = digits [i + first];
            
			decimalPoint -= digits.Length - (last + 1);
			digits = tmp;
		}

		/// <summary>
		/// Converts the value to a proper decimal string representation.
		/// </summary>
		public string ToString (NumberFormatInfo nfi)
		{
			char[] digitString = new char [digits.Length];            
			for (int i=0; i < digits.Length; i++)
				digitString [i] = (char)(digits [i] + '0');
            
			// Simplest case - nothing after the decimal point,
			// and last real digit is non-zero, eg value=35
			if (decimalPoint == 0) {
				return new string (digitString);
			}
            
			// Fairly simple case - nothing after the decimal
			// point, but some 0s to add, eg value=350
			if (decimalPoint < 0) {
				return new string (digitString) + new string ('0', -decimalPoint);
			}
            
			// Nothing before the decimal point, eg 0.035
			if (decimalPoint >= digitString.Length) {
				return "0" + nfi.NumberDecimalSeparator + 
					new string ('0',(decimalPoint-digitString.Length))+ new string (digitString);
			}

			// Most complicated case - part of the string comes
			// before the decimal point, part comes after it,
			// eg 3.5
			return new string (digitString, 0, digitString.Length - decimalPoint) +
				nfi.NumberDecimalSeparator + 
				new string (digitString, digitString.Length - decimalPoint, decimalPoint);
		}
	}
}
