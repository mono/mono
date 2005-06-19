// -*- Mode: csharp; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// System.Collections.ObjectModel.ReadOnlyCollection
//
// Author:
//    Zoltan Varga (vargaz@gmail.com)
//
// (C) 2005 Novell, Inc.
//

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

#if NET_2_0
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace System.Collections.ObjectModel
{
	[ComVisible(false)]
	[Serializable]
	public class ReadOnlyCollection<T> : IList<T>, ICollection<T>, IEnumerable<T>, IList, ICollection, IEnumerable
	{
		public ReadOnlyCollection (IList<T> list)
		{
			throw new NotImplementedException ();
		}

		public void Add (T item)
		{
			throw new NotImplementedException ();
		}

		public void Clear ()
		{
			throw new NotImplementedException ();
		}

		public bool Contains (T item)
		{
			throw new NotImplementedException ();
		}

		public void CopyTo (T[] array, int index)
		{
			throw new NotImplementedException ();
		}

		public IEnumerator<T> GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		public int IndexOf (T item)
		{
			throw new NotImplementedException ();
		}

		public void Insert (int index, T item)
		{
			throw new NotImplementedException ();
		}

		public bool Remove (T item)
		{
			throw new NotImplementedException ();
		}

		public void RemoveAt (int index)
		{
			throw new NotImplementedException ();
		}

		public virtual int Count {
			get {
				throw new NotImplementedException ();
			}
		}

		public virtual T this [int index] {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public bool IsReadOnly {
			get {
				throw new NotImplementedException ();
			}
		}

#region Not generic interface implementations
		void ICollection.CopyTo (Array array, int arrayIndex)
		{
			throw new NotImplementedException ();
		}
		
		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException ();
		}
		
		int IList.Add (object item)
		{
			throw new NotImplementedException ();
		}
		
		bool IList.Contains (object item)
		{
			throw new NotImplementedException ();
		}
		
		int IList.IndexOf (object item)
		{
			throw new NotImplementedException ();
		}
		
		void IList.Insert (int index, object item)
		{
			throw new NotImplementedException ();
		}
		
		void IList.Remove (object item)
		{
			throw new NotImplementedException ();
		}
		
		bool ICollection.IsSynchronized {
			get {
				throw new NotImplementedException ();
			}
		}
		
		object ICollection.SyncRoot {
			get {
				throw new NotImplementedException ();
			}
		}
		bool IList.IsFixedSize {
			get {
				throw new NotImplementedException ();
			}
		}
		
		bool IList.IsReadOnly {
			get {
				throw new NotImplementedException ();
			}
		}
		
		object IList.this [int index] {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
#endregion
	}
}
#endif
