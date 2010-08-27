//
// QueryOrderByNode.cs
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
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace System.Linq.Parallel.QueryNodes
{
	internal class QueryGroupByNode<TSource, TKey, TElement> : QueryStreamNode<IGrouping<TKey, TElement>, TSource>
	{
		Func<TSource, TKey> keySelector;
		Func<TSource, TElement> elementSelector;
		IEqualityComparer<TKey> comparer;
		
		public QueryGroupByNode (QueryBaseNode<TSource> parent,
		                         Func<TSource, TKey> keySelector, 
		                         Func<TSource, TElement> elementSelector,
		                         IEqualityComparer<TKey> comparer)
			: base (parent, false)
		{
			this.keySelector = keySelector;
			this.elementSelector = elementSelector;
			this.comparer = comparer;
		}
		
		internal override IEnumerable<IGrouping<TKey, TElement>> GetSequential ()
		{
			IEnumerable<TSource> src =  Parent.GetSequential ();
			
			return src.GroupBy (keySelector, elementSelector, comparer);
		}

		internal override IList<IEnumerable<IGrouping<TKey, TElement>>> GetEnumerables (QueryOptions options)
		{			
			return ParallelPartitioner.CreateForChunks (GetGroupedElements ()).GetPartitions (options.PartitionCount).Wrap ();
		}
		
		internal override IList<IEnumerable<KeyValuePair<long, IGrouping<TKey, TElement>>>> GetOrderedEnumerables (QueryOptions options)
		{
			return ParallelPartitioner.CreateForChunks (GetGroupedElements ()).GetOrderablePartitions (options.PartitionCount).Wrap ();
		}

		internal IEnumerable<IGrouping<TKey, TElement>> GetGroupedElements ()
		{
			return GetStore ().Select ((e) => new ConcurrentGrouping<TKey, TElement> (e.Key, e.Value));
		}

		internal ConcurrentDictionary<TKey, ConcurrentQueue<TElement>> GetStore ()
		{
			var store = new ConcurrentDictionary<TKey, ConcurrentQueue<TElement>> ();

			ParallelExecuter.ProcessAndBlock (Parent, (e, c) => {
					ConcurrentQueue<TElement> queue = store.GetOrAdd (keySelector (e), (_) => new ConcurrentQueue<TElement> ());
					queue.Enqueue (elementSelector (e));
				});

			return store;
		}
	}
}

#endif
