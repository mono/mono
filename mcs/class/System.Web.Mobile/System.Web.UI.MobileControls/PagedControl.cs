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
		private static readonly object ItemDataBindEvent = new object();
		private static readonly object ItemCommandEvent  = new object();

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

		private void OnLoadItems()
		{
			OnLoadItems(new LoadItemsEventArgs(PagerItemIndex, PagerItemCount));
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

		protected virtual void OnLoadItems(LoadItemsEventArgs e)
		{
			LoadItemsEventHandler lieh = (LoadItemsEventHandler)(Events[LoadItemsEvent]);
			if(lieh != null)
				lieh(this, e);
		}

		protected override bool OnBubbleEvent(object sender, EventArgs e)
		{
			if(e is ListCommandEventArgs)
			{
				OnItemCommand((ListCommandEventArgs)e);
				return true;
			}
			return false;
		}

		protected override void OnDataBinding(EventArgs e)
		{
			base.OnDataBinding(e);
			throw new NotImplementedException();
		}

		protected void OnItemDataBind(ListDataBindEventArgs e)
		{
			ListDataBindEventHandler ldbeh = (ListDataBindEventHandler)(Events[ItemDataBindEvent]);
			if(ldbeh != null)
				ldbeh(this, e);
		}

		protected virtual void OnItemCommand(ListCommandEventArgs e)
		{
			ListCommandEventHandler lceh = (ListCommandEventHandler)(Events[ItemCommandEvent]);
			if(lceh != null)
				lceh(this, e);
		}
	}
}
