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

using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace System.Collections.Generic
{
	[ComVisible (false)]
	[Serializable]
	[DebuggerDisplay ("Count={Count}")]
	[DebuggerTypeProxy (typeof (CollectionDebuggerView))]
	public class Stack <T> : IEnumerable <T>, ICollection, IEnumerable
	{
		T [] _array;
		int _size;
		int _version;
		
		private const int INITIAL_SIZE = 16;

		public Stack ()
		{
		}
		
		public Stack (int count)
		{
			if (count < 0)
				throw new ArgumentOutOfRangeException ("count");

			_array = new T [count];
		}
		
		public Stack (IEnumerable <T> collection)
		{
			if (collection == null)
				throw new ArgumentNullException ("collection");
			
			ICollection <T> col = collection as ICollection <T>;
			
			if (col != null) {
				_size = col.Count;
				_array = new T [_size];
				col.CopyTo (_array, 0);
			} else {
				foreach (T t in collection)
					Push (t);
			}
		}
		
		public void Clear ()
		{
			if (_array != null)
				Array.Clear (_array, 0, _array.Length);
			
			_size = 0;
			_version ++;
		}
		
		public bool Contains (T t)
		{		
			return _array != null && Array.IndexOf (_array, t, 0, _size) != -1;
		}
		
		public void CopyTo (T [] dest, int idx)
		{
			if (dest == null)
				throw new ArgumentNullException ("dest");
			if (idx < 0)
				throw new ArgumentOutOfRangeException ("idx");
			
			// this gets copied in the order that it is poped
			if (_array != null) {
				Array.Copy (_array, 0, dest, idx, _size);
				Array.Reverse (dest, idx, _size);
			}
		}
		
		public T Peek ()
		{
			if (_size == 0)
				throw new InvalidOperationException ();
			
			return _array [_size - 1];
		}
		
		public T Pop ()
		{
			if (_size == 0)
				throw new InvalidOperationException ();
			
			_version ++;
			T popped = _array [--_size];
			// clear stuff out to make the GC happy
			_array [_size] = default(T);
			return popped;
		}

		public void Push (T t)
		{
			if (_array == null || _size == _array.Length)
				Array.Resize <T> (ref _array, _size == 0 ? INITIAL_SIZE : 2 * _size);
			
			_version ++;
			
			_array [_size++] = t;
		}
		
		public T [] ToArray ()
		{
			T [] copy = new T [_size];
			CopyTo (copy, 0);
			return copy;
		}

		public void TrimExcess ()
		{
			if (_array != null && (_size < _array.Length * 0.9))
				Array.Resize <T> (ref _array, _size);
			_version ++;
		}
		
		public int Count {
			get { return _size; }
		}
		
		bool ICollection.IsSynchronized {
			get { return false; }
		}
		
		object ICollection.SyncRoot {
			get { return this; }
		}
		
		void ICollection.CopyTo (Array dest, int idx)
		{
			try {
				if (_array != null) {
					_array.CopyTo (dest, idx);
					Array.Reverse (dest, idx, _size);
				}
			} catch (ArrayTypeMismatchException) {
				throw new ArgumentException ();
			}
		}
		
		public Enumerator GetEnumerator ()
		{
			return new Enumerator (this);
		}

		IEnumerator <T> IEnumerable<T>.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}
		
		[Serializable]
		public struct Enumerator : IEnumerator <T>, IEnumerator, IDisposable {
			const int NOT_STARTED = -2;
			
			// this MUST be -1, because we depend on it in move next.
			// we just decr the _size, so, 0 - 1 == FINISHED
			const int FINISHED = -1;
			
			Stack <T> parent;
			int idx;
			int _version;
			
			internal Enumerator (Stack <T> t)
			{
				parent = t;
				idx = NOT_STARTED;
				_version = t._version;
			}
			
			// for some fucked up reason, MSFT added a useless dispose to this class
			// It means that in foreach, we must still do a try/finally. Broken, very
			// broken.
			public void Dispose ()
			{
				idx = FINISHED;
			}
			
			public bool MoveNext ()
			{
				if (_version != parent._version)
					throw new InvalidOperationException ();
				
				if (idx == -2)
					idx = parent._size;
				
				return idx != FINISHED && -- idx != FINISHED;
			}
			
			public T Current {
				get {
					if (idx < 0)
						throw new InvalidOperationException ();
					
					return parent._array [idx];
				}
			}
			
			void IEnumerator.Reset ()
			{
				if (_version != parent._version)
					throw new InvalidOperationException ();
				
				idx = NOT_STARTED;
			}
			
			object IEnumerator.Current {
				get { return Current; }
			}
			
		}
	}
}
