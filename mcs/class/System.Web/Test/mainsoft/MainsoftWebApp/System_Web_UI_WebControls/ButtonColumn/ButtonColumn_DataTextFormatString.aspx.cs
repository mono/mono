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
	public class ButtonColumn_DataTextFormatString
		: GHTBaseWeb 
	{
		protected System.Web.UI.WebControls.DataGrid DataGrid1;
		protected System.Web.UI.WebControls.DataGrid DataGrid2;
		protected GHTWebControls.GHTSubTest Ghtsubtest2;
		protected System.Web.UI.WebControls.DataGrid DataGrid3;
		protected GHTWebControls.GHTSubTest Ghtsubtest3;
		protected System.Web.UI.WebControls.DataGrid DataGrid4;
		protected GHTWebControls.GHTSubTest Ghtsubtest4;
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

		}
		#endregion

		private void Page_Load(object sender, EventArgs e)
		{
			HtmlForm form1 = (HtmlForm) this.FindControl("Form1");
			this.GHTTestBegin(form1);
			base.GHTActiveSubTest = this.GHTSubTest1;
			try
			{
				this.DataGrid1.DataSource = GHDataSources.DSDataTable();
				ButtonColumn column2 = new ButtonColumn();
				ButtonColumn column3 = new ButtonColumn();
				ButtonColumn column1 = new ButtonColumn();
				column2.DataTextField = "ID";
				column3.DataTextField = "Name";
				column1.DataTextField = "Company";
				column2.DataTextFormatString = "";
				column3.DataTextFormatString = "(format str)";
				column1.DataTextFormatString = "{0:c5}";
				this.DataGrid1.Columns.Add(column2);
				this.DataGrid1.Columns.Add(column3);
				this.DataGrid1.Columns.Add(column1);
				this.DataGrid1.DataBind();
				this.GHTSubTestAddResult(column2.DataTextFormatString);
				this.GHTSubTestAddResult(column3.DataTextFormatString);
				this.GHTSubTestAddResult(column1.DataTextFormatString);
			}
			catch (Exception exception5)
			{
				// ProjectData.SetProjectError(exception5);
				Exception exception1 = exception5;
				this.GHTSubTestUnexpectedExceptionCaught(exception1);
				// ProjectData.ClearProjectError();
			}
			base.GHTActiveSubTest = this.Ghtsubtest2;
			try
			{
				this.DataGrid2.DataSource = GHDataSources.DSDataTable();
				ButtonColumn column4 = new ButtonColumn();
				ButtonColumn column5 = new ButtonColumn();
				ButtonColumn column6 = new ButtonColumn();
				column4.DataTextField = "ID";
				column5.DataTextField = "ID";
				column6.DataTextField = "ID";
				column4.DataTextFormatString = "";
				column5.DataTextFormatString = "(format str)";
				column6.DataTextFormatString = "{0:c5}";
				this.DataGrid2.Columns.Add(column4);
				this.DataGrid2.Columns.Add(column5);
				this.DataGrid2.Columns.Add(column6);
				this.DataGrid2.DataBind();
				this.GHTSubTestAddResult(column4.DataTextFormatString);
				this.GHTSubTestAddResult(column5.DataTextFormatString);
				this.GHTSubTestAddResult(column6.DataTextFormatString);
			}
			catch (Exception exception6)
			{
				// ProjectData.SetProjectError(exception6);
				Exception exception2 = exception6;
				this.GHTSubTestUnexpectedExceptionCaught(exception2);
				// ProjectData.ClearProjectError();
			}
			base.GHTActiveSubTest = this.Ghtsubtest3;
			try
			{
				this.DataGrid3.DataSource = GHDataSources.DSDataTable();
				this.DataGrid3.DataBind();
				this.GHTSubTestAddResult(((ButtonColumn) this.DataGrid3.Columns[0]).DataTextFormatString);
				this.GHTSubTestAddResult(((ButtonColumn) this.DataGrid3.Columns[1]).DataTextFormatString);
				this.GHTSubTestAddResult(((ButtonColumn) this.DataGrid3.Columns[2]).DataTextFormatString);
			}
			catch (Exception exception7)
			{
				// ProjectData.SetProjectError(exception7);
				Exception exception3 = exception7;
				this.GHTSubTestUnexpectedExceptionCaught(exception3);
				// ProjectData.ClearProjectError();
			}
			base.GHTActiveSubTest = this.Ghtsubtest4;
			try
			{
				this.DataGrid4.DataSource = GHDataSources.DSDataTable();
				this.DataGrid4.DataBind();
				this.GHTSubTestAddResult(((ButtonColumn) this.DataGrid4.Columns[0]).DataTextFormatString);
				this.GHTSubTestAddResult(((ButtonColumn) this.DataGrid4.Columns[1]).DataTextFormatString);
				this.GHTSubTestAddResult(((ButtonColumn) this.DataGrid4.Columns[2]).DataTextFormatString);
			}
			catch (Exception exception8)
			{
				// ProjectData.SetProjectError(exception8);
				Exception exception4 = exception8;
				this.GHTSubTestUnexpectedExceptionCaught(exception4);
				// ProjectData.ClearProjectError();
			}
			this.GHTTestEnd();
		}
 
	}
}
