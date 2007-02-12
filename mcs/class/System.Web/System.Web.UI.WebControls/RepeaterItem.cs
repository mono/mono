//
// System.Web.UI.WebControls.RepeaterItem
//
// Authors:
//	Ben Maurer (bmaurer@novell.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.ComponentModel;

namespace System.Web.UI.WebControls {
	[ToolboxItem ("")]
	public class RepeaterItem : Control, INamingContainer
#if NET_2_0
		, IDataItemContainer
#endif
	{
	
		public RepeaterItem (int itemIndex, ListItemType itemType)
		{
			idx = itemIndex;
			type = itemType;
		}

		// see ... "Building DataBound Templated Custom ASP.NET " on msdn
		//
		// This technique is used in the DataGrid, DataList, and Repeater to handle the
		// Command event of Buttons, LinkButtons, and ImageButtons within the
		// controls. Since the button's Command event calls RaiseBubbleEvent(), this
		// percolates the event up to the button's parent.
		protected override bool OnBubbleEvent (object source, EventArgs e)
		{
			CommandEventArgs ce = e as CommandEventArgs;
			if (ce != null) {
				base.RaiseBubbleEvent (this, new RepeaterCommandEventArgs (this, source, ce));
				return true;
			}

			return false;
		}
	
		public virtual object DataItem {
			get {
				return data_item;
			}
			set {
				data_item = value;
			}
		}
	
		public virtual int ItemIndex {
			get {
				return idx;
			}
		}
	
		public virtual ListItemType ItemType {
			get {
				return type;
			}
		}

		object data_item;
		int idx;
		ListItemType type;

#if NET_2_0

		int IDataItemContainer.DataItemIndex {
			get { return ItemIndex; }
		}

		int IDataItemContainer.DisplayIndex {
			get { return ItemIndex; }
		}

#endif
	}
}
