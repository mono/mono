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
	internal class QuerySelectManyNode<TSource, TCollection, TResult> : QueryStreamNode<TResult, TSource>
	{
		Func<TSource, IEnumerable<TCollection>> collectionSelector;
		Func<TSource, int, IEnumerable<TCollection>> collectionSelectorIndexed;
		Func<TSource, TCollection, TResult> resultSelector;
		
		internal QuerySelectManyNode (QueryBaseNode<TSource> parent,
		                              Func<TSource, int, IEnumerable<TCollection>> collectionSelectorIndexed,
		                              Func<TSource, TCollection, TResult> resultSelector)
			: base (parent, true)
		{
			this.collectionSelectorIndexed = collectionSelectorIndexed;
			this.resultSelector = resultSelector;
		}
		
		internal QuerySelectManyNode (QueryBaseNode<TSource> parent,
		                              Func<TSource, IEnumerable<TCollection>> collectionSelector,
		                              Func<TSource, TCollection, TResult> resultSelector)
			: base (parent, false)
		{
			this.collectionSelector = collectionSelector;
			this.resultSelector = resultSelector;
		}
		
		internal override IEnumerable<TResult> GetSequential ()
		{
			IEnumerable<TSource> source = Parent.GetSequential ();
			
			return IsIndexed ?
				source.SelectMany (collectionSelectorIndexed, resultSelector) :
				source.SelectMany (collectionSelector, resultSelector);
		}
		
		internal override IList<IEnumerable<TResult>> GetEnumerablesIndexed (QueryOptions options)
		{
			return Parent.GetOrderedEnumerables (options)
				.Select ((i) => GetEnumerableInternal (i,
				                                       (kv) => collectionSelectorIndexed (kv.Value, (int)kv.Key),
				                                       (e, c) => resultSelector (e.Value, c)))
				.ToList ();
		}

		internal override IList<IEnumerable<TResult>> GetEnumerablesNonIndexed (QueryOptions options)
		{
			return Parent.GetEnumerables (options)
				.Select ((i) => GetEnumerableInternal (i,
				                                       collectionSelector,
				                                       (e, c) => resultSelector (e, c)))
				.ToList ();
		}

		internal override IList<IEnumerable<KeyValuePair<long, TResult>>> GetOrderedEnumerables (QueryOptions options)
		{
			var source = Parent.GetOrderedEnumerables (options);
			var sizeRequests = new SizeRequest[source.Count];
			long deviation = 0;

			Barrier barrier = new Barrier (source.Count, delegate (Barrier b) {
					Array.Sort (sizeRequests, KeyComparator);
					for (int i = 0; i < b.ParticipantCount; ++i) {
						if (sizeRequests[i].Key == -1)
							continue;
						sizeRequests[i].Key = deviation;
						deviation += sizeRequests[i].Size;
					}
				});

			return source
				.Select ((i, ind) => GetOrderedEnumerableInternal (i, sizeRequests, ind, barrier))
				.ToList ();
		}
		
		IEnumerable<TResult> GetEnumerableInternal<T> (IEnumerable<T> source,
		                                               Func<T, IEnumerable<TCollection>> collectionner,
		                                               Func<T, TCollection, TResult> packer)
		{
			foreach (T element in source)
				foreach (TCollection item in collectionner (element))
					yield return packer (element, item);
		}
		
		IEnumerable<KeyValuePair<long, TResult>> GetOrderedEnumerableInternal (IEnumerable<KeyValuePair<long, TSource>> source,
		                                                                       SizeRequest[] sizeRequests,
		                                                                       int index,
		                                                                       Barrier barrier)
		{
			IEnumerator<KeyValuePair<long, TSource>> enumerator = source.GetEnumerator ();
			bool isIndexed = IsIndexed;

			try {
				while (true) {
					KeyValuePair<long, TSource> element;
					IEnumerable<TCollection> collection;

					if (enumerator.MoveNext ()) {
						element = enumerator.Current;
						collection = isIndexed ?
							collectionSelectorIndexed (element.Value, (int)element.Key) :
							collectionSelector (element.Value);

						var count = GetCount (ref collection);

						sizeRequests[index].Update (element.Key, count, collection, element.Value);
					}

					barrier.SignalAndWait ();

					long i = sizeRequests[index].Key;
					collection = sizeRequests[index].Collection;
					var elementValue = sizeRequests[index].ElementValue;

					sizeRequests[index].Clear ();

					if (i == -1)
						break;

					foreach (TCollection item in collection)
						yield return new KeyValuePair<long, TResult> (i++, resultSelector (elementValue, item));
				}
			} finally {
				barrier.RemoveParticipant ();
			}
		}

		/* If getting Count is a O(1) operation (i.e. actual is a ICollection) then return it immediatly
		 * if not process the IEnumerable into a List and return the Count from that (i.e. enumerable
		 * processing will only happen once in case of e.g. a Linq query)
		 */
		static int GetCount<T> (ref IEnumerable<T> actual)
		{
			ICollection coll = actual as ICollection;
			if (coll != null)
				return coll.Count;

			var foo = actual.ToList ();
			actual = foo;

			return foo.Count;
		}

		static int KeyComparator (SizeRequest e1, SizeRequest e2)
		{
			return e1.Key.CompareTo (e2.Key);
		}

		struct SizeRequest
		{
			public long Key;
			public int Size;
			public IEnumerable<TCollection> Collection;
			public TSource ElementValue;

			public void Update (long k, int s, IEnumerable<TCollection> c, TSource ev)
			{
				Key = k;
				Size = s;
				Collection = c;
				ElementValue = ev;
			}

			public void Clear ()
			{
				Key = -1;
				Size = 0;
				Collection = null;
				ElementValue = default (TSource);
			}
		}
	}
}

#endif
