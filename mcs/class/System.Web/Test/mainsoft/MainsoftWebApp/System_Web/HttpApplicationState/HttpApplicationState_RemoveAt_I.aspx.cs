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

namespace GHTTests.System_Web_dll.System_Web
{public class HttpApplicationState_RemoveAt_I
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
			 this.Application.Clear();
			 this.Application["var1"] = "variable1";
			 this.Application["var2"] = "variable2";
			 this.Application["var3"] = "variable3";
			 this.Application.RemoveAt(2);
			 int num8 = this.Application.Count - 1;
			 for (int num1 = 0; num1 <= num8; num1++)
			 {
				 this.GHTSubTestAddResult((string)("Application.Item(" + num1.ToString() + ") = " + this.Application[num1]));
			 }
		 }
		 catch (Exception exception8)
		 {
			 // ProjectData.SetProjectError(exception8);
			 Exception exception1 = exception8;
			 this.GHTSubTestUnexpectedExceptionCaught(exception1);
			 // ProjectData.ClearProjectError();
		 }
		 this.GHTSubTestEnd();
		 this.GHTSubTestBegin("GHTSubTest2");
		 try
		 {
			 this.Application.RemoveAt(2);
			 int num7 = this.Application.Count - 1;
			 for (int num2 = 0; num2 <= num7; num2++)
			 {
				 this.GHTSubTestAddResult((string)("Application.Item(" + num2.ToString() + ") = " + this.Application[num2]));
			 }
			 this.GHTSubTestExpectedExceptionNotCaught("ArgumentOutOfRangeException");
		 }
		 catch (ArgumentOutOfRangeException exception9)
		 {
			 // ProjectData.SetProjectError(exception9);
			 ArgumentOutOfRangeException exception2 = exception9;
			 this.GHTSubTestExpectedExceptionCaught(exception2);
			 // ProjectData.ClearProjectError();
		 }
		 catch (Exception exception10)
		 {
			 // ProjectData.SetProjectError(exception10);
			 Exception exception3 = exception10;
			 this.GHTSubTestUnexpectedExceptionCaught(exception3);
			 // ProjectData.ClearProjectError();
		 }
		 this.GHTSubTestEnd();
		 this.GHTSubTestBegin("GHTSubTest3");
		 try
		 {
			 this.Application.RemoveAt(0x7d);
			 int num6 = this.Application.Count - 1;
			 for (int num3 = 0; num3 <= num6; num3++)
			 {
				 this.GHTSubTestAddResult((string)("Application.Item(" + num3.ToString() + ") = " + this.Application[num3]));
			 }
			 this.GHTSubTestExpectedExceptionNotCaught("ArgumentOutOfRangeException");
		 }
		 catch (ArgumentOutOfRangeException exception11)
		 {
			 // ProjectData.SetProjectError(exception11);
			 ArgumentOutOfRangeException exception4 = exception11;
			 this.GHTSubTestExpectedExceptionCaught(exception4);
			 // ProjectData.ClearProjectError();
		 }
		 catch (Exception exception12)
		 {
			 // ProjectData.SetProjectError(exception12);
			 Exception exception5 = exception12;
			 this.GHTSubTestUnexpectedExceptionCaught(exception5);
			 // ProjectData.ClearProjectError();
		 }
		 this.GHTSubTestEnd();
		 this.GHTSubTestBegin("GHTSubTest4");
		 try
		 {
			 this.Application.RemoveAt(-125);
			 int num5 = this.Application.Count - 1;
			 for (int num4 = 0; num4 <= num5; num4++)
			 {
				 this.GHTSubTestAddResult((string)("Application.Item(" + num4.ToString() + ") = " + this.Application[num4].ToString()));
			 }
			 this.GHTSubTestExpectedExceptionNotCaught("ArgumentOutOfRangeException");
		 }
		 catch (ArgumentOutOfRangeException exception13)
		 {
			 // ProjectData.SetProjectError(exception13);
			 ArgumentOutOfRangeException exception6 = exception13;
			 this.GHTSubTestExpectedExceptionCaught(exception6);
			 // ProjectData.ClearProjectError();
		 }
		 catch (Exception exception14)
		 {
			 // ProjectData.SetProjectError(exception14);
			 Exception exception7 = exception14;
			 this.GHTSubTestUnexpectedExceptionCaught(exception7);
			 // ProjectData.ClearProjectError();
		 }
		 this.GHTSubTestEnd();
	 }
 
 }
}
