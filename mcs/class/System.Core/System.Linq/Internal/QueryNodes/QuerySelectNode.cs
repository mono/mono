#if NET_4_0
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
			if (IsIndexed)
				return Parent.GetSequential ().Select (indexedSelector);
			else
				return Parent.GetSequential ().Select (selector);
		}

		internal override IList<IEnumerable<TResult>> GetEnumerables (QueryOptions options)
		{
			if (IsIndexed) {
				IList<IEnumerable<KeyValuePair<long, TSource>>> sources = Parent.GetOrderedEnumerables (options);
				IEnumerable<TResult>[] results = new IEnumerable<TResult>[sources.Count];
				for (int i = 0; i < results.Length; i++)
					results[i] = sources[i].Select ((e) => indexedSelector (e.Value, (int)e.Key));
				return results;
			} else {
				IList<IEnumerable<TSource>> sources = Parent.GetEnumerables (options);
				IEnumerable<TResult>[] results = new IEnumerable<TResult>[sources.Count];

				for (int i = 0; i < results.Length; i++)
					results[i] = sources[i].Select (selector);
				return results;
			}
		}

		internal override IList<IEnumerable<KeyValuePair<long, TResult>>> GetOrderedEnumerables (QueryOptions options)
		{
			IList<IEnumerable<KeyValuePair<long, TSource>>> sources = Parent.GetOrderedEnumerables (options);
			IEnumerable<KeyValuePair<long, TResult>>[] results = new IEnumerable<KeyValuePair<long, TResult>>[sources.Count];

			if (IsIndexed) {
				for (int i = 0; i < results.Length; i++)
					results[i] = sources[i].
						Select ((e) => new KeyValuePair<long, TResult> (e.Key, indexedSelector (e.Value, (int)e.Key)));
			} else {
				for (int i = 0; i < results.Length; i++)
					results[i] = sources[i].
						Select ((e) => new KeyValuePair<long, TResult> (e.Key, selector (e.Value)));
			}

			return results;
		}
	}
}
#endif
