//
// System.Web.UI.WebControls.DataListItem.cs
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005-2010 Novell, Inc (http://www.novell.com)
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

using System.Collections;
using System.ComponentModel;
using System.Security.Permissions;

namespace System.Web.UI.WebControls
{
	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[ToolboxItem ("")]
	public class DataListItem : WebControl, INamingContainer, IDataItemContainer
	{
		int index;
		ListItemType type;
		object item;

		public DataListItem (int itemIndex, ListItemType itemType)
		{
			index = itemIndex;
			type = itemType;
		}

		public virtual object DataItem {
			get { return item; }
			set { item = value; }
		}

		public virtual int ItemIndex {
			get { return index; }
		}

		public virtual ListItemType ItemType {
			get { return type; }
		}
#if NET_4_0
		public override bool SupportsDisabledAttribute {
			get { return RenderingCompatibilityLessThan40; }
		}
#endif
		protected override Style CreateControlStyle ()
		{
			return new TableItemStyle (ViewState);
		}

		// see ... "Building DataBound Templated Custom ASP.NET" on msdn
		// http://msdn.microsoft.com/library/en-us/dnaspp/html/databoundtemplatedcontrols.asp
		//
		// This technique is used in the DataGrid, DataList, and Repeater to handle the
		// Command event of Buttons, LinkButtons, and ImageButtons within the
		// controls. Since the button's Command event calls RaiseBubbleEvent(), this
		// percolates the event up to the button's parent.
		protected override bool OnBubbleEvent (object source, EventArgs e)
		{
			CommandEventArgs ce = e as CommandEventArgs;
			if (ce != null) {
				base.RaiseBubbleEvent (this, new DataListCommandEventArgs (this, source, ce));
				return true;
			}
			return false;
		}

		public virtual void RenderItem (HtmlTextWriter writer, bool extractRows, bool tableLayout)
		{
			bool span = (!extractRows && !tableLayout); 
			if (span)
				writer.RenderBeginTag (TagKey);
			
			if (HasControls ()) {
				if (extractRows) {
					bool table = false;
					foreach (Control c in Controls) {
						Table t = (c as Table);
						if (t != null) {
							table = true;
							foreach (TableRow tr in t.Rows) {
								if (ControlStyleCreated && !ControlStyle.IsEmpty)
									tr.ControlStyle.MergeWith (ControlStyle);
								tr.RenderControl (writer);
							}
							break; // ignore all, but the first, table
						}
					}
					if (!table) {
						// if it has controls then it must have a Table control
						throw new HttpException ("No Table found in DataList template.");
						// other controls are ignored (as long as a Table is there)
					}
				} else {
					// this get rides of the extra <span>...</span> around our stuff
					RenderContents (writer);
				}
			}
			
			if (span)
				writer.RenderEndTag ();
		}

		protected virtual void SetItemType (ListItemType itemType)
		{
			type = itemType;
		}

		object IDataItemContainer.DataItem {
			get { return item; }
		}

		int IDataItemContainer.DataItemIndex {
			get { return index; }
		}

		int IDataItemContainer.DisplayIndex {
			get { return index; }
		}
	}
}
