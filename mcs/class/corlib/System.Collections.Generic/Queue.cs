//
// System.Collections.Generic.Queue
//
// Author:
//    Martin Baulig (martin@ximian.com)
//    Ben Maurer (bmaurer@ximian.com)
//
// (C) 2003, 2004 Novell, Inc.
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
	public class Queue<T> : ICollection<T>, ICollection
	{
		T [] data;
		int head;
		int tail;
		int size;
		int version;
		
		public Queue ()
		{
		}
		
		public Queue (int count)
		{
			if (count < 0)
				throw new ArgumentOutOfRangeException ("count");
			
			data = new T [count];
		}
		
		public Queue (IEnumerable <T> collection)
		{
			if (collection == null)
				throw new ArgumentNullException ("collection");
			
				foreach (T t in collection)
					Enqueue (t);
		}
		
		public void Clear ()
		{
			Array.Clear (data, 0, data.Length);
		}
		
		public bool Contains (T item)
		{
			if (item == null) {
				foreach (T t in this)
					if (t == null)
						return true;
			} else {
				foreach (T t in this)
					if (item.Equals (t))
						return true;
			}
			
			return false;
		}
		
		public void CopyTo (T [] array, int idx)
		{
			if (array == null)
				throw new ArgumentNullException ();
			
			if ((uint) idx < (uint) array.Length)
				throw new ArgumentOutOfRangeException ();
			
			if (array.Length - idx < size)
				throw new ArgumentOutOfRangeException ();
			
			int contents_length = data.Length;
			int length_from_head = contents_length - head;
			
			Array.Copy (data, head, array, idx, Math.Min (size, length_from_head));
			if (size > length_from_head)
				Array.Copy (data, 0, array, 
					    idx  + length_from_head,
					    size - length_from_head);
			
		}
		
		void ICollection.CopyTo (Array array, int idx)
		{
			if (array == null)
				throw new ArgumentNullException ();
			
			if ((uint) idx < (uint) array.Length)
				throw new ArgumentOutOfRangeException ();
			
			if (array.Length - idx < size)
				throw new ArgumentOutOfRangeException ();
			
			if (size == 0)
				return;
			
			try {
				int contents_length = data.Length;
				int length_from_head = contents_length - head;
				
				Array.Copy (data, head, array, idx, Math.Min (size, length_from_head));
				if (size > length_from_head)
					Array.Copy (data, 0, array, 
						    idx  + length_from_head,
						    size - length_from_head);
			} catch (ArrayTypeMismatchException) {
				throw new ArgumentException ();
			}
		}
		
		public T Dequeue ()
		{
			T ret = Peek ();
			
			// clear stuff out to make the GC happy
			data [head] = default (T);
			
			if (++head == data.Length)
				head = 0;
			size --;
			version ++;
			
			return ret;
		}
		
		public T Peek ()
		{
			if (size == 0)
				throw new InvalidOperationException ();
			
			return data [head];
		}
		
		public void Enqueue (T item)
		{
			if (data == null || size == data.Length)
				SetCapacity (Math.Max (size * 2, 4));
			
			data [tail] = item;
			
			if (++tail == data.Length)
				tail = 0;
			
			size ++;
			version ++;
		}
		
		public T [] ToArray ()
		{
			T [] t = new T [size];
			CopyTo (t, 0);
			return t;
		}
		
		public void TrimToSize ()
		{
			SetCapacity (size);
		}
		
		void SetCapacity (int new_size)
		{
			if (data != null && new_size == data.Length)
				return;
			
			if (new_size < size)
				throw new InvalidOperationException ("shouldnt happen");
			
			T [] new_data = new T [new_size];
			if (size > 0)
				CopyTo (new_data, 0);
			
			data = new_data;
			tail = head = 0;
			version ++;
		}
		
		public int Count {
			get { return size; }
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
		
		void ICollection <T>.Add (T t)
		{
			Enqueue (t);
		}
		
		bool ICollection <T>.Remove (T t)
		{
			throw new InvalidOperationException ("");
		}
		

		public Enumerator <T> GetEnumerator ()
		{
			return new Enumerator <T> (this);
		}

		IEnumerator <T> IEnumerable<T>.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}
		
		public struct Enumerator <T> : IEnumerator <T>, IEnumerator, IDisposable {
			const int NOT_STARTED = -2;
			
			// this MUST be -1, because we depend on it in move next.
			// we just decr the size, so, 0 - 1 == FINISHED
			const int FINISHED = -1;
			
			Queue <T> q;
			int idx;
			int ver;
			
			internal Enumerator (Queue <T> q)
			{
				this.q = q;
				idx = NOT_STARTED;
				ver = q.version;
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
				if (ver != q.version)
					throw new InvalidOperationException ();
				
				if (idx == NOT_STARTED)
					idx = q.size;
				
				return idx != FINISHED && -- idx != FINISHED;
			}
			
			public T Current {
				get {
					if (idx < 0)
						throw new InvalidOperationException ();
					
					return q.data [(q.size - 1 - idx + q.head) % q.data.Length];
				}
			}
			
			void IEnumerator.Reset ()
			{
				if (ver != q.version)
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
