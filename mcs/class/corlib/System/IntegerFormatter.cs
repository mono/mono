//
// System.IntegerFormatter.cs
//
// Author:
//   Derek Holden  (dholden@draper.com)
//
// (C) Derek Holden  dholden@draper.com
//

//
// Format integer types. Completely based off ECMA docs
// for IFormattable specification. Has been tested w/ 
// all integral types, from boundry to boundry, w/ all 
// formats A## ("G", "G0" ... "G99", "P", "P0" ... "P99").
//
// If you make any changes, please make sure to check the
// boundry format precisions (0, 99) and the min / max values
// of the data types (Int32.[Max/Min]Value).
//
// Using int as an example, it is currently set up as
//
// Int32 {
//   int value;
//   public string ToString (string format, NumberFormatInfo nfi) {
//      return IntegerFormatter.NumberToString (format, nfi, value);
//   }
//
// IntegerFormatter {
//   public string NumberToString (string format, NumberFormatInfo nfi, int value) {
//      ParseFormat (format);
//      switch (format type) {
//        case 'G' FormatGeneral(value, precision);
//        case 'R' throw Exception("Invalid blah blah");
//        case 'C' FromatCurrency(value, precision, nfi);
//        etc...
//      }
//   }
// }
//
// For every integral type.
//
// Before every Format<Format Type> block there is a small paragraph
// detailing its requirements, and a blurb of what I was thinking
// at the time.
//
// Some speedup suggestions to be done when after this appears
// to be working properly:
//
//   * Deal w/ out of range numbers better. Specifically with
//     regards to boundry cases such as Long.MinValue etc.
//     The previous way of if (value < 0) value = -value;
//     fails under this assumption, since the largest
//     possible MaxValue is < absolute value of the MinValue.
//     I do the first iteration outside of the loop, and then
//     convert the number to positive, then continue in the loop.
//
//   * Replace all occurances of max<Type>Length with their 
//     numerical values. Plus the places where things are set
//     to max<Type>Length - 1. Hardcode these to numbers.
//
//   * Move the code for all the NumberToString()'s into the
//     the main ToString (string, NumberFormatInfo) method in
//     the data types themselves. That way they'd be throwing
//     their own exceptions on error and it'd save a function
//     call.
//
//   * For integer to char buffer transformation, you could
//     implement the calculations of the 10's and 100's place
//     the same time w/ another table to shorten loop time.
//
//   * Someone smarter can prolly find a much more efficient 
//     way of formatting the exponential notation. It's still
//     done in pass, just may have too many repositioning
//     calculations.
//   
//   * Decide whether it be better to have functions that
//     handle formatting for all types, or just cast their
//     values out and format them. Just if library size is
//     more important than speed in saving a cast and a 
//     function call.
//

using System.Globalization;

namespace System {

	internal sealed class IntegerFormatter {

		private static int maxByteLength = 4;
		private static int maxShortLength = 6;
		private static int maxIntLength = 12;
		private static int maxLongLength = 22;

		private static char[] digitLowerTable = 
       		{ '0', '1', '2', '3', '4', '5', '6', '7', 
		  '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };

		private static char[] digitUpperTable = 
		{ '0', '1', '2', '3', '4', '5', '6', '7', 
		  '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

		private static bool ParseFormat (string format, out char specifier,  out int precision)
		{		 		 
			precision = -1;
			specifier = '\0';
			
			int length = format.Length;
			if (length < 1 || length > 3)
				return false;
			
			char[] chars = format.ToCharArray ();
			specifier = chars[0];

			if (length == 1) 
				return true;
			
			if (length == 2) {
				if (chars[1] < '0' || chars[1] > '9')
					return false;
				
				precision = chars[1] - '0';
			} else {
				if (chars[1] < '0' || chars[2] < '0' || chars[1] > '9' || chars[2] > '9')
					return false;
				
				precision = (chars[1] - '0') * 10 + (chars[2] - '0');
			}
			
			return true;
		}	 

		// ============ Public Interface to all the integer types ============ //
		
		public static string NumberToString (string format, NumberFormatInfo nfi, byte value)
		{
			char specifier;
			int precision;
			
			if (!ParseFormat (format, out specifier, out precision))
				throw new FormatException ("The specified format is invalid");

			switch(specifier) {
			case 'c': return FormatCurrency (value, precision, nfi);
			case 'C': return FormatCurrency (value, precision, nfi);
			case 'd': return FormatDecimal (value, precision);
			case 'D': return FormatDecimal (value, precision);
			case 'e': return FormatExponential (value, precision, false);
			case 'E': return FormatExponential (value, precision, true);
			case 'f': return FormatFixedPoint (value, precision, nfi);	
			case 'F': return FormatFixedPoint (value, precision, nfi);	
			case 'g': return FormatGeneral (value, precision, nfi, false);
			case 'G': return FormatGeneral (value, precision, nfi, true);
			case 'n': return FormatNumber (value, precision, nfi);
			case 'N': return FormatNumber (value, precision, nfi);
			case 'p': return FormatPercent (value, precision, nfi);
			case 'P': return FormatPercent (value, precision, nfi);
			case 'r': throw new FormatException ("The specified format cannot be used in this instance");
			case 'R': throw new FormatException ("The specified format cannot be used in this instance");
			case 'x': return FormatHexadecimal (value, precision, false);
			case 'X': return FormatHexadecimal (value, precision, true);
			default: 
				throw new FormatException ("The specified format is invalid");
			}
		}		

		public static string NumberToString (string format, NumberFormatInfo nfi, short value)
		{
			char specifier;
			int precision;
			
			if (!ParseFormat (format, out specifier, out precision))
				throw new FormatException ("The specified format is invalid");

			switch(specifier) {
			case 'c': return FormatCurrency (value, precision, nfi);
			case 'C': return FormatCurrency (value, precision, nfi);
			case 'd': return FormatDecimal (value, precision);
			case 'D': return FormatDecimal (value, precision);
			case 'e': return FormatExponential (value, precision, false);
			case 'E': return FormatExponential (value, precision, true);
			case 'f': return FormatFixedPoint (value, precision, nfi);	
			case 'F': return FormatFixedPoint (value, precision, nfi);	
			case 'g': return FormatGeneral (value, precision, nfi, false);
			case 'G': return FormatGeneral (value, precision, nfi, true);
			case 'n': return FormatNumber (value, precision, nfi);
			case 'N': return FormatNumber (value, precision, nfi);
			case 'p': return FormatPercent (value, precision, nfi);
			case 'P': return FormatPercent (value, precision, nfi);
			case 'r': throw new FormatException ("The specified format cannot be used in this instance");
			case 'R': throw new FormatException ("The specified format cannot be used in this instance");
			case 'x': return FormatHexadecimal (value, precision, false);
			case 'X': return FormatHexadecimal (value, precision, true);
			default: 
				throw new FormatException ("The specified format is invalid");
			}
		}

		public static string NumberToString (string format, NumberFormatInfo nfi, int value)
		{
			char specifier;
			int precision;
			
			if (!ParseFormat (format, out specifier, out precision))
				throw new FormatException ("The specified format is invalid");
			
			switch(specifier) {
			case 'c': return FormatCurrency (value, precision, nfi);	
			case 'C': return FormatCurrency (value, precision, nfi);	
			case 'd': return FormatDecimal (value, precision);
			case 'D': return FormatDecimal (value, precision);
			case 'e': return FormatExponential (value, precision, false);
			case 'E': return FormatExponential (value, precision, true);
			case 'f': return FormatFixedPoint (value, precision, nfi);	
			case 'F': return FormatFixedPoint (value, precision, nfi);	
			case 'g': return FormatGeneral (value, precision, nfi, false);
			case 'G': return FormatGeneral (value, precision, nfi, true);
			case 'n': return FormatNumber (value, precision, nfi);
			case 'N': return FormatNumber (value, precision, nfi);
			case 'p': return FormatPercent (value, precision, nfi);
			case 'P': return FormatPercent (value, precision, nfi);
			case 'r': throw new FormatException ("The specified format cannot be used in this instance");
			case 'R': throw new FormatException ("The specified format cannot be used in this instance");
			case 'x': return FormatHexadecimal (value, precision, false);
			case 'X': return FormatHexadecimal (value, precision, true);
			default: 
				throw new FormatException ("The specified format is invalid");
			}
		}

		public static string NumberToString (string format, NumberFormatInfo nfi, long value)
		{
			char specifier;
			int precision;
			
			if (!ParseFormat (format, out specifier, out precision))
				throw new FormatException ("The specified format is invalid");
			
			switch(specifier) {
			case 'c': return FormatCurrency (value, precision, nfi);
			case 'C': return FormatCurrency (value, precision, nfi);
			case 'd': return FormatDecimal (value, precision);
			case 'D': return FormatDecimal (value, precision);
			case 'e': return FormatExponential (value, precision, false);
			case 'E': return FormatExponential (value, precision, true);
			case 'f': return FormatFixedPoint (value, precision, nfi);	
			case 'F': return FormatFixedPoint (value, precision, nfi);	
			case 'g': return FormatGeneral (value, precision, nfi, false);
			case 'G': return FormatGeneral (value, precision, nfi, true);
			case 'n': return FormatNumber (value, precision, nfi);
			case 'N': return FormatNumber (value, precision, nfi);
			case 'p': return FormatPercent (value, precision, nfi);
			case 'P': return FormatPercent (value, precision, nfi);
			case 'r': throw new FormatException ("The specified format cannot be used in this instance");
			case 'R': throw new FormatException ("The specified format cannot be used in this instance");
			case 'x': return FormatHexadecimal (value, precision, false);
			case 'X': return FormatHexadecimal (value, precision, true);
			default: 
				throw new FormatException ("The specified format is invalid");
			}			
		}

		public static string NumberToString (string format, NumberFormatInfo nfi, sbyte value)
		{
			char specifier;
			int precision;
			
			if (!ParseFormat (format, out specifier, out precision))
				throw new FormatException ("The specified format is invalid");
			
			switch(specifier) {
			case 'c': return FormatCurrency (value, precision, nfi);
			case 'C': return FormatCurrency (value, precision, nfi);
			case 'd': return FormatDecimal (value, precision);
			case 'D': return FormatDecimal (value, precision);
			case 'e': return FormatExponential (value, precision, false);
			case 'E': return FormatExponential (value, precision, true);
			case 'f': return FormatFixedPoint (value, precision, nfi);	
			case 'F': return FormatFixedPoint (value, precision, nfi);	
			case 'g': return FormatGeneral (value, precision, nfi, false);
			case 'G': return FormatGeneral (value, precision, nfi, true);
			case 'n': return FormatNumber (value, precision, nfi);
			case 'N': return FormatNumber (value, precision, nfi);
			case 'p': return FormatPercent (value, precision, nfi);
			case 'P': return FormatPercent (value, precision, nfi);
			case 'r': throw new FormatException ("The specified format cannot be used in this instance");
			case 'R': throw new FormatException ("The specified format cannot be used in this instance");
			case 'x': return FormatHexadecimal (value, precision, false);
			case 'X': return FormatHexadecimal (value, precision, true);
			default: 
				throw new FormatException ("The specified format is invalid");
			}
		}

		public static string NumberToString (string format, NumberFormatInfo nfi, ushort value)
		{
			char specifier;
			int precision;
			
			if (!ParseFormat (format, out specifier, out precision))
				throw new FormatException ("The specified format is invalid");
			
			switch(specifier) {
			case 'c': return FormatCurrency (value, precision, nfi);
			case 'C': return FormatCurrency (value, precision, nfi);
			case 'd': return FormatDecimal (value, precision);
			case 'D': return FormatDecimal (value, precision);
			case 'e': return FormatExponential (value, precision, false);
			case 'E': return FormatExponential (value, precision, true);
			case 'f': return FormatFixedPoint (value, precision, nfi);	
			case 'F': return FormatFixedPoint (value, precision, nfi);	
			case 'g': return FormatGeneral (value, precision, nfi, false);
			case 'G': return FormatGeneral (value, precision, nfi, true);
			case 'n': return FormatNumber (value, precision, nfi);
			case 'N': return FormatNumber (value, precision, nfi);
			case 'p': return FormatPercent (value, precision, nfi);
			case 'P': return FormatPercent (value, precision, nfi);
			case 'r': throw new FormatException ("The specified format cannot be used in this instance");
			case 'R': throw new FormatException ("The specified format cannot be used in this instance");
			case 'x': return FormatHexadecimal (value, precision, false);
			case 'X': return FormatHexadecimal (value, precision, true);
			default: 
				throw new FormatException ("The specified format is invalid");
			}
		}

		public static string NumberToString (string format, NumberFormatInfo nfi, uint value)
		{
			char specifier;
			int precision;
			
			if (!ParseFormat (format, out specifier, out precision))
				throw new FormatException ("The specified format is invalid");
			
			switch(specifier) {
			case 'c': return FormatCurrency (value, precision, nfi);
			case 'C': return FormatCurrency (value, precision, nfi);
			case 'd': return FormatDecimal (value, precision);
			case 'D': return FormatDecimal (value, precision);
			case 'e': return FormatExponential (value, precision, false);
			case 'E': return FormatExponential (value, precision, true);
			case 'f': return FormatFixedPoint (value, precision, nfi);	
			case 'F': return FormatFixedPoint (value, precision, nfi);	
			case 'g': return FormatGeneral (value, precision, nfi, false);
			case 'G': return FormatGeneral (value, precision, nfi, true);
			case 'n': return FormatNumber (value, precision, nfi);
			case 'N': return FormatNumber (value, precision, nfi);
			case 'p': return FormatPercent (value, precision, nfi);
			case 'P': return FormatPercent (value, precision, nfi);
			case 'r': throw new FormatException ("The specified format cannot be used in this instance");
			case 'R': throw new FormatException ("The specified format cannot be used in this instance");
			case 'x': return FormatHexadecimal (value, precision, false);
			case 'X': return FormatHexadecimal (value, precision, true);
			default: 
				throw new FormatException ("The specified format is invalid");
			}
		}

		public static string NumberToString (string format, NumberFormatInfo nfi, ulong value)
		{
			char specifier;
			int precision;
			
			if (!ParseFormat (format, out specifier, out precision))
				throw new FormatException ("The specified format is invalid");
			
			switch(specifier) {
			case 'c': return FormatCurrency (value, precision, nfi);
			case 'C': return FormatCurrency (value, precision, nfi);
			case 'd': return FormatDecimal (value, precision);
			case 'D': return FormatDecimal (value, precision);
			case 'e': return FormatExponential (value, precision, false);
			case 'E': return FormatExponential (value, precision, true);
			case 'f': return FormatFixedPoint (value, precision, nfi);	
			case 'F': return FormatFixedPoint (value, precision, nfi);	
			case 'g': return FormatGeneral (value, precision, nfi, false);
			case 'G': return FormatGeneral (value, precision, nfi, true);
			case 'n': return FormatNumber (value, precision, nfi);
			case 'N': return FormatNumber (value, precision, nfi);
			case 'p': return FormatPercent (value, precision, nfi);
			case 'P': return FormatPercent (value, precision, nfi);
			case 'r': throw new FormatException ("The specified format cannot be used in this instance");
			case 'R': throw new FormatException ("The specified format cannot be used in this instance");
			case 'x': return FormatHexadecimal (value, precision, false);
			case 'X': return FormatHexadecimal (value, precision, true);
			default: 
				throw new FormatException ("The specified format is invalid");
			}
		}

		// ============ Currency Type Formating ============ //

		//
		//  Currency Format: Used for strings containing a monetary value. The
		//  CurrencySymbol, CurrencyGroupSizes, CurrencyGroupSeparator, and
		//  CurrencyDecimalSeparator members of a NumberFormatInfo supply
		//  the currency symbol, size and separator for digit groupings, and
		//  decimal separator, respectively.
		//  CurrencyNegativePattern and CurrencyPositivePattern determine the
		//  symbols used to represent negative and positive values. For example,
		//  a negative value may be prefixed with a minus sign, or enclosed in
		//  parentheses.
		//  If the precision specifier is omitted
		//  NumberFormatInfo.CurrencyDecimalDigits determines the number of
		//  decimal places in the string. Results are rounded to the nearest
		//  representable value when necessary.
		//
		//  The pattern of the NumberFormatInfo determines how the output looks, where
		//  the dollar sign goes, where the negative sign goes, etc.
		//  IFormattable documentation lists the patterns and their values,
		//  I have them commented out in the large switch statement
		//

		private static string FormatCurrency (byte value, int precision, NumberFormatInfo nfi) 
		{
			return FormatCurrency ((uint)value, precision, nfi);
		}

		private static string FormatCurrency (short value, int precision, NumberFormatInfo nfi) 
		{
			return FormatCurrency ((int)value, precision, nfi);			
		}
			
		private static string FormatCurrency (int value, int precision, NumberFormatInfo nfi) 
		{
			int i, j, k;
			bool negative = (value < 0);

			char[] groupSeparator = nfi.CurrencyGroupSeparator.ToCharArray ();
			char[] decimalSeparator = nfi.CurrencyDecimalSeparator.ToCharArray ();
			char[] currencySymbol = nfi.CurrencySymbol.ToCharArray ();
			int[] groupSizes = nfi.CurrencyGroupSizes;
			int pattern = negative ? nfi.CurrencyNegativePattern : nfi.CurrencyPositivePattern;
			int symbolLength = currencySymbol.Length;
			
			int padding = (precision >= 0) ? precision : nfi.CurrencyDecimalDigits;	     
			int size = maxIntLength + (groupSeparator.Length * maxIntLength) + padding + 2 + 
			decimalSeparator.Length + symbolLength;	
			char[] buffy = new char[size];
			int position = size;

			// set up the pattern from IFormattible
			if (negative) {
				i = symbolLength; 
 
				switch (pattern) {
				case 0: // ($nnn)
					buffy[--position] = ')'; 
					break;
				// case 1: // -$nnn
				//	break;
				// case 2: // $-nnn
				//	break;
				case 3: // $nnn-
					buffy[--position] = '-';
					break;
				case 4:	// (nnn$)
					buffy[--position] = ')'; 
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					break;
				case 5:	// -nnn$
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					break;
				case 6:	// nnn-$
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					buffy[--position] = '-'; 
					break;
				case 7: // nnn$-
					buffy[--position] = '-'; 
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					break;
				case 8: // -nnn $
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					buffy[--position] = ' '; 
					break;
				// case 9: // -$ nnn
				//	break;
				case 10: // nnn $-
					buffy[--position] = '-'; 
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					buffy[--position] = ' '; 
					break;
				case 11: // $ nnn-
					buffy[--position] = '-'; 
					break;
				// case 12: // $ -nnn
				//	break;
				case 13: // nnn- $
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					buffy[--position] = ' '; 
					buffy[--position] = '-'; 
					break;
				case 14: // ($ nnn)
					buffy[--position] = ')'; 
					break;
				case 15: // (nnn $)
					buffy[--position] = ')'; 
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					break;				
				}
			} else {
				i = symbolLength; 
				switch (pattern) {
				// case 0: // $nnn
				//	break;
				case 1: // nnn$
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					break;
				// case 2: // $ nnn
				//	break;
				case 3: // nnn $
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					buffy[--position] = ' '; 
					break;
				}
			}
			
			// right pad it w/ precision 0's
			while (padding-- > 0)
				buffy[--position] = '0';

			// put on decimal separator if we moved over and put a 0 
			if (position < size && buffy[position] == '0') {
				i = decimalSeparator.Length; 
				do {
					buffy[--position] = decimalSeparator[--i];
				} while (i > 0);			
			}

			// loop through, keeping track of where you are in the
			// group sizes array and putting out the group separator
			// when needed
			j = 0;
			k = groupSizes[j++];

			// just in place to take care of the negative boundries (Int32.MinValue)
			if (negative) {
				if (value <= -10) {
					buffy[--position] = digitLowerTable[-(value % 10)];
					value = value / -10;
					
					if (--k == 0) {
						i = groupSeparator.Length; 
						do {
							buffy[--position] = groupSeparator[--i];
						} while (i > 0);
						
						k = (j < groupSizes.Length) ? 
						groupSizes[j++] : groupSizes[(groupSizes.Length - 1)];
					}
				} else value = -value;
			}

			while (value >= 10) {
				buffy[--position] = digitLowerTable[(value % 10)];
				value /= 10;

				if (--k == 0) {
					i = groupSeparator.Length; 
					do {
						buffy[--position] = groupSeparator[--i];
					} while (i > 0);
					
					k = (j < groupSizes.Length) ? 
					groupSizes[j++] : groupSizes[(groupSizes.Length - 1)];
				}
			}		 

			buffy[--position] = digitLowerTable[value];

			// end the pattern on the left hand side
			if (negative) {
				i = symbolLength; 
 
				switch (pattern) {
				case 0: // ($nnn)
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					buffy[--position] = '('; 
					break;
				case 1: // -$nnn
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					buffy[--position] = '-'; 
					break;
				case 2: // $-nnn
					buffy[--position] = '-'; 
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					break;
				case 3: // $nnn-
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					break;
				case 4:	// (nnn$)
					buffy[--position] = '('; 
					break;
				case 5:	// -nnn$
					buffy[--position] = '-'; 
					break;
				// case 6: // nnn-$
				//	break;
				// case 7: // nnn$-
				//	break;
				case 8: // -nnn $
					buffy[--position] = '-'; 
					break;
				// case 9: // -$ nnn
				//	break;
				// case 10: // nnn $-
				//	break;
				case 11: // $ nnn-
					buffy[--position] = ' '; 
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					break;
				case 12: // $ -nnn
					buffy[--position] = '-'; 
					buffy[--position] = ' '; 
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					break;
				// case 13: // nnn- $
				//	break;
				case 14: // ($ nnn)
					buffy[--position] = ' '; 
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					buffy[--position] = '('; 
					break;
				case 15: // (nnn $)
					buffy[--position] = '('; 
					break;				
				}
			} else {
				i = symbolLength; 
				switch (pattern) {
				case 0: // $nnn
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					break;
				// case 1: // nnn$
				//	break;
				case 2: // $ nnn
					buffy[--position] = ' '; 
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					break;
				// case 3: // nnn $
				//	break;
				}
			}
			
			return new string (buffy, position, (size - position));
		}

		private static string FormatCurrency (long value, int precision, NumberFormatInfo nfi) 
		{
			int i, j, k;
			bool negative = (value < 0);

			char[] groupSeparator = nfi.CurrencyGroupSeparator.ToCharArray ();
			char[] decimalSeparator = nfi.CurrencyDecimalSeparator.ToCharArray ();
			char[] currencySymbol = nfi.CurrencySymbol.ToCharArray ();
			int[] groupSizes = nfi.CurrencyGroupSizes;
			int pattern = negative ? nfi.CurrencyNegativePattern : nfi.CurrencyPositivePattern;
			int symbolLength = currencySymbol.Length;
			
			int padding = (precision >= 0) ? precision : nfi.CurrencyDecimalDigits;	     
			int size = maxLongLength + (groupSeparator.Length * maxLongLength) + padding + 2 + 
			decimalSeparator.Length + symbolLength;	
			char[] buffy = new char[size];
			int position = size;

			// set up the pattern from IFormattible
			if (negative) {
				i = symbolLength; 
 
				switch (pattern) {
				case 0: // ($nnn)
					buffy[--position] = ')'; 
					break;
				// case 1: // -$nnn
				//	break;
				// case 2: // $-nnn
				//	break;
				case 3: // $nnn-
					buffy[--position] = '-';
					break;
				case 4:	// (nnn$)
					buffy[--position] = ')'; 
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					break;
				case 5:	// -nnn$
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					break;
				case 6:	// nnn-$
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					buffy[--position] = '-'; 
					break;
				case 7: // nnn$-
					buffy[--position] = '-'; 
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					break;
				case 8: // -nnn $
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					buffy[--position] = ' '; 
					break;
				// case 9: // -$ nnn
				//	break;
				case 10: // nnn $-
					buffy[--position] = '-'; 
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					buffy[--position] = ' '; 
					break;
				case 11: // $ nnn-
					buffy[--position] = '-'; 
					break;
				// case 12: // $ -nnn
				//	break;
				case 13: // nnn- $
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					buffy[--position] = ' '; 
					buffy[--position] = '-'; 
					break;
				case 14: // ($ nnn)
					buffy[--position] = ')'; 
					break;
				case 15: // (nnn $)
					buffy[--position] = ')'; 
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					break;				
				}
			} else {
				i = symbolLength; 
				switch (pattern) {
				// case 0: // $nnn
				//	break;
				case 1: // nnn$
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					break;
				// case 2: // $ nnn
				//	break;
				case 3: // nnn $
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					buffy[--position] = ' '; 
					break;
				}
			}
			
			// right pad it w/ precision 0's
			while (padding-- > 0)
				buffy[--position] = '0';

			// put on decimal separator if we moved over and put a 0 
			if (position < size && buffy[position] == '0') {
				i = decimalSeparator.Length; 
				do {
					buffy[--position] = decimalSeparator[--i];
				} while (i > 0);			
			}

			// loop through, keeping track of where you are in the
			// group sizes array and putting out the group separator
			// when needed
			j = 0;
			k = groupSizes[j++];
		       
			if (negative) {
				if (value <= -10) {
					buffy[--position] = digitLowerTable[-(value % 10)];
					value = value / -10;
					
					if (--k == 0) {
						i = groupSeparator.Length; 
						do {
							buffy[--position] = groupSeparator[--i];
						} while (i > 0);
						
						k = (j < groupSizes.Length) ? 
						groupSizes[j++] : groupSizes[(groupSizes.Length - 1)];
					}
				} else value = -value;
			}

			while (value >= 10) {
				buffy[--position] = digitLowerTable[(value % 10)];
				value /= 10;

				if (--k == 0) {
					i = groupSeparator.Length; 
					do {
						buffy[--position] = groupSeparator[--i];
					} while (i > 0);
					
					k = (j < groupSizes.Length) ? 
					groupSizes[j++] : groupSizes[(groupSizes.Length - 1)];
				}
			}		 

			buffy[--position] = digitLowerTable[value];

			// end the pattern on the left hand side
			if (negative) {
				i = symbolLength; 
 
				switch (pattern) {
				case 0: // ($nnn)
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					buffy[--position] = '('; 
					break;
				case 1: // -$nnn
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					buffy[--position] = '-'; 
					break;
				case 2: // $-nnn
					buffy[--position] = '-'; 
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					break;
				case 3: // $nnn-
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					break;
				case 4:	// (nnn$)
					buffy[--position] = '('; 
					break;
				case 5:	// -nnn$
					buffy[--position] = '-'; 
					break;
				// case 6: // nnn-$
				//	break;
				// case 7: // nnn$-
				//	break;
				case 8: // -nnn $
					buffy[--position] = '-'; 
					break;
				// case 9: // -$ nnn
				//	break;
				// case 10: // nnn $-
				//	break;
				case 11: // $ nnn-
					buffy[--position] = ' '; 
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					break;
				case 12: // $ -nnn
					buffy[--position] = '-'; 
					buffy[--position] = ' '; 
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					break;
				// case 13: // nnn- $
				//	break;
				case 14: // ($ nnn)
					buffy[--position] = ' '; 
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					buffy[--position] = '('; 
					break;
				case 15: // (nnn $)
					buffy[--position] = '('; 
					break;				
				}
			} else {
				i = symbolLength; 
				switch (pattern) {
				case 0: // $nnn
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					break;
				// case 1: // nnn$
				//	break;
				case 2: // $ nnn
					buffy[--position] = ' '; 
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					break;
				// case 3: // nnn $
				//	break;
				}
			}
			
			return new string (buffy, position, (size - position));
		}

		private static string FormatCurrency (sbyte value, int precision, NumberFormatInfo nfi) 
		{
			return FormatCurrency ((int)value, precision, nfi);
		}

		private static string FormatCurrency (ushort value, int precision, NumberFormatInfo nfi) 
		{
			return FormatCurrency ((uint)value, precision, nfi);			
		}

		private static string FormatCurrency (uint value, int precision, NumberFormatInfo nfi) 
		{
			int i, j, k;

			char[] groupSeparator = nfi.CurrencyGroupSeparator.ToCharArray ();
			char[] decimalSeparator = nfi.CurrencyDecimalSeparator.ToCharArray ();
			char[] currencySymbol = nfi.CurrencySymbol.ToCharArray ();
			int[] groupSizes = nfi.CurrencyGroupSizes;
			int pattern = nfi.CurrencyPositivePattern;
			int symbolLength = currencySymbol.Length;
			
			int padding = (precision >= 0) ? precision : nfi.CurrencyDecimalDigits;	     
			int size = maxIntLength + (groupSeparator.Length * maxIntLength) + padding + 2 + 
			decimalSeparator.Length + symbolLength;	
			char[] buffy = new char[size];
			int position = size;

			// set up the pattern from IFormattible, no negative
			i = symbolLength; 
			switch (pattern) {
			// case 0: // $nnn
			//	break;
			case 1: // nnn$
				do {
					buffy[--position] = currencySymbol[--i];
				} while (i > 0);
				break;
			// case 2: // $ nnn
			//	break;
			case 3: // nnn $
				do {
					buffy[--position] = currencySymbol[--i];
				} while (i > 0);
				buffy[--position] = ' '; 
				break;
			}
			
			// right pad it w/ precision 0's
			while (padding-- > 0)
				buffy[--position] = '0';

			// put on decimal separator if we moved over and put a 0 
			if (position < size && buffy[position] == '0') {
				i = decimalSeparator.Length; 
				do {
					buffy[--position] = decimalSeparator[--i];
				} while (i > 0);			
			}

			// loop through, keeping track of where you are in the
			// group sizes array and putting out the group separator
			// when needed
			j = 0;
			k = groupSizes[j++];
		       
			while (value >= 10) {
				buffy[--position] = digitLowerTable[(value % 10)];
				value /= 10;

				if (--k == 0) {
					i = groupSeparator.Length; 
					do {
						buffy[--position] = groupSeparator[--i];
					} while (i > 0);
					
					k = (j < groupSizes.Length) ? 
					groupSizes[j++] : groupSizes[(groupSizes.Length - 1)];
				}
			}		 

			buffy[--position] = digitLowerTable[value];

			// end the pattern on the left hand side
			i = symbolLength; 
			switch (pattern) {
			case 0: // $nnn
				do {
					buffy[--position] = currencySymbol[--i];
				} while (i > 0);
				break;
			// case 1: // nnn$
			//	break;
			case 2: // $ nnn
				buffy[--position] = ' '; 
				do {
					buffy[--position] = currencySymbol[--i];
				} while (i > 0);
				break;
			// case 3: // nnn $
				//	break;
			}
			
			return new string (buffy, position, (size - position));
		}

		private static string FormatCurrency (ulong value, int precision, NumberFormatInfo nfi) 
		{
			int i, j, k;

			char[] groupSeparator = nfi.CurrencyGroupSeparator.ToCharArray ();
			char[] decimalSeparator = nfi.CurrencyDecimalSeparator.ToCharArray ();
			char[] currencySymbol = nfi.CurrencySymbol.ToCharArray ();
			int[] groupSizes = nfi.CurrencyGroupSizes;
			int pattern = nfi.CurrencyPositivePattern;
			int symbolLength = currencySymbol.Length;
			
			int padding = (precision >= 0) ? precision : nfi.CurrencyDecimalDigits;	     
			int size = maxLongLength + (groupSeparator.Length * maxLongLength) + padding + 2 + 
			decimalSeparator.Length + symbolLength;	
			char[] buffy = new char[size];
			int position = size;

			// set up the pattern from IFormattible, no negative
			i = symbolLength; 
			switch (pattern) {
			// case 0: // $nnn
			//	break;
			case 1: // nnn$
				do {
					buffy[--position] = currencySymbol[--i];
				} while (i > 0);
				break;
			// case 2: // $ nnn
			//	break;
			case 3: // nnn $
				do {
					buffy[--position] = currencySymbol[--i];
				} while (i > 0);
				buffy[--position] = ' '; 
				break;
			}
			
			// right pad it w/ precision 0's
			while (padding-- > 0)
				buffy[--position] = '0';

			// put on decimal separator if we moved over and put a 0 
			if (position < size && buffy[position] == '0') {
				i = decimalSeparator.Length; 
				do {
					buffy[--position] = decimalSeparator[--i];
				} while (i > 0);			
			}

			// loop through, keeping track of where you are in the
			// group sizes array and putting out the group separator
			// when needed
			j = 0;
			k = groupSizes[j++];
		       
			while (value >= 10) {
				buffy[--position] = digitLowerTable[(value % 10)];
				value /= 10;

				if (--k == 0) {
					i = groupSeparator.Length; 
					do {
						buffy[--position] = groupSeparator[--i];
					} while (i > 0);
					
					k = (j < groupSizes.Length) ? 
					groupSizes[j++] : groupSizes[(groupSizes.Length - 1)];
				}
			}		 

			buffy[--position] = digitLowerTable[value];

			// end the pattern on the left hand side
			i = symbolLength; 
			switch (pattern) {
			case 0: // $nnn
				do {
					buffy[--position] = currencySymbol[--i];
				} while (i > 0);
				break;
			// case 1: // nnn$
			//	break;
			case 2: // $ nnn
				buffy[--position] = ' '; 
				do {
					buffy[--position] = currencySymbol[--i];
				} while (i > 0);
				break;
			// case 3: // nnn $
				//	break;
			}
			
			return new string (buffy, position, (size - position));
		}
		
		// ============ Format Decimal Types ============ //

		//
		// Used only for integral data types. Negative values are 
		// represented by using a '-' sign. The precision specifies
		// how many digits are to appear in the string. If it is >
		// how many digits we need, the left side is padded w/ 0's.
		// If it is smaller than what we need, it is discarded.
		//
		// Fairly simple implementation. Fill the buffer from right
		// to left w/ numbers, then if we still have precision left
		// over, pad w/ zeros.
		//

		private static string FormatDecimal (byte value, int precision) 
		{
			return FormatDecimal ((uint)value, precision);
		}

		private static string FormatDecimal (short value, int precision) 
		{
			return FormatDecimal ((int)value, precision);
		}
	
		private static string FormatDecimal (int value, int precision)
		{
			int size = (precision > 0) ? (maxIntLength + precision) : maxIntLength;
			char[] buffy = new char[size];
			int position = size;
			bool negative = (value < 0);
			
			if (negative) 
				if (value <= -10) {
					buffy[--position] = digitLowerTable[-(value % 10)];
					value = value / -10;
				} else value = -value;
			
			// get our value into a buffer from right to left
			while (value >= 10) {
				buffy[--position] = digitLowerTable[(value % 10)];
				value /= 10;
			}
				
			buffy[--position] = digitLowerTable[value];

			// if we have precision left over, fill with 0's
			precision -= (size - position); 
			while (precision-- > 0 && position > 1) 
				buffy[--position] = '0';

			if (negative) 
				buffy[--position] = '-';
			
			return new string (buffy, position, (size - position));  
		}

		private static string FormatDecimal (long value, int precision)
		{
			int size = (precision > 0) ? (maxLongLength + precision) : maxLongLength;
			char[] buffy = new char[size];
			int position = size;
			bool negative = (value < 0);

			if (negative) 
				if (value <= -10) {
					buffy[--position] = digitLowerTable[-(value % 10)];
					value = value / -10;
				} else value = -value;

			// get our value into a buffer from right to left
			while (value >= 10) {
				buffy[--position] = digitLowerTable[(value % 10)];
				value /= 10;
			}
				
			buffy[--position] = digitLowerTable[value];
			
			// if we have precision left over, fill with 0's
			precision -= (size - position); 
			while (precision-- > 0 && position > 1)
				buffy[--position] = '0';

			if (negative) 
				buffy[--position] = '-';
			
			return new string (buffy, position, (size - position));  
		}

		private static string FormatDecimal (sbyte value, int precision) 
		{
			return FormatDecimal ((int)value, precision);
		}

		private static string FormatDecimal (ushort value, int precision) 
		{
			return FormatDecimal ((uint)value, precision);
		}

		private static string FormatDecimal (uint value, int precision)
		{
			int size = (precision > 0) ? (maxIntLength + precision) : maxIntLength;
			char[] buffy = new char[size];
			int position = size;

			// get our value into a buffer from right to left
			while (value >= 10) {
				buffy[--position] = digitLowerTable[(value % 10)];
				value /= 10;
			}
				
			buffy[--position] = digitLowerTable[value];
			
			// if we have precision left over, fill with 0's
			precision -= (size - position); 
			while (precision-- > 0 && position > 1) 
				buffy[--position] = '0';

			return new string (buffy, position, (size - position));  
		}

		private static string FormatDecimal (ulong value, int precision)
		{
			int size = (precision > 0) ? (maxLongLength + precision) : maxLongLength;
			char[] buffy = new char[size];
			int position = size;

			// get our value into a buffer from right to left
			while (value >= 10) {
				buffy[--position] = digitLowerTable[(value % 10)];
				value /= 10;
			}
				
			buffy[--position] = digitLowerTable[value];
			
			// if we have precision left over, fill with 0's
			precision -= (size - position); 
			while (precision-- > 0 && position > 1)
				buffy[--position] = '0';

			return new string (buffy, position, (size - position));  
		}

		// ============ Format Exponentials ============ //

		//
		// Used for strings in the format [-]M.DDDDDDe+XXX.
		// Exaclty one non-zero digit must appear in M, w/ 
		// a '-' sign if negative. The precision determines 
		// number of decimal places, if not given go 6 places.
		// If precision > the number of places we need, it
		// is right padded w/ 0's. If it is smaller than what
		// we need, we cut off and round. The format specifier
		// decides whether we use an uppercase E or lowercase e.
		// 
		// Tried to do this in one pass of one buffer, but it
		// wasn't happening. Get a buffer + 7 extra slots for
		// the -, ., E, +, and XXX. Parse the value into another
		// temp buffer, then build the new string. For the
		// integral data types, there are a couple things that
		// can be hardcoded. Since an int and a long can't be
		// larger than 20 something spaces, the first X w/ 
		// always be 0, and the the exponential value will only
		// be 2 digits long. Also integer types w/ always
		// have a positive exponential.
		//
		
		private static string FormatExponential (byte value, int precision, bool upper) 
		{
			return FormatExponential ((uint)value, precision, upper);
		}

		private static string FormatExponential (short value, int precision, bool upper) 
		{
			return FormatExponential ((int)value, precision, upper);
		}

		private static string FormatExponential (int value, int precision, bool upper)
		{
			bool negative = (value < 0);
			int padding = (precision >= 0) ? precision : 6;
			char[] buffy = new char[(padding + 8)];
			char[] tmp = new char [maxIntLength];
			int exponent = 0, position = maxIntLength;
			int exp = 0, idx = 0;
			ulong pow = 10;

			// ugly, but doing it since abs(Int32.MinValue) > Int.MaxValue
			uint number = (negative) ? (uint)((-(value + 1)) + 1) : (uint)value;

			// need to calculate the number of places to know if we need to round later
			if (negative && value <= -10) {
				value /= -10;
				exp++;
			}

			while (value >= 10) {
				value /= 10;
				exp++;
			}
							
			if (exp > padding) {

				// highest number we should goto before we round
				while (idx++ <= padding)
					pow *= 10;

				// get our value into a buffer
				while (number > pow) {
					tmp[--position] = digitLowerTable[(number % 10)];
					number /= 10;
					exponent++;
				}
			
				number += 5;
			}

			while (number >= 10) {
				tmp[--position] = digitLowerTable[(number% 10)];
				number /= 10;
				exponent++;
			}		       

			tmp[--position] = digitLowerTable[number];
			idx = 0;
			
			// go left to right in filling up new string
			if (negative)
				buffy[idx++] = '-';

			// we know we have at least one in there, followed 
			// by a decimal point
			buffy[idx++] = tmp[position++];
			if (precision != 0)
				buffy[idx++] = '.';

			// copy over the remaining digits until we run out,
			// or we've passed our specified precision
			while (padding > 0 && position < maxIntLength) {
				buffy[idx++] = tmp[position++];
				padding--;
			}
			
			// if we still have more precision to go, add some
			// zeros
			while (padding > 0) {
				buffy[idx++] = '0';
				padding--;
			}
			
			// we know these next 3 spots
			buffy[idx++] = upper ? 'E' : 'e';
			buffy[idx++] = '+';
			buffy[idx++] = '0';
			
			// next two digits depend on our length
			if (exponent >= 10) {
				buffy[idx++] = digitLowerTable[(exponent / 10)];
				buffy[idx] = digitLowerTable[(exponent % 10)];
			} else { 
				buffy[idx++] = '0';
				buffy[idx] = digitLowerTable[exponent];
			}

			return new string(buffy, 0, ++idx); 
		}

		private static string FormatExponential (long value, int precision, bool upper)
		{
			bool negative = (value < 0);
			int padding = (precision >= 0) ? precision : 6;
			char[] buffy = new char[(padding + 8)];
			char[] tmp = new char [maxLongLength];
			int exponent = 0, position = maxLongLength;
			int exp = 0, idx = 0;
			ulong pow = 10;

			// ugly, but doing it since abs(Int32.MinValue) > Int.MaxValue
			ulong number = (negative) ? (ulong)((-(value + 1)) + 1) : (ulong)value;

			// need to calculate the number of places to know if we need to round later
			if (negative && value <= -10) {
				value /= -10;
				exp++;
			}

			while (value >= 10) {
				value /= 10;
				exp++;
			}
							
			if (exp > padding) {
				
				// highest number we should goto before we round
				while (idx++ <= padding)
					pow *= 10;
				
				// get our value into a buffer
				while (number > pow) {
					tmp[--position] = digitLowerTable[(number % 10)];
					number /= 10;
					exponent++;
				}
			
				number += 5;
			}

			while (number >= 10) {
				tmp[--position] = digitLowerTable[(number% 10)];
				number /= 10;
				exponent++;
			}		       

			tmp[--position] = digitLowerTable[number];
			idx = 0;

			// go left to right in filling up new string
			if (negative)
				buffy[idx++] = '-';

			// we know we have at least one in there, followed 
			// by a decimal point
			buffy[idx++] = tmp[position++];
			if (precision != 0)
				buffy[idx++] = '.';

			// copy over the remaining digits until we run out,
			// or we've passed our specified precision
			while (padding > 0 && position < maxLongLength) {
				buffy[idx++] = tmp[position++];
				padding--;
			}
			
			// if we still have more precision to go, add some
			// zeros
			while (padding > 0) {
				buffy[idx++] = '0';
				padding--;
			}
			
			// we know these next 3 spots
			buffy[idx++] = upper ? 'E' : 'e';
			buffy[idx++] = '+';
			buffy[idx++] = '0';
			
			// next two digits depend on our length
			if (exponent >= 10) {
				buffy[idx++] = digitLowerTable[(exponent / 10)];
				buffy[idx] = digitLowerTable[(exponent % 10)];
			} else { 
				buffy[idx++] = '0';
				buffy[idx] = digitLowerTable[exponent];
			}

			return new string(buffy, 0, ++idx); 
		}

		private static string FormatExponential (sbyte value, int precision, bool upper) 
		{
			return FormatExponential ((int)value, precision, upper);
		}

		private static string FormatExponential (ushort value, int precision, bool upper) 
		{
			return FormatExponential ((uint)value, precision, upper);
		}

		private static string FormatExponential (uint value, int precision, bool upper)
		{
			int padding = (precision >= 0) ? precision : 6;
			char[] buffy = new char[(padding + 8)];
			char[] tmp = new char [maxIntLength];
			int exponent = 0, position = maxIntLength;
			int exp = 0, idx = 0;
			ulong pow = 10;
			uint number = value;

			// need to calculate the number of places to know if we need to round later
			while (value >= 10) {
				value /= 10;
				exp++;
			}
							
			if (exp > padding) {

				// highest number we should goto before we round
				while (idx++ <= padding)
					pow *= 10;
				
				// get our value into a buffer
				while (number > pow) {
					tmp[--position] = digitLowerTable[(number % 10)];
					number /= 10;
					exponent++;
				}
				
				number += 5;
			}

			while (number >= 10) {
				tmp[--position] = digitLowerTable[(number% 10)];
				number /= 10;
				exponent++;
			}		       

			tmp[--position] = digitLowerTable[number];
			idx = 0;

			// we know we have at least one in there, followed 
			// by a decimal point
			buffy[idx++] = tmp[position++];
			if (precision != 0)
				buffy[idx++] = '.';

			// copy over the remaining digits until we run out,
			// or we've passed our specified precision
			while (padding > 0 && position < maxIntLength) {
				buffy[idx++] = tmp[position++];
				padding--;
			}
			
			// if we still have more precision to go, add some
			// zeros
			while (padding > 0) {
				buffy[idx++] = '0';
				padding--;
			}
			
			// we know these next 3 spots
			buffy[idx++] = upper ? 'E' : 'e';
			buffy[idx++] = '+';
			buffy[idx++] = '0';
			
			// next two digits depend on our length
			if (exponent >= 10) {
				buffy[idx++] = digitLowerTable[(exponent / 10)];
				buffy[idx] = digitLowerTable[(exponent % 10)];
			} else { 
				buffy[idx++] = '0';
				buffy[idx] = digitLowerTable[exponent];
			}

			return new string(buffy, 0, ++idx); 
		}

		private static string FormatExponential (ulong value, int precision, bool upper)
		{
			int padding = (precision >= 0) ? precision : 6;
			char[] buffy = new char[(padding + 8)];
			char[] tmp = new char [maxLongLength];
			int exponent = 0, position = maxLongLength;
			int exp = 0, idx = 0;
			ulong pow = 10;
			ulong number = value;

			// need to calculate the number of places to know if we need to round later
			while (value >= 10) {
				value /= 10;
				exp++;
			}
							
			if (exp > padding) {

				// highest number we should goto before we round
				while (idx++ <= padding)
					pow *= 10;

				// get our value into a buffer
				while (number > pow) {
					tmp[--position] = digitLowerTable[(number % 10)];
					number /= 10;
					exponent++;
				}
			
				number += 5;
			}

			while (number >= 10) {
				tmp[--position] = digitLowerTable[(number% 10)];
				number /= 10;
				exponent++;
			}		       

			tmp[--position] = digitLowerTable[number];
			idx = 0;

			// we know we have at least one in there, followed 
			// by a decimal point
			buffy[idx++] = tmp[position++];
			if (precision != 0)
				buffy[idx++] = '.';

			// copy over the remaining digits until we run out,
			// or we've passed our specified precision
			while (padding > 0 && position < maxLongLength) {
				buffy[idx++] = tmp[position++];
				padding--;
			}
			
			// if we still have more precision to go, add some
			// zeros
			while (padding > 0) {
				buffy[idx++] = '0';
				padding--;
			}
			
			// we know these next 3 spots
			buffy[idx++] = upper ? 'E' : 'e';
			buffy[idx++] = '+';
			buffy[idx++] = '0';
			
			// next two digits depend on our length
			if (exponent >= 10) {
				buffy[idx++] = digitLowerTable[(exponent / 10)];
				buffy[idx] = digitLowerTable[(exponent % 10)];
			} else { 
				buffy[idx++] = '0';
				buffy[idx] = digitLowerTable[exponent];
			}

			return new string(buffy, 0, ++idx); 
		}

		// ============ Format Fixed Points ============ //

		//
		// Used for strings in the following form "[-]M.DD...D"
		// At least one non-zero digit precedes the '.', w/ a 
		// '-' before that if negative. Precision specifies number
		// of decimal places 'D' to go. If not given, use
		// NumberFormatInfo.NumbeDecimalDigits. Results are rounded
		// if necessary. 
		//
		// Fairly simple implementation for integral types. Going
		// from right to left, fill up precision number of 0's,
		// plop a . down, then go for our number. 
		//

		private static string FormatFixedPoint (byte value, int precision, NumberFormatInfo nfi)
		{
			return FormatFixedPoint ((uint)value, precision, nfi);
		}

		private static string FormatFixedPoint (short value, int precision, NumberFormatInfo nfi)
		{
			return FormatFixedPoint ((int)value, precision, nfi);
		}

		private static string FormatFixedPoint (int value, int precision, NumberFormatInfo nfi)
		{
			int padding = (precision >= 0) ? (precision + maxIntLength) : (nfi.NumberDecimalDigits + maxIntLength);
			char[] buffy = new char[padding];
			int position = padding;
			bool negative = (value < 0);
			
			// fill up w/ precision # of 0's
			while (position > (maxIntLength - 1)) 
				buffy[--position] = '0';

			if (precision != 0)
				buffy[position--] = '.';

			if (negative)
				if (value <= -10) {
					buffy[position--] = digitLowerTable[-(value % 10)];
					value = value / -10;
				} else value = -value;
			
			// fill up w/ the value
			while (value >= 10) {
				buffy[position--] = digitLowerTable[(value % 10)];
				value = value / 10;
			}

			buffy[position] = digitLowerTable[value];

			if (negative) 
				buffy[--position] = '-';
			
			return new string (buffy, position, (padding - position));
		}

		private static string FormatFixedPoint (long value, int precision, NumberFormatInfo nfi)
		{
			int padding = (precision >= 0) ? (precision + maxLongLength) : (nfi.NumberDecimalDigits + maxLongLength);
			char[] buffy = new char[padding];
			int position = padding;
			bool negative = (value < 0);
			
			// fill up w/ precision # of 0's
			while (position > (maxLongLength - 1)) 
				buffy[--position] = '0';

			if (precision != 0)
				buffy[position--] = '.';

			if (negative)
				if (value <= -10) {
					buffy[position--] = digitLowerTable[-(value % 10)];
					value = value / -10;
				} else value = -value;
			
			// fill up w/ the value
			while (value >= 10) {
				buffy[position--] = digitLowerTable[(value % 10)];
				value = value / 10;
			}

			buffy[position] = digitLowerTable[value];

			if (negative) 
				buffy[--position] = '-';
			
			return new string (buffy, position, (padding - position));
		}

		private static string FormatFixedPoint (sbyte value, int precision, NumberFormatInfo nfi)
		{
			return FormatFixedPoint ((int)value, precision, nfi);
		}

		private static string FormatFixedPoint (ushort value, int precision, NumberFormatInfo nfi)
		{
			return FormatFixedPoint ((uint)value, precision, nfi);
		}

		private static string FormatFixedPoint (uint value, int precision, NumberFormatInfo nfi)
		{
			int padding = (precision >= 0) ? (precision + maxIntLength) : (nfi.NumberDecimalDigits + maxIntLength);
			char[] buffy = new char[padding];
			int position = padding;

			// fill up w/ precision # of 0's
			while (position > (maxIntLength - 1)) 
				buffy[--position] = '0';

			if (precision != 0)
				buffy[position--] = '.';

			// fill up w/ the value
			while (value >= 10) {
				buffy[position--] = digitLowerTable[(value % 10)];
				value = value / 10;
			}

			buffy[position] = digitLowerTable[value];
			
			return new string (buffy, position, (padding - position));
		}

		private static string FormatFixedPoint (ulong value, int precision, NumberFormatInfo nfi)
		{
			int padding = (precision >= 0) ? (precision + maxLongLength) : (nfi.NumberDecimalDigits + maxLongLength);
			char[] buffy = new char[padding];
			int position = padding;

			// fill up w/ precision # of 0's
			while (position > (maxLongLength - 1)) 
				buffy[--position] = '0';

			if (precision != 0)
				buffy[position--] = '.';

			// fill up w/ the value
			while (value >= 10) {
				buffy[position--] = digitLowerTable[(value % 10)];
				value = value / 10;
			}

			buffy[position] = digitLowerTable[value];
			
			return new string (buffy, position, (padding - position));
		}

		// ============ Format General ============ //
		
		//
		// Strings are formatted in either Fixed Point or Exponential
		// format. Results are rounded when needed. If no precision is
		// given, the defaults are:
		//
		// short & ushort: 5
		// int & uint: 10
		// long & ulong: 19
		// float: 7
		// double: 15
		// decimal: 29
		//
		// The value is formatted using fixed-point if exponent >= -4
		// and exponent < precision, where exponent is he exponenent of
		// the value in exponential format. The decimal point and trailing
		// zeros are removed when possible.
		//
		// For all other values, exponential format is used. The case of
		// the format specifier determines whether 'e' or 'E' prefixes
		// the exponent.
		// 
		// In either case, the number of digits that appear in the result
		// (not including the exponent) will not exceed the value of the
		// precision. The result is rounded as needed.
		//
		// Integral values are formatted using Fixed Point whenever
		// precision is omitted. (This actually doesn't make sense when
		// coupled w/ the 1st paragraph).
		//		
		// Okay, so the decimal point is removed along with any trailing
		// zeros. So, ignoring the last paragraph, we can consider an int
		// ToString() to format it w/ exponential format w/ a default
		// precision of 10, but since it will just be .00000000, it's
		// discarded.
		//

		private static string FormatGeneral (byte value, int precision, NumberFormatInfo nfi, bool upper) {
			return FormatGeneral ((uint)value, precision, nfi, upper);
		}

		private static string FormatGeneral (short value, int precision, NumberFormatInfo nfi, bool upper) {
			return FormatGeneral ((int)value, precision, nfi, upper);
		}

		private static string FormatGeneral (int value, int precision, NumberFormatInfo nfi, bool upper) 
		{
			bool negative = (value < 0);
			char[] tmp = new char [maxIntLength];
			int exponent = 0;
			int position = maxIntLength;
			
			// ugly, but doing it since abs(Int32.MinValue) > Int.MaxValue
			uint number = (negative) ? (uint)((-(value + 1)) + 1) : (uint)value;

			// get number into a buffer, going to be doing this no matter what
			if (negative)
				if (value <= -10) {
					tmp[--position] = digitLowerTable[-(value % 10)];
					value /= -10;
					exponent++;
				} else value = -value;

			while (value >= 10) {
				tmp[--position] = digitLowerTable[(value % 10)];
				value /= 10;
				exponent++;
			}
			
			tmp[--position] = digitLowerTable[value];

			// integral values are formatted using fixed point when precision
			// is not specified. But also trailing decimal point and zeros are
			// discared. So for int's it will always be .00, so just compute
			// here and save the call to FormatFixedPoint & trim.
			if (precision <= 0 || exponent < precision) {
				if (negative) 
					tmp[--position] = '-';
				
				return new string (tmp, position, (maxIntLength - position)); 
			}

			// else our exponent was > precision, use exponential format
			// precision = number of digits to show. 
			int idx = 0, pow = 1;

			exponent = 0;
			position = maxIntLength;
			
			// Loop through while our number is less than the 10 ^ precision, then
			// add 5 to that to round it out, and keep continuing
			while (idx++ <= precision)
				pow *= 10;
			
			while (number > pow) {
				tmp[--position] = digitLowerTable[(number % 10)];
				number /= 10;
				exponent++;
			}

			number += 5;

			while (number >= 10) {
				tmp[--position] = digitLowerTable[(number % 10)];
				number /= 10;
				exponent++;
			}

			tmp[--position] = digitLowerTable[number];

			// finally, make our final buffer, at least precision + 6 for 'E+XX' and '-'
			// and reuse pow for size
			idx = position;
			position = 0;
			pow = precision + 6;
			char[] buffy = new char[pow];

			if (negative)
				buffy[position++] = '-';
			
			buffy[position++] = tmp[idx++];
			buffy[position] = '.';

			// for the remaining precisions copy over rounded tmp
			precision--;
			while (precision-- > 0)
				buffy[++position] = tmp[idx++];

			// get rid of ending zeros
			while (buffy[position] == '0')
				position--;

			// if we backed up all the way to the ., over write it
			if (buffy[position] != '.')
				position++;			

			// ints can only be +, e or E depending on format, plus XX
			buffy[position++] = upper ? 'E' : 'e';
			buffy[position++] = '+';

			if (exponent >= 10) {
				buffy[position++] = digitLowerTable[(exponent / 10)];
				buffy[position++] = digitLowerTable[(exponent % 10)];
			} else { 
				buffy[position++] = '0';
				buffy[position++] = digitLowerTable[exponent];
			}
			
			return new string (buffy, 0, position);
		}

		private static string FormatGeneral (long value, int precision, NumberFormatInfo nfi, bool upper) 
		{
			bool negative = (value < 0);
			char[] tmp = new char [maxLongLength];
			int exponent = 0;
			int position = maxLongLength;

			// ugly, but doing it since abs(Int32.MinValue) > Int.MaxValue
			ulong number = (negative) ? (ulong)(-(value + 1) + 1) : (ulong)value;

			// get number into a buffer, going to be doing this no matter what
			if (negative)
				if (value <= -10) {
					tmp[--position] = digitLowerTable[-(value % 10)];
					value /= -10;
				} else value = -value;

			while (value >= 10) {
				tmp[--position] = digitLowerTable[(value % 10)];
				value /= 10;
			}
			
			tmp[--position] = digitLowerTable[value];
			exponent = (maxLongLength - position) - 1;

			// integral values are formatted using fixed point when precision
			// is not specified. But also trailing decimal point and zeros are
			// discared. So for int's it will always be .00, so just compute
			// here and save the call to FormatFixedPoint & trim.
			if (precision <= 0 || exponent < precision) {
				if (negative) 
					tmp[--position] = '-';
				
				return new string (tmp, position, (maxLongLength - position)); 
			}

			// else our exponent was > precision, use exponential format
			// precision = number of digits to show. 
			int idx = 0;
			ulong pow = 1;

			exponent = 0;
			position = maxLongLength;

			// Loop through while our number is less than the 10 ^ precision, then
			// add 5 to that to round it out, and keep continuing
			while (idx++ <= precision)
				pow *= 10;
			
			while (number > pow) {
				tmp[--position] = digitLowerTable[(number % 10)];
				number /= 10;
				exponent++;
			}

			number += 5;

			while (number >= 10) {
				tmp[--position] = digitLowerTable[(number % 10)];
				number /= 10;
				exponent++;
			}

			tmp[--position] = digitLowerTable[number];

			// finally, make our final buffer, at least precision + 6 for 'E+XX' and '-'
			// and reuse pow for size
			idx = position;
			position = 0;
			pow = (ulong)precision + 6;
			char[] buffy = new char[pow];

			if (negative)
				buffy[position++] = '-';
			
			buffy[position++] = tmp[idx++];
			buffy[position] = '.';

			// for the remaining precisions copy over rounded tmp
			precision--;
			while (precision-- > 0)
				buffy[++position] = tmp[idx++];

			// get rid of ending zeros
			while (buffy[position] == '0')
				position--;

			// if we backed up all the way to the ., over write it
			if (buffy[position] != '.')
				position++;			

			// ints can only be +, e or E depending on format, plus XX
			buffy[position++] = upper ? 'E' : 'e';
			buffy[position++] = '+';

			if (exponent >= 10) {
				buffy[position++] = digitLowerTable[(exponent / 10)];
				buffy[position++] = digitLowerTable[(exponent % 10)];
			} else { 
				buffy[position++] = '0';
				buffy[position++] = digitLowerTable[exponent];
			}
			
			return new string (buffy, 0, position);
		}

		private static string FormatGeneral (sbyte value, int precision, NumberFormatInfo nfi, bool upper) {
			return FormatGeneral ((int)value, precision, nfi, upper);
		}

		private static string FormatGeneral (ushort value, int precision, NumberFormatInfo nfi, bool upper) {
			return FormatGeneral ((uint)value, precision, nfi, upper);
		}

		private static string FormatGeneral (uint value, int precision, NumberFormatInfo nfi, bool upper) 
		{
			char[] tmp = new char [maxIntLength];
			int exponent = 0;
			int position = maxIntLength;
			uint number = value;

			// get number into a buffer, going to be doing this no matter what
			while (value >= 10) {
				tmp[--position] = digitLowerTable[(value % 10)];
				value /= 10;
			}
			
			tmp[--position] = digitLowerTable[value];
			exponent = (maxIntLength - position) - 1;

			// integral values are formatted using fixed point when precision
			// is not specified. But also trailing decimal point and zeros are
			// discared. So for int's it will always be .00, so just compute
			// here and save the call to FormatFixedPoint & trim.
			if (precision <= 0 || exponent < precision) 
				return new string (tmp, position, (maxIntLength - position)); 

			// else our exponent was > precision, use exponential format
			// precision = number of digits to show. 
			int idx = 0, pow = 1;

			exponent = 0;
			position = maxIntLength;
						
			// Loop through while our number is less than the 10 ^ precision, then
			// add 5 to that to round it out, and keep continuing
			while (idx++ <= precision)
				pow *= 10;

			while (number > pow) {
				tmp[--position] = digitLowerTable[(number % 10)];
				number /= 10;
				exponent++;
			}

			number += 5;

			while (number >= 10) {
				tmp[--position] = digitLowerTable[(number % 10)];
				number /= 10;
				exponent++;
			}

			tmp[--position] = digitLowerTable[number]; 	

			// finally, make our final buffer, at least precision + 6 for 'E+XX' and '-'
			// and reuse pow for size
			idx = position;
			position = 0;
			pow = precision + 6;
			char[] buffy = new char[pow];

			buffy[position++] = tmp[idx++];
			buffy[position] = '.';

			// for the remaining precisions copy over rounded tmp
			precision--;
			while (precision-- > 0)
				buffy[++position] = tmp[idx++];

			// get rid of ending zeros
			while (buffy[position] == '0')
				position--;

			// if we backed up all the way to the ., over write it
			if (buffy[position] != '.')
				position++;			

			// ints can only be +, e or E depending on format, plus XX
			buffy[position++] = upper ? 'E' : 'e';
			buffy[position++] = '+';

			if (exponent >= 10) {
				buffy[position++] = digitLowerTable[(exponent / 10)];
				buffy[position++] = digitLowerTable[(exponent % 10)];
			} else { 
				buffy[position++] = '0';
				buffy[position++] = digitLowerTable[exponent];
			}
			
			return new string (buffy, 0, position);
		}

		private static string FormatGeneral (ulong value, int precision, NumberFormatInfo nfi, bool upper) 
		{
			char[] tmp = new char [maxLongLength];
			int exponent = 0;
			int position = maxLongLength;
			ulong number = value;

			// get number into a buffer, going to be doing this no matter what
			while (value >= 10) {
				tmp[--position] = digitLowerTable[(value % 10)];
				value /= 10;
				exponent++;
			}
			
			tmp[--position] = digitLowerTable[value];

			// integral values are formatted using fixed point when precision
			// is not specified. But also trailing decimal point and zeros are
			// discared. So for int's it will always be .00, so just compute
			// here and save the call to FormatFixedPoint & trim.
			if (precision <= 0 || exponent < precision) 
				return new string (tmp, position, (maxLongLength - position)); 

			// else our exponent was > precision, use exponential format
			// precision = number of digits to show. 
			int idx = 0;
			ulong pow = 1;

			exponent = 0;
			position = maxLongLength;

			// Loop through while our number is less than the 10 ^ precision, then
			// add 5 to that to round it out, and keep continuing
			while (idx++ <= precision)
				pow *= 10;
			
			while (number > pow) {
				tmp[--position] = digitLowerTable[(number % 10)];
				number /= 10;
				exponent++;
			}

			number += 5;

			while (number >= 10) {
				tmp[--position] = digitLowerTable[(number % 10)];
				number /= 10;
				exponent++;
			}

			tmp[--position] = digitLowerTable[number];

			// finally, make our final buffer, at least precision + 6 for 'E+XX' and '-'
			// and reuse pow for size
			idx = position;
			position = 0;
			pow = (ulong)precision + 6;
			char[] buffy = new char[pow];

			buffy[position++] = tmp[idx++];
			buffy[position] = '.';

			// for the remaining precisions copy over rounded tmp
			precision--;
			while (precision-- > 0)
				buffy[++position] = tmp[idx++];

			// get rid of ending zeros
			while (buffy[position] == '0')
				position--;

			// if we backed up all the way to the ., over write it
			if (buffy[position] != '.')
				position++;			

			// ints can only be +, e or E depending on format, plus XX
			buffy[position++] = upper ? 'E' : 'e';
			buffy[position++] = '+';

			if (exponent >= 10) {
				buffy[position++] = digitLowerTable[(exponent / 10)];
				buffy[position++] = digitLowerTable[(exponent % 10)];
			} else { 
				buffy[position++] = '0';
				buffy[position++] = digitLowerTable[exponent];
			}
			
			return new string (buffy, 0, position);
		}

		// ============ Format Number ============ //

		// 
		// Used for strings in the following form "[-]d,ddd,ddd.dd...d"
		// The minus sign only appears if it is negative. At least one
		// non-zero digit preceeds the decimal separator. The precision
		// specifier determines the number of decimal places. If it is 
		// not given, use NumberFormatInfo.NumberDecimalDigits.
		// The NumberGroupSizes, NumberGroupSeparator, and NumberDecimalSeparator
		// members of NumberFormatInfo supply the size and separator
		// for digit groupings. See IFormattable.
		//
		// The group sizes is an array of ints that determine the grouping
		// of numbers. All digits are in the range 1-9, with the last digit
		// being between 0-9. The number formats the string backwards, with
		// the last digit being the group size for the rest of (leftmost) the
		// the string, 0 being none.
		//
		// For instance:
		//		groupSizes = { 3, 2, 1, 0 }; 
		//		int n = 1234567890 => "1234,5,67,890"
		//		groupSizes = { 3, 2, 1 }; 
		//		int n = 1234567890 => "1,2,3,4,5,67,890"
		//		groupSizes = { 2, 0 };
		//		int n = 1234567890 => "1234567,90";
		//
		// Not too difficult, jsut keep track of where you are in the array
		// and when to print the separator
		//
		// The max size of the buffer is assume we have a separator every 
		// number, plus the precision on the end, plus a spot for the negative
		// and a spot for decimal separator.
		//

		private static string FormatNumber (byte value, int precision, NumberFormatInfo nfi) {
			return FormatNumber ((uint)value, precision, nfi);
		}

		private static string FormatNumber (short value, int precision, NumberFormatInfo nfi) {
			return FormatNumber ((int)value, precision, nfi);
		}

		private static string FormatNumber (int value, int precision, NumberFormatInfo nfi) 
		{
			int i, j, k;
			char[] groupSeparator = nfi.NumberGroupSeparator.ToCharArray ();
			char[] decimalSeparator = nfi.NumberDecimalSeparator.ToCharArray ();
			int[] groupSizes = nfi.NumberGroupSizes;

			int padding = (precision >= 0) ? precision : nfi.NumberDecimalDigits;
			int size = maxIntLength + (maxIntLength * groupSeparator.Length) + padding +
			decimalSeparator.Length + 2;
			char[] buffy = new char[size];
			int position = size;
			bool negative = (value < 0);
			
			// right pad it w/ precision 0's
			while (padding-- > 0)
				buffy[--position] = '0';

			// put on decimal separator
			if (position != size) {
				i = decimalSeparator.Length; 
				do {
					buffy[--position] = decimalSeparator[--i];
				} while (i > 0);			
			}

			// loop through, keeping track of where you are in the
			// group sizes array and putting out the group separator
			// when needed
			j = 0;
			k = groupSizes[j++];

			// negative hack for numbers past MinValue
			if (negative)
				if (value <= -10) {
					buffy[--position] = digitLowerTable[-(value % 10)];
					value = value / -10;
					
					if (--k == 0) {
						i = groupSeparator.Length; 
						do {
							buffy[--position] = groupSeparator[--i];
						} while (i > 0);
						
						k = (j < groupSizes.Length) ? 
						groupSizes[j++] : groupSizes[(groupSizes.Length - 1)];
					}
				} else value = -value;

			while (value >= 10) {
				buffy[--position] = digitLowerTable[(value % 10)];
				value /= 10;

				if (--k == 0) {
					i = groupSeparator.Length; 
					do {
						buffy[--position] = groupSeparator[--i];
					} while (i > 0);
					
					k = (j < groupSizes.Length) ? 
					groupSizes[j++] : groupSizes[(groupSizes.Length - 1)];
				}
			}		 

			buffy[--position] = digitLowerTable[value];

			if (negative)
				buffy[--position] = '-';
			
			return new string (buffy, position, (size - position));
		}

		private static string FormatNumber (long value, int precision, NumberFormatInfo nfi) 
		{
			int i, j, k;
			char[] groupSeparator = nfi.NumberGroupSeparator.ToCharArray ();
			char[] decimalSeparator = nfi.NumberDecimalSeparator.ToCharArray ();
			int[] groupSizes = nfi.NumberGroupSizes;

			int padding = (precision >= 0) ? precision : nfi.NumberDecimalDigits;
			int size = maxLongLength + (maxLongLength * groupSeparator.Length) + padding +
			decimalSeparator.Length + 2;
			char[] buffy = new char[size];
			int position = size;
			bool negative = (value < 0);
			
			// right pad it w/ precision 0's
			while (padding-- > 0)
				buffy[--position] = '0';
						
			// put on decimal separator
			if (position != size) {
				i = decimalSeparator.Length; 
				do {
					buffy[--position] = decimalSeparator[--i];
				} while (i > 0);			
			}

			// loop through, keeping track of where you are in the
			// group sizes array and putting out the group separator
			// when needed
			j = 0;
			k = groupSizes[j++];

			// negative hack for numbers past MinValue
			if (negative)
				if (value <= -10) {
					buffy[--position] = digitLowerTable[-(value % 10)];
					value = value / -10;
					
					if (--k == 0) {
						i = groupSeparator.Length; 
						do {
							buffy[--position] = groupSeparator[--i];
						} while (i > 0);
						
						k = (j < groupSizes.Length) ? 
						groupSizes[j++] : groupSizes[(groupSizes.Length - 1)];
					}
				} else value = -value;

			while (value >= 10) {
				buffy[--position] = digitLowerTable[(value % 10)];
				value /= 10;

				if (--k == 0) {
					i = groupSeparator.Length; 
					do {
						buffy[--position] = groupSeparator[--i];
					} while (i > 0);
					
					k = (j < groupSizes.Length) ? 
					groupSizes[j++] : groupSizes[(groupSizes.Length - 1)];
				}
			}		 

			buffy[--position] = digitLowerTable[value];

			if (negative)
				buffy[--position] = '-';
			
			return new string (buffy, position, (size - position));
		}

		private static string FormatNumber (sbyte value, int precision, NumberFormatInfo nfi) {
			return FormatNumber ((int)value, precision, nfi);
		}

		private static string FormatNumber (ushort value, int precision, NumberFormatInfo nfi) {
			return FormatNumber ((uint)value, precision, nfi);
		}

		private static string FormatNumber (uint value, int precision, NumberFormatInfo nfi) 
		{
			int i, j, k;
			char[] groupSeparator = nfi.NumberGroupSeparator.ToCharArray ();
			char[] decimalSeparator = nfi.NumberDecimalSeparator.ToCharArray ();
			int[] groupSizes = nfi.NumberGroupSizes;

			int padding = (precision >= 0) ? precision : nfi.NumberDecimalDigits;
			int size = maxIntLength + (maxIntLength * groupSeparator.Length) + padding +
			decimalSeparator.Length + 2;
			char[] buffy = new char[size];
			int position = size;
			
			// right pad it w/ precision 0's
			while (padding-- > 0)
				buffy[--position] = '0';
						
			// put on decimal separator
			if (position != size) {
				i = decimalSeparator.Length; 
				do {
					buffy[--position] = decimalSeparator[--i];
				} while (i > 0);			
			}

			// loop through, keeping track of where you are in the
			// group sizes array and putting out the group separator
			// when needed
			j = 0;
			k = groupSizes[j++];

			while (value >= 10) {
				buffy[--position] = digitLowerTable[(value % 10)];
				value /= 10;

				if (--k == 0) {
					i = groupSeparator.Length; 
					do {
						buffy[--position] = groupSeparator[--i];
					} while (i > 0);
					
					k = (j < groupSizes.Length) ? 
					groupSizes[j++] : groupSizes[(groupSizes.Length - 1)];
				}
			}		 

			buffy[--position] = digitLowerTable[value];

			return new string (buffy, position, (size - position));
		}

		private static string FormatNumber (ulong value, int precision, NumberFormatInfo nfi) 
		{
			int i, j, k;
			char[] groupSeparator = nfi.NumberGroupSeparator.ToCharArray ();
			char[] decimalSeparator = nfi.NumberDecimalSeparator.ToCharArray ();
			int[] groupSizes = nfi.NumberGroupSizes;

			int padding = (precision >= 0) ? precision : nfi.NumberDecimalDigits;
			int size = maxLongLength + (maxLongLength * groupSeparator.Length) + padding +
			decimalSeparator.Length + 2;
			char[] buffy = new char[size];
			int position = size;
			
			// right pad it w/ precision 0's
			while (padding-- > 0)
				buffy[--position] = '0';
			
			// put on decimal separator
			if (position != size) {
				i = decimalSeparator.Length; 
				do {
					buffy[--position] = decimalSeparator[--i];
				} while (i > 0);			
			}

			// loop through, keeping track of where you are in the
			// group sizes array and putting out the group separator
			// when needed
			j = 0;
			k = groupSizes[j++];

			while (value >= 10) {
				buffy[--position] = digitLowerTable[(value % 10)];
				value /= 10;

				if (--k == 0) {
					i = groupSeparator.Length; 
					do {
						buffy[--position] = groupSeparator[--i];
					} while (i > 0);
					
					k = (j < groupSizes.Length) ? 
					groupSizes[j++] : groupSizes[(groupSizes.Length - 1)];
				}
			}		 

			buffy[--position] = digitLowerTable[value];

			return new string (buffy, position, (size - position));
		}		

		// ============ Percent Formatting ============ //

		//
		//  Percent Format: Used for strings containing a percentage. The
		//  PercentSymbol, PercentGroupSizes, PercentGroupSeparator, and
		//  PercentDecimalSeparator members of a NumberFormatInfo supply
		//  the Percent symbol, size and separator for digit groupings, and
		//  decimal separator, respectively.
		//  PercentNegativePattern and PercentPositivePattern determine the
		//  symbols used to represent negative and positive values. For example,
		//  a negative value may be prefixed with a minus sign, or enclosed in
		//  parentheses.
		//  If no precision is specified, the number of decimal places in the result
		//  is set by NumberFormatInfo.PercentDecimalDigits. Results are
		//  rounded to the nearest representable value when necessary.
		//  The result is scaled by 100 (.99 becomes 99%).
		//
		//  The pattern of the number determines how the output looks, where
		//  the percent sign goes, where the negative sign goes, etc.
		//  IFormattable documentation lists the patterns and their values,
		//  I have them commented out in the switch statement
		//

		private static string FormatPercent (byte value, int precision, NumberFormatInfo nfi) 
		{
			return FormatPercent ((uint)value, precision, nfi);
		}

		private static string FormatPercent (short value, int precision, NumberFormatInfo nfi) 
		{
			return FormatPercent ((int)value, precision, nfi);
		}

		private static string FormatPercent (int value, int precision, NumberFormatInfo nfi) 
		{
			int i, j, k;
			bool negative = (value < 0);

			char[] groupSeparator = nfi.PercentGroupSeparator.ToCharArray ();
			char[] decimalSeparator = nfi.PercentDecimalSeparator.ToCharArray ();
			char[] percentSymbol = nfi.PercentSymbol.ToCharArray ();
			int[] groupSizes = nfi.PercentGroupSizes;
			int pattern = negative ? nfi.PercentNegativePattern : nfi.PercentPositivePattern;
			int symbolLength = percentSymbol.Length;
			
			int padding = (precision >= 0) ? precision : nfi.PercentDecimalDigits;	     
			int size = maxIntLength + (groupSeparator.Length * maxIntLength) + padding + 2 + 
			decimalSeparator.Length + symbolLength;	
			char[] buffy = new char[size];
			int position = size;

			// set up the pattern from IFormattible
			if (negative) {
				i = symbolLength; 
 
				switch (pattern) {
				case 0: // -nnn %
					do {
						buffy[--position] = percentSymbol[--i];
					} while (i > 0);
					buffy[--position] = ' '; 
					break;
				case 1: // -nnn%
					do {
						buffy[--position] = percentSymbol[--i];
					} while (i > 0);
					break;
				// case 2: // -%nnn
				//	break;
				}
			} else {
				i = symbolLength; 
				switch (pattern) {
				case 0: // nnn %
					do {
						buffy[--position] = percentSymbol[--i];
					} while (i > 0);
					buffy[--position] = ' ';					
					break;
				case 1: // nnn%
					do {
						buffy[--position] = percentSymbol[--i];
					} while (i > 0);
					break;
				// case 2: // %nnn
				//	break;
				}
			}
			
			// right pad it w/ precision 0's
			while (padding-- > 0)
				buffy[--position] = '0';

			// put on decimal separator if we moved over and put a 0 
			if (position < size && buffy[position] == '0') {
				i = decimalSeparator.Length; 
				do {
					buffy[--position] = decimalSeparator[--i];
				} while (i > 0);			
			}

			// loop through, keeping track of where you are in the
			// group sizes array and putting out the group separator
			// when needed
			j = 0;
			k = groupSizes[j++];

			// all values are multiplied by 100, so tack on two 0's
			if (value != 0) 
				for (int c = 0; c < 2; c++) {
					buffy[--position] = '0';
					
					if (--k == 0) {
						i = groupSeparator.Length; 
						do {
							buffy[--position] = groupSeparator[--i];
						} while (i > 0);
						
						k = (j < groupSizes.Length) ? 
						groupSizes[j++] : groupSizes[(groupSizes.Length - 1)];
					}
				}

			// negative hack for numbers past MinValue
			if (negative)
				if (value <= -10) {
					buffy[--position] = digitLowerTable[-(value % 10)];
					value = value / -10;
					
					if (--k == 0) {
						i = groupSeparator.Length; 
						do {
							buffy[--position] = groupSeparator[--i];
						} while (i > 0);
						
						k = (j < groupSizes.Length) ? 
						groupSizes[j++] : groupSizes[(groupSizes.Length - 1)];
					}
				} else value = -value;

			while (value >= 10) {
				buffy[--position] = digitLowerTable[(value % 10)];
				value /= 10;

				if (--k == 0) {
					i = groupSeparator.Length; 
					do {
						buffy[--position] = groupSeparator[--i];
					} while (i > 0);
					
					k = (j < groupSizes.Length) ? 
					groupSizes[j++] : groupSizes[(groupSizes.Length - 1)];
				}
			}		 

			buffy[--position] = digitLowerTable[value];

			// end the pattern on the left hand side
			if (negative) {
				i = symbolLength; 
 
				switch (pattern) {
				case 0: // -nnn %
					buffy[--position] = '-'; 
					break;
				case 1: // -nnn%
					buffy[--position] = '-'; 
					break;
				case 2: // -%nnn
					do {
						buffy[--position] = percentSymbol[--i];
					} while (i > 0);
					buffy[--position] = '-'; 
					break;
				}
			} else {
				i = symbolLength; 
				switch (pattern) {
				// case 0: // nnn %
				//	break;
				// case 1: // nnn%
				//	break;
				case 2: // %nnn
					do {
						buffy[--position] = percentSymbol[--i];
					} while (i > 0);
					break;
				}
			}
			
			return new string (buffy, position, (size - position));
		}

		private static string FormatPercent (long value, int precision, NumberFormatInfo nfi) 
		{
			int i, j, k;
			bool negative = (value < 0);

			char[] groupSeparator = nfi.PercentGroupSeparator.ToCharArray ();
			char[] decimalSeparator = nfi.PercentDecimalSeparator.ToCharArray ();
			char[] percentSymbol = nfi.PercentSymbol.ToCharArray ();
			int[] groupSizes = nfi.PercentGroupSizes;
			int pattern = negative ? nfi.PercentNegativePattern : nfi.PercentPositivePattern;
			int symbolLength = percentSymbol.Length;
			
			int padding = (precision >= 0) ? precision : nfi.PercentDecimalDigits;	     
			int size = maxLongLength + (groupSeparator.Length * maxLongLength) + padding + 2 + 
			decimalSeparator.Length + symbolLength;	
			char[] buffy = new char[size];
			int position = size;

			// set up the pattern from IFormattible
			if (negative) {
				i = symbolLength; 
 
				switch (pattern) {
				case 0: // -nnn %
					do {
						buffy[--position] = percentSymbol[--i];
					} while (i > 0);
					buffy[--position] = ' '; 
					break;
				case 1: // -nnn%
					do {
						buffy[--position] = percentSymbol[--i];
					} while (i > 0);
					break;
				// case 2: // -%nnn
				//	break;
				}
			} else {
				i = symbolLength; 
				switch (pattern) {
				case 0: // nnn %
					do {
						buffy[--position] = percentSymbol[--i];
					} while (i > 0);
					buffy[--position] = ' ';					
					break;
				case 1: // nnn%
					do {
						buffy[--position] = percentSymbol[--i];
					} while (i > 0);
					break;
				// case 2: // %nnn
				//	break;
				}
			}
			
			// right pad it w/ precision 0's
			while (padding-- > 0)
				buffy[--position] = '0';

			// put on decimal separator if we moved over and put a 0 
			if (position < size && buffy[position] == '0') {
				i = decimalSeparator.Length; 
				do {
					buffy[--position] = decimalSeparator[--i];
				} while (i > 0);			
			}

			// loop through, keeping track of where you are in the
			// group sizes array and putting out the group separator
			// when needed
			j = 0;
			k = groupSizes[j++];

			// all values are multiplied by 100, so tack on two 0's
			if (value != 0) 
				for (int c = 0; c < 2; c++) {
					buffy[--position] = '0';
					
					if (--k == 0) {
						i = groupSeparator.Length; 
						do {
							buffy[--position] = groupSeparator[--i];
						} while (i > 0);
						
						k = (j < groupSizes.Length) ? 
						groupSizes[j++] : groupSizes[(groupSizes.Length - 1)];
					}
				}

			// negative hack for numbers past MinValue
			if (negative)
				if (value <= -10) {
					buffy[--position] = digitLowerTable[-(value % 10)];
					value = value / -10;
					
					if (--k == 0) {
						i = groupSeparator.Length; 
						do {
							buffy[--position] = groupSeparator[--i];
						} while (i > 0);
						
						k = (j < groupSizes.Length) ? 
						groupSizes[j++] : groupSizes[(groupSizes.Length - 1)];
					}
				} else value = -value;

			while (value >= 10) {
				buffy[--position] = digitLowerTable[(value % 10)];
				value /= 10;

				if (--k == 0) {
					i = groupSeparator.Length; 
					do {
						buffy[--position] = groupSeparator[--i];
					} while (i > 0);
					
					k = (j < groupSizes.Length) ? 
					groupSizes[j++] : groupSizes[(groupSizes.Length - 1)];
				}
			}		 

			buffy[--position] = digitLowerTable[value];

			// end the pattern on the left hand side
			if (negative) {
				i = symbolLength; 
 
				switch (pattern) {
				case 0: // -nnn %
					buffy[--position] = '-'; 
					break;
				case 1: // -nnn%
					buffy[--position] = '-'; 
					break;
				case 2: // -%nnn
					do {
						buffy[--position] = percentSymbol[--i];
					} while (i > 0);
					buffy[--position] = '-'; 
					break;
				}
			} else {
				i = symbolLength; 
				switch (pattern) {
				// case 0: // nnn %
				//	break;
				// case 1: // nnn%
				//	break;
				case 2: // %nnn
					do {
						buffy[--position] = percentSymbol[--i];
					} while (i > 0);
					break;
				}
			}
			
			return new string (buffy, position, (size - position));
		}

		private static string FormatPercent (sbyte value, int precision, NumberFormatInfo nfi) 
		{
			return FormatPercent ((int)value, precision, nfi);
		}

		private static string FormatPercent (ushort value, int precision, NumberFormatInfo nfi) 
		{
			return FormatPercent ((uint)value, precision, nfi);
		}

		private static string FormatPercent (uint value, int precision, NumberFormatInfo nfi) 
		{
			int i, j, k;

			char[] groupSeparator = nfi.PercentGroupSeparator.ToCharArray ();
			char[] decimalSeparator = nfi.PercentDecimalSeparator.ToCharArray ();
			char[] percentSymbol = nfi.PercentSymbol.ToCharArray ();
			int[] groupSizes = nfi.PercentGroupSizes;
			int pattern = nfi.PercentPositivePattern;
			int symbolLength = percentSymbol.Length;
			
			int padding = (precision >= 0) ? precision : nfi.PercentDecimalDigits;	     
			int size = maxIntLength + (groupSeparator.Length * maxIntLength) + padding + 2 + 
			decimalSeparator.Length + symbolLength;	
			char[] buffy = new char[size];
			int position = size;

			// set up the pattern from IFormattible
			i = symbolLength; 			
			switch (pattern) {
			case 0: // -nnn %
				do {
					buffy[--position] = percentSymbol[--i];
				} while (i > 0);
				buffy[--position] = ' '; 
				break;
			case 1: // -nnn%
				do {
					buffy[--position] = percentSymbol[--i];
				} while (i > 0);
				break;
			// case 2: // -%nnn
			//	break;
			}

			// right pad it w/ precision 0's
			while (padding-- > 0)
				buffy[--position] = '0';

			// put on decimal separator if we moved over and put a 0 
			if (position < size && buffy[position] == '0') {
				i = decimalSeparator.Length; 
				do {
					buffy[--position] = decimalSeparator[--i];
				} while (i > 0);			
			}

			// loop through, keeping track of where you are in the
			// group sizes array and putting out the group separator
			// when needed
			j = 0;
			k = groupSizes[j++];

			if (value != 0) 
				for (int c = 0; c < 2; c++) {
					buffy[--position] = '0';
					
					if (--k == 0) {
						i = groupSeparator.Length; 
						do {
							buffy[--position] = groupSeparator[--i];
						} while (i > 0);
						
						k = (j < groupSizes.Length) ? 
						groupSizes[j++] : groupSizes[(groupSizes.Length - 1)];
					}
				}

			while (value >= 10) {
				buffy[--position] = digitLowerTable[(value % 10)];
				value /= 10;

				if (--k == 0) {
					i = groupSeparator.Length; 
					do {
						buffy[--position] = groupSeparator[--i];
					} while (i > 0);
					
					k = (j < groupSizes.Length) ? 
					groupSizes[j++] : groupSizes[(groupSizes.Length - 1)];
				}
			}		 

			buffy[--position] = digitLowerTable[value];

			i = symbolLength; 
			switch (pattern) {
			// case 0: // nnn %
			//	break;
			// case 1: // nnn%
			//	break;
			case 2: // %nnn
				do {
					buffy[--position] = percentSymbol[--i];
				} while (i > 0);
				break;
			}
			
			return new string (buffy, position, (size - position));
		}

		private static string FormatPercent (ulong value, int precision, NumberFormatInfo nfi) 
		{
			int i, j, k;

			char[] groupSeparator = nfi.PercentGroupSeparator.ToCharArray ();
			char[] decimalSeparator = nfi.PercentDecimalSeparator.ToCharArray ();
			char[] percentSymbol = nfi.PercentSymbol.ToCharArray ();
			int[] groupSizes = nfi.PercentGroupSizes;
			int pattern = nfi.PercentPositivePattern;
			int symbolLength = percentSymbol.Length;
			
			int padding = (precision >= 0) ? precision : nfi.PercentDecimalDigits;	     
			int size = maxLongLength + (groupSeparator.Length * maxLongLength) + padding + 2 + 
			decimalSeparator.Length + symbolLength;	
			char[] buffy = new char[size];
			int position = size;

			// set up the pattern from IFormattible
			i = symbolLength; 			
			switch (pattern) {
			case 0: // -nnn %
				do {
					buffy[--position] = percentSymbol[--i];
				} while (i > 0);
				buffy[--position] = ' '; 
				break;
			case 1: // -nnn%
				do {
					buffy[--position] = percentSymbol[--i];
				} while (i > 0);
				break;
			// case 2: // -%nnn
			//	break;
			}

			// right pad it w/ precision 0's
			while (padding-- > 0)
				buffy[--position] = '0';

			// put on decimal separator if we moved over and put a 0 
			if (position < size && buffy[position] == '0') {
				i = decimalSeparator.Length; 
				do {
					buffy[--position] = decimalSeparator[--i];
				} while (i > 0);			
			}

			// loop through, keeping track of where you are in the
			// group sizes array and putting out the group separator
			// when needed
			j = 0;
			k = groupSizes[j++];

			if (value != 0) 
				for (int c = 0; c < 2; c++) {
					buffy[--position] = '0';
					
					if (--k == 0) {
						i = groupSeparator.Length; 
						do {
							buffy[--position] = groupSeparator[--i];
						} while (i > 0);
						
						k = (j < groupSizes.Length) ? 
						groupSizes[j++] : groupSizes[(groupSizes.Length - 1)];
					}
				}
			
			while (value >= 10) {
				buffy[--position] = digitLowerTable[(value % 10)];
				value /= 10;

				if (--k == 0) {
					i = groupSeparator.Length; 
					do {
						buffy[--position] = groupSeparator[--i];
					} while (i > 0);
					
					k = (j < groupSizes.Length) ? 
					groupSizes[j++] : groupSizes[(groupSizes.Length - 1)];
				}
			}		 

			buffy[--position] = digitLowerTable[value];

			i = symbolLength; 
			switch (pattern) {
			// case 0: // nnn %
			//	break;
			// case 1: // nnn%
			//	break;
			case 2: // %nnn
				do {
					buffy[--position] = percentSymbol[--i];
				} while (i > 0);
				break;
			}
			
			return new string (buffy, position, (size - position));
		}

		// ============ Format Hexadecimal ============ //

		// 
		// For strings in base 16. Only valid w/ integers. Precision 
		// specifies number of digits in the string, if it specifies
		// more digits than we need, left pad w/ 0's. The case of the
		// the format specifier 'X' or 'x' determines lowercase or
		// capital digits in the output.
		//
		// Whew. Straight forward Hex formatting, however only
		// go 8 places max when dealing with an int (not counting
		// precision padding) and 16 when dealing with a long. This
		// is to cut off the loop when dealing with negative values,
		// which will loop forever when you hit -1;
		//

		private static string FormatHexadecimal (byte value, int precision, bool upper)
		{		     
			if (precision < 0) precision = 0;
			int size = maxByteLength + precision;
			char[] buffy = new char[size];
			char[] table = upper ? digitUpperTable : digitLowerTable;
			int position = size;
			ushort mask = (1 << 4) - 1;

			// loop through right to left, shifting and looking up
			// our value. Don't worry about negative
			do {
				buffy[--position] = table[(value & mask)];
				value = (byte)(value >> 4);
			} while (value != 0);

			// pad w/ 0's if they want more length, if not, ignore
			precision -= (size - position);				
			while (precision > 0 && position > 1) {
				buffy[--position] = '0';
				precision--;
			}
			
			return new string(buffy, position, (size - position));
		}

		private static string FormatHexadecimal (short value, int precision, bool upper)
		{
			if (precision < 0) precision = 0;
			int size = maxShortLength + precision;
			char[] buffy = new char[size];
			char[] table = upper ? digitUpperTable : digitLowerTable;
			int position = size;
			short mask = (1 << 4) - 1;

			// loop through right to left, shifting and looking up
			// our value. If value is negavite stop after 4 F's
			do {
				buffy[--position] = table[(value & mask)];
				value = (short)(value >> 4);
			} while (value != 0 && position > (size - 4));

			// pad w/ 0's if they want more length, if not, ignore
			precision -= (size - position);				
			while (precision > 0 && position > 1) {
				buffy[--position] = '0';
				precision--;
			}
			
			return new string(buffy, position, (size - position));
		}

		private static string FormatHexadecimal (int value, int precision, bool upper)
		{
			if (precision < 0) precision = 0;
			int size = maxIntLength + precision;
			char[] buffy = new char[size];
			char[] table = upper ? digitUpperTable : digitLowerTable;
			int position = size;
			int mask = (1 << 4) - 1;

			// loop through right to left, shifting and looking up
			// our value. If value is negavite stop after 8 F's
			do {
				buffy[--position] = table[(value & mask)];
				value = value >> 4;
			} while (value != 0 && position > (size - 8));

			// pad w/ 0's if they want more length, if not, ignore
			precision -= (size - position);				
			while (precision > 0 && position > 1) {
				buffy[--position] = '0';
				precision--;
			}
			
			return new string(buffy, position, (size - position));
		}

		private static string FormatHexadecimal (long value, int precision, bool upper)
		{
			if (precision < 0) precision = 0;
			int size = maxLongLength + precision;
			char[] buffy = new char[size];
			char[] table = upper ? digitUpperTable : digitLowerTable;
			int position = size;
			long mask = (1 << 4) - 1;
			
			// loop through right to left, shifting and looking up
			// our value. If value is negavite stop after 16 F's
			do {
				buffy[--position] = table[(value & mask)];
				value = value >> 4;
			} while (value != 0 && position > (size - 16));

			// pad w/ 0's if they want more length, if not, ignore
			precision -= (size - position);				
			while (precision > 0 && position > 1) {
				buffy[--position] = '0';
				precision--;
			}
			
			return new string(buffy, position, (size - position));
		}

		private static string FormatHexadecimal (sbyte value, int precision, bool upper)
		{
			if (precision < 0) precision = 0;
			int size = maxByteLength + precision;
			char[] buffy = new char[size];
			char[] table = upper ? digitUpperTable : digitLowerTable;
			int position = size;
			short mask = (1 << 4) - 1;

			// loop through right to left, shifting and looking up
			// our value. If value is negavite stop after 2 F's
			do {
				buffy[--position] = table[(value & mask)];
				value = (sbyte)(value >> 4);
			} while (value != 0 && position > (size - 2));

			// pad w/ 0's if they want more length, if not, ignore
			precision -= (size - position);				
			while (precision > 0 && position > 1) {
				buffy[--position] = '0';
				precision--;
			}
			
			return new string(buffy, position, (size - position));
		}

		private static string FormatHexadecimal (ushort value, int precision, bool upper)
		{			
			if (precision < 0) precision = 0;
			int size = maxShortLength + precision;
			char[] buffy = new char[size];
			char[] table = upper ? digitUpperTable : digitLowerTable;
			int position = size;
			int mask = (1 << 4) - 1;

			// loop through right to left, shifting and looking up
			// our value. Don't worry about negative
			do {
				buffy[--position] = table[(value & mask)];
				value = (ushort)(value >> 4);
			} while (value != 0);

			// pad w/ 0's if they want more length, if not, ignore
			precision -= (size - position);				
			while (precision > 0 && position > 1) {
				buffy[--position] = '0';
				precision--;
			}
			
			return new string(buffy, position, (size - position));
		}

		private static string FormatHexadecimal (uint value, int precision, bool upper)
		{			
			if (precision < 0) precision = 0;
			int size = maxIntLength + precision;
			char[] buffy = new char[size];
			char[] table = upper ? digitUpperTable : digitLowerTable;
			int position = size;
			uint mask = (1 << 4) - 1;

			// loop through right to left, shifting and looking up
			// our value. Don't worry about negative
			do {
				buffy[--position] = table[(value & mask)];
				value = value >> 4;
			} while (value != 0);

			// pad w/ 0's if they want more length, if not, ignore
			precision -= (size - position);				
			while (precision > 0 && position > 1) {
				buffy[--position] = '0';
				precision--;
			}
			
			return new string(buffy, position, (size - position));
		}

		private static string FormatHexadecimal (ulong value, int precision, bool upper)
		{			
			if (precision < 0) precision = 0;
			int size = maxLongLength + precision;
			char[] buffy = new char[size];
			char[] table = upper ? digitUpperTable : digitLowerTable;
			int position = size;
			ulong mask = (1 << 4) - 1;
			
			// loop through right to left, shifting and looking up
			// our value. Don't worry about negative
			do {
				buffy[--position] = table[value & mask];
				value = value >> 4;
			} while (value != 0);
			
			// pad w/ 0's if they want more length, if not, ignore
			precision -= (size - position);
			while (precision > 0 && position > 1) {
				buffy[--position] = '0';
				precision--;
			}

			return new string(buffy, position, (size - position));
		}

	}
}
