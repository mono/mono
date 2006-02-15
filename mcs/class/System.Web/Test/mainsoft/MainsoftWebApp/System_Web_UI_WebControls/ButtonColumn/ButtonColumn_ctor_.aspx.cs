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
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace GHTTests.System_Web_dll.System_Web_UI_WebControls
{
	/// <summary>
	/// Summary description for ButtonColumn_ctor_.
	/// </summary>
	public class ButtonColumn_ctor_ : GHTBaseWeb 
	{
		protected System.Web.UI.WebControls.DataGrid DataGrid1;
		protected GHTWebControls.GHTSubTest GHTSubTest1;
		protected GHTWebControls.GHTSubTest Ghtsubtest2;
		protected System.Web.UI.WebControls.DataGrid DataGrid2;
	
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
				this.DataGrid1.Columns.Add(column2);
				this.DataGrid1.Columns.Add(column3);
				this.DataGrid1.Columns.Add(column1);
				this.DataGrid1.DataBind();
			}
			catch (Exception exception3)
			{
				// ProjectData.SetProjectError(exception3);
				Exception exception1 = exception3;
				this.GHTSubTestUnexpectedExceptionCaught(exception1);
				// ProjectData.ClearProjectError();
			}
			base.GHTActiveSubTest = this.Ghtsubtest2;
			try
			{
				this.DataGrid2.DataSource = GHDataSources.DSDataTable();
				this.DataGrid2.DataBind();
			}
			catch (Exception exception4)
			{
				// ProjectData.SetProjectError(exception4);
				Exception exception2 = exception4;
				this.GHTSubTestUnexpectedExceptionCaught(exception2);
				// ProjectData.ClearProjectError();
			}
			this.GHTTestEnd();
		}
 

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
	}
}
