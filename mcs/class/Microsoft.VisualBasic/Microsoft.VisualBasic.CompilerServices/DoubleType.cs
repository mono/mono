//
// DoubleType.cs
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
/**
 * Class that converts objects to double value.
 */
using System;
using System.Globalization;
using System.ComponentModel;

namespace Microsoft.VisualBasic.CompilerServices
{
	[StandardModule, EditorBrowsableAttribute(EditorBrowsableState.Never)] 
	sealed public class DoubleType {
		private DoubleType () {}

		/**
		 * Converts given string to double
		 * @param value string to convert
		 * @return double double representation of given string
		 */
		public static double FromString(string Value) {
			return FromString(Value, null);
		}
    
		/**
		 * The method try to convert given string to double in a following way:
		 * 1. If input string is null return 0.
		 * 2. If input string represents number: return value of this number, 
		 * @exception InvalidCastException - in case if number translation failed 
		 * @param str - The string that converted to double
		 * @return doubleThe value that extracted from the input string.
		 * @see Microsoft.VisualBasic.VBUtils#isNumber
		 */
		public static double FromString(string Value, NumberFormatInfo numberFormat) {
			if (Value == null)
				return 0.0;

			//try {
				double[] lRes = new double[1];
			if (VBUtils.isNumber(Value, lRes))
				return lRes[0];
			//}
			//catch (Exception e) {
			//	throw new InvalidCastException(
			//		Utils.GetResourceString("InvalidCast_FromStringTo", 
			//		Value, "Double"), e);
			//}
			return 0.0;
		}

		/**
		 * Converts given object to double.
		 * @param value value to convert to
		 * @return double value converted from given object
		 */
		public static double FromObject(object Value) {
			return FromObject(Value, null);
		}
    
		/**
		 * The method converts given object to double by the following logic:
		 * 1. If input object is null - return 0
		 * 2. If input object is String - run FromString method
		 * 3. Otherwise run .NET default conversion - Convert.ToDouble
		 * @param value - The object that going to be converted
		 * @return double The double value that converted from the source object
		 * @see system.Convert#ToDouble
		 */
		public static double FromObject(object Value, NumberFormatInfo numberFormat) {
			if (Value == null)
				return 0.0;

			if (Value is string)
				return FromString((string) Value, numberFormat);

			//try {
			return Convert.ToDouble(Value, numberFormat);
			//}
			//catch(java.lang.Exception e) {
			//	throw new InvalidCastException(
			//		Utils.GetResourceString("InvalidCast_FromTo", 
			//		Utils.VBFriendlyName(Value), "Double"));
			//}
		}


		/**
		 * Parse given string to double value
		 * @param value string to parse
		 * @return double resulted value 
		 */
		public static double Parse(string Value) {
			return Parse(Value, null);
		}

		public static bool TryParse(string Value, out double result) {
			return  Double.TryParse(Value, NumberStyles.Any, null, out result);
		}

		/**
		 * This method try to parse given string using all available styles, if an 
		 * error is thrown then it parses without a currency style.
		 * @param value string to parse
		 * @param numberFormat NumberFormatInfo to use
		 * @return double the resulted value
		 */
		public static double Parse(string Value, NumberFormatInfo numberFormat) {
			double d;

			try {
				//			d = ClrDouble.Parse(Value, NumberStyles.Any, numberFormat);
				d = double.Parse(Value, NumberStyles.Any, numberFormat);
			}
			catch /*(Exception e)*/ {
				//			d = ClrDouble.Parse(Value, 255, numberFormat);
				d = double.Parse(Value, (NumberStyles)255, numberFormat);
			}
			return d;
		}
	}
}
