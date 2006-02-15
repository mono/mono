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
using System.Data;
using System.Collections;

namespace GHTTests.System_Web_dll.System_Web_UI_WebControls
{public class ButtonColumn_ButtonType
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
			 this.DataGrid1.DataSource = GHTTests.GHDataSources.DSDataTable();
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
			 this.GHTSubTestAddResult(column2.ButtonType.ToString());
			 this.GHTSubTestAddResult(column3.ButtonType.ToString());
			 this.GHTSubTestAddResult(column1.ButtonType.ToString());
		 }
		 catch (Exception exception7)
		 {
			 // ProjectData.SetProjectError(exception7);
			 Exception exception1 = exception7;
			 this.GHTSubTestUnexpectedExceptionCaught(exception1);
			 // ProjectData.ClearProjectError();
		 }
		 base.GHTActiveSubTest = this.Ghtsubtest2;
		 try
		 {
			 this.DataGrid2.DataSource = GHTTests.GHDataSources.DSDataTable();
			 ButtonColumn column5 = new ButtonColumn();
			 ButtonColumn column6 = new ButtonColumn();
			 ButtonColumn column4 = new ButtonColumn();
			 column5.DataTextField = "ID";
			 column6.DataTextField = "Name";
			 column4.DataTextField = "Company";
			 column5.ButtonType = ButtonColumnType.LinkButton;
			 column6.ButtonType = ButtonColumnType.LinkButton;
			 column4.ButtonType = ButtonColumnType.LinkButton;
			 this.DataGrid2.Columns.Add(column5);
			 this.DataGrid2.Columns.Add(column6);
			 this.DataGrid2.Columns.Add(column4);
			 this.DataGrid2.DataBind();
			 this.GHTSubTestAddResult(column5.ButtonType.ToString());
			 this.GHTSubTestAddResult(column6.ButtonType.ToString());
			 this.GHTSubTestAddResult(column4.ButtonType.ToString());
		 }
		 catch (Exception exception8)
		 {
			 // ProjectData.SetProjectError(exception8);
			 Exception exception2 = exception8;
			 this.GHTSubTestUnexpectedExceptionCaught(exception2);
			 // ProjectData.ClearProjectError();
		 }
		 base.GHTActiveSubTest = this.Ghtsubtest3;
		 try
		 {
			 this.DataGrid3.DataSource = GHTTests.GHDataSources.DSDataTable();
			 ButtonColumn column8 = new ButtonColumn();
			 ButtonColumn column9 = new ButtonColumn();
			 ButtonColumn column7 = new ButtonColumn();
			 column8.DataTextField = "ID";
			 column9.DataTextField = "Name";
			 column7.DataTextField = "Company";
			 column8.ButtonType = ButtonColumnType.PushButton;
			 column9.ButtonType = ButtonColumnType.PushButton;
			 column7.ButtonType = ButtonColumnType.PushButton;
			 this.DataGrid3.Columns.Add(column8);
			 this.DataGrid3.Columns.Add(column9);
			 this.DataGrid3.Columns.Add(column7);
			 this.DataGrid3.DataBind();
			 this.GHTSubTestAddResult(column8.ButtonType.ToString());
			 this.GHTSubTestAddResult(column9.ButtonType.ToString());
			 this.GHTSubTestAddResult(column7.ButtonType.ToString());
		 }
		 catch (Exception exception9)
		 {
			 // ProjectData.SetProjectError(exception9);
			 Exception exception3 = exception9;
			 this.GHTSubTestUnexpectedExceptionCaught(exception3);
			 // ProjectData.ClearProjectError();
		 }
		 base.GHTActiveSubTest = this.Ghtsubtest4;
		 try
		 {
			 this.DataGrid4.DataSource = GHTTests.GHDataSources.DSDataTable();
			 this.DataGrid4.DataBind();
			 this.GHTSubTestAddResult(((ButtonColumn) this.DataGrid4.Columns[0]).ButtonType.ToString());
			 this.GHTSubTestAddResult(((ButtonColumn) this.DataGrid4.Columns[1]).ButtonType.ToString());
			 this.GHTSubTestAddResult(((ButtonColumn) this.DataGrid4.Columns[2]).ButtonType.ToString());
		 }
		 catch (Exception exception10)
		 {
			 // ProjectData.SetProjectError(exception10);
			 Exception exception4 = exception10;
			 this.GHTSubTestUnexpectedExceptionCaught(exception4);
			 // ProjectData.ClearProjectError();
		 }
		 base.GHTActiveSubTest = this.Ghtsubtest5;
		 try
		 {
			 this.DataGrid5.DataSource = GHTTests.GHDataSources.DSDataTable();
			 this.DataGrid5.DataBind();
			 this.GHTSubTestAddResult(((ButtonColumn) this.DataGrid5.Columns[0]).ButtonType.ToString());
			 this.GHTSubTestAddResult(((ButtonColumn) this.DataGrid5.Columns[1]).ButtonType.ToString());
			 this.GHTSubTestAddResult(((ButtonColumn) this.DataGrid5.Columns[2]).ButtonType.ToString());
		 }
		 catch (Exception exception11)
		 {
			 // ProjectData.SetProjectError(exception11);
			 Exception exception5 = exception11;
			 this.GHTSubTestUnexpectedExceptionCaught(exception5);
			 // ProjectData.ClearProjectError();
		 }
		 base.GHTActiveSubTest = this.Ghtsubtest6;
		 try
		 {
			 this.DataGrid6.DataSource = GHTTests.GHDataSources.DSDataTable();
			 this.DataGrid6.DataBind();
			 this.GHTSubTestAddResult(((ButtonColumn) this.DataGrid6.Columns[0]).ButtonType.ToString());
			 this.GHTSubTestAddResult(((ButtonColumn) this.DataGrid6.Columns[1]).ButtonType.ToString());
			 this.GHTSubTestAddResult(((ButtonColumn) this.DataGrid6.Columns[2]).ButtonType.ToString());
		 }
		 catch (Exception exception12)
		 {
			 // ProjectData.SetProjectError(exception12);
			 Exception exception6 = exception12;
			 this.GHTSubTestUnexpectedExceptionCaught(exception6);
			 // ProjectData.ClearProjectError();
		 }
		 this.GHTTestEnd();
	 }
 
 }
}
