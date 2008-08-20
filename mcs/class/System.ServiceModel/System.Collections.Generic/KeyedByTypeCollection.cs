//
// KeyedByTypeCollection.cs
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
using System.Collections.ObjectModel;

namespace System.Collections.Generic
{
	public class KeyedByTypeCollection<TItem>
		: KeyedCollection<Type, TItem>
	{
		public KeyedByTypeCollection ()
		{
		}

		public KeyedByTypeCollection (IEnumerable<TItem> items)
		{
			foreach (TItem item in items)
				Add (item);
		}

		protected override Type GetKeyForItem (TItem item)
		{
			return item.GetType ();
		}

		public T Find<T> ()
		{
			foreach (TItem k in this)
				if (k is T)
					return (T) (object) k;
			return default (T);
		}

		public Collection<T> FindAll<T> ()
		{
			Collection<T> list = new Collection<T> ();
			foreach (TItem k in this)
				if (k is T)
					list.Add ((T) (object) k);
			return list;
		}

		protected override void InsertItem (int index, TItem kind)
		{
			base.InsertItem (index, kind);
		}

		protected override void SetItem (int index, TItem kind)
		{
			base.SetItem (index, kind);
		}

		public T Remove<T> ()
		{
			foreach (TItem k in this)
				if (k is T) {
					Remove (k);
					return (T) (object) k;
				}
			return default (T);
		}

		public Collection<T> RemoveAll<T> ()
		{
			return RemoveAll<T> ();
		}
	}
}
