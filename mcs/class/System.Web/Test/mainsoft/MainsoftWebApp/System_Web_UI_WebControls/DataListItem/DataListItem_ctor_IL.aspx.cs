//
// Authors:
//   Rafael Mizrahi   <rafim@mainsoft.com>
//   Erez Lotan       <erezl@mainsoft.com>
//   Vladimir Krasnov <vladimirk@mainsoft.com>
//
//
// Copyright (c) 2002-2005 Mainsoft Corporation.
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
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace GHTTests.System_Web_dll.System_Web_UI_WebControls
{
	public class DataListItem_ctor_IL
		: GHTBaseWeb 
	{
		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e) 
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() 
		{    
			this.Load += new System.EventHandler(this.Page_Load);
		}
		#endregion

		private void Page_Load(object sender, System.EventArgs e) 
		{
			HtmlForm frm = (HtmlForm)FindControl("form1");
			GHTTestBegin(frm);
            
			// AlternatingItem
			GHTSubTestBegin("AlternatingItem");
			try 
			{
				System.Web.UI.WebControls.DataListItem item = new System.Web.UI.WebControls.DataListItem(1, ListItemType.AlternatingItem);
				Compare(item.ItemType, ListItemType.AlternatingItem);
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}
			GHTSubTestEnd();

			// EditItem
			GHTSubTestBegin("EditItem");
			try 
			{
				System.Web.UI.WebControls.DataListItem item = new System.Web.UI.WebControls.DataListItem(1, ListItemType.EditItem);
				Compare(item.ItemType, ListItemType.EditItem);
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}
			GHTSubTestEnd();

			// Footer
			GHTSubTestBegin("Footer");
			try 
			{
				System.Web.UI.WebControls.DataListItem item = new System.Web.UI.WebControls.DataListItem(1, ListItemType.Footer);
				Compare(item.ItemType, ListItemType.Footer);
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}
			GHTSubTestEnd();

			// Header
			GHTSubTestBegin("Header");
			try 
			{
				System.Web.UI.WebControls.DataListItem item = new System.Web.UI.WebControls.DataListItem(1, ListItemType.Header);
				Compare(item.ItemType, ListItemType.Header);
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}
			GHTSubTestEnd();

			// Item
			GHTSubTestBegin("Item");
			try 
			{
				System.Web.UI.WebControls.DataListItem item = new System.Web.UI.WebControls.DataListItem(1, ListItemType.Item);
				Compare(item.ItemType, ListItemType.Item);
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}
			GHTSubTestEnd();

			// Pager
			GHTSubTestBegin("Pager");
			try 
			{
				System.Web.UI.WebControls.DataListItem item = new System.Web.UI.WebControls.DataListItem(1, ListItemType.Pager);
				Compare(item.ItemType, ListItemType.Pager);
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}
			GHTSubTestEnd();

			// SelectedItem
			GHTSubTestBegin("SelectedItem");
			try 
			{
				System.Web.UI.WebControls.DataListItem item = new System.Web.UI.WebControls.DataListItem(1, ListItemType.SelectedItem);
				Compare(item.ItemType, ListItemType.SelectedItem);
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}
			GHTSubTestEnd();

			// Separator
			GHTSubTestBegin("Separator");
			try 
			{
				System.Web.UI.WebControls.DataListItem item = new System.Web.UI.WebControls.DataListItem(1, ListItemType.Separator);
				Compare(item.ItemType, ListItemType.Separator);
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}
			GHTSubTestEnd();

			// Positive  index
			GHTSubTestBegin("Positive index");
			try 
			{
				System.Web.UI.WebControls.DataListItem item = new System.Web.UI.WebControls.DataListItem(10, ListItemType.Item);
				Compare(item.ItemIndex, 10);
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}
			GHTSubTestEnd();

			// Negative index
			GHTSubTestBegin("Negative index");
				try 
				{
					System.Web.UI.WebControls.DataListItem item = new System.Web.UI.WebControls.DataListItem(-10, ListItemType.Item);
					Compare(item.ItemIndex, -10);
				}
				catch (Exception ex) 
				{
					GHTSubTestUnexpectedExceptionCaught(ex);
				}
			GHTSubTestEnd();

			// -1
			GHTSubTestBegin("-1");
			try 
			{
				System.Web.UI.WebControls.DataListItem item = new System.Web.UI.WebControls.DataListItem(-1, ListItemType.Item);
				Compare(item.ItemIndex, -1);
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}
			GHTSubTestEnd();

			GHTTestEnd();

		}
	}
}
