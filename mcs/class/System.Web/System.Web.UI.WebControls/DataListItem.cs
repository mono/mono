/**
 * Namespace: System.Web.UI.WebControls
 * Class:     DataListItem
 *
 * Authors:  Gaurav Vaish, Gonzalo Paniagua (gonzalo@ximian.com)
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  100%
 *
 * (C) Gaurav Vaish (2002)
 * (c) 2002 Ximian, Inc. (http://www.ximian.com)
 */

using System;
using System.Collections;
using System.Web;
using System.Web.UI;
using System.ComponentModel;

namespace System.Web.UI.WebControls
{
	[ToolboxItem(false)]
	public class DataListItem : WebControl, INamingContainer
	{
		int itemIndex;
		ListItemType itemType;
		object dataItem;

		public DataListItem(int itemIndex, ListItemType itemType)
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

		public virtual void RenderItem (HtmlTextWriter writer, bool extractRows, bool tableLayout)
		{
			if (extractRows){
				Table tbl = null;
				foreach (Control ctrl in Controls){
					if (ctrl is Table){
						tbl = (Table) ctrl;
						break;
					}
				}
				
				if (tbl == null)
					throw new HttpException ("Template table not found!");

				foreach (TableRow row in tbl.Rows)
					row.RenderControl (writer);
			} else {
				if (tableLayout)
					RenderContents (writer);
				else
					RenderControl (writer);
			}

		}

		protected override Style CreateControlStyle()
		{
			return new TableItemStyle();
		}

		protected override bool OnBubbleEvent(object source, EventArgs e)
		{
			if(e is CommandEventArgs)
			{
				RaiseBubbleEvent(this, new DataListCommandEventArgs(this, source, (CommandEventArgs)e));
				return true;
			}
			return false;
		}

		protected internal virtual void SetItemType(ListItemType itemType)
		{
			if(Enum.IsDefined(typeof(ListItemType), itemType))
			{
				this.itemType = itemType;
			}
		}
	}
}
