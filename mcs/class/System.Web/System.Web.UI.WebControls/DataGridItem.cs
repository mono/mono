
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
#if NET_2_0
		, IDataItemContainer
#endif
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
				return true;
			}
			return false;
		}

		protected internal virtual void SetItemType(ListItemType itemType)
		{
			this.itemType = itemType;
		}
	}
}
