
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
 * Class     : PagedControl
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System.Web.UI;
using System.Web.Mobile;

namespace System.Web.UI.MobileControls
{
	public abstract class PagedControl : MobileControl
	{
		private static readonly object LoadItemsEvent    = new object();

		private int itemCount = 0;
		private ItemPager itemPager;
		private bool pagingCharsChanged = false;

		protected PagedControl()
		{
		}

		public event LoadItemsEventHandler LoadItems
		{
			add
			{
				Events.AddHandler(LoadItemsEvent, value);
			}
			remove
			{
				Events.RemoveHandler(LoadItemsEvent, value);
			}
		}

		private int PagerItemIndex
		{
			get
			{
				return (itemPager == null ? 0 : itemPager.ItemIndex);
			}
		}

		private int PagerItemCount
		{
			get
			{
				return (itemPager == null ? InternalItemCount :
					    itemPager.ItemCount);
			}
		}

		protected abstract int InternalItemCount { get; }

		public int FirstVisibleItemIndex
		{
			get
			{
				if(!IsCustomPaging && EnablePagination)
					return PagerItemIndex;
				return 0;
			}
		}

		private bool IsCustomPaging
		{
			get
			{
				return (itemCount > 0);
			}
		}

		public int ItemCount
		{
			get
			{
				return itemCount;
			}
			set
			{
				itemCount = value;
			}
		}

		public int ItemWeight
		{
			get
			{
				return ControlPager.DefaultWeight;
			}
		}

		public int ItemsPerPage
		{
			get
			{
				object o = ViewState["ItemsPerPage"];
				if(o != null)
					return (int)o;
				return 0;
			}
			set
			{
				ViewState["ItemsPerPage"] = value;
			}
		}

		public int VisibleItemCount
		{
			get
			{
				if(IsCustomPaging || !EnablePagination)
					return InternalItemCount;
				return PagerItemCount;
			}
		}

		public override int VisibleWeight
		{
			get
			{
				if(VisibleItemCount == -1)
					return 0;
				return VisibleItemCount * GetItemWeight();
			}
		}

		private int GetItemWeight()
		{
			int iw = Adapter.ItemWeight;
			if(iw == ControlPager.UseDefaultWeight)
				return ItemWeight;
			return iw;
		}

		protected virtual void OnLoadItems(LoadItemsEventArgs e)
		{
			LoadItemsEventHandler lieh = (LoadItemsEventHandler)(Events[LoadItemsEvent]);
			if(lieh != null)
				lieh(this, e);
		}

		private void OnLoadItems()
		{
			OnLoadItems(new LoadItemsEventArgs(PagerItemIndex, PagerItemCount));
		}

		protected override void OnPageChange(int oldPageIndex, int newPageIndex)
		{
			pagingCharsChanged = true;
		}

		protected override void OnPreRender(EventArgs e)
		{
			if(IsCustomPaging)
			{
				if(!Page.IsPostBack || Form.PaginationStateChanged
				   || pagingCharsChanged || !IsViewStateEnabled())
				{
					OnLoadItems();
				}
			}
			base.OnPreRender(e);
		}

		private bool IsViewStateEnabled()
		{
			Control ctrl = this;
			while(ctrl != null && ctrl.EnableViewState)
			{
				ctrl = ctrl.Parent;
			}
			return (ctrl == null);
		}

		public override void PaginateRecursive(ControlPager pager)
		{
			int ic = 0;
			int ipp = 0;
			if(IsCustomPaging || InternalItemCount == 0)
				ic = ItemCount;
			ipp = ItemsPerPage;
			itemPager = pager.GetItemPager(this, ic, ipp, GetItemWeight());
		}
	}
}
