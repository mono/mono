//
// System.Collections.Generic.ReadOnlyCollection
//
// Author:
//    Carlos Alberto Cortez (carlos@unixmexico.org)
//
// (C) 2004 Carlos Alberto Cortez
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
	[CLSCompliant (false)]
	[ComVisible (false)]
	public class ReadOnlyCollection <T> : ICollection<T>, IEnumerable<T>, IList<T>,
		ICollection, IEnumerable, IList
	{

		Collection<T> collection;

		public ReadOnlyCollection(IList <T> list)
		{
			collection = new Collection<T> (list, list.Count);
		}

		public bool Contains (T value)
		{
			return collection.Contains (value);
		}

		public void CopyTo (T[] array, int index)
		{
			collection.CopyTo (array, index);
		}

		public IEnumerator<T> GetEnumerator()
		{
			return collection.GetEnumerator ();
		}

		void ICollection.CopyTo (Array array, int index)
		{
			((IList) collection).CopyTo (array, index);
		}

		void ICollection<T>.Add (T item)
		{
			throw new NotSupportedException ();
		}

		void ICollection<T>.Clear ()
		{
			throw new NotSupportedException ();
		}

		bool ICollection<T>.Remove (T item)
		{
			throw new NotSupportedException ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return ((IEnumerable) collection).GetEnumerator ();
		}

		int IList.Add (object value)
		{
			throw new NotSupportedException ();
		}

		void IList.Clear()
		{
			throw new NotSupportedException ();
		}

		bool IList.Contains (object value)
		{
			return ((IList) collection).Contains (value);
		}

		int IList.IndexOf (object value)
		{
			return ((IList) collection).IndexOf (value);
		}

		void IList.Insert (int index, object value)
		{
			throw new NotSupportedException ();
		}

		void IList.Remove (object value)
		{
			throw new NotSupportedException ();
		}

		void IList.RemoveAt(int index)
		{
			throw new NotSupportedException ();
		}

		void IList<T>.Insert (int index, T item)
		{
			throw new NotSupportedException ();
		}

		void IList<T>.RemoveAt (int index)
		{
			throw new NotSupportedException ();
		}

		public int IndexOf (T value)
		{
			return collection.IndexOf (value);
		}

		public int Count {
			
			get {
				return collection.Count;
			}

		}

		bool ICollection.IsSynchronized {
			
			get {
				return false;
			}
			
		}

		object ICollection.SyncRoot {
			
			get {
				return ((IList) collection).SyncRoot;
			}
			
		}

		bool ICollection<T>.IsReadOnly {
			
			get {
				return true;
			}

		}

		bool IList.IsReadOnly {
			
			get {
				return true;
			}

		}

		bool IList.IsFixedSize {
			
			get {
				return true;
			}
			
		}

		object IList.this [int index] {
			
			get {
				return ((IList) collection) [index];
			}

			set
			{
				throw new NotSupportedException ();
			}

		}

		public T this [int index] {
			
			get {
				return collection [index];
			}

			set {
				throw new NotSupportedException ();
			}
		}

		protected IList<T> Items {
			
			get {
				return collection.Items;
			}
			
		}

	}
}

#endif

