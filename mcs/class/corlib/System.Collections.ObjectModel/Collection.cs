// -*- Mode: csharp; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// System.Collections.ObjectModel.Collection
//
// Authors:
//    Zoltan Varga (vargaz@gmail.com)
//    David Waite (mass@akuma.org)
//    Marek Safar (marek.safar@gmail.com)
//
// (C) 2005 Novell, Inc.
// (C) 2005 David Waite
//

//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
// Copyright (C) 2005 David Waite
// Copyright (C) 2011 Xamarin, Inc (http://www.xamarin.com)
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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace System.Collections.ObjectModel
{
	[ComVisible (false)]
	[Serializable]
	[DebuggerDisplay ("Count={Count}")]
	[DebuggerTypeProxy (typeof (CollectionDebuggerView<>))]
	public class Collection<T> : IList<T>, IList
#if NET_4_5
		, IReadOnlyList<T>
#endif
	{
		IList <T> items;
		
		[NonSerialized]
		object syncRoot;
		
		public Collection ()
		{
			List <T> l = new List <T> ();
			IList l2 = l as IList;
			syncRoot = l2.SyncRoot;
			items = l;
		}

		public Collection (IList <T> list)
		{
			if (list == null)
				throw new ArgumentNullException ("list");
			this.items = list;
			ICollection l = list as ICollection;
			syncRoot = (l != null) ? l.SyncRoot : new object ();
		}

		public void Add (T item)
		{
			int idx = items.Count;
			InsertItem (idx, item);
		}

		public void Clear ()
		{
			ClearItems ();
		}

		protected virtual void ClearItems ()
		{
			items.Clear ();
		}

		public bool Contains (T item)
		{
			return items.Contains (item);
		}

		public void CopyTo (T [] array, int index)
		{
			items.CopyTo (array, index);
		}

		public IEnumerator <T> GetEnumerator ()
		{
			return items.GetEnumerator ();
		}

		public int IndexOf (T item)
		{
			return items.IndexOf (item);
		}

		public void Insert (int index, T item)
		{
			InsertItem (index, item);
		}

		protected virtual void InsertItem (int index, T item)
		{
			items.Insert (index, item);
		}

		protected IList<T> Items {
			get { return items; }
		}

		public bool Remove (T item)
		{
			int idx = IndexOf (item);
			if (idx == -1) 
				return false;
			
			RemoveItem (idx);
			
			return true;
		}

		public void RemoveAt (int index)
		{
			RemoveItem (index);
		}

		protected virtual void RemoveItem (int index)
		{
			items.RemoveAt (index);
		}

		public int Count {
			get { return items.Count; }
		}

		public T this [int index] {
			get { return items [index]; }
			set { SetItem (index, value); }
		}

		bool ICollection<T>.IsReadOnly {
			get { return items.IsReadOnly; }
		}

		protected virtual void SetItem (int index, T item)
		{
			items[index] = item;
		}

		
#region Helper methods for non-generic interfaces
		
		internal static T ConvertItem (object item)
		{
			if (CollectionHelpers.IsValidItem<T> (item))
				return (T)item;
			throw new ArgumentException ("item");
		}
		
		internal static void CheckWritable (IList <T> items)
		{
			if (items.IsReadOnly)
				throw new NotSupportedException ();
		}
		
		internal static bool IsSynchronized (IList <T> items)
		{
			ICollection c = items as ICollection;
			return (c != null) ? c.IsSynchronized : false;
		}
		
		internal static bool IsFixedSize (IList <T> items)
		{
			IList l = items as IList;
			return (l != null) ? l.IsFixedSize : false;
		}
#endregion

#region Not generic interface implementations
		void ICollection.CopyTo (Array array, int index)
		{
			((ICollection)items).CopyTo (array, index);
		}
		
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return (IEnumerator) items.GetEnumerator ();
		}
				
		int IList.Add (object value)
		{
			int idx = items.Count;
			InsertItem (idx, ConvertItem (value));
			return idx;
		}
		
		bool IList.Contains (object value)
		{
			if (CollectionHelpers.IsValidItem<T> (value))
				return items.Contains ((T) value);
			return false;
		}
		
		int IList.IndexOf (object value)
		{
			if (CollectionHelpers.IsValidItem<T> (value))
				return items.IndexOf ((T) value);
			return -1;
		}
		
		void IList.Insert (int index, object value)
		{
			InsertItem (index, ConvertItem (value));
		}
		
		void IList.Remove (object value)
		{
			CheckWritable (items);

			int idx = IndexOf (ConvertItem (value));

			RemoveItem (idx);
		}
		
		bool ICollection.IsSynchronized {
			get { return IsSynchronized (items); }
		}
		
		object ICollection.SyncRoot {
			get { return syncRoot; }
		}
		bool IList.IsFixedSize {
			get { return IsFixedSize (items); }
		}
		
		bool IList.IsReadOnly {
			get { return items.IsReadOnly; }
		}
		
		object IList.this [int index] {
			get { return items [index]; }
			set { SetItem (index, ConvertItem (value)); }
		}
#endregion
	}

	static class CollectionHelpers
	{
		public static bool IsValidItem<T> (object item)
		{
			return item is T || (item == null && ! typeof (T).IsValueType);
		}
	}
}
