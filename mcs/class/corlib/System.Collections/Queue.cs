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

		private object[] _array;
		private int _head = 0;   // points to the first used slot
		private int _size = 0;
		private int _tail = 0;
		private int _growFactor;
		private int _version = 0;

		public Queue () : this (32, 2.0F) {}
		public Queue (int initialCapacity) : this (initialCapacity, 2.0F) {}
		public Queue(ICollection col) : this (col == null ? 32 : col.Count)
		{
			if (col == null)
				throw new ArgumentNullException ("col");
			
			_size = _array.Length;
			_tail = _size;
			col.CopyTo (_array, 0);
		}
			
		public Queue (int initialCapacity, float growFactor) {
			if (initialCapacity < 0)
				throw new ArgumentOutOfRangeException("capacity", "Needs a non-negative number");
			if (!(growFactor >= 1.0F && growFactor <= 10.0F))
				throw new ArgumentOutOfRangeException("growFactor", "Queue growth factor must be between 1.0 and 10.0, inclusive");
	    
			_array = new object[initialCapacity];

			this._growFactor = (int)(growFactor * 100);
		}
		
		// from ICollection

		public virtual int Count {
			get { return _size; }
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
			    || _size > array.Length - index)
				throw new ArgumentException ();
			
			int contents_length = _array.Length;
			int length_from_head = contents_length - _head;
			// copy the _array of the circular array
			Array.Copy (_array, _head, array, index,
				    Math.Min (_size, length_from_head));
			if (_size >  length_from_head)
				Array.Copy (_array, 0, array, 
					    index + length_from_head,
					    _size - length_from_head);
		}

		// from IEnumerable
		
		public virtual IEnumerator GetEnumerator () {
			return new QueueEnumerator (this);
		}

		// from ICloneable
		
		public virtual object Clone () {
			Queue newQueue;
			
			newQueue = new Queue (this._array.Length);
			newQueue._growFactor = _growFactor;
			
			Array.Copy (this._array, 0, newQueue._array, 0,
				    this._array.Length);
			newQueue._head = this._head;
			newQueue._size = this._size;
			newQueue._tail = this._tail;

			return newQueue;
		}

		public virtual void Clear () {
			_version++;
			_head = 0;
			_size = 0;
			_tail = 0;
			for (int length = _array.Length - 1; length >= 0; length--)
				_array [length] = null;
		}

		public virtual bool Contains (object obj) {
			int tail = _head + _size;
			if (obj == null) {
				for (int i = _head; i < tail; i++) {
					if (_array[i % _array.Length] == null) 
						return true;
				}
			} else {
				for (int i = _head; i < tail; i++) {
					if (obj.Equals (_array[i % _array.Length]))
						return true;
				}
			}
			return false;
		}
		
		public virtual object Dequeue ()
		{
			_version++;
			if (_size < 1)
				throw new InvalidOperationException ();
			object result = _array[_head];
			_array [_head] = null;
			_head = (_head + 1) % _array.Length;
			_size--;
			return result;
		}

		public virtual void Enqueue (object obj) {
			_version++;
			if (_size == _array.Length) 
				grow ();
			_array[_tail] = obj;
			_tail = (_tail+1) % _array.Length;
			_size++;

		}

		public virtual object Peek () {
			if (_size < 1)
				throw new InvalidOperationException ();
			return _array[_head];
		}

		public static Queue Synchronized (Queue queue) {
			if (queue == null) {
				throw new ArgumentNullException ("queue");
			}
			return new SyncQueue (queue);
		}

		public virtual object[] ToArray () {
			object[] ret = new object[_size];
			CopyTo (ret, 0);
			return ret;
		}

		public virtual void TrimToSize() {
			_version++;
			object[] trimmed = new object [_size];
			CopyTo (trimmed, 0);
			_array = trimmed;
			_head = 0;
			_tail = _head + _size;
		}

		// private methods

		private void grow () {
			int newCapacity = (_array.Length * _growFactor) / 100;
			object[] newContents = new object[newCapacity];
			CopyTo (newContents, 0);
			_array = newContents;
			_head = 0;
			_tail = _head + _size;
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
						return queue._size; 
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
					return new SyncQueue((Queue) queue.Clone ());
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

			public override void TrimToSize () {
				lock (queue) {
					queue.TrimToSize ();
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
			private int _version;
			private int current;

			internal QueueEnumerator (Queue q) {
				queue = q;
				_version = q._version;
				current = -1;  // one element before the _head
			}

			public object Clone ()
			{
				QueueEnumerator q = new QueueEnumerator (queue);
				q._version = _version;
				q.current = current;
				return q;
			}

			public virtual object Current {
				get {
					if (_version != queue._version 
					    || current < 0
					    || current >= queue._size)
						throw new InvalidOperationException ();
					return queue._array[(queue._head + current) % queue._array.Length];
				}
			}

			public virtual bool MoveNext () {
				if (_version != queue._version) {
					throw new InvalidOperationException ();
				}

				if (current >= queue._size - 1) {
					return false;
				} else {
					current++;
					return true;
				}
			}

			public virtual void Reset () {
				if (_version != queue._version) {
					throw new InvalidOperationException();
				}
				current = -1;
			}
		}
	}
}

