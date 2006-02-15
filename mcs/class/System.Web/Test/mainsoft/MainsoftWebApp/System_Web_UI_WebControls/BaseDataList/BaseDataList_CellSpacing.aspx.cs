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
using GHTTests;
using GHTWebControls;

namespace GHTTests.System_Web_dll.System_Web_UI_WebControls
{
	public class BaseDataList_CellSpacing
		: GHTDataListBase
	{
		protected System.Web.UI.WebControls.DataGrid DataGrid1;
		protected GHTWebControls.GHTSubTest GHTSubTest1;
		protected System.Web.UI.WebControls.DataGrid DataGrid2;
		protected GHTWebControls.GHTSubTest GHTSubTest2;
		protected System.Web.UI.WebControls.DataGrid DataGrid3;
		protected GHTWebControls.GHTSubTest GHTSubTest3;
		protected System.Web.UI.WebControls.DataGrid DataGrid4;
		protected GHTWebControls.GHTSubTest GHTSubTest4;
		protected System.Web.UI.WebControls.DataList DataList1;
		protected GHTWebControls.GHTSubTest GHTSubTest5;
		protected System.Web.UI.WebControls.DataList DataList2;
		protected GHTWebControls.GHTSubTest GHTSubTest6;
		protected System.Web.UI.WebControls.DataList DataList3;
		protected GHTWebControls.GHTSubTest GHTSubTest7;
		protected System.Web.UI.WebControls.DataList DataList4;
		protected GHTWebControls.GHTSubTest GHTSubTest8;
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
			HtmlForm frm  = (HtmlForm)FindControl("Form1");
			GHTTestBegin(frm);
			TestCellSpacing(DataGrid1, GHTSubTest1, -1);
			TestCellSpacing(DataGrid2, GHTSubTest2, 0);
			TestCellSpacing(DataGrid3, GHTSubTest3, 2);
			TestCellSpacingError(DataGrid4, GHTSubTest4, -2);
			TestCellSpacing(DataList2, GHTSubTest6, 0);
			TestCellSpacing(DataList1, GHTSubTest5, -1);
			TestCellSpacing(DataList3, GHTSubTest7, 2);
			TestCellSpacingError(DataList4, GHTSubTest8, -2);
			GHTTestEnd();
		}
		private void TestCellSpacingError(BaseDataList ctl, GHTSubTest SubTest, int CellSpacing)
		{
			try
			{
				base.GHTActiveSubTest = SubTest;
				ctl.DataBind();
				ctl.DataSource = GHTDataListBase.GHTGetSampleDataSource();
				ctl.CellSpacing = CellSpacing;
				this.GHTSubTestExpectedExceptionNotCaught("ArgumentException");
			}
			catch (ArgumentException exception3)
			{
				this.GHTSubTestExpectedExceptionCaught(exception3);
			}
			catch (Exception exception4)
			{
				this.GHTSubTestUnexpectedExceptionCaught(exception4);
			}
		}
 
		private void TestCellSpacing(BaseDataList ctl, GHTSubTest SubTest, int CellSpacing)
		{
			try
			{
				base.GHTActiveSubTest = SubTest;
				ctl.DataSource = GHTDataListBase.GHTGetSampleDataSource();
				ctl.DataBind();
				ctl.CellSpacing = CellSpacing;
			}
			catch (Exception exception2)
			{
				this.GHTSubTestUnexpectedExceptionCaught(exception2);
			}
		}
 
	}
}
