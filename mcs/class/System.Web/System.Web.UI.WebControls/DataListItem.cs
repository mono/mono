
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
#if NET_2_0
		, IDataItemContainer
#endif
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
