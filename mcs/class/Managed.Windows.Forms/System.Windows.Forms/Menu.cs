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
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Jordi Mas i Hernandez, jordi@ximian.com
//
//	TODO:
//		- Merge menus
//		- MDI integration
//		- ShortCut navigation
//
// NOT COMPLETE

using System.Collections;
using System.ComponentModel;

namespace System.Windows.Forms
{
	public abstract class Menu : Component
	{
		internal MenuItemCollection menu_items;
		internal IntPtr menu_handle = IntPtr.Zero;
		internal bool is_dirty = true;
		internal bool creating = false;

		public const int FindHandle = 0;
		public const int FindShortcut = 1;

 		protected Menu (MenuItem[] items)
		{
			menu_items = new MenuItemCollection (this);

			if (items != null)
				menu_items.AddRange (items);
		}


		#region Public Properties
		public IntPtr Handle {
			get {
				if (is_dirty && creating == false) {
					Dispose (true);
				}

				if (menu_handle == IntPtr.Zero) {
					Console.WriteLine ("Create Handle");
					menu_handle = CreateMenuHandle ();
					CreateItems ();
					is_dirty = false;
				}

				return menu_handle;
			}
		}

		public virtual bool IsParent {
			get {
				if (menu_items != null && menu_items.Count > 0)
					return true;
				else
					return false;
			}
		}

		public MenuItem MdiListItem {
			get {
				throw new NotImplementedException ();
			}
		}

		public MenuItemCollection MenuItems {
			get { return menu_items; }
		}

		#endregion Public Properties

		#region Private Properties

		internal bool IsDirty {
			get { return is_dirty; }
			set { is_dirty = value; }
		}

		#endregion Private Properties

		#region Public Methods

		protected void CloneMenu (Menu menuSrc)
		{
			Dispose (true);

			menu_items = new MenuItemCollection (this);

			for (int i = 0; i < menuSrc.MenuItems.Count ; i++)
				menu_items.Add (menuSrc.MenuItems [i]);
		}

		protected virtual IntPtr CreateMenuHandle ()
		{
			IntPtr menu;

			menu = MenuAPI.CreatePopupMenu ();
			return menu;
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				if (menu_handle != IntPtr.Zero)
					MenuAPI.DestroyMenu (menu_handle);
					menu_handle = IntPtr.Zero;
			}
		}

		public MenuItem FindMenuItem (int type, IntPtr value)
		{
			throw new NotImplementedException ();
		}

		protected int FindMergePosition (int mergeOrder)
		{
			throw new NotImplementedException ();
		}

		public ContextMenu GetContextMenu ()
		{
			if (this is ContextMenu)
				return (ContextMenu) this;
			else
				return null;
		}

		public MainMenu GetMainMenu ()
		{
			if (this is MainMenu)
				return (MainMenu) this;
			else
				return null;
		}

		public virtual void MergeMenu (Menu menuSrc)
		{
			if (menuSrc == this)
				throw new ArgumentException ("The menu cannot be merged with itself");

		}

		protected internal virtual bool ProcessCmdKey (ref Message msg, Keys keyData)
		{
			return false;
		}

		public override string ToString ()
		{
			return base.ToString ();
		}

		#endregion Public Methods

		#region Private Methods

		internal void CreateItems ()
		{
			creating = true;

			for (int i = 0; i < menu_items.Count; i++)
				menu_items[i].Create ();

			creating = false;
		}

		#endregion Private Methods

		public class MenuItemCollection : IList, ICollection, IEnumerable
		{
			private Menu owner;
			private ArrayList items = new ArrayList ();

			public MenuItemCollection (Menu owner)
			{
				this.owner = owner;
			}

			#region Public Properties

			public virtual int Count {
				get { return items.Count;}
			}

			public virtual bool IsReadOnly {
				get { return false;}
			}

			bool ICollection.IsSynchronized {
				get { return false;}
			}

			object ICollection.SyncRoot {
				get { return this;}
			}

			bool IList.IsFixedSize {
				get { return false;}
			}

			public MenuItem this [int index] {
				get {
					if (index < 0 || index >= Count)
						throw new ArgumentOutOfRangeException ("Index of out range");

					return (MenuItem) items[index];
				}
			}

			object IList.this[int index] {
				get { return items[index]; }
				set { throw new NotSupportedException (); }
			}

			#endregion Public Properties

			#region Public Methods

			public virtual int Add (MenuItem mi)
			{
				mi.parent_menu = owner;
				mi.Index = items.Count;
				items.Add (mi);

				owner.IsDirty = true;
				return items.Count - 1;
			}

			public virtual MenuItem Add (string s)
			{
				MenuItem item = new MenuItem (s);
				Add (item);
				return item;
			}

			public virtual int Add (int index, MenuItem mi)
			{
				if (index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException ("Index of out range");

				ArrayList new_items = new ArrayList (Count + 1);

				for (int i = 0; i < index; i++)
					new_items.Add (items[i]);

				new_items.Add (mi);

				for (int i = index; i < Count; i++)
					new_items.Add (items[i]);

				items = new_items;
				UpdateItemsIndices ();
				owner.IsDirty = true;

				return index;
			}

			public virtual MenuItem Add (string s, EventHandler e)
			{
				MenuItem item = new MenuItem (s, e);
				Add (item);

				return item;
			}

			public virtual MenuItem Add (string s, MenuItem[] items)
			{
				MenuItem item = new MenuItem (s, items);
				Add (item);

				return item;
			}

			public virtual void AddRange (MenuItem[] items)
			{
				if (items == null)
					throw new ArgumentNullException ("items");

				foreach (MenuItem mi in items)
					Add (mi);
			}

			public virtual void Clear ()
			{
				items.Clear ();
				owner.IsDirty = true;
			}

			public bool Contains (MenuItem value)
			{
				return items.Contains (value);
			}

			public virtual void CopyTo (Array dest, int index)
			{
				items.CopyTo (dest, index);
			}

			public virtual IEnumerator GetEnumerator ()
			{
				return items.GetEnumerator ();
			}

			int IList.Add (object value)
			{
				return Add ((MenuItem)value);
			}

			bool IList.Contains (object value)
			{
				return Contains ((MenuItem)value);
			}

			int IList.IndexOf (object value)
			{
				return IndexOf ((MenuItem)value);
			}

			void IList.Insert (int index, object value)
			{
				Add (index, (MenuItem) value);
			}

			void IList.Remove (object value)
			{
				Remove ((MenuItem) value);
			}

			public int IndexOf (MenuItem value)
			{
				return items.IndexOf (value);
			}

			public virtual void Remove (MenuItem item)
			{
				RemoveAt (item.Index);
			}

			public virtual void RemoveAt (int index)
			{
				if (index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException ("Index of out range");

				items.RemoveAt (index);

				UpdateItemsIndices ();
				owner.IsDirty = true;
			}

			#endregion Public Methods

			#region Private Methods

			private void UpdateItemsIndices ()
			{
				for (int i = 0; i < Count; i++)	// Recalculate indeces
					((MenuItem)items[i]).Index = i;
			}

			#endregion Private Methods
		}
	}
}


