//
// BooleanType.cs
//
//      Author:
//      Chris J Breisch (cjbreisch@altavista.net) 
//      Dennis Hayes        (dennish@raytek.com)
//
//      (C) 2002 Chris J Breisch
//		(C) 2004 Joerg Rosenkranz <JoergR@voelcker.com>
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

namespace Microsoft.VisualBasic.CompilerServices
{
	[StandardModule, EditorBrowsableAttribute(EditorBrowsableState.Never)] 
	sealed public class BooleanType {
		private BooleanType () {}

		/**
				 * The method converts given object to boolean by the following logic:
				 * 1. If input object is null - return null
				 * 2. If input object is String - run FromString method
				 * 3. Otherwise run .NET default conversion - Convert.ToBoolean
				 * @param value - The object that going to be converted
				 * @return boolean The boolean value that converted from the source object
				 */
		public static System.Boolean FromObject (object Value) {
			if (Value == null)
				return false;

			if (Value is string)
				return FromString((string)Value);

			//This throws the correct execption, Mainsoft java code has to catch and rethrow to map java to .net
			return Convert.ToBoolean(Value);
		}

		/**
				 * The method try to convert given string to boolean in a following way:
				 * 1. If input value is True or False string - return corresponding value
				 * 2. If input string represents number: return true if this number is not 
				 *    equals to zero, otherwise return false;
				 * @exception InvalidCastException - in case if number translation failed 
				 *  due to NumberFormatException.
				 * @exception All other thrown exceptions from ClrDobule.Parse 
				 * @param str - The string that converted to boolean
				 * @return boolean The value that extracted from the input string. 
				 */
		public static Boolean FromString (string Value) {
			if(Value == null)
				return false;

			if (string.Compare(Value, bool.TrueString, true) == 0)
				return true;
			if (string.Compare(Value, bool.FalseString, true) == 0)
				return false;
                        
			double conv;
			if (double.TryParse(Value, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out conv))
				return (conv != 0);
                         
			throw new InvalidCastException (
				string.Format (
				"Cast from string \"{0}\" to type 'Boolean' is not valid.",
				Value));
		}        
	}
}
