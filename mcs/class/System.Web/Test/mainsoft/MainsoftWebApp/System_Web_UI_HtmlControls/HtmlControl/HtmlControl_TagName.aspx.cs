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

namespace GHTTests.System_Web_dll.System_Web_UI_HtmlControls
{public class HtmlControl_TagName
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

	 private void TestTagNameControl(Type HTMLCtlType, string TestName) 
	 { 
		 System.Web.UI.HtmlControls.HtmlControl ctl; 
		 GHTSubTestBegin(TestName); 
		 try 
		 { 
			 ctl = (HtmlControl)GHTElementClone(HTMLCtlType); 
			 GHTActiveSubTest.Controls.Add(ctl); 
			 GHTSubTestAddResult(ctl.TagName); 
		 } 
		 catch (Exception ex) 
		 { 
			 GHTSubTestUnexpectedExceptionCaught(ex); 
			 ctl = null; 
		 } 
		 GHTSubTestEnd(); 
	 }
	 
		 private void Page_Load(object sender, System.EventArgs e) 
	 {
            //Put user code to initialize the page here

            System.Web.UI.HtmlControls.HtmlForm frm = (HtmlForm)this.FindControl("Form1");
            GHTTestBegin(frm);

            TestTagNameControl(typeof(System.Web.UI.HtmlControls.HtmlInputButton), "HTMLInputButton_TagName");
            TestTagNameControl(typeof(System.Web.UI.HtmlControls.HtmlButton), "HTMLButton_TagName");
            TestTagNameControl(typeof(System.Web.UI.HtmlControls.HtmlAnchor), "HTMLAnchor_TagName");
            TestTagNameControl(typeof(System.Web.UI.HtmlControls.HtmlInputHidden), "HtmlInputHidden_TagName");
            TestTagNameControl(typeof(System.Web.UI.HtmlControls.HtmlInputImage), "HtmlInputImage_TagName");
            TestTagNameControl(typeof(System.Web.UI.HtmlControls.HtmlTextArea), "HtmlTextArea_TagName");
            TestTagNameControl(typeof(System.Web.UI.HtmlControls.HtmlGenericControl), "HtmlGenericControl_TagName");

            GHTTestEnd();
        }
    }
}
