//
// BindingElementCollection.cs
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
using System.Collections.ObjectModel;

namespace System.ServiceModel.Channels
{
	[MonoTODO]
	public class BindingElementCollection : Collection<BindingElement>
	{
		public BindingElementCollection ()
		{
		}

		public BindingElementCollection (BindingElement [] elements)
		{
			AddRange (elements);
		}

		public BindingElementCollection (IEnumerable<BindingElement> elements)
		{
			foreach (BindingElement e in elements)
				Add (e);
		}

		public void AddRange (params BindingElement[] elements)
		{
			foreach (BindingElement e in elements)
				Add (e);
		}

		public BindingElementCollection Clone ()
		{
			return new BindingElementCollection (this);
		}

		public bool Contains (Type bindingElementType)
		{
			foreach (BindingElement b in this)
				if (bindingElementType.IsAssignableFrom (b.GetType ()))
					return true;
			return false;
		}

		public T Find<T> ()
		{
			foreach (BindingElement b in this)
				if ((object) b is T)
					return (T) (object) b;
			return default (T);
		}

		public Collection<T> FindAll<T> ()
		{
			Collection<T> coll = new Collection<T> ();
			foreach (BindingElement b in this)
				if ((object) b is T)
					coll.Add ((T) (object) b);
			return coll;
		}

		public T Remove<T> ()
		{
			foreach (BindingElement b in this) {
				if ((object) b is T) {
					Remove (b);
					return (T) (object) b;
				}
			}
			return default (T);
		}

		public Collection<T> RemoveAll<T> ()
		{
			Collection<T> coll = new Collection<T> ();
			foreach (BindingElement b in this) {
				if ((object) b is T) {
					Remove (b);
					coll.Add ((T) (object) b);
				}
			}
			return coll;
		}

		protected override void InsertItem (int index,
			BindingElement item)
		{
			Items.Insert (index, item);
		}

		protected override void SetItem (int index, BindingElement item)
		{
			Items [index] = item;
		}
	}
}
