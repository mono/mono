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
// Copyright (c) 2008 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace System.ComponentModel {

	public class SortDescriptionCollection : Collection<SortDescription>, INotifyCollectionChanged
	{
		public static readonly SortDescriptionCollection Empty = new SortDescriptionCollection ();

		public SortDescriptionCollection ()
		{
		}

		event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged {
			add { CollectionChanged += value; }
			remove { CollectionChanged -= value; }
		}
			
		protected event NotifyCollectionChangedEventHandler CollectionChanged;

		protected override void ClearItems ()
		{
			throw new NotImplementedException ();
		}

		protected override void InsertItem (int index, SortDescription item)
		{
			throw new NotImplementedException ();
		}

		protected override void RemoveItem (int index)
		{
			throw new NotImplementedException ();
		}

		protected override void SetItem (int index, SortDescription item)
		{
			throw new NotImplementedException ();
		}
	}

}

