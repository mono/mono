//
// System.Web.UI.WebControls.SiteMapNodeItem.cs
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
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
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//

#if NET_2_0
using System;
using System.ComponentModel;
using System.Drawing;

namespace System.Web.UI.WebControls
{
	[ToolboxItem (false)]
	public class SiteMapNodeItem: WebControl, IDataItemContainer, INamingContainer
	{
		int itemIndex;
		SiteMapNodeItemType itemType;
		SiteMapNode node;
		
		public SiteMapNodeItem (int itemIndex, SiteMapNodeItemType itemType)
		{
			this.itemIndex = itemIndex;
			SetItemType (itemType);
		}
		
		protected internal virtual void SetItemType (SiteMapNodeItemType itemType)
		{
			this.itemType = itemType;
		}
		
		public virtual int ItemIndex {
			get { return itemIndex; }
		}
		
		public virtual SiteMapNodeItemType ItemType {
			get { return itemType; }
		}
		
		public virtual SiteMapNode SiteMapNode {
			get { return node; }
			set { node = value; }
		}
		
		object IDataItemContainer.DataItem {
			get { return node; }
		}
		
		int IDataItemContainer.DataItemIndex {
			get { return itemIndex; }
		}
		
		int IDataItemContainer.DisplayIndex {
			get { return itemIndex; }
		}
	}
}

#endif
