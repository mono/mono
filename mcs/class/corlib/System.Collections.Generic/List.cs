// -*- Mode: csharp; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// System.Collections.Generic.List
//
// Author:
//    Martin Baulig (martin@ximian.com)
//
// (C) 2004 Novell, Inc.
//

#if GENERICS
using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace System.Collections.Generic
{
	[CLSCompliant(false)]
	[ComVisible(false)]
	public class List<T> : IList<T>, ICollection<T>, IEnumerable<T>,
		ICollection, IEnumerable
	{
		protected int count;
		protected int capacity;
		protected T[] contents;
		protected int modified;

		protected void Resize (int size)
		{
			if (size <= capacity)
				return;

			T[] ncontents = new T [size];
			Array.Copy (contents, 0, ncontents, 0, count);

			modified++;
			contents = ncontents;
			capacity = size;
		}

		public int Add (T item)
		{
			if (count >= capacity)
				Resize (2 * capacity);

			contents [count] = item;
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

		public int IndexOf (T item)
		{
			for (int i = 0; i < count; i++)
				if (contents [i] == item)
					return i;

			return -1;
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

		public void Remove (T item)
		{
			int index = IndexOf (item);
			if (index >= 0)
				RemoveAt (index);
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

		public void CopyTo (T[] array, int arrayIndex)
		{
			Array.Copy (contents, 0, array, arrayIndex, count);
		}

		public int Count {
			get {
				return count;
			}
		}

		public IEnumerator<T> GetEnumerator ()
		{
			return new Enumerator (this);
		}

		protected class Enumerator : IEnumerator<T>
		{
			List<T> list;
			int modified;
			int current;

			public Enumerator (List<T> list)
			{
				this.list = list;
				this.modified = list.modified;
				this.current = 0;
			}

			public T Current {
				get {
					if (list.modified != modified)
						throw new InvalidOperationException ();
					if (current > list.count)
						throw new ArgumentException ();
					return list.contents [current];
				}
			}

			public bool MoveNext ()
			{
				if (list.modified != modified)
					throw new InvalidOperationException ();

				current++;
				return current < list.count;
			}

			public void Dispose ()
			{
				modified = -1;
			}
		}
	}
}
#endif
