//
// System.ServiceModel.SynchronizedReadOnlyCollection.cs
//
// Author: Duncan Mak (duncan@novell.com)
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Collections;
using System.Runtime.InteropServices;

namespace System.Collections.Generic
{
	[ComVisible (false)]
	public class SynchronizedReadOnlyCollection<T>
		: IList<T>, ICollection<T>, IEnumerable<T>, IList, ICollection, IEnumerable
	{
		List<T> l;
		object sync_root;

		public SynchronizedReadOnlyCollection ()
			: this (new object ())
		{
		}

		public SynchronizedReadOnlyCollection (object sync_root)
			: this (sync_root, new List<T> ())
		{
		}

		public SynchronizedReadOnlyCollection (object sync_root, IEnumerable<T> list)
		{
			if (sync_root == null)
				throw new ArgumentNullException ("sync_root");

			if (list == null)
				throw new ArgumentNullException ("list");

			this.sync_root = sync_root;
			this.l = new List<T> (list);
		}

		public SynchronizedReadOnlyCollection (object sync_root, params T [] list)
			: this (sync_root, (IEnumerable<T>) list)
		{
		}

		public SynchronizedReadOnlyCollection (object sync_root, List<T> list, bool make_copy)
			: this (sync_root,
				list == null ? null : make_copy ? new List<T> (list) : list)
		{
		}

		public bool Contains (T value)
		{
			bool retval;

			lock (sync_root) {
				retval = l.Contains (value);
			}

			return retval;
		}

		public void CopyTo (T [] array, int index)
		{
			lock (sync_root) {
				l.CopyTo (array, index);
			}
		}

		public IEnumerator<T> GetEnumerator ()
		{
			IEnumerator<T> retval;

			lock (sync_root) {
				retval = l.GetEnumerator ();
			}

			return retval;
		}

		public int IndexOf (T value)
		{
			int retval;

			lock (sync_root) {
				retval = l.IndexOf (value);
			}

			return retval;
		}

		void ICollection<T>.Add (T value) { throw new NotSupportedException (); }
		void ICollection<T>.Clear () { throw new NotSupportedException (); }
		bool ICollection<T>.Remove (T value) { throw new NotSupportedException (); }

		void IList<T>.Insert (int index, T value) { throw new NotSupportedException (); }
		void IList<T>.RemoveAt (int index) { throw new NotSupportedException (); }

		void ICollection.CopyTo (Array array, int index)
		{
			ICollection<T> a = array as ICollection<T>;

			if (a == null)
				throw new ArgumentException ("The array type is not compatible.");

			lock (sync_root) {
				((ICollection) l).CopyTo (array, index);
			}
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		int IList.Add (object value) { throw new NotSupportedException (); }
		void IList.Clear () { throw new NotSupportedException (); }

		bool IList.Contains (object value)
		{
			if (typeof (T).IsValueType)
				throw new ArgumentException ("This is a collection of ValueTypes.");

			// null always gets thru
			if (value is T == false && value != null)
				throw new ArgumentException ("value is not of the same type as this collection.");

			bool retval;
			T val = (T) value;
			lock (sync_root) {
				retval = l.Contains (val);
			}

			return retval;
		}

		int IList.IndexOf (object value)
		{
			if (typeof (T).IsValueType)
				throw new ArgumentException ("This is a collection of ValueTypes.");

			if (value is T == false)
				throw new ArgumentException ("value is not of the same type as this collection.");

			int retval;
			T val = (T) value;
			lock (sync_root) {
				retval = l.IndexOf (val);
			}

			return retval;
		}

		void IList.Insert (int index, object value) { throw new NotSupportedException (); }
		void IList.Remove (object value) { throw new NotSupportedException (); }
		void IList.RemoveAt (int index) { throw new NotSupportedException (); }

		public int Count {
			get {
				int retval;
				lock (sync_root) {
					retval = l.Count;
				}
				return retval;
			}
		}

		public T this [int index] {
			get {
				T retval;
				lock (sync_root) {
					retval = l [index];
				}
				return retval;
			}
		}

		protected IList<T> Items {
			get { return l; }
		}


		bool ICollection<T>.IsReadOnly { get { return true; }}

		bool ICollection.IsSynchronized { get { return true; }}
		object ICollection.SyncRoot { get { return sync_root; }}

		bool IList.IsFixedSize { get { return true; }}
		bool IList.IsReadOnly { get { return true; }}

		T IList<T>.this [int index] {
			get { return this [index]; }
			set { throw new NotSupportedException (); }
		}

		object IList.this [int index] {
			get { return this [index]; }
			set { throw new NotSupportedException (); }
		}
	}
}