//
// DateType.cs
//
//	Author:
//	Chris J Breisch (cjbreisch@altavista.net) 
//	Francesco Delfino (pluto@tipic.com)
//	Dennis Hayes (dennish@raytek.com)
//
//	(C) copyright 2002 Chris J Breisch
//	2002 Tipic, Inc (http://www.tipic.com)
//	2004 Novell
//
 /*
  * Copyright (c) 2002-2003 Mainsoft Corporation.
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

/**
 * Class that converts objects to DateTime object
 */
namespace Microsoft.VisualBasic.CompilerServices {
	[Microsoft.VisualBasic.CompilerServices.StandardModuleAttribute] 
	[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)] 
	sealed public class DateType {
		// Methods
		/**
		  * The method converts given object to DateTime by the following logic:
		  * 1. If input object is null - return null
		  * 2. If input object is String - run FromString method
		  * 3. If input object is DateTime - run ToDateTime on this value
		  * 4. Otherwise throw InvalidCastException.
		  * @exception InvalidCastException - if given object is not nulll, String
		  *  or DateTime.
		  * @param value - The object that going to be converted
		  * @return DateTime The DateTime value that converted from the source object
		  * @see system.Convert#ToDateTime
		  */
		public static System.DateTime FromObject (System.Object Value) { 
			if ((object)Value == null)
				return new DateTime();	//Mainsoft code was wrong, struct are value types and can't be null
			if (Value is string) 
				return FromString((string)Value);
			if(Value is DateTime)
				return (DateTime)Value;
			throw new InvalidCastException("InvalidCast_From " + Value.GetType().Name + " ToDate");
		}

		/**
		 * The method converts given string to DateTime using current CultureInfo.
		 * @param value The value to convert.
		 * @return DateTime The value that extracted from the input string.
		 */
		public static System.DateTime FromString (System.String Value) {
			return FromString(Value, System.Globalization.CultureInfo.CurrentCulture);
		}

		/**
		 * The method try to convert given string to DateTime by calling
		 * DateTime.Parse.
		 * @exception InvalidCastException - in case if date translation failed
		 *  due to any Exception.
		 * @param value - The string that converted to DateTime
		 * @return DateTime The value that extracted from the input string.
		 */
		public static System.DateTime FromString (System.String Value, System.Globalization.CultureInfo culture) { 
			string val = Value;
			if (Value != null && Value.Length > 2 && Value.StartsWith("#") && Value.EndsWith("#"))
			    val = Value.Substring(1, Value.Length - 2);
			return DateTime.Parse(val, culture, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.NoCurrentDateDefault);
		}
	};
}

















