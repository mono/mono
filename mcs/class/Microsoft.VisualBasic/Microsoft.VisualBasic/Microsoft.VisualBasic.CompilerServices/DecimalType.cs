//
// DecimalType.cs
//
//	Author:
//	Chris J Breisch (cjbreisch@altavista.net) 
//	Dennis Hayes (dennish@raytek.com)
//
//	(C) copyright 2002 Chris J Breisch
//
 /*
  * Copyright (c) 2002-2003 Mainsoft Corporation.
  * Copyright (C) 2004 Novell, Inc (http://www.novell.com)
  *
  * Permission is hereby granted, free of charge, to any person obtaining a
  * copy of this software and associated documentation files (the "Software"),
  * to deal in the Software without restriction, including without limitation
  * the rights to use, copy, modify, merge, publish, distribute, sublicense,
  * and/or sell copies of the Software, and to permit persons to whom the
  * Software is furnished to do so, subject to the following conditions:
  * 
  * The above copyright notice and this permission notice shall be included in
  * all copies or substantial portions of the Software.
  * 
  * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
  * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
  * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
  * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
  * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
  * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
  * DEALINGS IN THE SOFTWARE.
  */
using System;
using System.Globalization;
using System.ComponentModel;

namespace Microsoft.VisualBasic.CompilerServices {
	[StandardModule, EditorBrowsable(EditorBrowsableState.Never)] 
	sealed public class DecimalType {
		private DecimalType () {}

		// Methods
		/**
		 * This method converts given boolean to Decimal. true is converted to -1
		 * and false to 0. 
		 * @param value The boolean that going to be converted
		 * @return Decimal The Decimal value that converted from the boolean
		 */
		public static System.Decimal FromBoolean (System.Boolean Value) {
			if (Value)return Decimal.MinusOne;
			return Decimal.Zero;
		}

		public static System.Decimal FromString (System.String Value) {
			return FromString(Value, null);
		}

		public static System.Decimal FromObject (System.Object Value) {
			return DecimalType.FromObject(Value, null);
		}
		/**
		 * The method try to convert given string to Decimal in a following way:
		 * 1. If input string is null return 0.
		 * 2. If input string represents number: return value of this number, 
		 * @exception OverflowException - if number is out of Decimal range
		 * @exception InvalidCastException - in case if number translation failed 
		 *  due to NumberFormatException.
		 * @exception All other thrown exceptions from Decimal.Parse 
		 * @param str - The string that converted to Decimal
		 * @return Decimal The value that extracted from the input string.
		 * @see Microsoft.VisualBasic.VBUtils#isNumber
		 */
		public static Decimal FromString(String Value, NumberFormatInfo numberFormat) {
			if (Value == null)return Decimal.Zero;
			
			//TODO: remove this line
			//return Parse(Value, numberFormat);

			//TODO convert this to C# and uncomment
			//try {
				//double d;
				long[] lRes = new long[1];
				bool b = StringType.IsHexOrOctValue(Value, lRes);
				if (b == true)return (decimal)lRes[0];
				return Parse(Value, numberFormat);
			//}
			//catch (OverflowException exp) {
			//	throw (RuntimeException)ExceptionUtils.VbMakeException(6);
			//}
			//catch (FormatException exp) {
			//	throw new InvalidCastException(
			//		Utils.GetResourceString("InvalidCast_FromStringTo", 
			//		str, "Decimal"));
			//}
		}
		/**
		 * The method converts given object to decimal by the following logic:
		 * 1. If input object is null - return 0
		 * 2. If input object is String - run FromString method
		 * 3. Otherwise run .NET default conversion - Convert.ToDecimal
		 * @param value - The object that going to be converted
		 * @return Decimal The Decimal value that converted from the source object
		 * @see system.Convert#ToDecimal
		 */
		public static System.Decimal FromObject (System.Object Value, System.Globalization.NumberFormatInfo NumberFormat) {
			if (Value == null)return Decimal.Zero;

			if (Value is string)return FromString((string) Value, NumberFormat);
			
			return Convert.ToDecimal(Value);
		}

		/**
		 * This method try to parse this string first of all by allowing that the 
		 * string will contain currency symbol. if an error is thrown the a parse
		 * without a currenct is tried.
		 * @param value the string that should be parse to Decimal
		 * @param numberFormat the relevant NumberFormat.  
		 * @return Decimal the Decimal value of the string.
		 */
		public static System.Decimal Parse (System.String Value, System.Globalization.NumberFormatInfo NumberFormat) {
			return Decimal.Parse(Value, NumberStyles.Any, NumberFormat);
		}
	};
}

