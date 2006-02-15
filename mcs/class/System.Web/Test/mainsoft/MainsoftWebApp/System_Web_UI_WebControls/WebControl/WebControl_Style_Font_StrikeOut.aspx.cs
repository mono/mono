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
{public class WebControl_Style_Font_StrikeOut
        : GHTWebControlBase
 {
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

        private void Page_Load(object sender, System.EventArgs e) {
            HtmlForm frm  = (HtmlForm)FindControl("Form1");
            GHTTestBegin(frm);
			foreach (Type currentType in TypesToTest)
			{
				GHTHeader(currentType.ToString());
				Test(currentType);
			}
			GHTTestEnd();
        }

	 private void Test(Type ctrlType)
	 {
		 try
		 {
			 this.GHTSubTestBegin(ctrlType, "StrikOut = true.");
			 this.TestedControl.ControlStyle.Font.Strikeout = true;
		 }
		 catch (Exception exception5)
		 {
			 // ProjectData.SetProjectError(exception5);
			 Exception exception1 = exception5;
			 this.GHTSubTestUnexpectedExceptionCaught(exception1);
			 // ProjectData.ClearProjectError();
		 }
		 try
		 {
			 this.GHTSubTestBegin(ctrlType, "StrikeOut = False.");
			 this.TestedControl.ControlStyle.Font.Strikeout = false;
		 }
		 catch (Exception exception6)
		 {
			 // ProjectData.SetProjectError(exception6);
			 Exception exception2 = exception6;
			 this.GHTSubTestUnexpectedExceptionCaught(exception2);
			 // ProjectData.ClearProjectError();
		 }
		 try
		 {
			 this.GHTSubTestBegin(ctrlType, "Set twice.");
			 this.TestedControl.ControlStyle.Font.Strikeout = true;
			 this.TestedControl.ControlStyle.Font.Strikeout = false;
		 }
		 catch (Exception exception7)
		 {
			 // ProjectData.SetProjectError(exception7);
			 Exception exception3 = exception7;
			 this.GHTSubTestUnexpectedExceptionCaught(exception3);
			 // ProjectData.ClearProjectError();
		 }
		 try
		 {
			 this.GHTSubTestBegin(ctrlType, "Set StrikeOut + CssClass.");
			 this.TestedControl.ControlStyle.CssClass = "CssClass1";
			 this.TestedControl.ControlStyle.Font.Strikeout = false;
		 }
		 catch (Exception exception8)
		 {
			 // ProjectData.SetProjectError(exception8);
			 Exception exception4 = exception8;
			 this.GHTSubTestUnexpectedExceptionCaught(exception4);
			 // ProjectData.ClearProjectError();
		 }
		 this.GHTSubTestEnd();
	 }
 
 }
}
