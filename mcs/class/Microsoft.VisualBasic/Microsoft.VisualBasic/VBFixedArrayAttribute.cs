//
// VBFixedArrayAttribute.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net) 
//   Rafael Teixeira (rafaelteixeirabr@hotmail.com)
//	Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Chris J Breisch
// (C) 2004 Rafael Teixeira
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
using Microsoft.VisualBasic.CompilerServices;

namespace Microsoft.VisualBasic {
	[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)] 
	sealed public class VBFixedArrayAttribute : Attribute {

		// Declarations
		private int upperBound1;
		private int upperBound2;
		private bool bidimensional; 

		// Constructors
		public VBFixedArrayAttribute(int UpperBound1) { 
			if (UpperBound1 < 0) {
				throw new ArgumentException(
					Utils.GetResourceString("Invalid_VBFixedArray"));
			}
			upperBound1 = UpperBound1; 
			bidimensional = false;
		}

		public VBFixedArrayAttribute(int UpperBound1, int UpperBound2) {
			if (UpperBound1 < 0) {
				throw new ArgumentException(
					Utils.GetResourceString("Invalid_VBFixedArray"));
			}
			if (UpperBound2 < 0) {
				throw new ArgumentException(
					Utils.GetResourceString("Invalid_VBFixedArray"));
			}
			upperBound1 = UpperBound1; 
			upperBound2 = UpperBound2; 
			bidimensional = true;
		}

		// Properties
		public int Length { 
			get { 
				if (bidimensional)
					return (upperBound1+1)*(upperBound2+1);
				return upperBound1+1;
			} 
		}

		public int[] Bounds { 
			get { 
				if (bidimensional)
					return new int[] { upperBound1, upperBound2 };
				return new int[] { upperBound1 }; 
			} 
		}
	};
}
