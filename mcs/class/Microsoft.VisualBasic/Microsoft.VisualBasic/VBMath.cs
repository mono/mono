//
// VBMath.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net) 
//   Francesco Delfino (pluto@tipic.com)
//
// (C) 2002 Chris J Breisch
// (C) 2002 Tipic Inc
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using Microsoft.VisualBasic.CompilerServices;

namespace Microsoft.VisualBasic {
	[StandardModule] 
	sealed public class VBMath {

		private VBMath ()
		{
			//Nobody should see constructor
		}

		// Declarations
		// Constructors
		// Properties
		static Random rnd = new Random();
		static float last = (float)rnd.NextDouble();
		// Methods
		public static float Rnd () {
			last =  (float)rnd.NextDouble();
			return last; 
		}
		public static float Rnd (float Number) 
		{
			if (Number == 0.0)
			{
				return last;
			} 
			else if (Number < 0.0 )
			{
				//fd: What does this mean?
				//fd: ms-help://MS.VSCC/MS.MSDNVS/script56/html/vsstmRandomize
				//fd: ms-help://MS.VSCC/MS.MSDNVS/script56/html/vsfctrnd.htm
				Randomize(Number);
				return Rnd();
			}
			return Rnd();
		} 
		public static void Randomize () { 
			rnd = new Random();
		} 
		[MonoTODO("Rethink the double => int conversion")]
		public static void Randomize (double Number) 
		{ 
			rnd = new Random((int)Number);
		}
		// Events
	};
}
