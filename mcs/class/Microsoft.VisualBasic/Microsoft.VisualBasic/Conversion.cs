//
// Conversion.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net) 
//
// (C) 2002 Chris J Breisch
//

//
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
using System.Text.RegularExpressions;
using Microsoft.VisualBasic.CompilerServices;

namespace Microsoft.VisualBasic {
	[StandardModule] 
	sealed public class Conversion {
		///
		/// <summary>
		///		Collection : The BASIC Collection Object
		///	</summary>
		///
		///	<remarks>
		///	</remarks>
		private Conversion ()
		{
			//Nothing to do, nobody should see this constructor
		}

		// Declarations
		private static readonly char[] _HexDigits = {
			'0', '1', '2', '3', '4', '5', '6', '7', 
			'8', '9', 'A', 'B', 'C', 'D', 'E', 'F'
		};
		private static readonly char[] _OctDigits = {
			'0', '1', '2', '3', '4', '5', '6', '7'
		};
		private static readonly long[] _Maxes = {
			32767, 2147483647, 9223372036854775807
		};
		private enum SizeIndexes {
			Int16 = 0,
			Int32 = 1,
			Int64 = 2
		};

		// Constructors
		// Properties
		// Methods

		public static string ErrorToString () { 
			return Information.Err().Description;
		}

		public static string ErrorToString (System.Int32 ErrorNumber) {
			if(ErrorNumber >= 65535)
				throw new ArgumentException(VBUtils.GetResourceString("MaxErrNumber"));
			if(ErrorNumber == 0)
				return Information.Err().Description;

			String errStr = VBUtils.GetResourceString(ErrorNumber);
		
			if (errStr == null)
				errStr = VBUtils.GetResourceString(95);
		
			return errStr;
		}

		// Return whether d is +/- Could do this with a macro, 
		// but this is cleaner
		private static int Sign(double d) { return d > 0 ? 1 : -1;}

		// try to cast an Object to a string...used in several places
		private static string CastToString (System.Object Expression) {
			try {
				return Expression.ToString();
			}
			catch {
				throw new InvalidCastException();
			}
		}
		
		// Fix on Integer types doesn't do anything
		public static short Fix (short Number) { return Number; }
		public static int Fix (int Number) { return Number; }
		public static long Fix (long Number) { return Number; }

		// Fix on other numberic types = Sign(Number) * Int(Abs(Number))
		public static double Fix (double Number) { 
			return Sign(Number) * Int (Math.Abs (Number)); 
		}
		public static float Fix (float Number) { 
			return Sign(Number) * Int (Math.Abs (Number)); 
		}
		public static decimal Fix (decimal Number) { 
			return Sign((double)Number) * Int (Math.Abs (Number)); 
		}

		// Fix on an Object type is trickier
		// first we have to cast it to the right type
		public static System.Object Fix (System.Object Number)
		{
			// always start out by throwing an exception 
			// if Number is null
			if (Number == null) {
				throw new ArgumentNullException ("Number", 
					"Value cannot be null");
			}

			TypeCode TC = Type.GetTypeCode (Number.GetType ());

			// switch on TypeCode and call appropriate Fix method
			switch (TC) {
				case TypeCode.Decimal:
					return Fix (Convert.ToDecimal (Number));
				case TypeCode.Double:
					return Fix (Convert.ToDouble (Number));
				case TypeCode.Single:
					return Fix (Convert.ToSingle (Number));
				case TypeCode.String:
					return Fix (Double.Parse (
						CastToString (Number)));

				// for integer types, don't need to do anything
				case TypeCode.Byte:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
				case TypeCode.SByte:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
					return Number;

				// spec defines Empty as returning 0
				case TypeCode.Empty:
					return 0;

				// we can't convert these types
				case TypeCode.Boolean:
				case TypeCode.Char:
				case TypeCode.DateTime:
				case TypeCode.DBNull:
				case TypeCode.Object:
				default:
					throw new ArgumentException (
						"Type of argument 'Number' is '"
						+ Number.GetType().FullName + 
						"', which is not numeric.");
			}
		}

		// Int on Integer types doesn't do anything
		public static short Int (short Number) { return Number; }
		public static int Int (int Number) { return Number; }
		public static long Int (long Number) { return Number; }

		// Int on other numberic types is same thing as "Floor"
		public static double Int (double Number) { 
			return (double) Math.Floor(Number); 
		}
		public static float Int (float Number) {
			return (float) Math.Floor(Number); 
		}
		public static decimal Int (decimal Number) {
			return Decimal.Floor(Number); 
		}

		// doing an Int on an Object is trickier
		// first we have to cast to the correct type
		public static System.Object Int (System.Object Number) {
			// always start out by throwing an exception 
			// if Number is null
			if (Number == null) {
				throw new ArgumentNullException("Number", 
					"Value cannot be null");
			}

			TypeCode TC = Type.GetTypeCode (Number.GetType ());

			// switch on TypeCode and call appropriate Int method
			switch (TC) {
				case TypeCode.Decimal:
					return Int (Convert.ToDecimal (Number));
				case TypeCode.Double:
					return Int (Convert.ToDouble (Number));
				case TypeCode.Single:
					return Int (Convert.ToSingle (Number));
				case TypeCode.String:
					return Int (Double.Parse (
					CastToString(Number)));

				// Int on integer types does nothing
				case TypeCode.Byte:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
				case TypeCode.SByte:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
					return Number;

				// Spec defines Empty as returning 0
				case TypeCode.Empty:
					return 0;

				// otherwise, it's we can't cast to a numeric
				case TypeCode.Boolean:
				case TypeCode.Char:
				case TypeCode.DateTime:
				case TypeCode.DBNull:
				case TypeCode.Object:
				default:
					throw new ArgumentException (
					"Type of argument 'Number' is '" + 
					Number.GetType().FullName + 
					"', which is not numeric.");			
			}
		}

		// we use this internally to get a string 
		// representation of a number in a specific base
		private static string ToBase (ulong Number, int Length, 
			char[] BaseDigits, uint Base) {
			int i;
			ulong r;
			// we use a char array here for performance
			char [] c = new Char[Length];
			string s = null;
			
			
			for (i = Length - 1; i >= 0; i--) {
				r = Number % Base;
				Number = Number / Base;
				c[i] = BaseDigits[r];
				if (Number == 0) {
					s = new string (c, i, Length - i);
					break;
				}
			}
			if (s == null) {
				return new string (c);
			}
			else {
				return s;
			}
		}


		// convert a number to Hex
		// a little bit of magic goes on here with negative #'s
		private static string ToHex(long Number, int Digits, 
			SizeIndexes Size) {
			ulong UNumber;

			if (Number < 0) {
				// we add maxint of the Number's type 
				// twice and then 2 more...this has the
				// effect of turning it into a ulong 
				// that has the same hex representation
				UNumber = (ulong)((Number + 2) + 
					_Maxes[(int)Size]) + 
					(ulong)_Maxes[(int)Size];
			}
			else {
				UNumber = (ulong)Number;
			}
			return ToBase(UNumber, Digits, _HexDigits, 16);
		}

		// call our private function, 
		// passing it the size of the item to convert
		public static string Hex (short Number) { 
			return ToHex(Number, 4, SizeIndexes.Int16); 
		}
		public static string Hex (byte Number) { 
			return ToHex(Number, 2, SizeIndexes.Int16); 
		}
		public static string Hex (int Number) { 
			return ToHex(Number, 8, SizeIndexes.Int32); 
		}
		public static string Hex (long Number) { 
			return ToHex(Number, 16, SizeIndexes.Int64); 
		}

		// Objects are trickier
		// first we have to cast to appropriate type
		public static System.String Hex (System.Object Number) {
			// always start out by throwing an exception 
			// if Number is null
			long lval;
			if (Number == null) {
				throw new ArgumentNullException ("Number", 
					"Value cannot be null");
			}

			TypeCode TC = Type.GetTypeCode (Number.GetType ());

			switch (TC) {
				// try to parse the string as an Int32, 
				// then an Int64, if that fails
				case TypeCode.String:
					try {
						return Hex (
							Int32.Parse (
							CastToString (Number)));
					}
					catch {
						return Hex (
							Int64.Parse (
							CastToString (Number)));
					}

				// for the int types, 
				// just call the normal "Hex" for that type
				case TypeCode.Byte:
					return Hex ((byte)Number);
				case TypeCode.Int16:
					return Hex ((short)Number);
				case TypeCode.Int32:
					return Hex ((int)Number);
				case TypeCode.Int64:
					return Hex ((long)Number);

				// empty is defined as returning 0
				case TypeCode.Empty:
					return "0";
				case TypeCode.Single:
					float fval = (float)Number;
					lval = (long) Math.Round(fval);
					if ((lval > Int32.MinValue) && (lval < Int32.MaxValue))
               					return Hex ((int)lval); 	

					return Hex(lval);
				case TypeCode.Double:
	 		            	double dval = (double)Number;
            				if (dval > Int64.MaxValue || dval < Int64.MinValue)
				                throw new OverflowException(
				                  VBUtils.GetResourceString("Overflow_Int64"));	
       				    	lval = (long) Math.Round(dval);
			            	if ((lval > Int32.MinValue) && (lval < Int32.MaxValue))
	      				          return Hex((int)lval);

			            	return Hex(lval);
        			
				case TypeCode.Decimal:
					Decimal big = new Decimal(Int64.MaxValue);
					Decimal small = new Decimal(Int64.MinValue);
					Decimal current = (Decimal)Number;

			    	        if (current.CompareTo(big) > 0 || 
						current.CompareTo(small) < 0)
				                throw new OverflowException(
				                    VBUtils.GetResourceString("Overflow_Int64"));

			     	       lval = Decimal.ToInt64(current);
			               if ((lval > Int32.MinValue) && (lval < Int32.MaxValue))
				                return Hex((int)lval);

			               return Hex(lval);
				// we can't do any of these types
				case TypeCode.Boolean:
				case TypeCode.Char:
				case TypeCode.DBNull:
				case TypeCode.DateTime:
				case TypeCode.Object:
				case TypeCode.SByte:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
				default:
					throw new ArgumentException (
					"Type of argument 'Number' is '" + 
					Number.GetType().FullName + 
					"', which is not numeric.");			
			}
		}
		
		// ToOct works just like ToHex, only in Octal.
		private static string ToOct(long Number, int Digits, 
			SizeIndexes Size) {
			ulong UNumber;

			if (Number < 0) {
				// for neg numbers add the maxint of 
				// the appropriate size twice, and then two more
				// this has the effect of turning it 
				// into a ulong with the same oct representation
				UNumber = (ulong)((Number + 2) + 
					_Maxes[(int)Size]) + 
					(ulong)(_Maxes[(int)Size]);
			}
			else {
				UNumber = (ulong)Number;
			}
			return ToBase (UNumber, Digits, _OctDigits, 8);
		}

		// call ToOct with appropriate information
		public static string Oct (short Number) { 
			return ToOct(Number, 6, SizeIndexes.Int16); 
		}
		public static string Oct (byte Number) { 
			return ToOct(Number, 3, SizeIndexes.Int16); 
		}
		public static string Oct (int Number) { 
			return ToOct(Number, 11, SizeIndexes.Int32); 
		}
		public static string Oct (long Number) { 
			return ToOct(Number, 22, SizeIndexes.Int64); 
		}

		// Objects are always trickier
		// first need to cast to appropriate type
		public static string Oct (System.Object Number) {
			// first, always throw an exception if Number is null
			if (Number == null) {
				throw new ArgumentNullException("Number", 
					"Value cannot be null");
			}
			long lval;
			TypeCode TC = Type.GetTypeCode (Number.GetType ());

			switch (TC) {
				// try to parse a string as an Int32 
				// and then an Int64
				case TypeCode.String:
					try {
						return Oct (
							Int32.Parse (
							CastToString (Number)));
					}
					catch {
						return Oct (
							Int64.Parse (
							CastToString (Number)));
					}

				// integer types just call the appropriate "Oct"
				case TypeCode.Byte:
					return Oct ((byte)Number);
				case TypeCode.Int16:
					return Oct ((short)Number);
				case TypeCode.Int32:
					return Oct ((int)Number);
				case TypeCode.Int64:
					return Oct ((long)Number);
				case TypeCode.Single:
					float fval = (float)Number;
			                lval = (long) Math.Round(fval);
			                if ((lval > Int32.MinValue) && (lval < Int32.MaxValue))
				                return Oct((int)lval);

			                return Oct(lval);	
				case TypeCode.Double:
					 double dval = (double)Number;
			                 if (dval > Int64.MaxValue || dval < Int64.MinValue)
				                throw new OverflowException(
				                    VBUtils.GetResourceString("Overflow_Int64"));

			                 lval = (long) Math.Round(dval);
			                 if ((lval > Int32.MinValue) && (lval < Int32.MaxValue))
				                return Oct((int)lval);

			                return Oct(lval);
				case TypeCode.Decimal:
					Decimal big = new Decimal(Int64.MaxValue);
					Decimal small = new Decimal(Int64.MinValue);
			                Decimal current = (Decimal) Number;

					if ((current.CompareTo(big) > 0) || 
						(current.CompareTo(small) < 0))
				                throw new OverflowException(
				                    VBUtils.GetResourceString("Overflow_Int64"));
            
					lval = Decimal.ToInt64(current);
					if ((lval > Int32.MinValue) && (lval < Int32.MaxValue))
						return Oct((int)lval);

					return Oct(lval);
				// Empty is defined as returning 0
				case TypeCode.Empty:
					return "0";

				// We can't convert these to Octal
				case TypeCode.Boolean:
				case TypeCode.Char:
				case TypeCode.DBNull:
				case TypeCode.DateTime:
				case TypeCode.Object:
				case TypeCode.SByte:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
				default:
					throw new ArgumentException (
					"Type of argument 'Number' is '" + 
					Number.GetType().FullName + 
					"', which is not numeric.");			
			}
		}

		// Str is pretty easy now that we have a language 
		// with a ToString method()
		public static string Str (System.Object Number) {

			// check for null as always and throw an exception
			if (Number == null) {
				throw new ArgumentNullException("Number");
			}

			switch (Type.GetTypeCode (Number.GetType ())) {
				// for unsigned types, just call ToString
				case TypeCode.Byte:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
					return Number.ToString();

				// for signed types, we have to leave a 
				// space for the missing + sign
				case TypeCode.Decimal:
					return ((decimal)Number > 0 ? " " : "")
						+ Number.ToString();
				case TypeCode.Double:
					return ((double)Number > 0 ? " " : "") 
						+ Number.ToString();
				case TypeCode.Int16:
					return ((short)Number > 0 ? " " : "") 
						+ Number.ToString();
				case TypeCode.Int32:
					return ((int)Number > 0 ? " " : "") 
						+ Number.ToString();
				case TypeCode.Int64:
					return ((long)Number > 0 ? " " : "") 
						+ Number.ToString();
				case TypeCode.SByte:
					return ((sbyte)Number > 0 ? " " : "") 
						+ Number.ToString();
				case TypeCode.Single:
					return ((float)Number > 0 ? " " : "") 
						+ Number.ToString();

				// can't cast anything else to a Number
				default:
					throw new InvalidCastException(
					"Argument 'Number' cannot be converted to a numeric value.");			
			}
		}

		// The Val function is pretty bizarre
		// Val ("&HFF") = 255
		// Val ("&O377") = 255
		// Val ("1234 Any Street") = 1234
		// Val ("     12   45    .   90  7   E    +   0 0 2  ") = 1245.907e+002 = 124590.7
		public static double Val (string InputStr) {
			int i;
			int Base; 
			int NumChars = 0;
			char c;
			int Length = InputStr.Length;
			char[] Number = new char[Length];
			bool FoundRadixPrefix = false;
			Regex NumberReg;
			Match NumberMatch;
			
			Base = 10;
			Number[0] = '\0';

			// go through string
			for (i = 0; i < Length; i++) {
				c = InputStr[i];

				// look for Radix prefix "&"
				if (i == 0 && c == '&') {
					FoundRadixPrefix = true;
				}
			
				// look for an H or O following the prefix
				else if (FoundRadixPrefix && i == 1 && 
					(char.ToLower(c) == 'h' || 
					char.ToLower(c) == 'o')) {
					if (c == 'H') {
						Base = 16;
					}
					else {
						Base = 8;
					}
				}

				// if we didn't find a radix prefix, 
				// ignore whitespace
				else if (char.IsWhiteSpace(c) && (Base == 10)) {
					continue;
				}

				// mash what's left together
				else {
					Number[NumChars++] = c;
				}
			}
			
			// now we have a string to parse
			switch (Base) {
				// FIXME : for Octal and Hex, 
				// Regex is probably overkill
				// Even for base 10, it might be faster 
				// to write our own parser
				case 8:
					NumberReg = new Regex ("^[0-7]*");
					NumberMatch = NumberReg.Match (
					new string(Number, 0, NumChars));
					break;
				case 16:
					NumberReg = new Regex ("^[0-9a-f]*", 
						RegexOptions.IgnoreCase);
					NumberMatch = NumberReg.Match (
					new string(Number, 0, NumChars));
					break;
				case 10:
				default:
					NumberReg = new Regex (
					"^[+-]?\\d*\\.?\\d*(e[+-]?\\d*)?", 
					RegexOptions.IgnoreCase);
					NumberMatch = NumberReg.Match (
					new string(Number, 0, NumChars));
					break;
				

			}
			
			// we found a match, try to convert it
			if (NumberMatch.Success) {
				try {
					if(NumberMatch.Length == 0)
						return (double)0;

					switch (Base) {
						case 10:
							return 
							Convert.ToDouble (
							NumberMatch.Value);
						case 8:
						case 16:
							return (double)
							Convert.ToInt64 (
							NumberMatch.Value, 
								Base);
						default:
							return (double)0;
					}
				} 
				catch {
					throw new OverflowException();
				}
			}
			else {
				return (double)0;
			}
		}

		// Val on a char type is pretty simple  '9' = 9, 'a' = exception
		public static int Val (char Expression) {
			if (char.IsDigit(Expression)) {
				return Expression - '0';
			} 
			else {
				throw new ArgumentException();
			}
		}

		// if it's an object, and we can't convert 
		// it to a string, it's an exception
		public static double Val (System.Object Expression) {
			// always check for null first
			if (Expression == null) {
				throw new ArgumentNullException ("Expression", 
					"Value cannot be null");
			}
		
			try {
				return Val (CastToString (Expression)); 
			} 
			catch {
				
				throw new ArgumentException(
				"Type of argument 'Expression' is '" + 
				Expression.GetType().FullName + 
				"', which can nt be converted to numeric.");			
			}
		}
		// Events
	}
}
