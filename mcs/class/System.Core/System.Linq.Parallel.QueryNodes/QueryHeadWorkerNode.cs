//
// QueryConcatNode.cs
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

namespace System.Linq.Parallel.QueryNodes
{
	internal interface QueryHeadWorkerNode : IVisitableNode {
		int? Count { get; }
	}
	/* This is the QueryNode used by Take(While) operator
	 * it symbolize operators that are preferably working on the head elements of a query and can prematurely
	 * stop providing elements following the one they were processing is of a greater value in a specific 
	 * order to be defined by the instance (e.g. simple numerical order when working on indexes).
	 */
	internal class QueryHeadWorkerNode<TSource> : QueryStreamNode<TSource, TSource>, QueryHeadWorkerNode
	{
		/* This variable will receive an index value that represent the "stop point"
		 * when used with GetOrderedEnumerables i.e. values that are above the indexes are discarded
		 * (if the partitioner is ordered in a partition it even stop the processing) and value below are still tested just
		 * in case and can still lower this gap limit.
		 */
		readonly int count;
		readonly Func<TSource, int, bool> predicate;
		
		internal QueryHeadWorkerNode (QueryBaseNode<TSource> parent, int count)
			: base (parent, false)
		{
			this.count = count;
		}

		internal QueryHeadWorkerNode (QueryBaseNode<TSource> parent, Func<TSource, int, bool> predicate, bool indexed)
			: base (parent, indexed)
		{
			this.predicate = predicate;
		}

		public int? Count {
			get {
				return predicate == null ? count : (int?)null;
			}
		}

		internal override IEnumerable<TSource> GetSequential ()
		{
			IEnumerable<TSource> parent = Parent.GetSequential ();

			return predicate == null ? parent.Take (count) : parent.TakeWhile (predicate);
		}

		public override void Visit (INodeVisitor visitor)
		{
			visitor.Visit ((QueryHeadWorkerNode)this);
		}

		internal override IList<IEnumerable<TSource>> GetEnumerablesIndexed (QueryOptions options)
		{	
			return Parent.GetOrderedEnumerables (options)
				.Select ((i) => i.TakeWhile ((e) => predicate (e.Value, (int)e.Key)).Select ((e) => e.Value))
				.ToList ();
		}

		internal override IList<IEnumerable<TSource>> GetEnumerablesNonIndexed (QueryOptions options)
		{
			return Parent.GetEnumerables (options)
				.Select (GetSelector (count))
				.ToList ();
		}

		Func<IEnumerable<TSource>, IEnumerable<TSource>> GetSelector (int c)
		{
			if (predicate == null)
				return (i) => i.TakeWhile ((e) => c > 0 && Interlocked.Decrement (ref c) >= 0);
			else
				return (i) => i.TakeWhile ((e) => predicate (e, -1));
		}

		internal override IList<IEnumerable<KeyValuePair<long, TSource>>> GetOrderedEnumerables (QueryOptions options)
		{
			return Parent.GetOrderedEnumerables (options)
				.Select ((i) => GetEnumerableInternal (i, options))
				.ToList ();
		}

		IEnumerable<KeyValuePair<long, TSource>> GetEnumerableInternal (IEnumerable<KeyValuePair<long, TSource>> source, QueryOptions options)
		{
			IEnumerator<KeyValuePair<long, TSource>> current = source.GetEnumerator ();
			long gapIndex = predicate == null ? count : long.MaxValue;
			Func<KeyValuePair<long, TSource>, bool> cond;
			if (predicate == null)
				cond = (kv) => kv.Key < count;
			else
				cond = (kv) => predicate (kv.Value, (int)kv.Key);
			
			try {
				while (current.MoveNext ()) {
					KeyValuePair<long, TSource> kvp = current.Current;

					/* When filtering is based on a predicate, this short-circuit is only valid 
					 * if the partitioner used ensure items are ordered in each partition 
					 * (valid w/ default partitioners)
					 */
					if (kvp.Key >= gapIndex && options.PartitionerSettings.Item2)
						break;

					if (!cond (kvp)) {
						if (gapIndex > kvp.Key && predicate != null)
							gapIndex = kvp.Key;

						continue;
					}
					
					yield return kvp;
				}
			} finally {
				current.Dispose ();
			}
		}
	}

}

#endif