    //
    // ByteType.cs
    //
    //	Author:
    //	Chris J Breisch (cjbreisch@altavista.net) 
    //	Francesco Delfino (pluto@tipic.com)
    //	Dennis Hayes (dennish@raytek.com)
    //
    //	(C) copyright 2002 Chris J Breisch
    //	2002 Tipic, Inc (http://www.tipic.com)
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
     * Class that converts objects to Byte value.
     */
using System;
using System.ComponentModel;

namespace Microsoft.VisualBasic.CompilerServices 
{
	[StandardModule, EditorBrowsableAttribute(EditorBrowsableState.Never)] 
	sealed public class ByteType {
		private ByteType () {}
    
		/**
		 * The method converts given object to byte by the following logic:
		 * 1. If input object is null - return 0
		 * 2. If input object is String - run FromString method
		 * 3. Otherwise run .NET default conversion - Convert.ToByte
		 * @param value - The object that going to be converted
		 * @return byte The byte value that converted from the source object
		 * @see system.Convert#ToByte
		 */
		public static System.Byte FromObject (object Value) { 
			if ((object)Value==null)
				return 0;

			if	(Value is string)
				return FromString((string) Value);

			return Convert.ToByte(Value);//Throws correct execption. Execption not converted from .java code.
		}
    
		// Methods
		/**
			 * The method try to convert given string to byte in a following way:
			 * 1. If input string is null return 0.
			 * 2. If input string represents number: return value of this number, 
			 * @exception InvalidCastException - in case if number translation failed 
			 * @param str - The string that converted to int
			 * @return int The value that extracted from the input string.
			 * @see Microsoft.VisualBasic.VBUtils#isNumber
			 */ 
		public static System.Byte FromString (string Value) {
			if(Value == null)return 0;
    
			int Base = 10;
			int start = 0;
			if(Value.Substring(0,1) == "&"){
				//is diff base
				if(Value.Substring(1,1).ToUpper() == "H")
					Base = 16;
				else if(Value.Substring(1,1).ToUpper() == "O")
					Base = 8;
				else {
					// I think we should just let convert take care of the execption.
					// Should we throw a special execption instead?
					// I think the Mainsoft java code below just converts execptions from java to C#
				}
				start = 2;
			}
			return Convert.ToByte(Value.Substring(start,Value.Length - start), Base);
  
			// Mainsoft java implmentation.
			// isNumber checks for leading &H or &O
			// leave for documentation.
			//
			//            if (VBUtils.isNumber(str, lRes))
			//            {
			//                long val = (long)java.lang.Math.rint(lRes[0]);
			//                if (val > ClrByte.MaxValue || val < ClrByte.MinValue)
			//                    throw new OverflowException(
			//                        Environment.GetResourceString("Overflow_Byte"));
			//                return (int) val;
			//            }
			//        }
			//        catch (java.lang.Exception e)
			//        {
			//            throw new InvalidCastException(
			//                Utils.GetResourceString("InvalidCast_FromStringTo", 
			//                    str, "Byte"), e);
			//        }
		}
	};
}
