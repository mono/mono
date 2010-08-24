//
// QueryJoinNode.cs
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
	internal class QueryJoinNode<TFirst, TSecond, TKey, TResult> : QueryMuxNode<TFirst, TSecond, TResult>
	{
		struct VSlot<T>
		{
			public readonly bool HasValue;
			public readonly T Value;

			public VSlot (T value)
			{
				HasValue = true;
				Value = value;
			}
		}

		Func<TFirst, TKey> firstKeySelector;
		Func<TSecond, TKey> secondKeySelector;
		Func<TFirst, TSecond, TResult> resultSelector;
		IEqualityComparer<TKey> comparer;

		internal QueryJoinNode (QueryBaseNode<TFirst> first,
		                        QueryBaseNode<TSecond> second, 
		                        Func<TFirst, TKey> firstKeySelector,
		                        Func<TSecond, TKey> secondKeySelector,
		                        Func<TFirst, TSecond, TResult> resultSelector,
		                        IEqualityComparer<TKey> comparer) : base (first, second)
		{
			this.firstKeySelector = firstKeySelector;
			this.secondKeySelector = secondKeySelector;
			this.resultSelector = resultSelector;
			this.comparer = comparer;
		}

		internal override IEnumerable<TResult> GetSequential ()
		{
			return Parent.GetSequential ().Join (Second.GetSequential (), 
			                                     firstKeySelector, 
			                                     secondKeySelector, 
			                                     resultSelector, 
			                                     comparer);
		}

		internal override IList<IEnumerable<TResult>> GetEnumerables (QueryOptions options)
		{
			var first = Parent.GetEnumerables (options);
			var second = Second.GetEnumerables (options);

			if (first.Count != second.Count)
				throw new InvalidOperationException ("Internal size mismatch");

			var store = new ConcurrentDictionary<TKey, Tuple<VSlot<TFirst>, VSlot<TSecond>>> (comparer);

			return first
				.Select ((f, i) => GetEnumerable (f, second[i], store, firstKeySelector, secondKeySelector, resultSelector))
				.ToList ();

		}		

		internal override IList<IEnumerable<KeyValuePair<long, TResult>>> GetOrderedEnumerables (QueryOptions options)
		{
			var first = Parent.GetOrderedEnumerables (options);
			var second = Second.GetOrderedEnumerables (options);

			if (first.Count != second.Count)
				throw new InvalidOperationException ("Internal size mismatch");

			var store = new ConcurrentDictionary<TKey, Tuple<VSlot<KeyValuePair<long, TFirst>>, VSlot<KeyValuePair<long, TSecond>>>> (comparer);
			
			return first
				.Select ((f, i) => GetEnumerable<KeyValuePair<long, TFirst>, KeyValuePair<long, TSecond>, KeyValuePair<long, TResult>> (f, 
				                                  second[i], 
				                                  store,
				                                  (e) => firstKeySelector (e.Value),
				                                  (e) => secondKeySelector (e.Value),
				                                  (e1, e2) => new KeyValuePair<long, TResult> (e1.Key, resultSelector (e1.Value, e2.Value))))
				.ToList ();
		}

		IEnumerable<T> GetEnumerable<U, V, T> (IEnumerable<U> first, 
		                                       IEnumerable<V> second,
		                                       ConcurrentDictionary<TKey, Tuple<VSlot<U>, VSlot<V>>> store,
		                                       Func<U, TKey> fKeySelect,
		                                       Func<V, TKey> sKeySelect,
		                                       Func<U, V, T> resultor)
		{
			IEnumerator<U> eFirst = first.GetEnumerator ();
			IEnumerator<V> eSecond = second.GetEnumerator ();

			try {
				while (eFirst.MoveNext ()) {
					if (!eSecond.MoveNext ())
						yield break;

					U e1 = eFirst.Current;
					V e2 = eSecond.Current;

					TKey key1 = fKeySelect (e1);
					TKey key2 = sKeySelect (e2);

					if (comparer.Equals (key1, key2)) {
						yield return resultor (e1, e2);
						continue;
					}
					
					Tuple<VSlot<U>, VSlot<V>> kvp;
					
					do {
						if (store.TryRemove (key1, out kvp) && kvp.Item2.HasValue) {
							yield return resultor (e1, kvp.Item2.Value);
							break;
						}
					} while (!store.TryAdd (key1, Tuple.Create (new VSlot<U> (e1), new VSlot<V> ())));
							
					do {
						if (store.TryRemove (key2, out kvp) && kvp.Item1.HasValue) {
							yield return resultor (kvp.Item1.Value, e2);
							break;
						}
					} while (!store.TryAdd (key2, Tuple.Create (new VSlot<U> (), new VSlot<V> (e2))));
				}
			} finally {
				eFirst.Dispose ();
				eSecond.Dispose ();
			}
		}
	}
}

#endif
