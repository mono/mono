//
// SynchronizedCollection.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

namespace System.Collections.Generic
{
	[ComVisibleAttribute (false)] 
	public class SynchronizedCollection<T> : IList<T>, ICollection<T>, 
		IEnumerable<T>, IList, ICollection, IEnumerable
	{
		object root;
		List<T> list;

		public SynchronizedCollection ()
			: this (new object (), null, false)
		{
		}

		public SynchronizedCollection (object syncRoot)
			: this (syncRoot, null, false)
		{
		}

		public SynchronizedCollection (object syncRoot,
			IEnumerable<T> list)
			: this (syncRoot, new List<T> (list), false)
		{
		}

		public SynchronizedCollection (object syncRoot,
			params T [] list)
			: this (syncRoot, new List<T> (list), false)
		{
		}

		public SynchronizedCollection (object syncRoot,
			List<T> list, bool makeCopy)
		{
			if (syncRoot == null)
				syncRoot = new object ();
			root = syncRoot;
			if (list == null)
				this.list = new List<T> ();
			else if (makeCopy)
				this.list = new List<T> (list);
			else
				this.list = list;
		}

		public int Count {
			get {
				lock (root) {
					return list.Count;
				}
			}
		}

		public T this [int index] {
			get {
				lock (root) {
					return list [index];
				}
			}
			set {
				SetItem (index, value);
			}
		}

		public object SyncRoot {
			get { return root; }
		}

		protected List<T> Items {
			get { return list; }
		}

		public void Add (T item)
		{
			InsertItem (list.Count, item);
		}

		public void Clear ()
		{
			ClearItems ();
		}

		public bool Contains (T item)
		{
			lock (root) {
				return list.Contains (item);
			}
		}

		public void CopyTo (T [] array, int index)
		{
			lock (root) {
				list.CopyTo (array, index);
			}
		}

		[MonoTODO ("Should be synchronized enumerator?")]
		public IEnumerator<T> GetEnumerator ()
		{
			lock (root) {
				return list.GetEnumerator ();
			}
		}

		public int IndexOf (T item)
		{
			lock (root) {
				return list.IndexOf (item);
			}
		}

		public void Insert (int index, T item)
		{
			InsertItem (index, item);
		}

		[MonoTODO ("should we lock and remove item without invoking RemoveItem() instead?")]
		public bool Remove (T item)
		{
			int index = IndexOf (item);
			if (index < 0)
				return false;
			RemoveAt (index);
			return true;
		}

		public void RemoveAt (int index)
		{
			RemoveItem (index);
		}

		protected virtual void ClearItems ()
		{
			lock (root) {
				list.Clear ();
			}
		}

		protected virtual void InsertItem (int index, T item)
		{
			lock (root) {
				list.Insert (index, item);
			}
		}

		protected virtual void RemoveItem (int index)
		{
			lock (root) {
				list.RemoveAt (index);
			}
		}

		protected virtual void SetItem (int index, T item)
		{
			lock (root) {
				list [index] = item;
			}
		}

		#region Explicit interface implementations

		void ICollection.CopyTo (Array array, int index)
		{
			CopyTo ((T []) array, index);
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		int IList.Add (object value)
		{
			lock (root) {
				Add ((T) value);
				return list.Count - 1;
			}
		}

		bool IList.Contains (object value)
		{
			return Contains ((T) value);
		}

		int IList.IndexOf (object value)
		{
			return IndexOf ((T) value);
		}

		void IList.Insert (int index, object value)
		{
			Insert (index, (T) value);
		}

		void IList.Remove (object value)
		{
			Remove ((T) value);
		}

		bool ICollection<T>.IsReadOnly {
			get { return false; }
		}

		bool ICollection.IsSynchronized {
			get { return true; }
		}

		object ICollection.SyncRoot {
			get { return root; }
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
	}
}
