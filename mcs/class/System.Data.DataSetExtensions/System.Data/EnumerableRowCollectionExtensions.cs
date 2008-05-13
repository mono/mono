//
// EnumerableRowCollectionExtensions.cs
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

namespace System.Data
{
	public static class EnumerableRowCollectionExtensions
	{
		[MonoTODO]
		public static EnumerableRowCollection<TResult> Cast<TResult> (this EnumerableRowCollection source)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static OrderedEnumerableRowCollection<TRow> OrderBy<TRow, TKey> (this EnumerableRowCollection<TRow> source, Func<TRow, TKey> keySelector)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static OrderedEnumerableRowCollection<TRow> OrderBy<TRow, TKey> (this EnumerableRowCollection<TRow> source, Func<TRow, TKey> keySelector, IComparer<TKey> comparer)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static OrderedEnumerableRowCollection<TRow> OrderByDescending<TRow, TKey> (this EnumerableRowCollection<TRow> source, Func<TRow, TKey> keySelector)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static OrderedEnumerableRowCollection<TRow> OrderByDescending<TRow, TKey> (this EnumerableRowCollection<TRow> source, Func<TRow, TKey> keySelector, IComparer<TKey> comparer)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static EnumerableRowCollection<S> Select<TRow, S> (this EnumerableRowCollection<TRow> source, Func<TRow, S> selector)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static OrderedEnumerableRowCollection<TRow> ThenBy<TRow, TKey> (this OrderedEnumerableRowCollection<TRow> source, Func<TRow, TKey> keySelector)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static OrderedEnumerableRowCollection<TRow> ThenBy<TRow, TKey> (this OrderedEnumerableRowCollection<TRow> source, Func<TRow, TKey> keySelector, IComparer<TKey> comparer)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static OrderedEnumerableRowCollection<TRow> ThenByDescending<TRow, TKey> (this OrderedEnumerableRowCollection<TRow> source, Func<TRow, TKey> keySelector)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static OrderedEnumerableRowCollection<TRow> ThenByDescending<TRow, TKey> (this OrderedEnumerableRowCollection<TRow> source, Func<TRow, TKey> keySelector, IComparer<TKey> comparer)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static EnumerableRowCollection<TRow> Where<TRow> (this EnumerableRowCollection<TRow> source, Func<TRow, bool> predicate)
		{
			throw new NotImplementedException ();
		}
	}
}
