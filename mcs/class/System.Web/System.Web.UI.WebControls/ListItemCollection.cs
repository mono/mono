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
	public sealed class ListItemCollection : IList, ICollection, IEnumerable, IStateManager {
		#region Fields
		private ArrayList	items;
		private bool		tracking;
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
		}

		public void Add(string item) {
			items.Add(new ListItem(item));
		}

		public void AddRange(ListItem[] items) {
			for (int i = 0; i < items.Length; i++) {
				Add(items[i]);
			}
		}

		public void Clear() {
			items.Clear();
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
		}

		public void Insert(int index, string item) {
			items.Insert(index, new ListItem(item));
		}

		public void Remove(ListItem item) {
			items.Remove(item);
		}

		public void Remove (string item)
		{
			for (int i = 0; i < items.Count; i++)
				if (item == this [i].Value)
					items.RemoveAt (i);
		}

		public void RemoveAt(int index) {
			items.RemoveAt(index);
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
				}
			}
		}

		int IList.Add(object value) {
			return items.Add((ListItem)value);
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

		void IStateManager.LoadViewState(object state) {
			Pair		pair;
			int		count;
			string[]	text;
			string[]	value;

			if (state == null) {
				return;
			}

			pair = (Pair)state;

			text = (string[])pair.First;
			value = (string[])pair.Second;

			count = text.Length;

			items = new ArrayList(count);
			for (int i = 0; i < count; i++) {
				items.Add(new ListItem(text[i], value[i]));
			}
		}

		object IStateManager.SaveViewState() {
			string[]	text;
			string[]	value;
			int		count;

			count = items.Count;
			text = new string[count];
			value = new string[count];

			for (int i = 0; i < count; i++) {
				text[i] = ((ListItem)items[i]).Text;
				value[i] = ((ListItem)items[i]).Value;
			}

			return new Pair(text, value);
		}

		void IStateManager.TrackViewState() {
			tracking = true;

			for (int i = 0; i < items.Count; i++) {
				((ListItem)items[i]).TrackViewState();
			}
		}
		#endregion	// Interface methods
	}
}
