//
// QueryWhereNode.cs
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
using System.Threading;
using System.Collections.Generic;

namespace System.Linq.Parallel.QueryNodes
{

	internal class QueryWhereNode<TSource> : QueryStreamNode<TSource, TSource>
	{
		Func<TSource, int, bool> indexedPredicate;
		Func<TSource, bool> predicate;

		internal QueryWhereNode (QueryBaseNode<TSource> parent, Func<TSource, bool> predicate)
			: base (parent, false)
		{
			this.predicate = predicate;
		}

		internal QueryWhereNode (QueryBaseNode<TSource> parent, Func<TSource, int, bool> predicate)
			: base (parent, true)
		{
			this.indexedPredicate = predicate;
		}

		internal override IEnumerable<TSource> GetSequential ()
		{
			IEnumerable<TSource> parent = Parent.GetSequential ();

			if (indexedPredicate != null)
				return parent.Where (indexedPredicate);
			else
				return parent.Where (predicate);
		}

		internal override IList<IEnumerable<TSource>> GetEnumerablesIndexed (QueryOptions options)
		{
			return Parent.GetOrderedEnumerables (options)
				.Select (i => i.Where (e => indexedPredicate (e.Value, (int)e.Key)).Select (e => e.Value))
				.ToList ();
		}

		internal override IList<IEnumerable<TSource>> GetEnumerablesNonIndexed (QueryOptions options)
		{
			return Parent.GetEnumerables (options)
				.Select (i => i.Where (predicate))
				.ToList ();
		}

		internal override IList<IEnumerable<KeyValuePair<long, TSource>>> GetOrderedEnumerables (QueryOptions options)
		{
			IList<IEnumerable<KeyValuePair<long, TSource>>> sources = Parent.GetOrderedEnumerables (options);

			ProcessingSlot[] store = new ProcessingSlot[sources.Count];
			Comparison<ProcessingSlot> arrayComparison = ArraySortMethod;
			long lastIndex = 0;

			Barrier barrier = new Barrier (sources.Count, delegate (Barrier b) {
				// Sort the store
				Array.Sort (store, arrayComparison);

				// Reassign a good index
				int i = 0;
				for (i = 0; i < store.Length && store[i].IsValid; ++i)
					store[i].Index = lastIndex + i;

				// Update lastIndex for next round
				lastIndex += i;
			});

			return sources
				.Select ((s, i) => GetEnumerator (s, barrier, options.MergedToken, store, i))
				.ToList ();
		}

		static int ArraySortMethod (ProcessingSlot lhs, ProcessingSlot rhs)
		{
			if (lhs.IsValid && !rhs.IsValid)
				return -1;
			if (!lhs.IsValid && rhs.IsValid)
				return 1;
			if (!lhs.IsValid && !rhs.IsValid)
				return 0;

			return lhs.Index < rhs.Index ? -1 : 1;
		}

		IEnumerable<KeyValuePair<long, TSource>> GetEnumerator (IEnumerable<KeyValuePair<long, TSource>> source,
		                                                        Barrier barrier,
		                                                        CancellationToken token,
		                                                        ProcessingSlot[] store, int index)
		{
			IEnumerator<KeyValuePair<long, TSource>> current = source.GetEnumerator ();
			bool isIndexed = IsIndexed;

			try {
				while (current.MoveNext ()) {
					KeyValuePair<long, TSource> curr = current.Current;

					bool result = isIndexed ? indexedPredicate (curr.Value, (int)curr.Key) : predicate (curr.Value);
					store[index].Update (curr.Value, curr.Key, result);

					barrier.SignalAndWait (token);

					var value = store [index];

					if (value.IsValid)
						yield return new KeyValuePair<long, TSource> (value.Index, value.Value);

					// Reset
					store[index].Clear ();
				}
			} finally {
				// Remove our participation
				barrier.RemoveParticipant ();
				current.Dispose ();
			}
		}

		struct ProcessingSlot
		{
			public TSource Value;
			public long Index;
			public bool IsValid;

			public void Update (TSource v, long i, bool t)
			{
				Value = v;
				Index = i;
				IsValid = t;
			}

			public void Clear ()
			{
				Update (default (TSource), long.MaxValue, false);
			}
		}
	}
}
#endif
