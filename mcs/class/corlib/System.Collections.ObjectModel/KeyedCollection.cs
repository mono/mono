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

#if NET_2_0
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace System.Collections.ObjectModel
{
	[ComVisible(false)]
	[Serializable]
	public abstract class KeyedCollection<TKey, TItem> : Collection<TItem>
	{
		public bool Contains (TKey key)
		{
			throw new NotImplementedException ();
		}

		public bool Remove (TKey key)
		{
			throw new NotImplementedException ();
		}

		public IEqualityComparer<TKey> Comparer {
			get {
				throw new NotImplementedException ();
			}
		}

		public TItem this[TKey key] {
			get {
				throw new NotImplementedException ();
			}
		}

		protected void ChangeItemKey (TItem item, TKey newKey)
		{
			throw new NotImplementedException ();
		}

		protected void ClearItems ()
		{
			throw new NotImplementedException ();
		}

		protected abstract TKey GetKeyForItem (TItem item);

		protected virtual void InsertItem (int index, TItem item)
		{
			throw new NotImplementedException ();
		}

		protected virtual void RemoveItem (int index)
		{
			throw new NotImplementedException ();
		}

		protected virtual void SetItem (int index, TItem item)
		{
			throw new NotImplementedException ();
		}

		protected IDictionary<TKey, TItem> Dictionary {
			get {
				throw new NotImplementedException ();
			}
		}
	}
}
#endif
