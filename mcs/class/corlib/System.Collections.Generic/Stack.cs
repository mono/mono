// -*- Mode: csharp; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// System.Collections.Generic.Stack
//
// Author:
//    Martin Baulig (martin@ximian.com)
//
// (C) 2003 Novell, Inc.
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if NET_2_0
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
		protected Node head;

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
			head = new Node (head, item);
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
			for (Node node = head; node != null; node = node.Next)
				if (node.Item == item)
					return true;

			return false;
		}

		public virtual void CopyTo (T[] array, int start)
		{
			// re-ordered to avoid possible integer overflow
			if (start >= array.Length - count)
				throw new ArgumentException ();

			for (Node node = head; node != null; node = node.Next)
				array [start++] = node.Item;
		}

		void ICollection.CopyTo (Array array, int start)
		{
			// re-ordered to avoid possible integer overflow
			if (start >= array.Length - count)
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
			for (Node current = head; current != null; current = current.Next)
				yield return current.Item;
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			for (Node current = head; current != null; current = current.Next)
				yield return current.Item;
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
	}
}
#endif
