//
// System.Web.UI.WebControls.MenuItemCollection.cs
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2004 Novell, Inc (http://www.novell.com)
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
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//


using System;
using System.Web.UI;
using System.Collections;

namespace System.Web.UI.WebControls
{
	public sealed class MenuItemCollection: ICollection, IEnumerable, IStateManager
	{
		ArrayList items = new ArrayList ();
		Menu menu;
		MenuItem parent;
		bool marked;
		bool dirty;
		
		public MenuItemCollection ()
		{
		}
		
		public MenuItemCollection (MenuItem owner)
		{
			this.parent = owner;
			this.menu = owner.Menu;
		}
		
		internal MenuItemCollection (Menu menu)
		{
			this.menu = menu;
		}
		
		internal void SetMenu (Menu menu)
		{
			this.menu = menu;
			foreach (MenuItem item in items)
				item.Menu = menu;
		}
		
		public MenuItem this [int index] {
			get { return (MenuItem) items [index]; }
		}
		
		public void Add (MenuItem child)
		{
			child.Index = items.Add (child);
			child.Menu = menu;
			child.SetParent (parent);
			if (marked) {
				((IStateManager)child).TrackViewState ();
				SetDirty ();
			}
		}

		internal void SetDirty () {
			for (int n = 0; n < Count; n++)
				this [n].SetDirty ();
			dirty = true;
		}
		
		public void AddAt (int index, MenuItem child)
		{
			items.Insert (index, child);
			child.Index = index;
			child.Menu = menu;
			child.SetParent (parent);
			for (int n=index+1; n<items.Count; n++)
				((MenuItem)items[n]).Index = n;
			if (marked) {
				((IStateManager)child).TrackViewState ();
				SetDirty ();
			}
		}
		
		public void Clear ()
		{
			if (menu != null || parent != null) {
				foreach (MenuItem nod in items) {
					nod.Menu = null;
					nod.SetParent (null);
				}
			}
			items.Clear ();
			if (marked) {
				SetDirty ();
			}
		}
		
		public bool Contains (MenuItem c)
		{
			return items.Contains (c);
		}
		
		public void CopyTo (Array array, int index)
		{
			items.CopyTo (array, index);
		}
		
		public void CopyTo (MenuItem[] array, int index)
		{
			items.CopyTo (array, index);
		}
		
		public IEnumerator GetEnumerator ()
		{
			return items.GetEnumerator ();
		}
		
		public int IndexOf (MenuItem value)
		{
			return items.IndexOf (value);
		}
		
		public void Remove (MenuItem value)
		{
			int i = IndexOf (value);
			if (i == -1) return;
			items.RemoveAt (i);
			if (menu != null)
				value.Menu = null;
			if (marked) {
				SetDirty ();
			}
		}
		
		public void RemoveAt (int index)
		{
			MenuItem item = (MenuItem) items [index];
			items.RemoveAt (index);
			if (menu != null)
				item.Menu = null;
			if (marked) {
				SetDirty ();
			}
		}
		
		public int Count {
			get { return items.Count; }
		}
		
		public bool IsSynchronized {
			get { return false; }
		}
		
		public object SyncRoot {
			get { return items; }
		}
		
		void System.Collections.ICollection.CopyTo (Array array, int index)
		{
			items.CopyTo (array, index);
		}

		void IStateManager.LoadViewState (object state)
		{
			if (state == null) return;
			object[] its = (object[]) state;
			
			dirty = (bool)its [0];

			if (dirty) {
				items.Clear ();

				for (int n = 1; n < its.Length; n++) {
					MenuItem item = new MenuItem ();
					Add (item);
					object ns = its [n];
					if (ns != null)
						((IStateManager) item).LoadViewState (ns);
				}
			}
			else {
				for (int n = 1; n < its.Length; n++) {
					Pair pair = (Pair) its [n];
					int oi = (int) pair.First;
					MenuItem node = (MenuItem) items [oi];
					((IStateManager) node).LoadViewState (pair.Second);
				}
			}
		}
		
		object IStateManager.SaveViewState ()
		{
			object[] state = null;
			bool hasData = false;
			
			if (dirty) {
				if (items.Count > 0) {
					hasData = true;
					state = new object [items.Count + 1];
					state [0] = true;
					for (int n = 0; n < items.Count; n++) {
						MenuItem item = items [n] as MenuItem;
						object ns = ((IStateManager) item).SaveViewState ();
						state [n + 1] = ns;
					}
				}
			} else {
				ArrayList list = new ArrayList ();
				for (int n=0; n<items.Count; n++) {
					MenuItem item = items[n] as MenuItem;
					object ns = ((IStateManager)item).SaveViewState ();
					if (ns != null) {
						hasData = true;
						list.Add (new Pair (n, ns));
					}
				}
				if (hasData) {
					list.Insert (0, false);
					state = list.ToArray ();
				}
			}
			
			if (hasData)
				return state;
			else
				return null;
		}
		
		void IStateManager.TrackViewState ()
		{
			marked = true;
			for (int n=0; n<items.Count; n++)
				((IStateManager) items [n]).TrackViewState ();
		}
		
		bool IStateManager.IsTrackingViewState {
			get { return marked; }
		}
	}
}

