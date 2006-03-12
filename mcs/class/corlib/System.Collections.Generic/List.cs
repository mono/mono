//
// System.Collections.Generic.List
//
// Authors:
//    Ben Maurer (bmaurer@ximian.com)
//    Martin Baulig (martin@ximian.com)
//    Carlos Alberto Cortez (calberto.cortez@gmail.com)
//    David Waite (mass@akuma.org)
//
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
// Copyright (C) 2005 David Waite
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

using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

namespace System.Collections.Generic {
	[Serializable]
	public class List <T> : IList <T>, IList, ICollection {
		T [] data;
		int size;
		int version;
		
		static readonly T [] EmptyArray = new T [0]; 
		const int DefaultCapacity = 4;
		
		public List ()
		{
			data = EmptyArray;
		}
		
		public List (IEnumerable <T> collection)
		{
			CheckCollection (collection);

			// initialize to needed size (if determinable)
			ICollection <T> c = collection as ICollection <T>;
			if (c == null)
			{
				data = EmptyArray;
				AddEnumerable (collection);
			}
			else
			{
				data = new T [c.Count];
				AddCollection (c);
			}
		}
		
		public List (int capacity)
		{
			if (capacity < 0)
				throw new ArgumentOutOfRangeException ("capacity");
			data = new T [capacity];
		}
		
		internal List (T [] data, int size)
		{
			this.data = data;
			this.size = size;
		}
		public void Add (T item)
		{
			GrowIfNeeded (1);
			data [size ++] = item;
		}
		
		void GrowIfNeeded (int newCount)
		{
			int minimumSize = size + newCount;
			if (minimumSize > data.Length)
				Capacity = Math.Max (Math.Max (Capacity * 2, DefaultCapacity), minimumSize);
		}
		
		void CheckRange (int idx, int count)
		{
			if (idx < 0)
				throw new ArgumentOutOfRangeException ("index");
			
			if (count < 0)
				throw new ArgumentOutOfRangeException ("count");

			if ((uint) idx + (uint) count > (uint) size)
				throw new ArgumentException ("index and count exceed length of list");
		}
		
		void AddCollection (ICollection <T> collection)
		{
			int collectionCount = collection.Count;
			GrowIfNeeded (collectionCount);			 
			collection.CopyTo (data, size);
			size += collectionCount;
		}
		void AddEnumerable (IEnumerable <T> enumerable)
		{
			foreach (T t in enumerable)
			{
				Add (t);
			}
		}

		public void AddRange (IEnumerable <T> collection)
		{
			CheckCollection (collection);
			
			ICollection <T> c = collection as ICollection <T>;
			if (c != null)
				AddCollection (c);
			else
				AddEnumerable (collection);
		}
		
		public ReadOnlyCollection <T> AsReadOnly ()
		{
			return new ReadOnlyCollection <T> (this);
		}
		
		public int BinarySearch (T item)
		{
			return Array.BinarySearch <T> (data, 0, size, item);
		}
		
		public int BinarySearch (T item, IComparer <T> comparer)
		{
			return Array.BinarySearch <T> (data, 0, size, item, comparer);
		}
		
		public int BinarySearch (int index, int count, T item, IComparer <T> comparer)
		{
			CheckRange (index, count);
			return Array.BinarySearch <T> (data, index, count, item, comparer);
		}
		
		public void Clear ()
		{
			Array.Clear (data, 0, data.Length);
			size = 0;
		}
		
		public bool Contains (T item)
		{
			return IndexOf (item) != -1;
		}
		
		public List <TOutput> ConvertAll <TOutput> (Converter <T, TOutput> converter)
		{
			if (converter == null)
				throw new ArgumentNullException ("converter");
			List <TOutput> u = new List <TOutput> (size);
			foreach (T t in this)
				u.Add (converter (t));
			return u;
		}
		
		public void CopyTo (T [] array)
		{
			Array.Copy (data, 0, array, 0, size);
		}
		
		public void CopyTo (T [] array, int arrayIndex)
		{
			Array.Copy (data, 0, array, arrayIndex, size);
		}
		
		public void CopyTo (int index, T [] array, int arrayIndex, int count)
		{
			CheckRange (index, count);
			Array.Copy (data, index, array, arrayIndex, count);
		}

		public bool Exists (Predicate <T> match)
		{
			return FindIndex (match) != -1;
		}
		
		public T Find (Predicate <T> match)
		{
			int i = FindIndex (match);
			return (i != -1) ? data [i] : default (T);
		}
		void CheckMatch (Predicate <T> match)
		{
			if (match == null)
				throw new ArgumentNullException ("match");
		}
		
		// Maybe we could make this faster. For example, you could
		// make a bit set with stackalloc for which elements to copy
		// then you could size the array correctly.
		public List <T> FindAll (Predicate <T> match)
		{
			CheckMatch (match);
			List <T> f = new List <T> ();
			
			foreach (T t in this)
				if (match (t))
					f.Add (t);
			
			return f;
		}
		
		public int FindIndex (Predicate <T> match)
		{
			CheckMatch (match);
			return GetIndex (0, size, match);
		}
		
		public int FindIndex (int startIndex, Predicate <T> match)
		{
			CheckMatch (match);
			CheckIndex (startIndex);
			return GetIndex (startIndex, size - startIndex, match);
		}
		public int FindIndex (int startIndex, int count, Predicate <T> match)
		{
			CheckMatch (match);
			CheckRange (startIndex, count);
			return GetIndex (startIndex, count, match);
		}
		int GetIndex (int startIndex, int count, Predicate <T> match)
		{
			for (int i = startIndex; i < startIndex + count; i ++)
				if (match (data [i]))
					return i;
				
			return -1;
		}
		
		public T FindLast (Predicate <T> match)
		{
			CheckMatch (match);
			int i = GetLastIndex (0, size, match);
			return i == -1 ? default (T) : this [i];
		}
		
		public int FindLastIndex (Predicate <T> match)
		{
			CheckMatch (match);
			return GetLastIndex (0, size, match);
		}
		
		public int FindLastIndex (int startIndex, Predicate <T> match)
		{
			CheckMatch (match);
			CheckIndex (startIndex);
			return GetLastIndex (0, startIndex + 1, match);
		}
		
		public int FindLastIndex (int startIndex, int count, Predicate <T> match)
		{
			CheckMatch (match);
			int start = startIndex - count + 1;
			CheckRange (start, count);
			return GetLastIndex (start, count, match);
		}

		int GetLastIndex (int startIndex, int count, Predicate <T> match)
		{
			// unlike FindLastIndex, takes regular params for search range
			for (int i = startIndex + count; i != startIndex;)
				if (match (data [--i]))
					return i;
			return -1;	
		}
		
		public void ForEach (Action <T> action)
		{
			if (action == null)
				throw new ArgumentNullException ("action");
			foreach (T t in this)
				action (t);
		}
		
		public Enumerator GetEnumerator ()
		{
			return new Enumerator (this);
		}
		
		public List <T> GetRange (int index, int count)
		{
			CheckRange (index, count);
			T [] tmpArray = new T [count];
			Array.Copy (data, index, tmpArray, 0, count);
			return new List <T> (tmpArray, count);
		}
		
		public int IndexOf (T item)
		{
			return Array.IndexOf (data, item, 0, size);
		}
		
		public int IndexOf (T item, int index)
		{
			CheckIndex (index);
			return Array.IndexOf (data, item, index, size - index);
		}
		
		public int IndexOf (T item, int index, int count)
		{
			if (index < 0)
				throw new ArgumentOutOfRangeException ("index");
			
			if (count < 0)
				throw new ArgumentOutOfRangeException ("count");

			if ((uint) index + (uint) count > (uint) size)
				throw new ArgumentOutOfRangeException ("index and count exceed length of list");

			return Array.IndexOf (data, item, index, count);
		}
		
		void Shift (int start, int delta)
		{
			if (delta < 0)
				start -= delta;
			
			Array.Copy (data, start, data, start + delta, size - start);
			
			size += delta;
		}

		void CheckIndex (int index)
		{
			if ((uint) index >= (uint) size)
				throw new ArgumentOutOfRangeException ("index");
		}
		
		public void Insert (int index, T item)
		{
			if ((uint) index > (uint) size)
				throw new ArgumentOutOfRangeException ("index");
			
			GrowIfNeeded (1);
			Shift (index, 1);
			this [index] = item;
				
		}

		void CheckCollection (IEnumerable <T> collection)
		{
			if (collection == null)
				throw new ArgumentNullException ("collection");
		}
		
		public void InsertRange (int index, IEnumerable <T> collection)
		{
			CheckCollection (collection);
			CheckIndex (index);
			ICollection <T> c = collection as ICollection <T>;
			if (c != null)
				InsertCollection (index, c);
			else
				InsertEnumeration (index, collection);
		}

		void InsertCollection (int index, ICollection <T> collection)
		{
			int collectionCount = collection.Count;
			GrowIfNeeded (collectionCount);
			
			Shift (index, collectionCount);
			collection.CopyTo (data, index);
		}
		void InsertEnumeration (int index, IEnumerable <T> enumerable)
		{
			foreach (T t in enumerable)
				Insert (index++, t);		
		}

		public int LastIndexOf (T item)
		{
			return Array.LastIndexOf (data, item, 0, size);
		}
		
		public int LastIndexOf (T item, int index)
		{
			CheckIndex (index);
			return Array.LastIndexOf (data, item, index, size - index);
		}
		
		public int LastIndexOf (T item, int index, int count)
		{
			CheckRange (index, count);			 
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
		public int RemoveAll (Predicate <T> match)
		{
			CheckMatch (match);

			int index = 0;
			int c = 0;
			while ((index = GetIndex (index, size - index, match)) != -1) {
				RemoveAt (index);
				c ++;
			}
			
			Array.Clear (data, size, c);
			return c;
		}
		
		public void RemoveAt (int index)
		{
			CheckIndex (index);
			Shift (index, -1);
			Array.Clear (data, size, 0);
		}
		
		public void RemoveRange (int index, int count)
		{
			CheckRange (index, count);
			Shift (index, -count);
			Array.Clear (data, size, count);
		}
		
		public void Reverse ()
		{
			Array.Reverse (data, 0, size);
		}
		public void Reverse (int index, int count)
		{
			CheckRange (index, count);
			Array.Reverse (data, index, count);
		}
		
		public void Sort ()
		{
			Array.Sort<T> (data, 0, size, Comparer <T>.Default);
		}
		public void Sort (IComparer <T> comparer)
		{
			Array.Sort<T> (data, 0, size, comparer);
		}
		
		// Waiting on Array
		[MonoTODO]
		public void Sort (Comparison <T> comparison)
		{
			throw new NotImplementedException ();
		}
		
		public void Sort (int index, int count, IComparer <T> comparer)
		{
			CheckRange (index, count);
			Array.Sort<T> (data, index, count, comparer);
		}

		public T [] ToArray ()
		{
			T [] t = new T [size];
			Array.Copy (data, t, size);
			
			return t;
		}
		
		public void TrimExcess ()
		{
			Capacity = size;
		}
		
		public bool TrueForAll (Predicate <T> match)
		{
			CheckMatch (match);

			foreach (T t in this)
				if (!match (t))
					return false;
				
			return true;
		}
		
		public int Capacity {
			get { 
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
					throw new ArgumentOutOfRangeException ("index");
				return data [index];
			}
			set {
				CheckIndex (index);
				data [index] = value;
			}
		}
		
#region Interface implementations.
		IEnumerator <T> IEnumerable <T>.GetEnumerator ()
		{
			return GetEnumerator ();
		}
		
		void ICollection.CopyTo (Array array, int arrayIndex)
		{
			Array.Copy (data, 0, array, arrayIndex, size);
		}
		
		IEnumerator IEnumerable.GetEnumerator ()
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
				
		[Serializable]
		public struct Enumerator : IEnumerator <T>, IDisposable {
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
