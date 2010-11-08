// -*- Mode: csharp; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// System.Collections.ObjectModel.ReadOnlyCollection
//
// Author:
//    Zoltan Varga (vargaz@gmail.com)
//    David Waite (mass@akuma.org)
//
// (C) 2005 Novell, Inc.
// (C) 2005 David Waite
//

//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace System.Collections.ObjectModel
{
	[ComVisible (false)]
	[Serializable]
	[DebuggerDisplay ("Count={Count}")]
	[DebuggerTypeProxy (typeof (CollectionDebuggerView<>))]	
	public class ReadOnlyCollection <T> : IList <T>, ICollection <T>, IEnumerable <T>, IList, ICollection, IEnumerable
	{
		IList <T> list;
		
		public ReadOnlyCollection (IList <T> list)
		{
			if (list == null)
				throw new ArgumentNullException ("list");
			this.list = list;
		}

		void ICollection<T>.Add (T item)
		{
			throw new NotSupportedException ();
		}
		
		void ICollection<T>.Clear ()
		{
			throw new NotSupportedException ();
		}

		public bool Contains (T value)
		{
			return list.Contains (value);
		}

		public void CopyTo (T [] array, int index)
		{
			list.CopyTo (array, index);
		}

		public IEnumerator <T> GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		public int IndexOf (T value)
		{
			return list.IndexOf (value);
		}

		void IList<T>.Insert (int index, T item)
		{
			throw new NotSupportedException ();
		}

		bool ICollection<T>.Remove (T item)
		{
			throw new NotSupportedException ();
		}

		void IList<T>.RemoveAt (int index)
		{
			throw new NotSupportedException ();
		}

		public int Count {
			get { return list.Count; }
		}

		protected IList<T> Items {
			get { return list; }
		}

		public T this [int index] {
			get { return list [index]; }
		}

		T IList<T>.this [int index] {
			get { return this [index]; }
			set { throw new NotSupportedException (); }
		}

		bool ICollection<T>.IsReadOnly {
			get { return true; }
		}

#region Not generic interface implementations
		void ICollection.CopyTo (Array array, int index)
		{
			((ICollection)list).CopyTo (array, index);
		}
				
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return ((IEnumerable) list).GetEnumerator ();
		}
		
		int IList.Add (object value)
		{
			throw new NotSupportedException ();
		}
		
		void IList.Clear ()
		{
			throw new NotSupportedException ();
		}

		bool IList.Contains (object value)
		{
			if (Collection <T>.IsValidItem (value))
				return list.Contains ((T) value);
			return false;
		}
		
		int IList.IndexOf (object value)
		{
			if (Collection <T>.IsValidItem (value))
				return list.IndexOf ((T) value);
			return -1;
		}
		
		void IList.Insert (int index, object value)
		{
			throw new NotSupportedException ();
		}
		
		void IList.Remove (object value)
		{
			throw new NotSupportedException ();
		}
		
		void IList.RemoveAt (int index)
		{
			throw new NotSupportedException ();
		}

		bool ICollection.IsSynchronized {
			get { return false; }
		}
		
		object ICollection.SyncRoot {
			get { return this; }
		}

		bool IList.IsFixedSize {
			get { return true; }
		}
		
		bool IList.IsReadOnly {
			get { return true; }
		}
		
		object IList.this [int index] {
			get { return list [index]; }
			set { throw new NotSupportedException (); }
		}
#endregion
	}
}
