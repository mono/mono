/**
* Namespace: System.Web.UI.WebControls
* Class:     RepeaterItem
*
* Author:  Gaurav Vaish
* Maintainer: gvaish@iitk.ac.in
* Implementation: yes
* Status:  100%
*
* (C) Gaurav Vaish (2001)
*/

using System;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	[ToolboxItem(false)]
	public class RepeaterItem: Control, INamingContainer
	{
		private int          itemIndex;
		private ListItemType itemType;
		private object       dataItem;

		public RepeaterItem(int itemIndex, ListItemType itemType)
		{
			this.itemIndex = itemIndex;
			this.itemType  = itemType;
		}

		public virtual object DataItem
		{
			get
			{
				return dataItem;
			}
			set
			{
				dataItem = value;
			}
		}

		public virtual int ItemIndex
		{
			get
			{
				return itemIndex;
			}
		}

		public virtual ListItemType ItemType
		{
			get
			{
				return itemType;
			}
		}

		protected override bool OnBubbleEvent(object source, EventArgs e)
		{
			if(e is CommandEventArgs)
			{
				RepeaterCommandEventArgs rcea = new RepeaterCommandEventArgs(this, source, (CommandEventArgs)e);
				RaiseBubbleEvent(source, rcea);
			}
		}
	}
}
