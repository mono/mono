/**
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls
 * Class     : ItemPager
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System.Web.Mobile;

namespace System.Web.UI.MobileControls
{
	public class ItemPager
	{
		private MobileControl control;

		private int firstPage;
		private int lastPage;

		private int firstPageItemCount;
		private int fullPageItemCount;
		private int lastPageItemCount;

		public ItemPager()
		{
		}

		public ItemPager(ControlPager pager, MobileControl control,
		                 int itemCount, int itemsPerPage, int itemWeight)
		{
			this.control = control;
			if(itemsPerPage > 0)
			{
				throw new NotImplementedException();
			} else
			{
				throw new NotImplementedException();
			}
		}
	}
}
