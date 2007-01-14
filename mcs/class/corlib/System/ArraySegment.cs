//
// ArraySegment.cs
//
// Authors:
//  Ben Maurer (bmaurer@ximian.com)
//  Jensen Somers <jensen.somers@gmail.com>
//
// Copyright (C) 2004 Novell
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
#if NET_2_0
namespace System {
	[Serializable]
	public struct ArraySegment <T> {
		T [] array;
		int offset, count;
		
		public ArraySegment (T [] array, int offset, int count)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			
			if (offset < 0)
				throw new ArgumentOutOfRangeException ("offset", "Non-negative number required.");

			if (count < 0)
				throw new ArgumentOutOfRangeException ("count", "Non-negative number required.");

			if (offset > array.Length)
				throw new ArgumentException ("out of bounds");

			// now offset is valid, or just beyond the end.
			// Check count -- do it this way to avoid overflow on 'offset + count'
			if (array.Length - offset < count)
				throw new ArgumentException ("out of bounds", "offset");
			
			this.array = array;
			this.offset = offset;
			this.count = count;
		}
		
		public ArraySegment (T [] array)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			
			this.array = array;
			this.offset = 0;
			this.count = array.Length;
		}
		
		public T [] Array {
			get { return array; }
		}
		
		public int Offset {
			get { return offset; }
		}
		
		public int Count {
			get { return count; }
		}
		
		public override bool Equals (Object obj)
		{
			if (obj is ArraySegment<T>) {
				return this.Equals((ArraySegment<T>) obj);
			}
			return false;
		}
		
		public bool Equals (ArraySegment<T> obj)
		{
			if ((this.array == obj.Array) && (this.offset == obj.Offset) && (this.count == obj.Count))
				return true;
			return false;
		}
		
		public override int GetHashCode ()
		{
			return ((this.array.GetHashCode() ^ this.offset) ^ this.count);
		}
		
		public static bool operator ==(ArraySegment<T> a, ArraySegment<T> b)
		{
			return a.Equals(b);
		}
		
		public static bool operator !=(ArraySegment<T> a, ArraySegment<T> b)
		{
			return !(a.Equals(b));
		}
	}
}
#endif
