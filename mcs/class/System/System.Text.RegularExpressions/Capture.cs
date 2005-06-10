//
// assembly:	System
// namespace:	System.Text.RegularExpressions
// file:	Capture.cs
//
// author:	Dan Lewis (dlewis@gmx.co.uk)
// 		(c) 2002

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

namespace System.Text.RegularExpressions {

	[Serializable]
	public class Capture {
		public int Index {
			get { return index; }
		}

		public int Length {
			get { return length; }
		}

		public string Value {
			get { return text == null ? String.Empty : text.Substring (index, length); }
		}

		public override string ToString ()
		{
			return Value;
		}

		// internal members

		internal Capture (string text) : this (text, 0, 0) { }

		internal Capture (string text, int index, int length)
		{
			this.text = text;
			this.index = index;
			this.length = length;
		}
		
		internal string Text {
			get { return text; }
		}

		// private

		internal int index, length;
		internal string text;
	}
}
