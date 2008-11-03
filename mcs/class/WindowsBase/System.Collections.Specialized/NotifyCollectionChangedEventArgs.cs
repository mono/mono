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
// Copyright (c) 2007 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//

using System;
using System.Collections;

namespace System.Collections.Specialized {

	public class NotifyCollectionChangedEventArgs : EventArgs {

		NotifyCollectionChangedAction action;

		public NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction action)
		{
			this.action = action;
		}

		public NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction action, IList changedItems)
			: this (action)
		{
		}

		public NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction action, object changedItem)
			: this (action)
		{
		}

		public NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction action, IList newItems, IList oldItems)
			: this (action)
		{
		}

		public NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction action, IList changedItems, int startingIndex)
			: this (action)
		{
		}

		public NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction action, object changedItem, int index)
			: this (action)
		{
		}

		public NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction action, object newItem, object oldItem)
			: this (action)
		{
		}

		public NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction action, IList newItems, IList oldItems, int startingIndex)
			: this (action)
		{
		}

		public NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction action, IList changedItem, int index, int oldIndex)
			: this (action)
		{
		}

		public NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction action, object changedItem, int index, int oldIndex)
			: this (action)
		{
		}

		public NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction action, object newItem, object oldItem, int index)
			: this (action)
		{
		}

		public NotifyCollectionChangedAction Action {
			get { return action; }
		}

		public IList NewItems {
			get { throw new NotImplementedException (); }
		}

		public int NewStartingIndex {
			get { throw new NotImplementedException (); }
		}

		public IList OldItems {
			get { throw new NotImplementedException (); }
		}

		public int OldStartingIndex {
			get { throw new NotImplementedException (); }
		}
	}	
}
