// ConcurrentQueue.cs
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

#if NET_4_0

using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace System.Collections.Concurrent
{

	[System.Diagnostics.DebuggerDisplay ("Count={Count}")]
	[System.Diagnostics.DebuggerTypeProxy (typeof (CollectionDebuggerView<>))]
	public class ConcurrentQueue<T> : IProducerConsumerCollection<T>, IEnumerable<T>, ICollection,
	                                  IEnumerable
	{
		class Node
		{
			public T Value;
			public Node Next;
		}
		
		Node head = new Node ();
		Node tail;
		int count;

		public ConcurrentQueue ()
		{
			tail = head;
		}
		
		public ConcurrentQueue (IEnumerable<T> collection): this()
		{
			foreach (T item in collection)
				Enqueue (item);
		}
		
		public void Enqueue (T item)
		{
			Node node = new Node ();
			node.Value = item;
			
			Node oldTail = null;
			Node oldNext = null;
			
			bool update = false;
			while (!update) {
				oldTail = tail;
				oldNext = oldTail.Next;
				
				// Did tail was already updated ?
				if (tail == oldTail) {
					if (oldNext == null) {
						// The place is for us
						update = Interlocked.CompareExchange (ref tail.Next, node, null) == null;
					} else {
						// another Thread already used the place so give him a hand by putting tail where it should be
						Interlocked.CompareExchange (ref tail, oldNext, oldTail);
					}
				}
			}
			// At this point we added correctly our node, now we have to update tail. If it fails then it will be done by another thread
			Interlocked.CompareExchange (ref tail, node, oldTail);
			Interlocked.Increment (ref count);
		}
		
		bool IProducerConsumerCollection<T>.TryAdd (T item)
		{
			Enqueue (item);
			return true;
		}

		public bool TryDequeue (out T result)
		{
			result = default (T);
			Node oldNext = null;
			bool advanced = false;

			while (!advanced) {
				Node oldHead = head;
				Node oldTail = tail;
				oldNext = oldHead.Next;
				
				if (oldHead == head) {
					// Empty case ?
					if (oldHead == oldTail) {
						// This should be false then
						if (oldNext != null) {
							// If not then the linked list is mal formed, update tail
							Interlocked.CompareExchange (ref tail, oldNext, oldTail);
							continue;
						}
						result = default (T);
						return false;
					} else {
						result = oldNext.Value;
						advanced = Interlocked.CompareExchange (ref head, oldNext, oldHead) == oldHead;
					}
				}
			}

			oldNext.Value = default (T);

			Interlocked.Decrement (ref count);

			return true;
		}
		
		public bool TryPeek (out T result)
		{
			result = default (T);
			bool update = true;
			
			while (update)
			{
				Node oldHead = head;
				Node oldNext = oldHead.Next;

				if (oldNext == null) {
					result = default (T);
					return false;
				}

				result = oldNext.Value;
				
				//check if head has been updated
				update = head != oldHead;
			}
			return true;
		}
		
		internal void Clear ()
		{
			count = 0;
			tail = head = new Node ();
		}
		
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return (IEnumerator)InternalGetEnumerator ();
		}
		
		public IEnumerator<T> GetEnumerator ()
		{
			return InternalGetEnumerator ();
		}
		
		IEnumerator<T> InternalGetEnumerator ()
		{
			Node my_head = head;
			while ((my_head = my_head.Next) != null) {
				yield return my_head.Value;
			}
		}
		
		void ICollection.CopyTo (Array array, int index)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			if (array.Rank > 1)
				throw new ArgumentException ("The array can't be multidimensional");
			if (array.GetLowerBound (0) != 0)
				throw new ArgumentException ("The array needs to be 0-based");

			T[] dest = array as T[];
			if (dest == null)
				throw new ArgumentException ("The array cannot be cast to the collection element type", "array");
			CopyTo (dest, index);
		}
		
		public void CopyTo (T[] array, int index)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			if (index < 0)
				throw new ArgumentOutOfRangeException ("index");
			if (index >= array.Length)
				throw new ArgumentException ("index is equals or greather than array length", "index");

			IEnumerator<T> e = InternalGetEnumerator ();
			int i = index;
			while (e.MoveNext ()) {
				if (i == array.Length - index)
					throw new ArgumentException ("The number of elememts in the collection exceeds the capacity of array", "array");
				array[i++] = e.Current;
			}
		}
		
		public T[] ToArray ()
		{
			return new List<T> (this).ToArray ();
		}
		
		bool ICollection.IsSynchronized {
			get { return true; }
		}

		bool IProducerConsumerCollection<T>.TryTake (out T item)
		{
			return TryDequeue (out item);
		}
		
		object syncRoot = new object();
		object ICollection.SyncRoot {
			get { return syncRoot; }
		}
		
		public int Count {
			get {
				return count;
			}
		}
		
		public bool IsEmpty {
			get {
				return count == 0;
			}
		}
	}
}
#endif
