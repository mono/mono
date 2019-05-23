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
using System.Data;
using System.Collections;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace GHTTests.System_Web_dll.System_Web_UI_WebControls
{
	public class HyperLinkColumn_InitializeCell_TIL
		: GHTBaseWeb 
	{
		protected GHTWebControls.GHTSubTest GHTSubTest1;
		protected System.Web.UI.WebControls.DataGrid DataGrid1;
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
			this.DataGrid1.ItemDataBound += new System.Web.UI.WebControls.DataGridItemEventHandler(this.DataGrid1_ItemDataBound);
			this.Load += new System.EventHandler(this.Page_Load);

		}
		#endregion

		private void Page_Load(object sender, System.EventArgs e) 
		{
			//Put user code to initialize the page here

			System.Web.UI.HtmlControls.HtmlForm frm = (HtmlForm)this.FindControl("Form1");
			GHTTestBegin(frm);

			GHTActiveSubTest = GHTSubTest1;
			try 
			{
				DataGrid1.DataSource = GHTTests.GHDataSources.DSDataTable(0, 1, "http://");
				System.Web.UI.WebControls.HyperLinkColumn c_id = new System.Web.UI.WebControls.HyperLinkColumn();
				System.Web.UI.WebControls.HyperLinkColumn c_name = new System.Web.UI.WebControls.HyperLinkColumn();
				System.Web.UI.WebControls.HyperLinkColumn c_company = new System.Web.UI.WebControls.HyperLinkColumn();

				c_id.DataTextField = "ID";
				c_id.NavigateUrl = "http://www.example.com";
				c_name.DataTextField = "Name";
				c_name.DataNavigateUrlField = "Name";
				c_company.DataTextField = "Company";
				c_company.DataNavigateUrlField = "Company";

				DataGrid1.Columns.Add(c_id);
				DataGrid1.Columns.Add(c_name);
				DataGrid1.Columns.Add(c_company);

				DataGrid1.DataBind();;
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTTestEnd();
		}

		private void DataGrid1_ItemDataBound(object sender, System.Web.UI.WebControls.DataGridItemEventArgs e)
		{
			ListItemType itemType = (ListItemType)e.Item.ItemType;

			if ((itemType != ListItemType.Header) &&
				(itemType != ListItemType.Footer) &&
				(itemType != ListItemType.Separator ))
			{
				TableCell currentCell = (TableCell)e.Item.Controls[0];
				DataGrid1.Columns[1].InitializeCell(currentCell, 1, ListItemType.Item);
				currentCell.Attributes.Add("Index", e.Item.ItemIndex.ToString());
			}
		}
	}
}
