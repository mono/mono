/**
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls
 * Class     : ListCommandEventArgs
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System.Web.UI.WebControls;
using System.Web.Mobile;

namespace System.Web.UI.MobileControls
{
	public class ListCommandEventArgs : CommandEventArgs
	{
		protected static readonly string DefaultCommand = "Default";

		private object cmdSource;
		private MobileListItem listItem;

		public ListCommandEventArgs(MobileListItem item,
		                            object commandSource)
		                            :base(DefaultCommand, commandSource)
		{
			this.listItem  = item;
			this.cmdSource = commandSource;
		}

		public ListCommandEventArgs(MobileListItem item,
		        object commandSource, CommandEventArgs originalArgs)
		        : base(originalArgs)
		{
			this.cmdSource = commandSource;
			this.listItem  = item;
		}

		public object CommandSource
		{
			get
			{
				return this.cmdSource;
			}
		}

		public MobileListItem ListItem
		{
			get
			{
				return this.listItem;
			}
		}
	}
}
