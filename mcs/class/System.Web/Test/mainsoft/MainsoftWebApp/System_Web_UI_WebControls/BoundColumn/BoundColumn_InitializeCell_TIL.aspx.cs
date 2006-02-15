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
	public class BoundColumn_InitializeCell_TIL
		: GHTBaseWeb 
	{
		protected System.Web.UI.WebControls.DataGrid DataGrid1;
		protected GHTWebControls.GHTSubTest GHTSubTest1;
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
			this.DataGrid1.ItemDataBound += new DataGridItemEventHandler(DataGrid1_ItemDataBound);
		}
		#endregion

		private void Page_Load(object sender, EventArgs e)
		{
			HtmlForm form1 = (HtmlForm) (HtmlForm)this.FindControl("Form1");
			this.GHTTestBegin(form1);
			base.GHTActiveSubTest = this.GHTSubTest1;
			try
			{
				this.DataGrid1.DataSource = GHTTests.GHDataSources.DSDataTable();
				BoundColumn column2 = new BoundColumn();
				BoundColumn column3 = new BoundColumn();
				BoundColumn column1 = new BoundColumn();
				column2.DataField = "ID";
				column3.DataField = "Name";
				column1.DataField = "Company";
				this.DataGrid1.Columns.Add(column2);
				this.DataGrid1.Columns.Add(column3);
				this.DataGrid1.Columns.Add(column1);
				this.DataGrid1.DataBind();
			}
			catch (Exception exception2)
			{
				// ProjectData.SetProjectError(exception2);
				Exception exception1 = exception2;
				this.GHTSubTestUnexpectedExceptionCaught(exception1);
				// ProjectData.ClearProjectError();
			}
			this.GHTTestEnd();
		}
 

		private void DataGrid1_ItemDataBound(object sender, DataGridItemEventArgs e)
		{
			ListItemType type1 = e.Item.ItemType;
			if (((type1 != ListItemType.Header) && (type1 != ListItemType.Footer)) && (type1 != ListItemType.Separator))
			{
				TableCell cell1 = (TableCell) e.Item.Controls[0];
				this.DataGrid1.Columns[1].InitializeCell(cell1, 1, ListItemType.Item);
				cell1.Attributes.Add("Index", e.Item.ItemIndex.ToString());
			}
		}
 
	}
}
