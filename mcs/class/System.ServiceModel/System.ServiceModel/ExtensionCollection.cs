//
// ExtensionCollection.cs
//
// Author: Atsushi Enomoto (atsushi@ximian.com)
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
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.ServiceModel
{
	public sealed class ExtensionCollection<T>
		: SynchronizedCollection<IExtension<T>>,
		IExtensionCollection<T>, ICollection<IExtension<T>>, 
		IEnumerable<IExtension<T>>, IEnumerable
		where T : IExtensibleObject<T>
	{
		T owner;

		public ExtensionCollection (T owner)
			: this (owner, new object ())
		{
		}

		public ExtensionCollection (T owner, object syncRoot)
			: base (syncRoot)
		{
			this.owner = owner;
		}

		public E Find<E> ()
		{
			foreach (IExtension<T> item in this)
				if (item is E)
					return (E) (object) item;
			return default (E);
		}

		public Collection<E> FindAll<E> ()
		{
			Collection<E> c = new Collection<E> ();
			foreach (IExtension<T> item in this)
				if (item is E)
					c.Add ((E) (object) item);
			return c;
		}

		bool ICollection<IExtension<T>>.IsReadOnly {
			get { return false; }
		}

		[MonoTODO]
		protected override void ClearItems ()
		{
			// FIXME: threadsafe?
			for (int i = 0; i < Count; i++) {
				this [i].Detach (owner);
			}
			base.ClearItems ();
		}

		[MonoTODO]
		protected override void InsertItem (int index, IExtension<T> item)
		{
			// FIXME: threadsafe?
			item.Attach (owner);
			base.InsertItem (index, item);
		}

		[MonoTODO]
		protected override void RemoveItem (int index)
		{
			// FIXME: threadsafe?
			this [index].Detach (owner);
			base.RemoveItem (index);
		}

		[MonoTODO]
		protected override void SetItem (int index, IExtension<T> item)
		{
			// FIXME: threadsafe?
			this [index].Detach (owner);
			item.Attach (owner);
			base.SetItem (index, item);
		}
	}
}
