/**
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls
 * Class     : ControlPager
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System.Web.Mobile;

namespace System.Web.UI.MobileControls
{
	public class ControlPager
	{
		private int pageCount = 0;
		private int maxPage   = -1;
		private int pageWt;
		private int remainingWt = 0;

		private Form form;

		// To ponder: will const be better?
		public static readonly int DefaultWeight = 100;
		public static readonly int UseDefaultWeight = -1;

		public ControlPager(Form form, int pageWeight)
		{
			this.form   = form;
			this.pageWt = pageWeight;
		}

		public int PageCount
		{
			get
			{
				return pageCount;
			}
			set
			{
				pageCount = value;
			}
		}

		public int MaximumPage
		{
			get
			{
				return maxPage;
			}
			set
			{
				maxPage = value;
			}
		}

		public int PageWeight
		{
			get
			{
				return pageWt;
			}
			set
			{
				pageWt = value;
			}
		}

		public int RemainingWeight
		{
			get
			{
				return remainingWt;
			}
			set
			{
				remainingWt = value;
			}
		}

		public ItemPager GetItemPager(MobileControl control, int itemCount,
		                              int itemsPerPage, int itemWeight)
		{
			return new ItemPager(this, control, itemCount,
			                     itemsPerPage, itemWeight);
		}

		public int GetPage(int weight)
		{
			if(weight > remainingWt)
			{
				PageCount += 1;
				RemainingWeight = PageWeight;
			}
			if(remainingWt > weight)
			{
				remainingWt -= weight;
			} else
			{
				remainingWt = 0;
			}
			return PageCount;
		}
	}
}
