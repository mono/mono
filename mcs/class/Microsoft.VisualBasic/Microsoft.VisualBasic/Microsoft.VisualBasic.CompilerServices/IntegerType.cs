//
// IntegerType.cs
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

namespace Microsoft.VisualBasic.CompilerServices {
	[Microsoft.VisualBasic.CompilerServices.StandardModuleAttribute] 
	[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)] 
	sealed public class IntegerType {
		// Methods
		/**
		 * The method try to convert given string to int in a following way:
		 * 1. If input string is null return 0.
		 * 2. If input string represents number: return value of this number, 
		 * @exception InvalidCastException - in case if number translation failed 
		 * @param str - The string that converted to int
		 * @return int The value that extracted from the input string.
		 * @see Microsoft.VisualBasic.VBUtils#isNumber
		 */ 
		public static System.Int32 FromString (System.String Value) {
			if(Value == null)return 0;
			double[] lRes = new double[1];
			
			return System.Int32.Parse(Value);
			//TODO:
			// converet the following, then remove the above line
			//
			//the following handles &H &O and other stuff. int.parse does not.
			//if (VBUtils.isNumber(value, lRes)) {
			//	long val = (long)java.lang.Math.rint(lRes[0]);
			//	if (val > ClrInt32.MaxValue || val < ClrInt32.MinValue)
			//		throw new OverflowException(
			//			Environment.GetResourceString("Overflow_Int32"));
			//	return (int) val;
			//}
			return 0;
		}

		public static System.Int32 FromObject (System.Object Value) { 
			if ((object)Value==null)return 0;
			if(Value is string)return FromString((string) Value);
			if(Value is int)return ((int)Value);
                        
			return System.Convert.ToInt32(Value);
		}
	};
}
