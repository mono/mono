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
{public class ButtonColumn_DataTextField
    : GHTBaseWeb {
 	protected System.Web.UI.WebControls.DataGrid DataGrid1;
 	protected System.Web.UI.WebControls.DataGrid DataGrid2;
 	protected GHTWebControls.GHTSubTest Ghtsubtest2;
 	protected System.Web.UI.WebControls.DataGrid DataGrid3;
 	protected GHTWebControls.GHTSubTest Ghtsubtest3;
 	protected System.Web.UI.WebControls.DataGrid DataGrid4;
 	protected GHTWebControls.GHTSubTest Ghtsubtest4;
 	protected System.Web.UI.WebControls.DataGrid DataGrid5;
 	protected GHTWebControls.GHTSubTest Ghtsubtest5;
 	protected System.Web.UI.WebControls.DataGrid DataGrid6;
 	protected GHTWebControls.GHTSubTest Ghtsubtest6;
 	protected GHTWebControls.GHTSubTest GHTSubTest1;
	 #region Web Form Designer generated code
	 override protected void OnInit(EventArgs e) {
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
	 private void InitializeComponent() {    
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
			 this.DataGrid1.Columns.Add(column2);
			 this.DataGrid1.Columns.Add(column3);
			 this.DataGrid1.Columns.Add(column1);
			 this.DataGrid1.DataBind();
			 this.GHTSubTestAddResult(column2.DataTextField);
			 this.GHTSubTestAddResult(column3.DataTextField);
			 this.GHTSubTestAddResult(column1.DataTextField);
		 }
		 catch (Exception exception9)
		 {
			 // ProjectData.SetProjectError(exception9);
			 Exception exception1 = exception9;
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
			 column4.DataTextField = "";
			 column5.DataTextField = "Name";
			 column6.DataTextField = "Name";
			 this.DataGrid2.Columns.Add(column4);
			 this.DataGrid2.Columns.Add(column5);
			 this.DataGrid2.Columns.Add(column6);
			 this.DataGrid2.DataBind();
			 this.GHTSubTestAddResult(column4.DataTextField);
			 this.GHTSubTestAddResult(column5.DataTextField);
			 this.GHTSubTestAddResult(column6.DataTextField);
		 }
		 catch (Exception exception10)
		 {
			 // ProjectData.SetProjectError(exception10);
			 Exception exception2 = exception10;
			 this.GHTSubTestUnexpectedExceptionCaught(exception2);
			 // ProjectData.ClearProjectError();
		 }
		 base.GHTActiveSubTest = this.Ghtsubtest3;
		 try
		 {
			 this.DataGrid3.DataSource = GHDataSources.DSDataTable();
			 ButtonColumn column7 = new ButtonColumn();
			 column7.DataTextField = "NotExist";
			 this.DataGrid3.Columns.Add(column7);
			 this.DataGrid3.DataBind();
			 this.GHTSubTestAddResult(column7.DataTextField);
			 this.GHTSubTestExpectedExceptionNotCaught("HttpException");
		 }
		 catch (HttpException exception11)
		 {
			 // ProjectData.SetProjectError(exception11);
			 HttpException exception3 = exception11;
			 this.GHTSubTestExpectedExceptionCaught(exception3);
			 // ProjectData.ClearProjectError();
		 }
		 catch (Exception exception12)
		 {
			 // ProjectData.SetProjectError(exception12);
			 Exception exception4 = exception12;
			 this.GHTSubTestUnexpectedExceptionCaught(exception4);
			 // ProjectData.ClearProjectError();
		 }
		 base.GHTActiveSubTest = this.Ghtsubtest4;
		 try
		 {
			 this.DataGrid4.DataSource = GHDataSources.DSDataTable();
			 this.DataGrid4.DataBind();
			 this.GHTSubTestAddResult(((ButtonColumn) this.DataGrid4.Columns[0]).DataTextField);
			 this.GHTSubTestAddResult(((ButtonColumn) this.DataGrid4.Columns[1]).DataTextField);
			 this.GHTSubTestAddResult(((ButtonColumn) this.DataGrid4.Columns[2]).DataTextField);
		 }
		 catch (Exception exception13)
		 {
			 // ProjectData.SetProjectError(exception13);
			 Exception exception5 = exception13;
			 this.GHTSubTestUnexpectedExceptionCaught(exception5);
			 // ProjectData.ClearProjectError();
		 }
		 base.GHTActiveSubTest = this.Ghtsubtest5;
		 try
		 {
			 this.DataGrid5.DataSource = GHDataSources.DSDataTable();
			 this.DataGrid5.DataBind();
			 this.GHTSubTestAddResult(((ButtonColumn) this.DataGrid5.Columns[0]).DataTextField);
			 this.GHTSubTestAddResult(((ButtonColumn) this.DataGrid5.Columns[1]).DataTextField);
			 this.GHTSubTestAddResult(((ButtonColumn) this.DataGrid5.Columns[2]).DataTextField);
		 }
		 catch (Exception exception14)
		 {
			 // ProjectData.SetProjectError(exception14);
			 Exception exception6 = exception14;
			 this.GHTSubTestUnexpectedExceptionCaught(exception6);
			 // ProjectData.ClearProjectError();
		 }
		 base.GHTActiveSubTest = this.Ghtsubtest6;
		 try
		 {
			 this.DataGrid6.DataSource = GHDataSources.DSDataTable();
			 this.DataGrid6.DataBind();
			 this.GHTSubTestAddResult(((ButtonColumn) this.DataGrid6.Columns[0]).DataTextField);
			 this.GHTSubTestExpectedExceptionNotCaught("HttpException");
		 }
		 catch (HttpException exception15)
		 {
			 // ProjectData.SetProjectError(exception15);
			 HttpException exception7 = exception15;
			 this.GHTSubTestExpectedExceptionCaught(exception7);
			 // ProjectData.ClearProjectError();
		 }
		 catch (Exception exception16)
		 {
			 // ProjectData.SetProjectError(exception16);
			 Exception exception8 = exception16;
			 this.GHTSubTestUnexpectedExceptionCaught(exception8);
			 // ProjectData.ClearProjectError();
		 }
		 this.GHTTestEnd();
	 }
 
 }
}
