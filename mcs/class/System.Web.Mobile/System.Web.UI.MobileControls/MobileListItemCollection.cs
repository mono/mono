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
			RemoveAt(IndexOf(new MobileListItem(item));
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
