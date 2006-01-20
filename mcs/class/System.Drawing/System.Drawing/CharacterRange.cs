//
// System.Windows.Drawing.CharacterRange.cs
//
// Author:
//	Dennis Hayes (dennish@raytek.com)
//
// Copyright (C) 2002 Ximian, Inc http://www.ximian.com
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

namespace System.Drawing
{
	public struct CharacterRange
	{
		private int first;
		private int length;

		public CharacterRange (int first, int length)
		{
			this.first = first;
			this.length = length;
		}

		public int First {
			get {
				return first;
			}
			set {
				first = value;
			}
		}

		public int Length {
			get {
				return length;
			}
			set {
				length = value;
			}
		}
#if NET_2_0
		public override bool Equals (Object o)
		{
			if (o is CharacterRange) {
				if ( ((CharacterRange)o).First == first && ((CharacterRange)o).Length == length)
					return true;
			}

			return false;
		}

		public override int GetHashCode ()
		{
			return (first * length);
		}
		
		public static bool operator == (CharacterRange cr1, CharacterRange cr2)
		{
			return cr1.Equals (cr2);
		}

		public static bool operator != (CharacterRange cr1, CharacterRange cr2)
		{
			return !cr1.Equals (cr2);
		}
#endif

	}
}
