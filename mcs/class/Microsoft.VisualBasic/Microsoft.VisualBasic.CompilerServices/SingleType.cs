//
// SingleType.cs
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
  /**
   * Class that converts objects to single value.
   */
using System;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.VisualBasic.CompilerServices {
	[StandardModuleAttribute, EditorBrowsableAttribute(EditorBrowsableState.Never)] 
	sealed public class SingleType {
		private SingleType () {}

		/**
		 * Converts given string to float
		 * @param value string to convert
		 * @return float float representation of given string
		 */
		public static float FromString(string Value) {
			return FromString(Value, null);
		}
    
		/**
		 * The method try to convert given string to float in a following way:
		 * 1. If input string is null return 0.
		 * 2. If input string represents number: return value of this number, 
		 * @exception InvalidCastException - in case if number translation failed 
		 * @param str - The string that converted to float
		 * @return float The value that extracted from the input string.
		 * @see Microsoft.VisualBasic.VBUtils#isNumber
		 */
		public static float FromString(string Value, NumberFormatInfo numberFormat) {
			if (Value == null)
				return 0.0f;

			return Convert.ToSingle(Value,numberFormat);

			//Actually we may need to downcast to long if H or O

			//This gets base correct, but this conversion does not allow base, so I 
			// think the java base check is unneeded
			//int Base = 10;
			//int start = 0;
			//if(Value.Substring(0,1) == "&"){
			//	//is diff base
			//	if(Value.Substring(1,1).ToUpper() == "H")
			//		Base = 16;
			//	else if(Value.Substring(1,1).ToUpper() == "B")
			//		Base = 8;
			//	else {
			//		// I think we should just let convert take care of the execption.
			//		// Should we throw a special execption instead?
			//		// I think the Mainsoft java code below just converts execptions from java to C#
			//	}
			//	start = 2;
			//}

			//return Convert.ToSingle(Value.Substring(start,Value.Length - start), Base);

			// This is the java code for the above line. I think .net throws the correct execpition.
			// verify correct implmentation and execptions and remove
			//
			//try
			//{
			//    double[] lRes = new double[1];
			//    if (VBUtils.isNumber(Value, lRes))
			//        return (float)lRes[0];
			//}
			//catch (java.lang.Exception e)
			//{
			//    throw new InvalidCastException(
			//        Utils.GetResourceString("InvalidCast_FromStringTo", 
			//           value, "Single"), e);
			//}
			//return 0.0f;
		}
    
		/**
		 * Converts given object to float.
		 * @param float value to convert to
		 * @return float value converted from given object
		 */
		public static float FromObject(object Value) {
			return FromObject(Value, null);
		}

		/**
		 * The method converts given object to float by the following logic:
		 * 1. If input object is null - return 0
		 * 2. If input object is String - run FromString method
		 * 3. Otherwise run .NET default conversion - Convert.ToSingle
		 * @param value - The object that going to be converted
		 * @return float The float value that converted from the source object
		 * @see system.Convert#ToSingle
		 */
		public static float FromObject(object Value, NumberFormatInfo numberFormat) {
			if (Value == null)
				return 0.0f;

			if (Value is string)
				return FromString((string) Value, numberFormat);

			return Convert.ToSingle(Value, numberFormat);
			// This is the java code for the above line. I think .net throws the correct execpition.
			//verify correct execptions and remove
			// try
			// {
			//     return Convert.ToSingle(Value, numberFormat);
			// }
			// catch(java.lang.Exception e)
			// {
			//     throw new InvalidCastException(
			//         Utils.GetResourceString("InvalidCast_FromTo", 
			//             Utils.VBFriendlyName(value), "Single"));
			// }
		}

	}
}
