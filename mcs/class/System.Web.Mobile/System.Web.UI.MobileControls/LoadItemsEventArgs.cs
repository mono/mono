/**
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls
 * Class     : LoadItemsEventArgs
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

namespace System.Web.UI.MobileControls
{
	public class LoadItemsEventArgs : EventArgs
	{
		private int index;
		private int count;

		public LoadItemsEventArgs(int index, int count)
		{
			this.index = index;
			this.count = count;
		}

		public int ItemCount
		{
			get
			{
				return count;
			}
		}

		public int ItemIndex
		{
			get
			{
				return index;
			}
		}
	}
}
