/**
 * Namespace: System.Web.UI.WebControls
 * Class:     DataGridItem
 * 
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  100%
 * 
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public class DataGridItem : TableRow, INamingContainer
	{
		private int itemIndex;
		private int dataSetIndex;
		private ListItemType itemType;
		private object dataItem;

		public DataGridItem(int itemIndex, int dataSetIndex, ListItemType itemType): base()
		{
			this.itemIndex    = itemIndex;
			this.dataSetIndex = dataSetIndex;
			this.itemType     = itemType;
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
		
		public virtual int DataSetIndex
		{
			get
			{
				return dataSetIndex;
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
				DataGridCommandEventArgs args = new DataGridCommandEventArgs(this, source, (CommandEventArgs)e);
				RaiseBubbleEvent(this, args);
			}
		}
	}
}
