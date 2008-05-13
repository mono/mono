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
			return new OrderedEnumerableRowCollection<TRow> (new Sorter<TRow, TKey> (source, keySelector, comparer, descending));
		}

		OrderedEnumerableRowCollection (IEnumerable<TRow> source)
			: base (source)
		{
		}
	}

	class Sorter<TRow, TKey> : IEnumerable<TRow>
	{
		IEnumerable<TRow> source;
		Func<TRow, TKey> key_selector;
		IComparer<TKey> comparer;
		bool descending;

		public Sorter (IEnumerable<TRow> source, Func<TRow, TKey> keySelector, IComparer<TKey> comparer, bool descending)
		{
			if (keySelector == null)
				throw new ArgumentNullException ("keySelector");
			if (comparer == null)
				comparer = Comparer<TKey>.Default;
			this.source = source;
			this.key_selector = keySelector;
			this.comparer = comparer;
			this.descending = descending;
		}

		public IEnumerator<TRow> GetEnumerator ()
		{
			var list = new List<TRow> ();
			foreach (TRow row in source)
				list.Add (row);
			list.Sort (delegate (TRow r1, TRow r2) {
				return comparer.Compare (key_selector (r1), key_selector (r2));
				});
			if (descending)
				for (int i = list.Count - 1; i >= 0; i--)
					yield return list [i];
			else
				for (int i = 0, c = list.Count; i < c; i++)
					yield return list [i];
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}
	}
}
