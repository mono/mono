/**
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls
 * Class     : ListDataBindEventArgs
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System;
using System.Web.Mobile;

namespace System.Web.UI.MobileControls
{
	public class ListDataBindEventArgs : EventArgs
	{
		private MobileListItem item;
		private object         dataItem;

		public ListDataBindEventArgs(MobileListItem item, object dataItem)
		{
			this.item     = item;
			this.dataItem = dataItem;
		}

		public object DataItem
		{
			get
			{
				return dataItem;
			}
		}

		public MobileListItem ListItem
		{
			get
			{
				return item;
			}
		}
	}
}
