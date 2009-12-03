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
//	Brian O'Keefe (zer0keefie@gmail.com)
//
#if NET_4_0
using System.Runtime.CompilerServices;

namespace System.Collections.Specialized
{
	[TypeForwardedFrom (Consts.WindowsBase_3_0)]
	public class NotifyCollectionChangedEventArgs : EventArgs
	{
		private NotifyCollectionChangedAction action;
		private IList oldItems, newItems;
		private int oldIndex = -1, newIndex = -1;

		#region Constructors

		public NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction action)
		{
			this.action = action;

			if (action != NotifyCollectionChangedAction.Reset)
				throw new ArgumentException ("This constructor can only be used with the Reset action.", "action");
		}

		public NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction action, IList changedItems)
			: this (action, changedItems, -1)
		{
		}

		public NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction action, object changedItem)
			: this (action, changedItem, -1)
		{
		}

		public NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction action, IList newItems, IList oldItems)
			: this (action, newItems, oldItems, -1)
		{
		}

		public NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction action, IList changedItems, int startingIndex)
		{
			this.action = action;

			if (action == NotifyCollectionChangedAction.Add || action == NotifyCollectionChangedAction.Remove) {
				if (changedItems == null)
					throw new ArgumentNullException ("changedItems");

				if (startingIndex < -1)
					throw new ArgumentException ("The value of startingIndex must be -1 or greater.", "startingIndex");

				if (action == NotifyCollectionChangedAction.Add)
					InitializeAdd (changedItems, startingIndex);
				else
					InitializeRemove (changedItems, startingIndex);
			} else if (action == NotifyCollectionChangedAction.Reset) {
				if (changedItems != null)
					throw new ArgumentException ("This constructor can only be used with the Reset action if changedItems is null", "changedItems");

				if (startingIndex != -1)
					throw new ArgumentException ("This constructor can only be used with the Reset action if startingIndex is -1", "startingIndex");
			} else {
				throw new ArgumentException ("This constructor can only be used with the Reset, Add, or Remove actions.", "action");
			}
		}

		public NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction action, object changedItem, int index)
		{
			IList changedItems = new object [] { changedItem };
			this.action = action;

			if (action == NotifyCollectionChangedAction.Add)
				InitializeAdd (changedItems, index);
			else if (action == NotifyCollectionChangedAction.Remove)
				InitializeRemove (changedItems, index);
			else if (action == NotifyCollectionChangedAction.Reset) {
				if (changedItem != null)
					throw new ArgumentException ("This constructor can only be used with the Reset action if changedItem is null", "changedItem");

				if (index != -1)
					throw new ArgumentException ("This constructor can only be used with the Reset action if index is -1", "index");
			} else {
				throw new ArgumentException ("This constructor can only be used with the Reset, Add, or Remove actions.", "action");
			}
		}

		public NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction action, object newItem, object oldItem)
			: this (action, newItem, oldItem, -1)
		{
		}

		public NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction action, IList newItems, IList oldItems, int index)
		{
			this.action = action;

			if (action != NotifyCollectionChangedAction.Replace)
				throw new ArgumentException ("This constructor can only be used with the Replace action.", "action");

			if (newItems == null)
				throw new ArgumentNullException ("newItems");

			if (oldItems == null)
				throw new ArgumentNullException ("oldItems");

			this.oldItems = oldItems;
			this.newItems = newItems;

			oldIndex = index;
			newIndex = index;
		}

		public NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction action, IList changedItems, int index, int oldIndex)
		{
			this.action = action;

			if (action != NotifyCollectionChangedAction.Move)
				throw new ArgumentException ("This constructor can only be used with the Move action.", "action");

			if (index < -1)
				throw new ArgumentException ("The value of index must be -1 or greater.", "index");

			InitializeMove (changedItems, index, oldIndex);
		}

		public NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction action, object changedItem, int index, int oldIndex)
			: this (action, new object [] { changedItem }, index, oldIndex)
		{
		}

		public NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction action, object newItem, object oldItem, int index)
		{
			this.action = action;

			if (action != NotifyCollectionChangedAction.Replace)
				throw new ArgumentException ("This constructor can only be used with the Replace action.", "action");

			InitializeReplace (new object [] { newItem }, new object [] { oldItem }, index);
		}

		#endregion

		#region Accessor Properties

		public NotifyCollectionChangedAction Action {
			get { return action; }
		}

		public IList NewItems {
			get { return newItems; }
		}

		public int NewStartingIndex {
			get { return newIndex; }
		}

		public IList OldItems {
			get { return oldItems; }
		}

		public int OldStartingIndex {
			get { return oldIndex; }
		}

		#endregion

		#region Initialize Methods

		private void InitializeAdd(IList items, int index)
		{
			this.newItems = ArrayList.ReadOnly (items);
			this.newIndex = index;
		}

		private void InitializeRemove(IList items, int index)
		{
			this.oldItems = ArrayList.ReadOnly (items);
			this.oldIndex = index;
		}

		private void InitializeMove(IList changedItems, int newItemIndex, int oldItemIndex)
		{
			InitializeAdd (changedItems, newItemIndex);
			InitializeRemove (changedItems, oldItemIndex);
		}

		private void InitializeReplace(IList addedItems, IList removedItems, int index)
		{
			InitializeAdd (addedItems, index);
			InitializeRemove (removedItems, index);
		}

		#endregion
	}
}
#endif