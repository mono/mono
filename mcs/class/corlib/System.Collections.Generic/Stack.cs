// -*- Mode: csharp; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// System.Collections.Generic.Stack
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
	public class Stack<T> : ICollection<T>, IEnumerable<T>,
		ICollection, IEnumerable
	{
		int count;
		protected int modified;
		protected Node<T> head;

		public Stack ()
		{ }

		public void Clear ()
		{
			head = null;
			count = 0;
			modified++;
		}

		public void Push (T item)
		{
			head = new Node<T> (head, item);
			count++;
			modified++;
		}

		public T Peek ()
		{
			if (head == null)
				throw new ArgumentException ();

			return head.Item;
		}

		public T Pop ()
		{
			if (head == null)
				throw new ArgumentException ();

			T retval = head.Item;
			head = head.Next;
			count--;
			modified++;
			return retval;
		}

		public bool Contains (T item)
		{
			for (Node<T> node = head; node != null; node = node.Next)
				if (node.Item == item)
					return true;

			return false;
		}

		public virtual void CopyTo (T[] array, int start)
		{
			if (start + count >= array.Length)
				throw new ArgumentException ();

			for (Node<T> node = head; node != null; node = node.Next)
				array [start++] = node.Item;
		}

		public T[] ToArray ()
		{
			int pos = 0;
			T[] retval = new T [count];
			for (Node<T> node = head; node != null; node = node.Next)
				retval [pos++] = node.Item;

			return retval;
		}

		public void TrimToSize ()
		{ }

		public int Count {
			get { return count; }
		}

		public IEnumerator<T> GetEnumerator ()
		{
			return new Enumerator<T> (this);
		}

		protected sealed class Node<T>
		{
			public readonly T Item;
			public readonly Node<T> Next;

			public Node (Node<T> next, T item)
			{
				this.Next = next;
				this.Item = item;
			}
		}

		protected struct Enumerator<T> : IEnumerator<T>
		{
			Stack<T> stack;
			int modified;
			Node<T> current;

			public Enumerator (Stack<T> stack)
			{
				this.stack = stack;
				this.modified = stack.modified;
				this.current = stack.head;
			}

			public T Current {
				get {
					if (stack.modified != modified)
						throw new InvalidOperationException ();
					if (current == null)
						throw new ArgumentException ();
					return current.Item;
				}
			}

			public bool MoveNext ()
			{
				if (stack.modified != modified)
					throw new InvalidOperationException ();
				if (current == null)
					throw new ArgumentException ();

				current = current.Next;
				return current != null;
			}

			public void Dispose ()
			{
				modified = -1;
			}
		}
	}
}
#endif
