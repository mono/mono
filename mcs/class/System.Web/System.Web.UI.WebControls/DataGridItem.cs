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
// Copyright (c) 2005-2010 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Bartok	(pbartok@novell.com)
//
//

using System.ComponentModel;
using System.Security.Permissions;

namespace System.Web.UI.WebControls
{
	// CAS
	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class DataGridItem : TableRow, INamingContainer, IDataItemContainer
	{
		#region Fields
		object item;
		int dataset_index;
		int item_index;
		ListItemType item_type;
		#endregion	// Fields

		#region Public Constructors
		public DataGridItem(int itemIndex, int dataSetIndex, ListItemType itemType) {
			item_index = itemIndex;
			dataset_index = dataSetIndex;
			item_type = itemType;
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		public virtual object DataItem {
			get { return item; }
			set { item = value; }
		}

		public virtual int DataSetIndex {
			get { return dataset_index; }
		}

		public virtual int ItemIndex {
			get { return item_index; }
		}

		public virtual ListItemType ItemType {
			get { return item_type; }
		}
		#endregion	// Public Instance Properties

		#region IDataItemContainer Properties
		object IDataItemContainer.DataItem {
			get { return item; }
		}

		int IDataItemContainer.DataItemIndex{
			get { return item_index; }
		}

		int IDataItemContainer.DisplayIndex{
			get { return item_index; }
		}
		#endregion	// IDataItemContainer Properties

		#region Public Instance Methods
		protected override bool OnBubbleEvent(object source, EventArgs args)
		{
			// Nikhil Kothari, pg 312-313:
			if (args is CommandEventArgs) {
				RaiseBubbleEvent(this, new DataGridCommandEventArgs(this, source, (CommandEventArgs)args));
				return true;
			}

			return base.OnBubbleEvent (source, args);
		}

		protected internal virtual void SetItemType(ListItemType itemType)
		{
			item_type = itemType;
			
		}
		#endregion	// Public Instance Methods
	}
}
