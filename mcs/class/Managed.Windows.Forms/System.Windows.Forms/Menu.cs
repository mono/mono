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

		public const int FindHandle = 0;
		public const int FindShortcut = 1;
		
 		protected Menu (MenuItem[] items)
		{
			//Console.WriteLine ("Menu.Menu " + (items != null));

			menu_items = new MenuItemCollection (this);

			if (items != null)
				menu_items.AddRange (items);
		}


		#region Public Properties
		public IntPtr Handle {
			get {
				if (menu_handle == IntPtr.Zero) {
					menu_handle = CreateMenuHandle ();
					CreateItems ();
				}

				return menu_handle;
			}
		}
		
		public virtual bool IsParent {
			get {
				throw new NotImplementedException ();
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

		#region Public Methods
			
		protected void CloneMenu (Menu menuSrc)
		{
			throw new NotImplementedException ();
		}
		
		protected virtual IntPtr CreateMenuHandle ()
		{
			IntPtr menu;
			menu  = MenuAPI.CreatePopupMenu ();
			Console.WriteLine ("Menu.CreateMenuHandle:" + menu);
			return menu;
		}

		protected override void Dispose (bool diposing)
		{
			throw new NotImplementedException ();
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
			throw new NotImplementedException ();
		}
		
		public MainMenu GetMainMenu ()
		{
			throw new NotImplementedException ();
		}
		
		public virtual void MergeMenu (Menu menuSrc)
		{
			throw new NotImplementedException ();
		}

		protected internal virtual bool ProcessCmdKey (ref Message msg, Keys keyData)
		{
			throw new NotImplementedException ();
		}

		public override string ToString ()
		{
			throw new NotImplementedException ();
		}

		#endregion Public Methods

		#region Private Methods

		protected void CreateItems ()
		{
			Console.WriteLine ("Menu.CreateItems:" + menu_items.Count);

			for (int i = 0; i < menu_items.Count; i++) 
				menu_items[i].Create ();
				
			Console.WriteLine ("End Menu.CreateItems:" + menu_items.Count);

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
				get { return (MenuItem) items[index]; }
			}

			object IList.this[int index] {
				get { return items[index]; }
				set { items[index] = value; }
			}

			#endregion Public Properties

			#region Public Methods

			public virtual int Add (MenuItem mi)
			{
				mi.parent_menu = owner;
				items.Add (mi);

				return items.Count - 1;
			}

			public virtual MenuItem Add (string s)
			{
				throw new NotImplementedException ();
			}

			public virtual int Add (int index, MenuItem mi)
			{
				throw new NotImplementedException ();
			}

			public virtual MenuItem Add (string s, EventHandler e)
			{
				throw new NotImplementedException ();
			}

			public virtual MenuItem Add (string s, MenuItem[] items)
			{
				throw new NotImplementedException ();
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
			}

			public bool Contains (MenuItem value)
			{
				throw new NotImplementedException ();
			}
			
			public virtual void CopyTo (Array dest, int index)
			{
				throw new NotImplementedException ();
			}
			
			public virtual IEnumerator GetEnumerator ()
			{
				throw new NotImplementedException ();
			}
			
			int IList.Add (object value)
			{
				throw new NotImplementedException ();
			}

			bool IList.Contains (object value)
			{
				throw new NotImplementedException ();
			}

			int IList.IndexOf (object value)
			{
				throw new NotImplementedException ();
			}

			void IList.Insert (int index, object value)
			{
				throw new NotImplementedException ();
			}

			void IList.Remove (object value)
			{
				throw new NotImplementedException ();
			}

			public int IndexOf (MenuItem value)
			{
				throw new NotImplementedException ();
			}

			public virtual void Remove (MenuItem item)
			{
				throw new NotImplementedException ();
			}
			
			public virtual void RemoveAt (int index)
			{
				throw new NotImplementedException ();
			}

			#endregion Public Methods
		}
	}
}


