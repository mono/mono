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
using System.Collections;

namespace GHTTests.System_Web_dll.System_Web
{public class HttpApplicationState_Item_S
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
			 IEnumerator enumerator4 = null;
			 this.Application.Clear();
			 this.Application["var1"] = "variable1";
			 this.Application["var2"] = "variable2";
			 this.Application["var3"] = "variable3";
			 try
			 {
				 enumerator4 = this.Application.GetEnumerator();
				 while (enumerator4.MoveNext())
				 {
					 string text1 = (string)(enumerator4.Current);
					 if (this.Application[text1] == null)
					 {
						 this.GHTSubTestAddResult(("Application(\"" + text1) + "\") = null");
						 continue;
					 }
					 this.GHTSubTestAddResult((string)(("Application(\"" + text1) + "\") = " + this.Application[text1]));
				 }
			 }
			 finally
			 {
				 if (enumerator4 is IDisposable)
				 {
					 ((IDisposable) enumerator4).Dispose();
				 }
			 }
		 }
		 catch (Exception exception5)
		 {
			 // ProjectData.SetProjectError(exception5);
			 Exception exception1 = exception5;
			 this.GHTSubTestUnexpectedExceptionCaught(exception1);
			 // ProjectData.ClearProjectError();
		 }
		 this.GHTSubTestEnd();
		 this.GHTSubTestBegin("GHTSubTest2");
		 try
		 {
			 IEnumerator enumerator3 = null;
			 this.Application["var1"] = "variable1";
			 this.Application["var2"] = "variable2";
			 this.Application["var3"] = "variable3";
			 try
			 {
				 enumerator3 = this.Application.GetEnumerator();
				 while (enumerator3.MoveNext())
				 {
					 string text2 = (string)(enumerator3.Current);
					 if (this.Application[text2] == null)
					 {
						 this.GHTSubTestAddResult(("Application(\"" + text2) + "\") = null");
						 continue;
					 }
					 this.GHTSubTestAddResult((string)(("Application(\"" + text2) + "\") = " + this.Application[text2]));
				 }
			 }
			 finally
			 {
				 if (enumerator3 is IDisposable)
				 {
					 ((IDisposable) enumerator3).Dispose();
				 }
			 }
		 }
		 catch (Exception exception6)
		 {
			 // ProjectData.SetProjectError(exception6);
			 Exception exception2 = exception6;
			 this.GHTSubTestUnexpectedExceptionCaught(exception2);
			 // ProjectData.ClearProjectError();
		 }
		 this.GHTSubTestEnd();
		 this.GHTSubTestBegin("GHTSubTest3");
		 try
		 {
			 IEnumerator enumerator2 = null;
			 this.Application[""] = "";
			 this.Application["Nothing"] = null;
			 try
			 {
				 enumerator2 = this.Application.GetEnumerator();
				 while (enumerator2.MoveNext())
				 {
					 string text3 = (string)(enumerator2.Current);
					 if (this.Application[text3] == null)
					 {
						 this.GHTSubTestAddResult(("Application(\"" + text3) + "\") = null");
						 continue;
					 }
					 this.GHTSubTestAddResult((string)(("Application(\"" + text3) + "\") = " + this.Application[text3]));
				 }
			 }
			 finally
			 {
				 if (enumerator2 is IDisposable)
				 {
					 ((IDisposable) enumerator2).Dispose();
				 }
			 }
		 }
		 catch (Exception exception7)
		 {
			 // ProjectData.SetProjectError(exception7);
			 Exception exception3 = exception7;
			 this.GHTSubTestUnexpectedExceptionCaught(exception3);
			 // ProjectData.ClearProjectError();
		 }
		 this.GHTSubTestEnd();
		 this.GHTSubTestBegin("GHTSubTest4");
		 try
		 {
			 IEnumerator enumerator1 = null;
			 try
			 {
				 enumerator1 = this.Application.GetEnumerator();
				 while (enumerator1.MoveNext())
				 {
					 string text4 = (string)(enumerator1.Current);
					 if (this.Application[text4 + "_" + text4] == null)
					 {
						 this.GHTSubTestAddResult(("Application(\"" + text4 + "_" + text4) + "\")  = null");
						 continue;
					 }
					 this.GHTSubTestAddResult((string)(((("Application(\"" + text4) + "_") + text4) + "\") = " + this.Application[text4 + "_" + text4]));
				 }
			 }
			 finally
			 {
				 if (enumerator1 is IDisposable)
				 {
					 ((IDisposable) enumerator1).Dispose();
				 }
			 }
		 }
		 catch (Exception exception8)
		 {
			 // ProjectData.SetProjectError(exception8);
			 Exception exception4 = exception8;
			 this.GHTSubTestUnexpectedExceptionCaught(exception4);
			 // ProjectData.ClearProjectError();
		 }
		 this.GHTSubTestEnd();
		 this.GHTTestEnd();
	 }
 
 }
}
