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
//	Chris Toshok (toshok@novell.com)
//	Brian O'Keefe (zer0keefie@gmail.com)
//
#if NET_4_0
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace System.Collections.ObjectModel
{
	[Serializable]
	[TypeForwardedFrom (Consts.WindowsBase_3_0)]
	public class ObservableCollection<T> : Collection<T>, INotifyCollectionChanged, INotifyPropertyChanged {
		
		private class Reentrant : IDisposable {
			private int count = 0;

			public Reentrant()
			{
			}

			public void Enter()
			{
				count++;
			}

			public void Dispose()
			{
				count--;
			}

			public bool Busy
			{
				get { return count > 0; }
			}
		}

		private Reentrant reentrant = new Reentrant ();

		public ObservableCollection()
		{
		}

		public ObservableCollection(IEnumerable<T> collection)
		{
			throw new NotImplementedException ();
		}

		public ObservableCollection(List<T> list)
			: base (list)
		{
		}

		public virtual event NotifyCollectionChangedEventHandler CollectionChanged;
		protected virtual event PropertyChangedEventHandler PropertyChanged;

		event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged {
			add { this.PropertyChanged += value; }
			remove { this.PropertyChanged -= value; }
		}

		protected IDisposable BlockReentrancy ()
		{
			reentrant.Enter ();
			return reentrant;
		}

		protected void CheckReentrancy ()
		{
			NotifyCollectionChangedEventHandler eh = CollectionChanged;

			// Only have a problem if we have more than one event listener.
			if (reentrant.Busy && eh != null && eh.GetInvocationList ().Length > 1)
				throw new InvalidOperationException ("Cannot modify the collection while reentrancy is blocked.");
		}

		protected override void ClearItems ()
		{
			CheckReentrancy ();

			base.ClearItems ();

			OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Reset));
			OnPropertyChanged (new PropertyChangedEventArgs ("Count"));
			OnPropertyChanged (new PropertyChangedEventArgs ("Item[]"));
		}

		protected override void InsertItem (int index, T item)
		{
			CheckReentrancy ();

			base.InsertItem (index, item);

			OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, item, index));
			OnPropertyChanged (new PropertyChangedEventArgs ("Count"));
			OnPropertyChanged (new PropertyChangedEventArgs ("Item[]"));
		}

		public void Move (int oldIndex, int newIndex)
		{
			MoveItem (oldIndex, newIndex);
		}

		protected virtual void MoveItem (int oldIndex, int newIndex)
		{
			CheckReentrancy ();

			T item = Items [oldIndex];
			base.RemoveItem (oldIndex);
			base.InsertItem (newIndex, item);

			OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Move, item, newIndex, oldIndex));
			OnPropertyChanged (new PropertyChangedEventArgs ("Item[]"));
		}

		protected virtual void OnCollectionChanged (NotifyCollectionChangedEventArgs e)
		{
			NotifyCollectionChangedEventHandler eh = CollectionChanged;

			if (eh != null) {
				// Make sure that the invocation is done before the collection changes,
				// Otherwise there's a chance of data corruption.
				using (BlockReentrancy ()) {
					eh (this, e);
				}
			}
		}

		protected virtual void OnPropertyChanged (PropertyChangedEventArgs e)
		{
			PropertyChangedEventHandler eh = PropertyChanged;

			if (eh != null)
				eh (this, e);
		}

		protected override void RemoveItem (int index)
		{
			CheckReentrancy ();

			T item = Items [index];

			base.RemoveItem (index);

			OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, item, index));
			OnPropertyChanged (new PropertyChangedEventArgs ("Count"));
			OnPropertyChanged (new PropertyChangedEventArgs ("Item[]"));
		}

		protected override void SetItem (int index, T item)
		{
			CheckReentrancy ();

			T oldItem = Items [index];

			base.SetItem (index, item);

			OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Replace, item, oldItem, index));
			OnPropertyChanged (new PropertyChangedEventArgs ("Item[]"));
		}
	}
}
#endif