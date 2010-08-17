//
// QuerySelectNode.cs
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
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace System.Linq
{
	internal class QuerySelectNode<TResult, TSource> : QueryStreamNode<TResult, TSource>
	{
		Func<TSource, int, TResult> indexedSelector;
		Func<TSource, TResult> selector;

		internal QuerySelectNode (QueryBaseNode<TSource> parent, Func<TSource, TResult> selector)
			: base (parent, false)
		{
			this.selector = selector;
		}

		internal QuerySelectNode (QueryBaseNode<TSource> parent, Func<TSource, int, TResult> selector)
			: base (parent, true)
		{
			this.indexedSelector = selector;
		}

		internal override IEnumerable<TResult> GetSequential ()
		{
			return IsIndexed ?
				Parent.GetSequential ().Select (indexedSelector) :
				Parent.GetSequential ().Select (selector);
		}

		internal override IList<IEnumerable<TResult>> GetEnumerablesIndexed (QueryOptions options)
		{
			return Parent.GetOrderedEnumerables (options)
				.Select ((i) => i.Select ((e) => indexedSelector (e.Value, (int)e.Key)))
				.ToArray ();
		}

		internal override IList<IEnumerable<TResult>> GetEnumerablesNonIndexed (QueryOptions options)
		{
			return Parent.GetEnumerables (options)
				.Select ((i) => i.Select (selector))
				.ToArray ();
		}

		internal override IList<IEnumerable<KeyValuePair<long, TResult>>> GetOrderedEnumerables (QueryOptions options)
		{
			return Parent.GetOrderedEnumerables (options)
				.Select ((i) => 
				         IsIndexed ? 
				         i.Select ((e) => new KeyValuePair<long, TResult> (e.Key, indexedSelector (e.Value, (int)e.Key))) :
				         i.Select ((e) => new KeyValuePair<long, TResult> (e.Key, selector (e.Value))))
				.ToArray ();
		}
	}
}
#endif
