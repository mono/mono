#if NET_4_0
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

using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace System.Collections.Concurrent
{
	public class BlockingCollection<T> : IEnumerable<T>, ICollection, IEnumerable, IDisposable
	{
		readonly IProducerConsumerCollection<T> underlyingColl;
		readonly int upperBound;
		readonly Func<bool> isFull;
		
		readonly SpinWait sw = new SpinWait ();
		
		AtomicBoolean isComplete;
		
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
			
			if (upperBound == -1)
				isFull = FalseIsFull;
			else
				isFull = CountBasedIsFull;
		}
		
		static bool FalseIsFull ()
		{
			return false;
		}
		
		bool CountBasedIsFull ()
		{
			return underlyingColl.Count >= upperBound;	
		}
		#endregion
		
		#region Add & Remove (+ Try)
		public void Add (T item)
		{
			while (true) {
				while (isFull ()) {
					if (isComplete.Value)
						throw new InvalidOperationException ("The BlockingCollection<T>"
						                                     + " has been marked as complete with regards to additions.");
					Block ();
				}
				// Extra check. The status might have changed after Block() or if isFull() is always false
				if (isComplete.Value)
					throw new InvalidOperationException ("The BlockingCollection<T> has"
					                                     + " been marked as complete with regards to additions.");
				// Go back in main waiting loop
				if (isFull ())
					continue;
				
				if (underlyingColl.TryAdd (item))
					break;
			}
		}
		
		public T Remove ()
		{
			T item;
			
			while (underlyingColl.Count == 0 || !underlyingColl.TryTake (out item)) {
				if (isComplete.Value)
					throw new OperationCanceledException ("The BlockingCollection<T> is empty and has been marked as complete with regards to additions.");
				Block ();
			}
			
			return item;
		}
		
		public bool TryAdd (T item)
		{
			if (isComplete.Value || isFull ()) {
					return false;
			}
			
			return underlyingColl.TryAdd (item);
		}
		
		public bool TryAdd (T item, TimeSpan ts)
		{
			return TryAdd (item, (int)ts.TotalMilliseconds);
		}
		
		public bool TryAdd (T item, int millisecondsTimeout)
		{
			Stopwatch sw = Stopwatch.StartNew ();
			while (isFull ()) {
				if (isComplete.Value || sw.ElapsedMilliseconds > millisecondsTimeout) {
					sw.Stop ();
					return false;
				}
				Block ();
			}
			return TryAdd (item);
		}
		
		public bool TryRemove (out T item)
		{
			return underlyingColl.TryTake (out item);
		}
		
		public bool TryRemove (out T item, TimeSpan ts)
		{
			return TryRemove (out item, (int)ts.TotalMilliseconds);
		}
		
		public bool TryRemove (out T item, int millisecondsTimeout)
		{
			Stopwatch sw = Stopwatch.StartNew ();
			while (underlyingColl.Count == 0) {
				if (isComplete.Value || sw.ElapsedMilliseconds > millisecondsTimeout) {
					item = default (T);
					return false;
				}
					
				Block ();
			}
			return TryRemove (out item);
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
		
		public static int AddAny (BlockingCollection<T>[] collections, T item)
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
		
		public static int TryAddAny (BlockingCollection<T>[] collections, T item)
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
		
		public static int TryAddAny (BlockingCollection<T>[] collections, T item, TimeSpan ts)
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
		
		public static int TryAddAny (BlockingCollection<T>[] collections, T item, int millisecondsTimeout)
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
		
		public static int RemoveAny (BlockingCollection<T>[] collections, out T item)
		{
			item = default (T);
			CheckArray (collections);
			int index = 0;
			foreach (var coll in collections) {
				try {
					item = coll.Remove ();
					return index;
				} catch {}
				index++;
			}
			return -1;
		}
		
		public static int TryRemoveAny (BlockingCollection<T>[] collections, out T item)
		{
			item = default (T);
			
			CheckArray (collections);
			int index = 0;
			foreach (var coll in collections) {
				if (coll.TryRemove (out item))
					return index;
				index++;
			}
			return -1;
		}
		
		public static int TryRemoveAny (BlockingCollection<T>[] collections, out T item, TimeSpan ts)
		{
			item = default (T);
			
			CheckArray (collections);
			int index = 0;
			foreach (var coll in collections) {
				if (coll.TryRemove (out item, ts))
					return index;
				index++;
			}
			return -1;
		}
		
		public static int TryRemoveAny (BlockingCollection<T>[] collections, out T item, int millisecondsTimeout)
		{
			item = default (T);
			
			CheckArray (collections);
			int index = 0;
			foreach (var coll in collections) {
				if (coll.TryRemove (out item, millisecondsTimeout))
					return index;
				index++;
			}
			return -1;
		}
		#endregion
		
		public void CompleteAdding ()
		{
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
			T item;
			while (underlyingColl.TryTake (out item)) {
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
		
		public IEnumerator<T> GetEnumerator ()
		{
			return ((IEnumerable<T>)underlyingColl).GetEnumerator ();
		}
		
		public void Dispose ()
		{
		}
		
		public T[] ToArray ()
		{
			return underlyingColl.ToArray ();
		}
		
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
				return isComplete.Value && underlyingColl.Count == 0;
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
