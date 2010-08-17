#if NET_4_0
//
// RangeList.cs
//
// Author:
//       Jérémie "Garuma" Laval <jeremie.laval@gmail.com>
//
// Copyright (c) 2010 Jérémie "Garuma" Laval
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections;
using System.Collections.Generic;

namespace System.Linq
{
	internal class RangeList : IList<int>
	{
		readonly int start;
		readonly int count;

		public RangeList (int start, int count)
		{
			this.start = start;
			this.count = count;
		}

		public int IndexOf (int item)
		{
			if (!Contains(item))
				return -1;

			return item - start;
		}

		public void Insert (int index, int item)
		{
			throw new NotImplementedException();
		}

		public void RemoveAt (int index)
		{
			throw new NotImplementedException();
		}

		public int this[int index] {
			get {
				if (start + index <= count)
					return start + index;
				else
					return -1;
			}
			set {
				throw new NotImplementedException();
			}
		}

		public void Add (int item)
		{
			throw new NotImplementedException();
		}

		public void Clear ()
		{
			throw new NotImplementedException();
		}

		public bool Contains (int item)
		{
			return start <= item && item <= start + count - 1;
		}

		public void CopyTo (int[] array, int arrayIndex)
		{
			int counter = start;
			for (int i = arrayIndex; i < array.Length && i < (i - arrayIndex) + count; i++)
				array[i] = counter++;
		}

		public bool Remove (int item)
		{
			throw new NotImplementedException();
		}

		public int Count {
			get {
				return count;
			}
		}

		public bool IsReadOnly {
			get {
				return true;
			}
		}

		IEnumerator<int> IEnumerable<int>.GetEnumerator ()
		{
			return null;
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return null;
		}
	}
}
#endif
