// -*- Mode: csharp; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// System.Collections.Generic.List
//
// Author:
//    Martin Baulig (martin@ximian.com)
//
// (C) 2004 Novell, Inc.
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
using System.Collections;
using System.Runtime.InteropServices;

namespace System.Collections.Generic
{
	[CLSCompliant(false)]
	[ComVisible(false)]
	public class List<T> : IList<T>, ICollection<T>, IEnumerable<T>,
		IList, ICollection, IEnumerable
	{
		protected int count;
		protected int capacity;
		protected T[] contents;
		protected int modified;

		public List ()
		{
		}

		public List (int capacity)
		{
			this.capacity = capacity;
			contents = new T [capacity];
		}

		public List (ICollection collection)
			: this (collection.Count)
		{
			collection.CopyTo (contents, 0);
			count = collection.Count;
		}

		protected void Resize (int size)
		{
			if (size < capacity)
				return;

			if (size < 10)
				size = 10;

			T[] ncontents = new T [size];
			if (count > 0)
				Array.Copy (contents, 0, ncontents, 0, count);

			modified++;
			contents = ncontents;
			capacity = size;
		}

		public void Add (T item)
		{
			if (count >= capacity)
				Resize (2 * capacity);

			contents [count] = item;
			count++;
		}

		int IList.Add (object item)
		{
			if (count >= capacity)
				Resize (2 * capacity);

			contents [count] = (T) item;
			return count++;
		}

		public void Clear ()
		{
			count = 0;
		}

		public bool Contains (T item)
		{
			for (int i = 0; i < count; i++)
				if (contents [i] == item)
					return true;

			return false;
		}

		bool IList.Contains (object item)
		{
			return Contains ((T) item);
		}

		public int IndexOf (T item)
		{
			for (int i = 0; i < count; i++)
				if (contents [i] == item)
					return i;

			return -1;
		}

		int IList.IndexOf (object item)
		{
			return IndexOf ((T) item);
		}

		public void Insert (int index, T item)
		{
			if (index < 0)
				throw new ArgumentException ();
			if (index > count)
				index = count;

			Resize (index);
			int rest = count - index;
			if (rest > 0)
				Array.Copy (contents, index, contents, index+1, rest);
			contents [index] = item;
		}

		void IList.Insert (int index, object item)
		{
			Insert (index, (T) item);
		}

		public bool Remove (T item)
		{
			int index = IndexOf (item);
			if (index < 0)
				return false;

			RemoveAt (index);
			return true;
		}

		void IList.Remove (object item)
		{
			Remove ((T) item);
		}

		public void RemoveAt (int index)
		{
			if ((index < 0) || (count == 0))
				throw new ArgumentException ();
			if (index > count)
				index = count;

			int rest = count - index;
			if (rest > 0)
				Array.Copy (contents, index+1, contents, index, rest);

			count--;
		}

		public bool IsFixedSize {
			get {
				return false;
			}
		}

		public bool IsReadOnly {
			get {
				return false;
			}
		}

		public T this [int index] {
			get {
				return contents [index];
			}

			set {
				contents [index] = value;
			}
		}

		object IList.this [int index] {
			get {
				return contents [index];
			}

			set {
				// contents [index] = (T) value;
			}
		}

		public void CopyTo (T[] array, int arrayIndex)
		{
			if (count > 0)
				Array.Copy (contents, 0, array, arrayIndex, count);
		}

		void ICollection.CopyTo (Array array, int arrayIndex)
		{
			if (count > 0)
				Array.Copy (contents, 0, array, arrayIndex, count);
		}

		public int Count {
			get {
				return count;
			}
		}

		public bool IsSynchronized {
			get { return false; }
		}

		public object SyncRoot {
			get { return this; }
		}

		public Enumerator GetEnumerator ()
		{
			return new Enumerator (this);
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator ()
		{
			return new Enumerator (this);
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return new Enumerator (this);
		}

		public struct Enumerator : IEnumerator<T>, IEnumerator
		{
			List<T> list;
			int modified;
			int current;

			public Enumerator (List<T> list)
			{
				this.list = list;
				this.modified = list.modified;
				this.current = -1;
			}

			public T Current {
				get {
					if (current < 0)
						throw new InvalidOperationException ("Enumeration has not been initialized; call MoveNext first.");
					if (current >= list.count)
						throw new InvalidOperationException ("Enumeration already finished.");
					
					return list.contents [current];
				}
			}

			object IEnumerator.Current {
				get {
					return Current;
				}
			}

			public bool MoveNext ()
			{
				if (list.modified != modified)
					throw new InvalidOperationException ("List was modified while enumerating.");

				current++;
				return current < list.count;
			}

			void IEnumerator.Reset ()
			{
				if (list.modified != modified)
					throw new InvalidOperationException ("List was modified while enumerating.");

				current = -1;
			}

			public void Dispose ()
			{
				modified = -1;
			}
		}
	}
}
#endif
