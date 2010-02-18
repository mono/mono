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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Bartok	(pbartok@novell.com)
//
//

using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Security.Permissions;

namespace System.Web.UI.WebControls {

	// CAS - no inheritance demand required because the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[Editor("System.Web.UI.Design.WebControls.ListItemsCollectionEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
	public sealed class ListItemCollection : IList, ICollection, IEnumerable, IStateManager 
	{
#region Fields
		ArrayList items;
		bool tracking;
		bool dirty;
		int lastDirty = 0;
#endregion	// Fields

#region Public Constructors
		public ListItemCollection() {
			items = new ArrayList();
		}
#endregion	// Public Constructors

#region Public Instance Properties
		public int Capacity {
			get {
				return items.Capacity;
			}

			set {
				items.Capacity = value;
			}
		}

		public int Count {
			get {
				return items.Count;
			}
		}

		public bool IsReadOnly {
			get {
				return items.IsReadOnly;
			}
		}

		public bool IsSynchronized {
			get {
				return items.IsSynchronized;
			}
		}

		public object SyncRoot {
			get {
				return items.SyncRoot;
			}
		}

		public ListItem this[int index] {
			get {
				return (ListItem)items[index];
			}
		}
		#endregion	// Public Instance Properties

		#region Public Instance Methods
		public void Add(ListItem item) {
			items.Add(item);
			if (tracking) {
				item.TrackViewState ();
				SetDirty ();
			}
		}

		public void Add(string item) {
			ListItem listItem = new ListItem (item);
			items.Add (listItem);

			if (tracking) {
				listItem.TrackViewState ();
				SetDirty ();
			}
		}

		public void AddRange(ListItem[] items) {
			for (int i = 0; i < items.Length; i++) {
				Add(items[i]);

				if (tracking) {
					items [i].TrackViewState ();
					SetDirty ();
				}
			}
		}

		public void Clear() {
			items.Clear();

			if (tracking)
				SetDirty ();
		}

		public bool Contains(ListItem item) {
			return items.Contains(item);
		}

		public void CopyTo(Array array, int index) {
			items.CopyTo(array, index);
		}

		public ListItem FindByText (string text)
		{
			for (int i = 0; i < items.Count; i++)
				if (text == this [i].Text)
					return this [i];
			
			return null;
		}

		public ListItem FindByValue (string value)
		{
			for (int i = 0; i < items.Count; i++)
				if (value == this [i].Value)
					return this [i];
			
			return null;
		}

		public IEnumerator GetEnumerator() {
			return items.GetEnumerator();
		}

		public int IndexOf(ListItem item) {
			return items.IndexOf(item);
		}

		internal int IndexOf(string value) {
			for (int i = 0; i < items.Count; i++)
				if (value == this [i].Value)
					return i;
			return -1;
		}

		public void Insert(int index, ListItem item) {
			items.Insert(index, item);

			if (tracking) {
				item.TrackViewState ();
				lastDirty = index;
				SetDirty ();
			}
		}

		public void Insert(int index, string item) {
			ListItem listItem = new ListItem(item);
			items.Insert (index, listItem);

			if (tracking) {
				listItem.TrackViewState ();
				lastDirty = index;
				SetDirty ();
			}
		}

		public void Remove(ListItem item) {
			items.Remove(item);
			
			if (tracking)
				SetDirty ();
		}

		public void Remove (string item)
		{
			for (int i = 0; i < items.Count; i++)
				if (item == this [i].Value) {
					items.RemoveAt (i);

					if (tracking)
						SetDirty ();
				}
		}

		public void RemoveAt(int index) {
			items.RemoveAt(index);

			if (tracking)
				SetDirty ();
		}
#endregion	// Public Instance Methods

#region Interface methods
		bool IList.IsFixedSize {
			get {
				return items.IsFixedSize;
			}
		}

		object IList.this[int index] {
			get {
				return this[index];
			}

			set {
				if ((index >= 0) && (index < items.Count)) {
					items[index] = (ListItem)value;

					if (tracking)
						((ListItem) value).TrackViewState ();
				}
			}
		}

		int IList.Add(object value) {
			int i = items.Add ((ListItem) value);

			if (tracking) {
				((IStateManager) value).TrackViewState ();
				SetDirty ();
			}
			return i;
		}

		bool IList.Contains(object value) {
			return Contains((ListItem)value);
		}

		int IList.IndexOf(object value) {
			return IndexOf((ListItem)value);
		}

		void IList.Insert(int index, object value) {
			Insert(index, (ListItem)value);
		}

		void IList.Remove(object value) {
			Remove((ListItem)value);
		}

		bool IStateManager.IsTrackingViewState {
			get {
				return tracking;
			}
		}

		void IStateManager.LoadViewState (object savedState)
		{
			Pair pair = savedState as Pair;
			if (pair == null)
				return;

			bool newCollection = (bool) pair.First;
			object [] itemsArray = (object []) pair.Second;
			int count = itemsArray==null ? 0 : itemsArray.Length;

			if (newCollection)
				if (count > 0)
					items = new ArrayList(count);
				else
					items = new ArrayList();

			for (int i = 0; i < count; i++) {
				ListItem item = new ListItem ();
				
				if (newCollection) {
					item.LoadViewState (itemsArray [i]);
					item.SetDirty ();
					Add (item);
				}
				else{
					if (itemsArray [i] != null){
						item.LoadViewState (itemsArray [i]);
						item.SetDirty ();
						items [i] = item;
					}
				}
			}
		}

		object IStateManager.SaveViewState() {
			int count;
			bool itemsDirty = false;

			count = items.Count;
			if (count == 0 && !dirty)
				return null;

			object [] itemsState = null;
			if (count > 0)
				itemsState = new object [count];

			for (int i = 0; i < count; i++) {
				itemsState [i] = ((IStateManager) items [i]).SaveViewState ();
				if (itemsState [i] != null)
					itemsDirty = true;
			}

			if (!dirty && !itemsDirty)
				return null;

			return new Pair (dirty, itemsState);
		}

		void IStateManager.TrackViewState() {
			tracking = true;

			for (int i = 0; i < items.Count; i++) {
				((ListItem)items[i]).TrackViewState();
			}
		}
#endregion	// Interface methods

		void SetDirty ()
		{
			dirty = true;
			for (int i = lastDirty; i < items.Count; i++)
				((ListItem) items [i]).SetDirty ();
			
			lastDirty = items.Count - 1;
			if (lastDirty < 0)
				lastDirty = 0;
		}
	}
}
