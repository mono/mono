//
// System.Collections.Generic.Stack
//
// Authors:
//	Martin Baulig (martin@ximian.com)
//	Ben Maurer (bmaurer@ximian.com)
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
	[CLSCompliant (false)]
	[ComVisible (false)]
	public class Stack <T> : ICollection <T>, IEnumerable <T>, ICollection, IEnumerable {
		
		T [] data;
		int size;
		int ver;
		
		public Stack ()
		{
		}
		
		public Stack (int count)
		{
			if (count < 0)
				throw new ArgumentOutOfRangeException ("count");
			
			data = new T [count];
		}
		
		public Stack (IEnumerable <T> collection)
		{
			if (collection == null)
				throw new ArgumentNullException ("collection");
			
			ICollection <T> col = collection as ICollection <T>;
			
			
			if (col != null) {
				size = col.Count;
				data = new T [size];
				col.CopyTo (data, 0);
			} else {
				foreach (T t in collection)
					Push (t);
			}
		}
		
		public void Clear ()
		{
			if (data != null)
				Array.Clear (data, 0, data.Length);
			
			size = 0;
			ver ++;
		}
		
		public bool Contains (T t)
		{		
			return data != null && Array.IndexOf (data, t, 0, size) != -1;
		}
		
		public void CopyTo (T [] dest, int idx)
		{
			// this gets copied in the order that it is poped
			if (data != null) {
				Array.Copy (data, 0, dest, idx, size);
				Array.Reverse (dest, idx, size);
			}
		}
		
		public T Peek ()
		{
			if (size == 0)
				throw new InvalidOperationException ();
			
			ver ++;
			
			return data [size - 1];
		}
		
		public T Pop ()
		{
			if (size == 0)
				throw new InvalidOperationException ();
			
			ver ++;
			
			return data [-- size];
		}

		public void Push (T t)
		{
			if (size == 0 || size == data.Length)
				Array.Resize <T> (ref data, size == 0 ? 10 : 2 * size);
			
			ver ++;
			
			data [size++] = t;
		}
		
		public T [] ToArray ()
		{
			T [] copy = new T [size];
			CopyTo (copy, 0);
			return copy;
		}
		
		public void TrimToSize ()
		{
			// for some broken reason, msft increments the version here
			ver ++;
			
			if (size == 0)
				data = null;
			else
				Array.Resize <T> (ref data, size);
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
			Push (t);
		}
		
		bool ICollection <T>.Remove (T t)
		{
			throw new InvalidOperationException ("");
		}
		
		void ICollection.CopyTo (Array dest, int idx)
		{
			try {
				if (data != null) {
					data.CopyTo (dest, idx);
					Array.Reverse (dest, idx, size);
				}
			} catch (ArrayTypeMismatchException) {
				throw new ArgumentException ();
			}
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
			
			Stack <T> parent;
			int idx;
			int ver;
			
			internal Enumerator (Stack <T> t)
			{
				parent = t;
				idx = NOT_STARTED;
				ver = t.ver;
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
				if (ver != parent.ver)
					throw new InvalidOperationException ();
				
				if (idx == -2)
					idx = parent.size;
				
				return idx != FINISHED && -- idx != FINISHED;
			}
			
			public T Current {
				get {
					if (idx < 0)
						throw new InvalidOperationException ();
					
					return parent.data [idx];
				}
			}
			
			void IEnumerator.Reset ()
			{
				if (ver != parent.ver)
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
