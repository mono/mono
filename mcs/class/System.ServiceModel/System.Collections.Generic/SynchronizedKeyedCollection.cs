//
// SynchronizedKeyedCollection.cs
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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.ServiceModel.Channels;

namespace System.Collections.Generic
{
	[ComVisibleAttribute (false)] 
	public abstract class SynchronizedKeyedCollection<K, T>
		: SynchronizedCollection<T>
	{
		Dictionary<K, T> dict;

		protected SynchronizedKeyedCollection ()
			: this (new object ())
		{
		}

		protected SynchronizedKeyedCollection (object syncRoot)
			: base (syncRoot)
		{
			dict = new Dictionary<K, T> ();
		}

		protected SynchronizedKeyedCollection (object syncRoot,
			IEqualityComparer<K> comparer)
			: base (syncRoot)
		{
			dict = new Dictionary<K, T> (comparer);
		}

		protected SynchronizedKeyedCollection (object syncRoot,
			IEqualityComparer<K> comparer, int capacity)
			: base (syncRoot)
		{
			dict = new Dictionary<K, T> (capacity, comparer);
		}

		// see bug #76417
		/*
		public T this [int index] {
			get { return base [index]; }
			set { base [index] = value; }
		}
		*/

		public T this [K key] {
			get {
				lock (SyncRoot) {
					return dict [key];
				}
			}
		}

		protected IDictionary<K, T> Dictionary {
			get { return dict; }
		}

		public bool Contains (K key)
		{
			lock (SyncRoot) {
				return dict.ContainsKey (key);
			}
		}

		public bool Remove (K key)
		{
			lock (SyncRoot) {
				return dict.Remove (key);
			}
		}

		protected void ChangeItemKey (T item, K newKey)
		{
			lock (SyncRoot) {
				K old = GetKeyForItem (item);
				dict [old] = default (T);
				dict [newKey] = item;
			}
		}

		[MonoTODO ("This lock is not an atomic.")]
		protected override void ClearItems ()
		{
			base.ClearItems ();
			lock (SyncRoot) {
				dict.Clear ();
			}
		}

		protected abstract K GetKeyForItem (T item);

		[MonoTODO ("This lock is not an atomic.")]
		protected override void InsertItem (int index, T item)
		{
			base.InsertItem (index, item);
			dict.Add (GetKeyForItem (item), item);
		}

		[MonoTODO ("This lock is not an atomic.")]
		protected override void RemoveItem (int index)
		{
			K key = GetKeyForItem (base [index]);
			base.RemoveItem (index);
			dict.Remove (key);
		}

		[MonoTODO ("This lock is not an atomic.")]
		protected override void SetItem (int index, T item)
		{
			base.SetItem (index, item);
			dict [GetKeyForItem (item)] = item;
		}
	}
}
