// -*- Mode: csharp; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// System.Collections.ObjectModel.KeyedCollection
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

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace System.Collections.ObjectModel
{
	[ComVisible(false)]
	[Serializable]
	public abstract class KeyedCollection<TKey, TItem> : Collection<TItem>
	{
		private Dictionary<TKey, TItem> dictionary;
		private IEqualityComparer<TKey> comparer;
		private int dictionaryCreationThreshold;

		protected KeyedCollection ()
			: this (null, 0)
		{ 
		}

		protected KeyedCollection (IEqualityComparer<TKey> comparer)
			: this(comparer, 0)
		{
		}

		protected KeyedCollection (IEqualityComparer<TKey> comparer, int dictionaryCreationThreshold)
		{
			if (comparer != null)
				this.comparer = comparer;
			else
				this.comparer = EqualityComparer<TKey>.Default;

			this.dictionaryCreationThreshold = dictionaryCreationThreshold;

			if (dictionaryCreationThreshold == 0)
				dictionary = new Dictionary<TKey, TItem> (this.comparer);
		}

		public bool Contains (TKey key)
		{
			if (dictionary != null)
				return dictionary.ContainsKey (key);
			return IndexOfKey (key) >= 0;
		}

		private int IndexOfKey (TKey key)
		{
			for (int i = Count - 1; i >= 0; i--)
			{
				TKey lkey = GetKeyForItem (this [i]);
				if (comparer.Equals (key, lkey))
					return i;
			}
			return -1;
		}

		public bool Remove (TKey key)
		{
			TItem item;
			if (dictionary != null)
			{
				if (dictionary.TryGetValue (key, out item))
					return base.Remove(item);
				else
					return false;
			}

			int idx = IndexOfKey (key);

			if (idx == -1)
				return false;
			
			RemoveAt(idx);
			return true;
		}

		public IEqualityComparer<TKey> Comparer {
			get {
				return comparer;
			}
		}

		public TItem this [TKey key] {
			get {
				if (dictionary != null)
					return dictionary [key];

				int idx = IndexOfKey (key);
				if (idx >= 0)
					return base [idx];
				else
					throw new KeyNotFoundException();
			}
		}

		protected void ChangeItemKey (TItem item, TKey newKey)
		{
			if (!Contains(item)) throw new ArgumentException();

			TKey oldKey = GetKeyForItem (item);
			if (comparer.Equals (oldKey, newKey)) return;

			if (Contains (newKey)) throw new ArgumentException();
			if (dictionary != null)
			{

				if (!dictionary.Remove (oldKey))
					throw new ArgumentException();

				dictionary.Add (newKey, item);
			}
		}

		protected override void ClearItems ()
		{
			if (dictionary != null)
			{
				dictionary.Clear();
			}

			base.ClearItems ();
		}

		protected abstract TKey GetKeyForItem (TItem item);

		protected override void InsertItem (int index, TItem item)
		{
			TKey key = GetKeyForItem (item);
			if (key == null)
				throw new ArgumentNullException ("GetKeyForItem(item)");

			if (dictionary != null && dictionary.ContainsKey (key))
				throw new ArgumentException ("An element with the same key already exists in the dictionary.");

			if (dictionary == null)
				for (int i = 0; i < Count; ++i) {
					if (comparer.Equals (key, GetKeyForItem (this [i]))) {
						throw new ArgumentException ("An element with the same key already exists in the dictionary.");
					}
				}

			base.InsertItem (index, item);

			if (dictionary != null)
				dictionary.Add (key, item);
			else if (dictionaryCreationThreshold != -1 && Count > dictionaryCreationThreshold) {
				dictionary = new Dictionary<TKey, TItem> (comparer);

				for (int i = 0; i < Count; ++i) {
					TItem dictitem = this [i];
					dictionary.Add (GetKeyForItem (dictitem), dictitem);
				}
			}
		}

		protected override void RemoveItem (int index)
		{
			if (dictionary != null)
			{
				TKey key = GetKeyForItem (this [index]);
				dictionary.Remove (key);
			}
			base.RemoveItem (index);
		}

		protected override void SetItem (int index, TItem item)
		{
			if (dictionary != null)
			{
				dictionary.Remove (GetKeyForItem (this [index]));
				dictionary.Add (GetKeyForItem (item), item);
			}
			base.SetItem (index, item);
		}

		protected IDictionary<TKey, TItem> Dictionary {
			get {
				return dictionary;
			}
		}
	}
}
