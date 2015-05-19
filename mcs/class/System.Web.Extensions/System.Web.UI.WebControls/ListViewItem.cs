//
// System.Web.UI.WebControls.ListViewItem
//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2007 Novell, Inc
//

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
using System;
using System.ComponentModel;

namespace System.Web.UI.WebControls
{
	[ToolboxItemAttribute (false)]
	public class ListViewItem : Control, INamingContainer
	, IDataItemContainer
	{
		internal ListViewItem ()
			: this (ListViewItemType.DataItem)
		{
		}
		
		public ListViewItem (ListViewItemType itemType)
		{
			ItemType = itemType;
		}
		
		protected override bool OnBubbleEvent (object source, EventArgs e)
		{
			CommandEventArgs args = e as CommandEventArgs;
			if (args != null) {
				RaiseBubbleEvent (this, new ListViewCommandEventArgs (this, source, args));
				return true;
			}
			
			return base.OnBubbleEvent (source, e);
		}
		
		public ListViewItemType ItemType {
			get;
			private set;
		}
		
		public virtual object DataItem {
			get;
			set;
		}
		
		public virtual int DataItemIndex {
			get;
			protected set;
		}
		
		public virtual int DisplayIndex {
			get;
			protected set;
		}
	}
}
