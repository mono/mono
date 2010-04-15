#if NET_4_0
//
// QueryZipNode.cs
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
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace System.Linq
{
	internal class QueryZipNode<TFirst, TSecond, TResult> : QueryMuxNode<TFirst, TSecond, TResult>
	{
		Func<TFirst, TSecond, TResult> resultSelector;

		public QueryZipNode (Func<TFirst, TSecond, TResult> resultSelector, QueryBaseNode<TFirst> first, QueryBaseNode<TSecond> second)
			: base (first, second)
		{
			this.resultSelector = resultSelector;
		}

		internal override IEnumerable<TResult> GetSequential ()
		{
			IEnumerable<TFirst> first = Parent.GetSequential ();
			IEnumerable<TSecond> second = Second.GetSequential ();

			return first.Zip (second, resultSelector);
		}

		internal override IList<IEnumerable<TResult>> GetEnumerables (QueryOptions options)
		{
			IList<IEnumerable<TFirst>> first = Parent.GetEnumerables (options);
			IList<IEnumerable<TSecond>> second = Second.GetEnumerables (options);

			if (first.Count != second.Count)
				throw new InvalidOperationException ("Internal size mismatch");

			IEnumerable<TResult>[] result = new IEnumerable<TResult>[first.Count];

			for (int i = 0; i < result.Length; i++)
				result[i] = GetEnumerable (first[i], second[i]);

			return result;
		}

		IEnumerable<TResult> GetEnumerable (IEnumerable<TFirst> first, IEnumerable<TSecond> second)
		{
			IEnumerator<TFirst> eFirst = first.GetEnumerator ();
			IEnumerator<TSecond> eSecond = second.GetEnumerator ();

			while (eFirst.MoveNext ()) {
				if (!eSecond.MoveNext ())
					yield break;

				yield return resultSelector (eFirst.Current, eSecond.Current);
			}
		}

		internal override IList<IEnumerable<KeyValuePair<long, TResult>>> GetOrderedEnumerables (QueryOptions options)
		{
			IList<IEnumerable<KeyValuePair<long, TFirst>>> first = Parent.GetOrderedEnumerables (options);
			IList<IEnumerable<KeyValuePair<long, TSecond>>> second = Second.GetOrderedEnumerables (options);

			if (first.Count != second.Count)
				throw new InvalidOperationException ("Internal size mismatch");

			IEnumerable<KeyValuePair<long, TResult>>[] result = new IEnumerable<KeyValuePair<long, TResult>>[first.Count];

			KeyValuePair<long, TFirst>[] store1 = new KeyValuePair<long, TFirst>[result.Length];
			KeyValuePair<long, TSecond>[] store2 = new KeyValuePair<long, TSecond>[result.Length];

			Barrier barrier = new Barrier (result.Length, delegate {
				Array.Sort (store1, (e1, e2) => e1.Key.CompareTo (e2.Key));
				Array.Sort (store2, (e1, e2) => e1.Key.CompareTo (e2.Key));
			});

			for (int i = 0; i < result.Length; i++)
				result[i] = GetEnumerable (first[i], second[i], i, store1, store2, barrier);

			return result;
		}

		IEnumerable<KeyValuePair<long, TResult>> GetEnumerable (IEnumerable<KeyValuePair<long, TFirst>> first,
		                                                        IEnumerable<KeyValuePair<long, TSecond>> second,
		                                                        int index,
		                                                        KeyValuePair<long, TFirst>[] store1,
		                                                        KeyValuePair<long, TSecond>[] store2,
		                                                        Barrier barrier)
		{
			IEnumerator<KeyValuePair<long, TFirst>> eFirst = first.GetEnumerator ();
			IEnumerator<KeyValuePair<long, TSecond>> eSecond = second.GetEnumerator ();

			while (eFirst.MoveNext ()) {
				if (!eSecond.MoveNext ())
					break;

				store1[index] = eFirst.Current;
				store2[index] = eSecond.Current;

				barrier.SignalAndWait ();

				yield return new KeyValuePair<long, TResult> (store1[index].Key,
				                                                 resultSelector (store1[index].Value, store2[index].Value));
			}

			barrier.RemoveParticipant ();
		}
	}
}

#endif
