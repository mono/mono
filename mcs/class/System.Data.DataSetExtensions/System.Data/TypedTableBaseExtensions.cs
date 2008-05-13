//
// TypedTableBaseExtensions.cs
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
using System.Collections.Generic;
using System.Linq;

namespace System.Data
{
	public static class TypedTableBaseExtensions
	{

		public static EnumerableRowCollection<TRow> AsEnumerable<TRow> (this TypedTableBase<TRow> source) where TRow : DataRow
		{
			return new EnumerableRowCollection<TRow> (source);
		}

		public static OrderedEnumerableRowCollection<TRow> OrderBy<TRow, TKey> (this TypedTableBase<TRow> source, Func<TRow, TKey> keySelector)
			where TRow : DataRow
		{
			return OrderBy<TRow, TKey> (source, keySelector, Comparer<TKey>.Default);
		}

		public static OrderedEnumerableRowCollection<TRow> OrderBy<TRow, TKey> (this TypedTableBase<TRow> source, Func<TRow, TKey> keySelector, IComparer<TKey> comparer)
			where TRow : DataRow
		{
			return OrderedEnumerableRowCollection<TRow>.Create<TRow, TKey> (source, keySelector, comparer, false);
		}

		public static OrderedEnumerableRowCollection<TRow> OrderByDescending<TRow, TKey> (this TypedTableBase<TRow> source, Func<TRow, TKey> keySelector)
			where TRow : DataRow
		{
			return OrderByDescending<TRow, TKey> (source, keySelector, Comparer<TKey>.Default);
		}

		public static OrderedEnumerableRowCollection<TRow> OrderByDescending<TRow, TKey> (this TypedTableBase<TRow> source, Func<TRow, TKey> keySelector, IComparer<TKey> comparer)
			where TRow : DataRow
		{
			return OrderedEnumerableRowCollection<TRow>.Create<TRow, TKey> (source, keySelector, comparer, true);
		}

		public static EnumerableRowCollection<S> Select<TRow, S> (this TypedTableBase<TRow> source, Func<TRow, S> selector)
			where TRow : DataRow
		{
			return new EnumerableRowCollection<S> (Enumerable.Select<TRow, S> (source, selector));
		}

		public static EnumerableRowCollection<TRow> Where<TRow> (this TypedTableBase<TRow> source, Func<TRow, bool> predicate)
			where TRow : DataRow
		{
			return new EnumerableRowCollection<TRow> (Enumerable.Where<TRow> (source, predicate));
		}
	}
}
