//
// VBFixedStringAttribute.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net) 
//	Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Chris J Breisch
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


namespace Microsoft.VisualBasic {
using Microsoft.VisualBasic.CompilerServices;        
	[System.AttributeUsageAttribute(System.AttributeTargets.Field)] 
	sealed public class VBFixedStringAttribute : System.Attribute {
		// Declarations
		private int _length; 
		// Constructors
		VBFixedStringAttribute(System.Int32 Length) { 
			if ((Length < 1) || (Length > 32767)) {
				throw new ArgumentException(
					Utils.GetResourceString("Invalid_VBFixedString"));
			}
			_length = Length;
		}
		// Properties
		public System.Int32 Length { 
			get {
				return _length;
			} 
		}
	}
}
