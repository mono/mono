
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
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls
 * Class     : MobileListItemCollection
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System;
using System.Collections;
using System.Web.UI;

namespace System.Web.UI.MobileControls
{
	public class MobileListItemCollection : ArrayListCollectionBase,
	                                        IStateManager
	{
		private int baseIndex = 0;

		private bool marked  = false;
		private bool saveAll = false;
		private bool saveSel = false;

		public MobileListItemCollection()
		{
		}

		public MobileListItemCollection(ArrayList items) : base(items)
		{
		}

		void IStateManager.LoadViewState(object state)
		{
			throw new NotImplementedException();
		}

		object IStateManager.SaveViewState()
		{
			throw new NotImplementedException();
		}

		void IStateManager.TrackViewState()
		{
			this.marked = true;
			throw new NotImplementedException();
		}

		bool IStateManager.IsTrackingViewState
		{
			get
			{
				return this.marked;
			}
		}

		public void Add(string item)
		{
			Add(new MobileListItem(item));
		}

		public void Add(MobileListItem item)
		{
			throw new NotImplementedException();
		}

		public MobileListItem this[int index]
		{
			get
			{
				return (MobileListItem)base.Items[index];
			}
		}

		public void Clear()
		{
			base.Items.Clear();
			if(this.marked)
				this.saveAll = true;
		}

		public bool Contains(MobileListItem item)
		{
			return Items.Contains(item);
		}

		public MobileListItem[] GetAll()
		{
			MobileListItem[] retVal = new MobileListItem[Items.Count];
			if(Items.Count > 0)
				Items.CopyTo(0, retVal, 0, Items.Count);
			return retVal;
		}

		public int IndexOf(MobileListItem item)
		{
			return Items.IndexOf(item);
		}

		public virtual void Insert(int index, string item)
		{
			Insert(index, new MobileListItem(item));
		}

		public void Insert(int index, MobileListItem item)
		{
			Items.Insert(index, item);
			throw new NotImplementedException();
		}

		public void Remove(string item)
		{
			RemoveAt(IndexOf(new MobileListItem(item)));
		}

		public void Remove(MobileListItem item)
		{
			RemoveAt(IndexOf(item));
		}

		public void RemoveAt(int index)
		{
			if(index >= 0)
			{
				Items.RemoveAt(index);
				throw new NotImplementedException();
			}
		}

		public void SetAll(MobileListItem[] items)
		{
			throw new NotImplementedException();
		}

		public int BaseIndex
		{
			get
			{
				return this.baseIndex;
			}
			set
			{
				this.baseIndex = value;
			}
		}

		public bool SaveSelection
		{
			get
			{
				return this.saveSel;
			}
			set
			{
				this.saveSel = value;
			}
		}
	}
}
