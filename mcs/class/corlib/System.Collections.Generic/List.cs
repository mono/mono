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
	public class List<T> : IList<T>, ICollection<T>, IEnumerable<T>, IList, ICollection, IEnumerable
	{
		T [] data;
		int size;
		int version;
			
		const int DefaultCapacity = 4;
		
		public List ()
		{
		}
		
		public List (IEnumerable <T> collection)
		{
			AddRange (collection);
		}
		
		public List (int capacity)
		{
			data = new T [capacity];
		}
		
		public void Add (T item)
		{
			if (data == null)
				Capacity = DefaultCapacity;
			else if (size == data.Length)
				Capacity = Math.Max (Capacity * 2, DefaultCapacity);
			
			data [size ++] = item;
		}
		
		public void CheckRange (int idx, int count)
		{
			if (idx < 0 || count < 0 || idx + count < size)
				throw new ArgumentOutOfRangeException ();
		}
		
		[MonoTODO ("PERFORMANCE: fix if it is an IList <T>")]
		public void AddRange(IEnumerable<T> collection)
		{
			foreach (T t in collection)
				Add (t);
		}
		
		[MonoTODO]
		public IList<T> AsReadOnly ()
		{
			throw new NotImplementedException ();
		}
		
		public int BinarySearch(T item)
		{
			return BinarySearch (item, Comparer <T>.Default);
		}
		
		public int BinarySearch(T item, IComparer<T> comparer)
		{
			return BinarySearch (0, size, item, comparer);
		}
		
		public int BinarySearch(int index, int count, T item, IComparer<T> comparer)
		{
			CheckRange (index, count);
			return Array.BinarySearch (data, index, size, item, comparer);
		}
		
		public void Clear ()
		{
			if (data == null)
				return;
			
			Array.Clear (data, 0, data.Length);
		}
		
		public bool Contains (T item)
		{
			return IndexOf (item) != -1;
		}
		
		public List <U> ConvertAll <U> (Converter<T, U> converter)
		{
			List <U> u = new List <U> (size);
			int i = 0;
			foreach (T t in this)
				u [i ++] = converter (t);
			
			return u;
		}
		
		public void CopyTo (T [] array)
		{
			CopyTo (array, 0);
		}
		
		public void CopyTo (T [] array, int arrayIndex)
		{
			CopyTo (0, array, arrayIndex, size);
		}
		
		public void CopyTo (int index, T[] array, int arrayIndex, int count)
		{
			CheckRange (index, count);
			Array.Copy (data, index, array, arrayIndex, count);
		}

		public bool Exists (Predicate<T> match)
		{
			foreach (T t in this)
				if (match (t))
					return true;
			
			return false;
		}
		
		public T Find (Predicate<T> match)
		{
			foreach (T t in this)
				if (match (t))
					return t;
			
			return default (T);
		}
		
		// Maybe we could make this faster. For example, you could
		// make a bit set with stackalloc for which elements to copy
		// then you could size the array correctly.
		public List<T> FindAll (Predicate<T> match)
		{
			List <T> f = new List <T> ();
			
			foreach (T t in this)
				if (match (t))
					f.Add (t);
			
			return f;
		}
		
		public int FindIndex (Predicate <T> match)
		{
			return FindIndex (0, match);
		}
		
		public int FindIndex (int startIndex, Predicate <T> match)
		{
			return FindIndex (startIndex, size - startIndex, match);
		}
		
		public int FindIndex (int startIndex, int count, Predicate <T> match)
		{
			CheckRange (startIndex, count);
			
			for (int i = startIndex; i < startIndex + count; i ++)
				if (match (data [i]))
					return i;
				
			return -1;
		}
		
		public T FindLast (Predicate <T> match)
		{
			int i = FindLastIndex (match);
			return i == -1 ? default (T) : this [i];
		}
		
		public int FindLastIndex (Predicate <T> match)
		{
			return FindLastIndex (0, match);
		}
		
		public int FindLastIndex (int startIndex, Predicate <T> match)
		{
			return FindLastIndex (startIndex, size - startIndex, match);
		}
		
		public int FindLastIndex (int startIndex, int count, Predicate <T> match)
		{
			CheckRange (startIndex, count);
			for (int i = startIndex + count; i != startIndex;)
				if (match (data [--i]))
					return i;
				
			return -1;	
		}
		
		public void ForEach (Action <T> action)
		{
			foreach (T t in this)
				action (t);
		}
		
		public Enumerator <T> GetEnumerator ()
		{
			return new Enumerator <T> (this);
		}
		
		[MonoTODO]
		public List <T> GetRange (int index, int count)
		{
			throw new NotImplementedException ();
		}
		
		public int IndexOf (T item)
		{
			return IndexOf (item, 0);
		}
		
		public int IndexOf (T item, int index)
		{
			return IndexOf (item, index, size - index);
		}
		
		public int IndexOf (T item, int index, int count)
		{
			CheckRange (index, count);
			if (data == null)
				return -1;
			
			return Array.IndexOf (data, item, index, count);
		}
		
		void Shift (int start, int delta)
		{
			Array.Copy (data, start, data, start + delta, size - start);
		}
		
		public void Insert (int index, T item)
		{
			if ((uint) index < (uint) size)
				throw new ArgumentOutOfRangeException ();
			
			Shift (index, 1);
			size ++;
			this [index] = item;
				
		}
		[MonoTODO ("Performance for collection")]
		public void InsertRange (int index, IEnumerable<T> collection)
		{
			foreach (T t in collection)
				Insert (index ++, t);
		}
		
		public int LastIndexOf (T item)
		{
			return LastIndexOf  (item, 0);
		}
		
		public int LastIndexOf  (T item, int index)
		{
			return LastIndexOf  (item, index, size - index);
		}
		
		public int LastIndexOf (T item, int index, int count)
		{
			CheckRange (index, count);
			if (data == null)
				return -1;
			
			return Array.LastIndexOf (data, item, index, count);
		}
		
		public bool Remove (T item)
		{
			int loc = IndexOf (item);
			if (loc != -1)
				RemoveAt (loc);
			
			return loc != -1;
		}
		
		[MonoTODO ("I can make it faster than this...")]
		public int RemoveAll (Predicate<T> match)
		{
			int index = 0;
			int c = 0;
			while ((index = FindIndex (index, match)) != -1) {
				RemoveAt (index);
				c ++;
			}
			
			return c;
		}
		
		public void RemoveAt (int index)
		{
			RemoveRange (index, 1);
		}
		
		public void RemoveRange (int index, int count)
		{
			CheckRange (index, count);
			Shift (index, -count);
		}
		
		public void Reverse ()
		{
			Reverse (0, size);
		}
		public void Reverse (int index, int count)
		{
			CheckRange (index, count);
			Array.Reverse (data, index, count);
		}
		
		public void Sort ()
		{
			Sort (Comparer <T>.Default);
		}
		public void Sort (IComparer<T> comparer)
		{
			Sort (0, size, comparer);
		}
		
		// Waiting on Array
		[MonoTODO]
		public void Sort (Comparison<T> comparison)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void Sort (int index, int count, IComparer<T> comparer)
		{
			CheckRange (index, count);
			throw new NotImplementedException ();
		}

		public T [] ToArray ()
		{
			T [] t = new T [size];
			if (data != null)
				data.CopyTo (t, 0);
			
			return t;
		}
		
		public void TrimToSize ()
		{
			Capacity = size;
		}
		
		public bool TrueForAll (Predicate <T> match)
		{
			foreach (T t in this)
				if (!match (t))
					return false;
				
			return true;
		}
		
		public int Capacity {
			get { 
				if (data == null)
					return DefaultCapacity;
				return data.Length;
			}
			set {
				if ((uint) value < (uint) size)
					throw new ArgumentOutOfRangeException ();
				
				Array.Resize (ref data, value);
			}
		}
		
		public int Count {
			get { return size; }
		}
		
		public T this [int index] {
			get {
				if ((uint) index >= (uint) size)
					throw new IndexOutOfRangeException ();
				return data [index];
			}
			set {
				if ((uint) index >= (uint) size)
					throw new IndexOutOfRangeException ();
				data [index] = value;
			}
		}
		
#region Interface Crap
		IEnumerator <T> IEnumerable <T>.GetEnumerator()
		{
			return GetEnumerator ();
		}
		
		void ICollection.CopyTo (Array array, int arrayIndex)
		{
			Array.Copy (data, 0, data, arrayIndex, size);
		}
		
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator ();
		}
		
		int IList.Add (object item)
		{
			Add ((T) item);
			return size - 1;
		}
		
		bool IList.Contains (object item)
		{
			return Contains ((T) item);
		}
		
		int IList.IndexOf (object item)
		{
			return IndexOf ((T) item);
		}
		
		void IList.Insert (int index, object item)
		{
			Insert (index, (T) item);
		}
		
		void IList.Remove (object item)
		{
			Remove ((T) item);
		}
		
		bool ICollection <T>.IsReadOnly {
			get { return false; }
		}
		bool ICollection.IsSynchronized {
			get { return false; }
		}
		
		object ICollection.SyncRoot {
			get { return this; }
		}
		bool IList.IsFixedSize {
			get { return false; }
		}
		
		bool IList.IsReadOnly {
			get { return false; }
		}
		
		object IList.this [int index] {
			get { return this [index]; }
			set { this [index] = (T) value; }
		}
#endregion
		
		
		public struct Enumerator <T> : IEnumerator <T>, IEnumerator, IDisposable {
			const int NOT_STARTED = -2;
			
			// this MUST be -1, because we depend on it in move next.
			// we just decr the size, so, 0 - 1 == FINISHED
			const int FINISHED = -1;
			
			List <T> l;
			int idx;
			int ver;
			
			internal Enumerator (List <T> l)
			{
				this.l = l;
				idx = NOT_STARTED;
				ver = l.version;
			}
			
			// for some fucked up reason, MSFT added a useless dispose to this class
			// It means that in foreach, we must still do a try/finally. Broken, very
			// broken.
			public void Dispose ()
			{
				idx = NOT_STARTED;
			}
			
			public bool MoveNext ()
			{
				if (ver != l.version)
					throw new InvalidOperationException ();
				
				if (idx == NOT_STARTED)
					idx = l.size;
				
				return idx != FINISHED && -- idx != FINISHED;
			}
			
			public T Current {
				get {
					if (idx < 0)
						throw new InvalidOperationException ();
					
					return l.data [l.size - 1 - idx];
				}
			}
			
			void IEnumerator.Reset ()
			{
				if (ver != l.version)
					throw new InvalidOperationException ();
				
				idx = NOT_STARTED;
			}
			
			object IEnumerator.Current {
				get { return Current; }
			}
			
		}
	}
}
#endif
