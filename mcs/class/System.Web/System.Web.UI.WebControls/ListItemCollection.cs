
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
/**
 * Namespace: System.Web.UI.WebControls
 * Class:     ListItemCollection
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  100%
 *
 * (C) Gaurav Vaish (2001)
 */

using System;
using System.Collections;
using System.Web;
using System.Web.UI;
using System.ComponentModel;
using System.Reflection;

namespace System.Web.UI.WebControls
{
	[Editor ("System.Web.UI.Design.WebControls.ListItemsCollectionEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
	public sealed class ListItemCollection : IList, ICollection, IEnumerable, IStateManager
	{
		private ArrayList items;
		private bool      saveAll;
		private bool      marked;

		public ListItemCollection()
		{
			items   = new ArrayList();
			saveAll = false;
			marked  = false;
		}

		public int Capacity
		{
			get
			{
				return items.Capacity;
			}
			set
			{
				items.Capacity = value;
			}
		}

		public int Count
		{
			get
			{
				return items.Count;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return items.IsReadOnly;
			}
		}

		public bool IsSynchronized
		{
			get
			{
				return items.IsSynchronized;
			}
		}

		public ListItem this[int index]
		{
			get
			{
				if(index < 0 || index >= Count)
					return null;
				return (ListItem)(items[index]);
			}		
		}

		public object SyncRoot
		{
			get
			{
				return this;
			}
		}

		public void Add(ListItem item)
		{
			items.Add(item);
			if(marked)
				item.Dirty = true;
		}

		public void Add(string item)
		{
			Add(new ListItem(item));
		}

		public void AddRange(ListItem[] items)
		{
			foreach(ListItem item in items)
			{
				if(item!=null)
					Add(item);
			}
		}

		public void Clear()
		{
			items.Clear();
			if(marked)
				saveAll = true;
		}

		public bool Contains(ListItem item)
		{
			return items.Contains(item);
		}

		public void CopyTo(Array array, int index)
		{
			items.CopyTo(array, index);
		}

		public ListItem FindByText(string text)
		{
			int i=-1;
			foreach(object current in items)
			{
				i++;
				if(((ListItem)current).Text == text)
					break;
			}
			return (i==-1 ? null : (ListItem)items[i]);
		}

		public ListItem FindByValue(string value)
		{
			foreach(ListItem current in items)
			{
				if(current.Value == value)
				{
					return current;
				}
			}
			return null;
		}

		internal int FindByValueInternal(string value)
		{
			int i = -1;
			foreach(ListItem current in items)
			{
				i++;
				if(current.Value == value)
				{
					return i;
				}
			}
			return -1;
		}

		public IEnumerator GetEnumerator()
		{
			return items.GetEnumerator();
		}

		public int IndexOf(ListItem item)
		{
			return items.IndexOf(item);
		}

		public void Insert(int index, ListItem item)
		{
			items.Insert(index, item);
			if(marked)
				saveAll = true;
		}

		public void Insert(int index, string item)
		{
			Insert(index, new ListItem(item));
		}

		public void RemoveAt(int index)
		{
			if(index < 0 || index >= items.Count)
				return;
			items.RemoveAt(index);
			if(marked)
				saveAll = true;
		}

		public void Remove(ListItem item)
		{
			RemoveAt(IndexOf(item));
		}

		public void Remove(string item)
		{
			RemoveAt(IndexOf(ListItem.FromString(item)));
		}

		internal object SaveViewState ()
		{
			int count = Count;
			if (saveAll) {
				string [] keys = new string [count];
				string [] vals = new string [count];
				for(int i = 0; i < count; i++) {
					keys[i] = this [i].Text;
					vals[i] = this [i].Value;
				}

				return new Triplet (count, keys, vals);
			}

			ArrayList indices = new ArrayList ();
			ArrayList states = new ArrayList ();
			object o;
			for(int i = 0; i < count; i++) {
				o = this [i].SaveViewState ();
				if (o == null)
					continue;

				indices.Add (i);
				states.Add (o);
			}

			if (indices.Count > 0)
				return new Pair (indices, states);

			return null;
		}

		internal void LoadViewState (object savedState)
		{
			if (savedState == null)
				return;

			int i, end;
			if (savedState is Pair) {
				Pair pair = (Pair) savedState;
				ArrayList indices = (ArrayList) pair.First;
				ArrayList states  = (ArrayList) pair.Second;

				end = indices.Count;
				for (i = 0; i < end; i++) {
					if ((int) indices [i] < Count ) {
						this [(int) indices [i]].LoadViewState (states [i]);
					} else {
						ListItem temp = new ListItem ();
						temp.LoadViewState (states [i]);
						Add (temp);
					}
				}
			} else if (savedState is Triplet) {
				Triplet t = (Triplet) savedState;
				items = new ArrayList ((int) t.First);
				saveAll = true;
				object [] text = (object []) t.Second;
				object [] vals = (object []) t.Third;
				end = text.Length;
				for(i = 0; i < end; i++)
					items.Add (new ListItem (text[i].ToString (), vals[i].ToString ()));
			}
		}

		internal void TrackViewState()
		{
			marked = true;
			foreach(ListItem current in items)
				current.TrackViewState();
		}

		bool IList.IsFixedSize
		{
			get
			{
				return false;
			}
		}

		object IList.this[int index]
		{
			get
			{
				return this[index];
			}
			
			set
			{
				if(index >= 0 && index < Count)
					if(value is ListItem)
						items[index] = (ListItem) value;
			}
		}

		int IList.Add(object item)
		{
			int index = (item is ListItem ? items.Add((ListItem)item) : -1);
			if(index!=-1 && marked)
				((ListItem)item).Dirty = true;
			return index;
		}

		bool IList.Contains(object item)
		{
			if(item is ListItem)
				return Contains((ListItem)item);
			return false;
		}

		int IList.IndexOf(object item)
		{
			if(item is ListItem)
				return IndexOf((ListItem)item);
			return -1;
		}

		void IList.Insert(int index, object item)
		{
			if(item is ListItem)
				Insert(index, (ListItem)item);
		}

		void IList.Remove(object item)
		{
			if(item is string)
				Remove((string)item);
			if(item is ListItem)
				Remove((ListItem)item);
		}

		bool IStateManager.IsTrackingViewState
		{
			get
			{
				return marked;
			}
		}

		void IStateManager.LoadViewState(object state)
		{
			LoadViewState(state);
		}

		object IStateManager.SaveViewState()
		{
			return SaveViewState();
		}

		void IStateManager.TrackViewState()
		{
			TrackViewState();
		}
	}
}
