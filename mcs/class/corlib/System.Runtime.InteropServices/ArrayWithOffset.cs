//
// System.Runtime.InteropServices.ArrayWithOffset.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Novell, Inc.  http://www.ximian.com
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

namespace System.Runtime.InteropServices {

#if NET_2_0
	[Serializable]
	[ComVisible (true)]
#endif
	public struct ArrayWithOffset {
		object array;
		int offset;

		public ArrayWithOffset (object array, int offset)
		{
			this.array = array;
			this.offset = offset;
		}

		public override bool Equals (object obj)
		{
			if (obj == null)
				return false;
			if (!(obj is ArrayWithOffset))
				return false;
			ArrayWithOffset other = (ArrayWithOffset) obj;

			return (other.array == array && other.offset == offset);
		}

		public bool Equals (ArrayWithOffset obj)
		{
			return obj.array == array && obj.offset == offset;
		}

#if NET_2_0
		public static bool operator == (ArrayWithOffset a, ArrayWithOffset b)
		{
			return a.Equals (b);
		}

		public static bool operator != (ArrayWithOffset a, ArrayWithOffset b)
		{
			return !a.Equals (b);
		}
#endif

		public override int GetHashCode ()
		{
			return offset;
		}

		public object GetArray ()
		{
			return array;
		}				

		public int GetOffset ()
		{
			return offset;
		}				
	}
}
