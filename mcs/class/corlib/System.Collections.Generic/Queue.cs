// -*- Mode: csharp; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// System.Collections.Generic.Queue
//
// Author:
//    Martin Baulig (martin@ximian.com)
//
// (C) 2003 Novell, Inc.
//

#if GENERICS
using System;
using System.Runtime.InteropServices;

namespace System.Collections.Generic
{
	[CLSCompliant(false)]
	[ComVisible(false)]
	public class Queue<T> : ICollection<T>, IEnumerable<T>,
		ICollection, IEnumerable
	{
		int count;
		protected int modified;
		protected Node head;
		Node tail;

		public void Clear ()
		{
			head = tail = null;
			count = 0;
			modified++;
		}

		public void Enqueue (T item)
		{
			tail = new Node (tail, item);
			if (head == null)
				head = tail;
			count++;
			modified++;
		}

		public T Peek ()
		{
			if (head == null)
				throw new ArgumentException ();

			return head.Item;
		}

		public T Dequeue ()
		{
			if (head == null)
				throw new ArgumentException ();

			T retval = head.Item;
			head = head.Next;
			if (head == null)
				tail = null;
			count--;
			modified++;
			return retval;
		}

		public bool Contains (T item)
		{
			for (Node node = head; node != null; node = node.Next)
				if (node.Item == item)
					return true;

			return false;
		}

		public virtual void CopyTo (T[] array, int start)
		{
			if (start + count >= array.Length)
				throw new ArgumentException ();

			for (Node node = head; node != null; node = node.Next)
				array [start++] = node.Item;
		}

		void ICollection.CopyTo (Array array, int start)
		{
			if (start + count >= array.Length)
				throw new ArgumentException ();

			for (Node node = head; node != null; node = node.Next)
				array.SetValue (node.Item, start++);
		}

		public T[] ToArray ()
		{
			int pos = 0;
			T[] retval = new T [count];
			for (Node node = head; node != null; node = node.Next)
				retval [pos++] = node.Item;

			return retval;
		}

		public void TrimToSize ()
		{ }

		public int Count {
			get { return count; }
		}

		public bool IsSynchronized {
			get { return false; }
		}

		public object SyncRoot {
			get { return this; }
		}

		public IEnumerator<T> GetEnumerator ()
		{
			return new Enumerator (this);
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return new Enumerator (this);
		}

		protected sealed class Node
		{
			public readonly T Item;
			public readonly Node Next;

			public Node (Node next, T item)
			{
				this.Next = next;
				this.Item = item;
			}
		}

		protected class Enumerator : IEnumerator<T>, IEnumerator
		{
			Queue<T> queue;
			int modified;
			Node current;

			public Enumerator (Queue<T> queue)
			{
				this.queue = queue;
				this.modified = queue.modified;
				this.current = queue.head;
			}

			public T Current {
				get {
					if (queue.modified != modified)
						throw new InvalidOperationException ();
					if (current == null)
						throw new ArgumentException ();
					return current.Item;
				}
			}

			object IEnumerator.Current {
				get {
					return Current;
				}
			}

			public bool MoveNext ()
			{
				if (queue.modified != modified)
					throw new InvalidOperationException ();
				if (current == null)
					throw new ArgumentException ();

				current = current.Next;
				return current != null;
			}

			public void Reset () {
				if (queue.modified != modified)
					throw new InvalidOperationException();

				current = queue.head;
			}

			public void Dispose ()
			{
				modified = -1;
			}
		}
	}
}
#endif
