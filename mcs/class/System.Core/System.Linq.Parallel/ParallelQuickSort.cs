//
// ParallelQuickSort.cs
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
using System.Linq;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace System.Linq
{
	// HACK: ATM: parallelization of the sort is disabled as task
	// add more overhead than gain
	internal class ParallelQuickSort<T>
	{
		readonly Comparison<T> comparison;
		readonly IList<T> list;
		readonly int[] indexes;

		class SortedCollection : IList<T>
		{
			int[] indexes;
			IList<T> source;

			public SortedCollection (IList<T> source, int[] indexes)
			{
				this.indexes = indexes;
				this.source = source;
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
					return source[indexes[index]];
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
					return source.Count;
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

		private ParallelQuickSort (IList<T> list, Comparison<T> comparison)
		{
			this.comparison = comparison;
			this.list = list;
			this.indexes = CreateIndexes (list.Count);
		}

		static int[] CreateIndexes (int length)
		{
			var indexes = new int[length];
			for (int i = 0; i < length; i++)
				indexes [i] = i;

			return indexes;
		}

		SortedCollection DoSort ()
		{
			if (list.Count > 1) {
				if (list.Count < 5)
					InsertionSort (0, list.Count - 1);
				else
					Sort (0, list.Count - 1);
			}

			return new SortedCollection (list, indexes);
		}

		int Comparison (int index1, int index2)
		{
			return comparison (list[index1], list[index2]);
		}

		void Sort (int left, int right)
		{
			if (left + 3 <= right) {
				int l = left, r = right - 1, pivot = MedianOfThree (left, right);
				while (true) {
					while (Comparison (indexes [++l], pivot) < 0) { }
					while (Comparison (indexes [--r], pivot) > 0) { }
					if (l < r)
						Swap (l, r);
					else
						break;
				}

				// Restore pivot
				Swap (l, right - 1);
				// Partition and sort
				Sort (left, l - 1);
				Sort (l + 1, right);
			} else
				// If there are three items in the subarray, insertion sort is better
				InsertionSort (left, right);
		}

		/*void Sort (int left, int right, int depth)
		{
			int l = left, r = right - 1, pivot = MedianOfThree (left, right);
			while (true) {
				while (Comparison (indexes[++l], pivot) < 0);
				while (Comparison (indexes[--r], pivot) > 0);
				if (l < r)
					Swap (l, r);
				else
					break;
			}

			// Restore pivot
			Swap (l, right - 1);

			// Partition and sort in parallel if appropriate
			/*if (depth < maxDepth) {
				depth <<= 1;
				Task t = Task.Factory.StartNew (() => Sort (left, l - 1, depth));
				Sort (l + 1, right, depth);

				t.Wait ();
			} else {*/
				// Sequential
		/*		Sort (left, l - 1);
				Sort (l + 1, right);
			//}
		}*/

		/*void ShellSort (int left, int right)
		{
			int[] gaps = new int[] { 4, 1};

			for (int ic = 0; ic < gaps.Length; ic++) {
				int inc = gaps[ic];
				int l = left + inc;
				for (int i = l; i <= right; i++) {
					T temp = list[i];
					int j = i;
					for (; j >= l && comparison (list[j - inc], temp) > 1; j -= inc)
						list[j] = list[j - inc];
					list[j] = temp;
				}
			}
		}*/

		void InsertionSort (int left, int right)
		{
			for (int i = left + 1; i <= right; i++) {
				int j, tmp = indexes [i];

				for (j = i; j > left && Comparison (tmp, indexes [j - 1]) < 0; j--)
					indexes [j] = indexes [j - 1];

				indexes [j] = tmp;
			}
		}

		/*
		void InsertionSort (int left, int right)
		{
			for (int i = left + 1; i <= right; i++) {
				int j;
				T tmp = list[i];

				for (j = i; j > left && comparison (tmp, list [j - 1]) < 0; j--)
					list [j] = list [j - 1];

				list [j] = tmp;
			}
		}*/

		void Swap (int left, int right)
		{
			int temp = indexes [right];
			indexes [right] = indexes [left];
			indexes [left] = temp;
		}

		int MedianOfThree (int left, int right)
		{
			int center = (left + right) >> 1;
			if (Comparison (indexes[center], indexes[left]) < 0)
				Swap (left, center);
			if (Comparison (indexes[right], indexes[left]) < 0)
				Swap (left, right);
			if (Comparison (indexes[right], indexes[center]) < 0)
				Swap (center, right);
			Swap (center, right - 1);

			return indexes[right - 1];
		}

		public static IList<T> Sort (IList<T> list, Comparison<T> comparison)
		{
			ParallelQuickSort<T> qs = new ParallelQuickSort<T> (list, comparison);

			return qs.DoSort ();
		}
	}
}
#endif
