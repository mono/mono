#if NET_4_0
//
// QueryReverseNode.cs
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

using System.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

namespace System.Linq
{
	internal class ReverseList<T> : IList<T>
	{
		readonly IList<T> backend;
		readonly int count;

		internal ReverseList (IList<T> backend)
		{
			this.backend = backend;
			this.count = backend.Count;
		}

		public int IndexOf (T item)
		{
			throw new NotImplementedException();
		}

		public void Insert (int index, T item)
		{
			throw new NotImplementedException();
		}

		public void RemoveAt (int index)
		{
			throw new NotImplementedException();
		}

		public T this[int index] {
			get {
				return backend[count - 1 - index];
			}
			set {
				throw new NotImplementedException();
			}
		}

		public void Add (T item)
		{
			throw new NotImplementedException();
		}

		public void Clear ()
		{
			throw new NotImplementedException();
		}

		public bool Contains (T item)
		{
			throw new NotImplementedException();
		}

		public void CopyTo (T[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public bool Remove (T item)
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

		IEnumerator<T> IEnumerable<T>.GetEnumerator ()
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
