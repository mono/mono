//
// AggregationList.cs
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

#if NET_4_0
using System;
using System.Collections;
using System.Collections.Generic;

namespace System.Linq
{
	internal class AggregationList<T> : IList<T>
	{
		readonly IList<IList<T>> listes;
		readonly int count;

		internal AggregationList (IList<IList<T>> listes)
		{
			this.listes = listes;
			foreach (var l in listes)
				count += l.Count;
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
				int listIndex, newIndex;
				GetModifiedIndexes (index, out listIndex, out newIndex);

				return listes[listIndex][newIndex];
			}
			set {
				throw new NotImplementedException();
			}
		}

		void GetModifiedIndexes (int index, out int listIndex, out int newIndex)
		{
			listIndex = 0;
			newIndex = index;

			while (newIndex >= listes[listIndex].Count) {
				newIndex -= listes[listIndex].Count;
				listIndex++;

				if (listIndex > listes.Count)
					throw new ArgumentOutOfRangeException ();
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
