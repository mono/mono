//
// CharArrayType.cs
//
//      Author:
//      Chris J Breisch (cjbreisch@altavista.net) 
//      Dennis Hayes (dennish@raytek.com)
//
//      (C) 2002 Chris J Breisch
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
using System.ComponentModel;

namespace Microsoft.VisualBasic.CompilerServices
{
	[StandardModule, EditorBrowsable(EditorBrowsableState.Never)] 
	sealed public class CharArrayType {
		private CharArrayType () {}

		/**
				 * The method converts given object to char[] by the following logic:
				 * 1. If input object is null - return empty char array
				 * 2. If input object is char array - return this object
				 * 3. If input object is String - return char array representing this String
				 * @param value - The object that going to be converted
				 * @return char[] The char array that converted from the source object
				 * @exception InvalidCastException - in case if value is not String or char[].
				 */
		public static char[] FromObject(object Value) {
			if (Value == null)
				return new char[]{};

			if (Value is char[])
				return (char[])Value;

			if (Value is string) 
				return FromString((string)Value);// could be replaced with Value.ToCharArray();, but spec says make the call.

			throw new InvalidCastException("InvalidCast_From " + Value.GetType().Name + " To char");
		}

		/**
		* The method converts given string to byte of chars:
		* @param str - The string that converted to char array
		* @return char[] The value that extracted from the input string.
		*/
		public static char[] FromString(string Value) {
			if (Value == null)return new char[]{};
			return Value.ToCharArray();
		}
	}
}


