//
// BlockingCollection.cs
//
// Copyright (c) 2008 Jérémie "Garuma" Laval
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
//
//

#if NET_4_0 || BOOTSTRAP_NET_4_0

using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace System.Collections.Concurrent
{
	[ComVisible (false)]
	[DebuggerDisplay ("Count={Count}")]
	[DebuggerTypeProxy (typeof (CollectionDebuggerView<>))]
	public class BlockingCollection<T> : IEnumerable<T>, ICollection, IEnumerable, IDisposable
	{
		readonly IProducerConsumerCollection<T> underlyingColl;
		readonly int upperBound;

		AtomicBoolean isComplete;
		long completeId;

		long addId = long.MinValue;
		long removeId = long.MinValue;

		#region ctors
		public BlockingCollection ()
			: this (new ConcurrentQueue<T> (), -1)
		{
		}

		public BlockingCollection (int upperBound)
			: this (new ConcurrentQueue<T> (), upperBound)
		{
		}

		public BlockingCollection (IProducerConsumerCollection<T> underlyingColl)
			: this (underlyingColl, -1)
		{
		}

		public BlockingCollection (IProducerConsumerCollection<T> underlyingColl, int upperBound)
		{
			this.underlyingColl = underlyingColl;
			this.upperBound     = upperBound;
			this.isComplete     = new AtomicBoolean ();
		}
		#endregion

		#region Add & Remove (+ Try)
		public void Add (T item)
		{
			Add (item, null);
		}

		public void Add (T item, CancellationToken token)
		{
			Add (item, () => token.IsCancellationRequested);
		}

		void Add (T item, Func<bool> cancellationFunc)
		{
			while (true) {
				long cachedAddId = addId;
				long cachedRemoveId = removeId;

				if (upperBound != -1) {
					if (cachedAddId - cachedRemoveId > upperBound) {
						Block ();
						continue;
					}
				}

				// Check our transaction id against completed stored one
				if (isComplete.Value && cachedAddId >= completeId)
					throw new InvalidOperationException ("The BlockingCollection<T> has"
					                                     + " been marked as complete with regards to additions.");

				if (Interlocked.CompareExchange (ref addId, cachedAddId + 1, cachedAddId) == cachedAddId)
					break;

				if (cancellationFunc != null && cancellationFunc ())
					throw new OperationCanceledException ("CancellationToken triggered");
			}


			if (!underlyingColl.TryAdd (item))
				throw new InvalidOperationException ("The underlying collection didn't accept the item.");
		}

		public T Take ()
		{
			return Take (null);
		}

		public T Take (CancellationToken token)
		{
			return Take (() => token.IsCancellationRequested);
		}

		T Take (Func<bool> cancellationFunc)
		{
			while (true) {
				long cachedRemoveId = removeId;
				long cachedAddId = addId;

				// Empty case
				if (cachedRemoveId == cachedAddId) {
					if (IsCompleted)
						throw new OperationCanceledException ("The BlockingCollection<T> has"
						                                      + " been marked as complete with regards to additions.");

					Block ();
					continue;
				}

				if (Interlocked.CompareExchange (ref removeId, cachedRemoveId + 1, cachedRemoveId) == cachedRemoveId)
					break;

				if (cancellationFunc != null && cancellationFunc ())
					throw new OperationCanceledException ("The CancellationToken has had cancellation requested.");
			}

			T item;
			while (!underlyingColl.TryTake (out item));

			return item;
		}

		public bool TryAdd (T item)
		{
			return TryAdd (item, null, null);
		}

		bool TryAdd (T item, Func<bool> contFunc, CancellationToken? token)
		{
			do {
				if (token.HasValue && token.Value.IsCancellationRequested)
					throw new OperationCanceledException ("The CancellationToken has had cancellation requested.");

				long cachedAddId = addId;
				long cachedRemoveId = removeId;

				if (upperBound != -1) {
					if (cachedAddId - cachedRemoveId > upperBound) {
						continue;
					}
				}

				// Check our transaction id against completed stored one
				if (isComplete.Value && cachedAddId >= completeId)
					throw new InvalidOperationException ("The BlockingCollection<T> has"
					                                     + " been marked as complete with regards to additions.");

				if (Interlocked.CompareExchange (ref addId, cachedAddId + 1, cachedAddId) != cachedAddId)
					continue;

				if (!underlyingColl.TryAdd (item))
					throw new InvalidOperationException ("The underlying collection didn't accept the item.");

				return true;
			} while (contFunc != null && contFunc ());

			return false;
		}

		public bool TryAdd (T item, TimeSpan ts)
		{
			return TryAdd (item, (int)ts.TotalMilliseconds);
		}

		public bool TryAdd (T item, int millisecondsTimeout)
		{
			Stopwatch sw = Stopwatch.StartNew ();
			return TryAdd (item, () => sw.ElapsedMilliseconds < millisecondsTimeout, null);
		}

		public bool TryAdd (T item, int millisecondsTimeout, CancellationToken token)
		{
			Stopwatch sw = Stopwatch.StartNew ();
			return TryAdd (item, () => sw.ElapsedMilliseconds < millisecondsTimeout, token);
		}

		public bool TryTake (out T item)
		{
			return TryTake (out item, null, null);
		}

		bool TryTake (out T item, Func<bool> contFunc, CancellationToken? token)
		{
			item = default (T);

			do {
				if (token.HasValue && token.Value.IsCancellationRequested)
					throw new OperationCanceledException ("The CancellationToken has had cancellation requested.");

				long cachedRemoveId = removeId;
				long cachedAddId = addId;

				// Empty case
				if (cachedRemoveId == cachedAddId) {
					if (IsCompleted)
						return false;

					continue;
				}

				if (Interlocked.CompareExchange (ref removeId, cachedRemoveId + 1, cachedRemoveId) != cachedRemoveId)
					continue;

				return underlyingColl.TryTake (out item);
			} while (contFunc != null && contFunc ());

			return false;
		}

		public bool TryTake (out T item, TimeSpan ts)
		{
			return TryTake (out item, (int)ts.TotalMilliseconds);
		}

		public bool TryTake (out T item, int millisecondsTimeout)
		{
			item = default (T);
			Stopwatch sw = Stopwatch.StartNew ();

			return TryTake (out item, () => sw.ElapsedMilliseconds < millisecondsTimeout, null);
		}

		public bool TryTake (out T item, int millisecondsTimeout, CancellationToken token)
		{
			item = default (T);
			Stopwatch sw = Stopwatch.StartNew ();

			return TryTake (out item, () => sw.ElapsedMilliseconds < millisecondsTimeout, token);
		}
		#endregion

		#region static methods
		static void CheckArray (BlockingCollection<T>[] collections)
		{
			if (collections == null)
				throw new ArgumentNullException ("collections");
			if (collections.Length == 0 || IsThereANullElement (collections))
				throw new ArgumentException ("The collections argument is a 0-length array or contains a null element.", "collections");
		}

		static bool IsThereANullElement (BlockingCollection<T>[] collections)
		{
			foreach (BlockingCollection<T> e in collections)
				if (e == null)
					return true;
			return false;
		}

		public static int AddToAny (BlockingCollection<T>[] collections, T item)
		{
			CheckArray (collections);
			int index = 0;
			foreach (var coll in collections) {
				try {
					coll.Add (item);
					return index;
				} catch {}
				index++;
			}
			return -1;
		}

		public static int AddToAny (BlockingCollection<T>[] collections, T item, CancellationToken token)
		{
			CheckArray (collections);
			int index = 0;
			foreach (var coll in collections) {
				try {
					coll.Add (item, token);
					return index;
				} catch {}
				index++;
			}
			return -1;
		}

		public static int TryAddToAny (BlockingCollection<T>[] collections, T item)
		{
			CheckArray (collections);
			int index = 0;
			foreach (var coll in collections) {
				if (coll.TryAdd (item))
					return index;
				index++;
			}
			return -1;
		}

		public static int TryAddToAny (BlockingCollection<T>[] collections, T item, TimeSpan ts)
		{
			CheckArray (collections);
			int index = 0;
			foreach (var coll in collections) {
				if (coll.TryAdd (item, ts))
					return index;
				index++;
			}
			return -1;
		}

		public static int TryAddToAny (BlockingCollection<T>[] collections, T item, int millisecondsTimeout)
		{
			CheckArray (collections);
			int index = 0;
			foreach (var coll in collections) {
				if (coll.TryAdd (item, millisecondsTimeout))
					return index;
				index++;
			}
			return -1;
		}

		public static int TryAddToAny (BlockingCollection<T>[] collections, T item, int millisecondsTimeout,
		                               CancellationToken token)
		{
			CheckArray (collections);
			int index = 0;
			foreach (var coll in collections) {
				if (coll.TryAdd (item, millisecondsTimeout, token))
					return index;
				index++;
			}
			return -1;
		}

		public static int TakeFromAny (BlockingCollection<T>[] collections, out T item)
		{
			item = default (T);
			CheckArray (collections);
			int index = 0;
			foreach (var coll in collections) {
				try {
					item = coll.Take ();
					return index;
				} catch {}
				index++;
			}
			return -1;
		}

		public static int TakeFromAny (BlockingCollection<T>[] collections, out T item, CancellationToken token)
		{
			item = default (T);
			CheckArray (collections);
			int index = 0;
			foreach (var coll in collections) {
				try {
					item = coll.Take (token);
					return index;
				} catch {}
				index++;
			}
			return -1;
		}

		public static int TryTakeFromAny (BlockingCollection<T>[] collections, out T item)
		{
			item = default (T);

			CheckArray (collections);
			int index = 0;
			foreach (var coll in collections) {
				if (coll.TryTake (out item))
					return index;
				index++;
			}
			return -1;
		}

		public static int TryTakeFromAny (BlockingCollection<T>[] collections, out T item, TimeSpan ts)
		{
			item = default (T);

			CheckArray (collections);
			int index = 0;
			foreach (var coll in collections) {
				if (coll.TryTake (out item, ts))
					return index;
				index++;
			}
			return -1;
		}

		public static int TryTakeFromAny (BlockingCollection<T>[] collections, out T item, int millisecondsTimeout)
		{
			item = default (T);

			CheckArray (collections);
			int index = 0;
			foreach (var coll in collections) {
				if (coll.TryTake (out item, millisecondsTimeout))
					return index;
				index++;
			}
			return -1;
		}

		public static int TryTakeFromAny (BlockingCollection<T>[] collections, out T item, int millisecondsTimeout,
		                                  CancellationToken token)
		{
			item = default (T);

			CheckArray (collections);
			int index = 0;
			foreach (var coll in collections) {
				if (coll.TryTake (out item, millisecondsTimeout, token))
					return index;
				index++;
			}
			return -1;
		}
		#endregion

		public void CompleteAdding ()
		{
		  // No further add beside that point
		  completeId = addId;
		  isComplete.Value = true;
		}

		void ICollection.CopyTo (Array array, int index)
		{
			underlyingColl.CopyTo (array, index);
		}

		public void CopyTo (T[] array, int index)
		{
			underlyingColl.CopyTo (array, index);
		}

		public IEnumerable<T> GetConsumingEnumerable ()
		{
			return GetConsumingEnumerable (Take);
		}

		public IEnumerable<T> GetConsumingEnumerable (CancellationToken token)
		{
			return GetConsumingEnumerable (() => Take (token));
		}

		IEnumerable<T> GetConsumingEnumerable (Func<T> getFunc)
		{
			while (true) {
				T item = default (T);

				try {
					item = getFunc ();
				} catch {
					break;
				}

				yield return item;
			}
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return ((IEnumerable)underlyingColl).GetEnumerator ();
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator ()
		{
			return ((IEnumerable<T>)underlyingColl).GetEnumerator ();
		}

		public void Dispose ()
		{

		}

		protected virtual void Dispose (bool managedRes)
		{

		}

		public T[] ToArray ()
		{
			return underlyingColl.ToArray ();
		}
		
		[ThreadStatic]
		SpinWait sw;

		// Method used to stall the thread for a limited period of time before retrying an operation
		void Block ()
		{
			sw.SpinOnce ();
		}

		public int BoundedCapacity {
			get {
				return upperBound;
			}
		}

		public int Count {
			get {
				return underlyingColl.Count;
			}
		}

		public bool IsAddingCompleted {
			get {
				return isComplete.Value;
			}
		}

		public bool IsCompleted {
			get {
				return isComplete.Value && addId == removeId;
			}
		}

		object ICollection.SyncRoot {
			get {
				return underlyingColl.SyncRoot;
			}
		}

		bool ICollection.IsSynchronized {
			get {
				return underlyingColl.IsSynchronized;
			}
		}
	}
}
#endif
