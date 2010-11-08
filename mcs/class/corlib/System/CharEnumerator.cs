//
// System.CharEnumerator.cs
//
// Author:
//   Duncan Mak (duncan@ximian.com)
//
// (C) Ximian, Inc.
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

using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace System
{
	[Serializable]
	[ComVisible (true)]
	public sealed class CharEnumerator : IEnumerator, ICloneable, IEnumerator<char>
	{
		private string str;
		private int index;
		private int length;
		// Representation invariant:
		// length == str.Length
		// -1 <= index <= length
		
		// Constructor
		internal CharEnumerator (string s)
		{
			str = s;
			index = -1;
			length = s.Length;
		}

		// Properties
		public char Current {
			get {
				if (index == -1 || index >= length)
					throw new InvalidOperationException (Locale.GetText ("The position is not valid."));
				return str [index];
			}
		}

		object IEnumerator.Current {
			get { 
				return Current;
			}
		}
		
#if NET_4_0
		public void Dispose ()
#else
		void IDisposable.Dispose ()
#endif
		{
			// nop
		}		

		// Methods
		public object Clone ()
		{
			CharEnumerator x = new CharEnumerator (str);
			x.index = index;
			return x;
		}

		public bool MoveNext ()
		{
			// Representation invariant holds: -1 <= index <= length

			index ++;

			// Now: 0 <= index <= length+1;
			//   <=>
			// 0 <= index < length (OK) || 
			// length <= index <= length+1 (Out of bounds)
			
			if (index >= length) {
				index = length;
				// Invariant restored:
				// length == index
				//   =>
				// -1 <= index <= length
				return false;	
			}
			else
				return true;
		}

		public void Reset ()
		{
			index = -1;
		}
	}
}
