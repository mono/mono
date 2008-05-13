//
// EnumerableRowCollection.cs
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
	public abstract class EnumerableRowCollection : IEnumerable
	{
		internal static IEnumerable<TResult> Cast<TResult> (IEnumerable source)
		{
			foreach (object o in source)
				yield return (TResult) o;
		}

		internal static IEnumerable<S> Select<TRow, S> (IEnumerable<TRow> source, Func<TRow, S> selector)
		{
			foreach (TRow row in source)
				yield return selector (row);
		}

		internal static IEnumerable<TRow> Where<TRow> (IEnumerable<TRow> source, Func<TRow, bool> predicate)
		{
			foreach (TRow row in source)
				if (predicate (row))
					yield return row;
		}

		internal EnumerableRowCollection ()
		{
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			// it is the documented behavior.
			return null;
		}
	}
}
