//
// System.Collections.Queue
//
// Author:
//    Ricardo Fernández Pascual
//
// (C) 2001 Ricardo Fernández Pascual
//

using System;
using System.Collections;

namespace System.Collections {

	[Serializable]
	public class Queue : ICollection, IEnumerable, ICloneable {

		private object[] contents;
		private int head = 0;   // points to the first used slot
		private int count = 0;
		private int capacity;
		private float growFactor;
		private int modCount = 0;

		public Queue () : this (32, 2.0F) {}
		public Queue (int initialCapacity) : this (initialCapacity, 2.0F) {}
		public Queue(ICollection col) : this (col == null ? 32 : col.Count)
		{
			if (col == null)
				throw new ArgumentNullException ("col");
			
			count = capacity;
			col.CopyTo (contents, 0);
		}
			
		public Queue (int initialCapacity, float growFactor) {
			if (initialCapacity < 0)
				throw new ArgumentOutOfRangeException("capacity", "Needs a non-negative number");
			if (!(growFactor >= 1.0F && growFactor <= 10.0F))
				throw new ArgumentOutOfRangeException("growFactor", "Queue growth factor must be between 1.0 and 10.0, inclusive");
	    
			capacity = initialCapacity;
			contents = new object[capacity];

			this.growFactor = growFactor;
		}

		// from ICollection

		public virtual int Count {
			get { return count; }
		}

		public virtual bool IsSynchronized {
			get { return false; }
		}

		public virtual object SyncRoot {
			get { return this; }
		}

		public virtual void CopyTo (Array array, int index)
		{
			if (array == null)
				throw new ArgumentNullException ("array");

			if (index < 0)
				throw new ArgumentOutOfRangeException ("index");

			if (array.Rank > 1 
			    || (index != 0 && index >= array.Length)
			    || count > array.Length - index)
				throw new ArgumentException ();
			
			int contents_length = contents.Length;
			int length_from_head = contents_length - head;
			// copy the contents of the circular array
			Array.Copy (contents, head, array, index,
				    Math.Min (count, length_from_head));
			if (count >  length_from_head)
				Array.Copy (contents, 0, array, 
					    index + length_from_head,
					    count - length_from_head);
		}

		// from IEnumerable
		
		public virtual IEnumerator GetEnumerator () {
			return new QueueEnumerator (this);
		}

		// from ICloneable
		
		public virtual object Clone () {
			Queue newQueue;
			
			newQueue = new Queue (); // FIXME: improve this...
			
			newQueue.contents = new object[this.contents.Length];
			Array.Copy (this.contents, 0, newQueue.contents, 0,
				    this.contents.Length);
			newQueue.head = this.head;
			newQueue.count = this.count;
			newQueue.capacity = this.capacity;
			newQueue.growFactor = this.growFactor;

			return newQueue;
		}

		// FIXME: should override Equals?

		// from Queue spec

/*
		public virtual bool IsReadOnly {
			get { return false; }
		}
*/

		public virtual void Clear () {
			modCount++;
			head = 0;
			count = 0;
			// FIXME: Should allocate a new contents array? 
			//        Should null the current array?
		}

		public virtual bool Contains (object obj) {
			int tail = head + count;
			if (obj == null) {
				for (int i = head; i < tail; i++) {
					if (contents[i % capacity] == null) 
						return true;
				}
			} else {
				for (int i = head; i < tail; i++) {
					if (obj.Equals (contents[i % capacity]))
						return true;
				}
			}
			return false;
		}
		
		public virtual object Dequeue ()
		{
			modCount++;
			if (count < 1)
				throw new InvalidOperationException ();
			object result = contents[head];
			contents [head] = null;
			head = (head + 1) % capacity;
			count--;
			return result;
		}

		public virtual void Enqueue (object obj) {
			modCount++;
			if (count == capacity) 
				grow ();
			contents[(head + count) % capacity] = obj;
			count++;

		}

		public virtual object Peek () {
			if (count < 1)
				throw new InvalidOperationException ();
			return contents[head];
		}

		public static Queue Synchronized (Queue queue) {
			if (queue == null) {
				throw new ArgumentNullException ();
			}
			return new SyncQueue (queue);
		}

		public virtual object[] ToArray () {
			object[] ret = new object[count];
			CopyTo (ret, 0);
			return ret;
		}

		public virtual void TrimToSize() {
			object[] trimmed = new object [count];
			CopyTo (trimmed, 0);
			contents = trimmed;
		}

		// private methods

		private void grow () {
			int newCapacity = (int) Math.Ceiling
				(contents.Length * growFactor);
			object[] newContents = new object[newCapacity];
			CopyTo (newContents, 0);
			contents = newContents;
                        capacity = newCapacity;
                        head = 0;
		}

		// private classes

		private class SyncQueue : Queue {
			Queue queue;
			
			internal SyncQueue (Queue queue) {
				this.queue = queue;
			}
			
			public override int Count {
				get { 
					lock (queue) {
						return queue.count; 
					}
				}
			}

			public override bool IsSynchronized {
				get { 
					return true;
				}
			}

			public override object SyncRoot {
				get { 
					return queue.SyncRoot; 
				}
			}

			public override void CopyTo (Array array, int index) {
				lock (queue) {
					queue.CopyTo (array, index);
				}
			}
			
			public override IEnumerator GetEnumerator () {
				lock (queue) {
					return queue.GetEnumerator ();
				}
			}
			
			public override object Clone () {
				lock (queue) {
					return queue.Clone ();
				}
			}
			
/*
			public override bool IsReadOnly {
				get { 
					lock (queue) {
						return queue.IsReadOnly;
					}
				}
			}
*/

			public override void Clear () {
				lock (queue) {
					queue.Clear ();
				}
			}

			public override bool Contains (object obj) {
				lock (queue) {
					return queue.Contains (obj);
				}
			}
		
			public override object Dequeue () {
				lock (queue) {
					return queue.Dequeue ();
				}
			}
			
			public override void Enqueue (object obj) {
				lock (queue) {
					queue.Enqueue (obj);
				}
			}

			public override object Peek () {
				lock (queue) {
					return queue.Peek ();
				}
			}

			public override object[] ToArray () {
				lock (queue) {
					return queue.ToArray ();
				}
			}
		}

		[Serializable]
		private class QueueEnumerator : IEnumerator, ICloneable {
			Queue queue;
			private int modCount;
			private int current;

			internal QueueEnumerator (Queue q) {
				queue = q;
				modCount = q.modCount;
				current = -1;  // one element before the head
			}

			public object Clone ()
			{
				QueueEnumerator q = new QueueEnumerator (queue);
				q.modCount = modCount;
				q.current = current;
				return q;
			}

			public virtual object Current {
				get {
					if (modCount != queue.modCount 
					    || current < 0
					    || current >= queue.count)
						throw new InvalidOperationException ();
					return queue.contents[(queue.head + current) % queue.contents.Length];
				}
			}

			public virtual bool MoveNext () {
				if (modCount != queue.modCount) {
					throw new InvalidOperationException ();
				}

				if (current >= queue.count - 1) {
					return false;
				} else {
					current++;
					return true;
				}
			}

			public virtual void Reset () {
				if (modCount != queue.modCount) {
					throw new InvalidOperationException();
				}
				current = -1;
			}
		}
	}
}

