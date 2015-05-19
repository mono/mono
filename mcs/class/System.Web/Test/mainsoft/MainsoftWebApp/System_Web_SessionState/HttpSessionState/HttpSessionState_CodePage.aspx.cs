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

namespace GHTTests.System_Web_dll.System_Web_SessionState
{public class HttpSessionState_CodePage
    : GHTBaseWeb {
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
		 HtmlForm form1 = (HtmlForm) (HtmlForm)this.FindControl("Form1");
		 this.GHTTestBegin(form1);
		 this.GHTSubTestBegin("GHTSubTest1");
		 try
		 {
			 this.GHTSubTestAddResult(this.Session.CodePage.ToString());
		 }
		 catch (Exception exception6)
		 {
			 // ProjectData.SetProjectError(exception6);
			 Exception exception1 = exception6;
			 this.GHTSubTestUnexpectedExceptionCaught(exception1);
			 // ProjectData.ClearProjectError();
		 }
		 this.GHTSubTestEnd();
		 this.GHTSubTestBegin("GHTSubTest2");
		 try
		 {
			 this.Session.CodePage = 0x7b;
			 this.GHTSubTestAddResult(this.Session.CodePage.ToString());
			 this.GHTSubTestExpectedExceptionNotCaught("ArgumentException");
		 }
		 catch (NotSupportedException exception7)
		 {
			 // ProjectData.SetProjectError(exception7);
			 NotSupportedException exception2 = exception7;
			 this.GHTSubTestExpectedExceptionCaught(exception2);
			 // ProjectData.ClearProjectError();
		 }
		 catch (ArgumentException exception7)
		 {
			 // ProjectData.SetProjectError(exception7);
			 ArgumentException exception2 = exception7;
			 this.GHTSubTestExpectedExceptionCaught(exception2);
			 // ProjectData.ClearProjectError();
		 }

		 catch (Exception exception8)
		 {
			 // ProjectData.SetProjectError(exception8);
			 Exception exception3 = exception8;
			 this.GHTSubTestUnexpectedExceptionCaught(exception3);
			 // ProjectData.ClearProjectError();
		 }
		 this.GHTSubTestEnd();
		 this.GHTSubTestBegin("GHTSubTest4");
		 try
		 {
			 this.Session.CodePage = -125;
			 this.GHTSubTestAddResult(this.Session.CodePage.ToString());
			 this.GHTSubTestExpectedExceptionNotCaught("ArgumentException");
		 }
		 catch (ArgumentException exception9)
		 {
			 // ProjectData.SetProjectError(exception9);
			 ArgumentException exception4 = exception9;
			 this.GHTSubTestExpectedExceptionCaught(exception4);
			 // ProjectData.ClearProjectError();
		 }
		 catch (Exception exception10)
		 {
			 // ProjectData.SetProjectError(exception10);
			 Exception exception5 = exception10;
			 this.GHTSubTestUnexpectedExceptionCaught(exception5);
			 // ProjectData.ClearProjectError();
		 }
		 this.GHTSubTestEnd();
		 this.GHTTestEnd();
	 }
 
 }
}
