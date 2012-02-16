//
// OrderedEnumerableRowCollection.cs
//
// Author:
//   Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell, Inc. http://www.novell.com
//

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
using System.Collections;
using System.Collections.Generic;

namespace System.Data
{
	public sealed class OrderedEnumerableRowCollection<TRow>
		: EnumerableRowCollection<TRow>
	{
		internal static OrderedEnumerableRowCollection<TRow> Create<TRow, TKey> (IEnumerable<TRow> source, Func<TRow, TKey> keySelector, IComparer<TKey> comparer, bool descending)
		{
			var sorter = new SortComparer<TRow> ();
			sorter.AddSort<TKey> (keySelector, comparer, descending);
			return new OrderedEnumerableRowCollection<TRow> (new SortedEnumerable <TRow> (source, sorter));
		}

		internal static OrderedEnumerableRowCollection<TRow> AddSort<TRow, TKey> (OrderedEnumerableRowCollection<TRow> source, Func<TRow, TKey> keySelector, IComparer<TKey> comparer, bool descending)
		{
			source.source.Sorter.AddSort<TKey> (keySelector, comparer, descending);
			return source;
		}

		OrderedEnumerableRowCollection (SortedEnumerable<TRow> source)
			: base (source)
		{
			this.source = source;
		}

		SortedEnumerable<TRow> source;
	}

	class SortComparer<TRow> : IComparer<TRow>
	{
		public SortComparer ()
		{
		}

		List<Comparison<TRow>> comparers = new List<Comparison<TRow>> ();

		public void AddSort (Comparison<TRow> comparer)
		{
			comparers.Add (comparer);
		}

		public void AddSort<TKey> (Func<TRow, TKey> keySelector, IComparer<TKey> comparer, bool descending)
		{
			if (keySelector == null)
				throw new ArgumentNullException ("keySelector");
			if (comparer == null)
				comparer = Comparer<TKey>.Default;
			comparers.Add (delegate (TRow r1, TRow r2) {
				int ret = comparer.Compare (keySelector (r1), keySelector (r2));
				return descending ? -ret : ret;
				});
		}

		public int Compare (TRow r1, TRow r2)
		{
			foreach (var c in comparers) {
				int ret = c (r1, r2);
				if (ret != 0)
					return ret;
			}
			return 0;
		}
	}

	class SortedEnumerable<TRow> : IEnumerable<TRow>
	{
		IEnumerable<TRow> source;
		SortComparer<TRow> sorter;

		public SortedEnumerable (IEnumerable<TRow> source, SortComparer<TRow> sorter)
		{
			this.source = source;
			this.sorter = sorter;
		}

		public SortComparer<TRow> Sorter {
			get { return sorter; }
		}

		public IEnumerator<TRow> GetEnumerator ()
		{
			var list = new List<TRow> ();
			foreach (TRow row in source)
				list.Add (row);
			list.Sort (sorter);
			for (int i = 0, c = list.Count; i < c; i++)
				yield return list [i];
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}
	}
}
