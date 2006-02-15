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
    public class WebControl_Style_Font_Name
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
			string[] textArray1;
			try
			{
				this.GHTSubTestBegin(ctrlType, "Set a legal name.");
				this.TestedControl.ControlStyle.Font.Name = "Arial";
			}
			catch (Exception exception14)
			{
				// ProjectData.SetProjectError(exception14);
				Exception exception1 = exception14;
				this.GHTSubTestUnexpectedExceptionCaught(exception1);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			try
			{
				this.GHTSubTestBegin(ctrlType, "Set a legal name twice.");
				this.TestedControl.ControlStyle.Font.Name = "Arial";
				this.TestedControl.ControlStyle.Font.Name = "David";
			}
			catch (Exception exception15)
			{
				// ProjectData.SetProjectError(exception15);
				Exception exception2 = exception15;
				this.GHTSubTestUnexpectedExceptionCaught(exception2);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			try
			{
				this.GHTSubTestBegin(ctrlType, "Default is String.empty.");
				this.GHTSubTestAddResult(this.TestedControl.ControlStyle.Font.Name);
			}
			catch (Exception exception16)
			{
				// ProjectData.SetProjectError(exception16);
				Exception exception3 = exception16;
				this.GHTSubTestUnexpectedExceptionCaught(exception3);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			try
			{
				this.GHTSubTestBegin(ctrlType, "Null throws exception.");
				this.TestedControl.ControlStyle.Font.Name = null;
				this.GHTSubTestExpectedExceptionNotCaught("ArgumentException");
			}
			catch (ArgumentException exception17)
			{
				// ProjectData.SetProjectError(exception17);
				ArgumentException exception4 = exception17;
				this.GHTSubTestAddResult("Test passed. Expected ArgumentException exception was caught.");
				// ProjectData.ClearProjectError();
			}
			catch (Exception exception18)
			{
				// ProjectData.SetProjectError(exception18);
				Exception exception5 = exception18;
				this.GHTSubTestUnexpectedExceptionCaught(exception5);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			try
			{
				this.GHTSubTestBegin(ctrlType, "Set Name in code + Name in CssClass.");
				this.TestedControl.ControlStyle.CssClass = "NameCssClass";
				this.TestedControl.ControlStyle.Font.Name = "Times New Roman";
			}
			catch (ArgumentException exception19)
			{
				// ProjectData.SetProjectError(exception19);
				ArgumentException exception6 = exception19;
				this.GHTSubTestExpectedExceptionCaught(exception6);
				// ProjectData.ClearProjectError();
			}
			catch (Exception exception20)
			{
				// ProjectData.SetProjectError(exception20);
				Exception exception7 = exception20;
				this.GHTSubTestUnexpectedExceptionCaught(exception7);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			try
			{
				this.GHTSubTestBegin(ctrlType, "Set Name in code + Name in CssClass.");
				this.TestedControl.ControlStyle.CssClass = "NamesCssClass";
				this.TestedControl.ControlStyle.Font.Name = "Times New Roman";
			}
			catch (ArgumentException exception21)
			{
				// ProjectData.SetProjectError(exception21);
				ArgumentException exception8 = exception21;
				this.GHTSubTestExpectedExceptionCaught(exception8);
				// ProjectData.ClearProjectError();
			}
			catch (Exception exception22)
			{
				// ProjectData.SetProjectError(exception22);
				Exception exception9 = exception22;
				this.GHTSubTestUnexpectedExceptionCaught(exception9);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			try
			{
				this.GHTSubTestBegin(ctrlType, "Set Names, See that Name is changed accordingly.");
				this.TestedControl.ControlStyle.Font.Name = "Times New Roman";
				textArray1 = new string[] { "Aharoni", "Courier New", "David" } ;
				this.TestedControl.ControlStyle.Font.Names = textArray1;
				this.Compare(this.TestedControl.ControlStyle.Font.Name, this.TestedControl.ControlStyle.Font.Names[0]);
			}
			catch (ArgumentException exception23)
			{
				// ProjectData.SetProjectError(exception23);
				ArgumentException exception10 = exception23;
				this.GHTSubTestExpectedExceptionCaught(exception10);
				// ProjectData.ClearProjectError();
			}
			catch (Exception exception24)
			{
				// ProjectData.SetProjectError(exception24);
				Exception exception11 = exception24;
				this.GHTSubTestUnexpectedExceptionCaught(exception11);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			try
			{
				this.GHTSubTestBegin(ctrlType, "Set Names, See that Name is changed accordingly.");
				FontInfo info1 = this.TestedControl.ControlStyle.Font;
				info1.Name = "Times New Roman";
				textArray1 = new string[] { "Aharoni", "Courier New", "David" } ;
				info1.Names = textArray1;
				string text1 = info1.Names[0];
				info1.Names[0] = info1.Names[1];
				info1.Names[1] = text1;
				info1 = null;
				this.Compare(this.TestedControl.ControlStyle.Font.Name, this.TestedControl.ControlStyle.Font.Names[0]);
			}
			catch (ArgumentException exception25)
			{
				// ProjectData.SetProjectError(exception25);
				ArgumentException exception12 = exception25;
				this.GHTSubTestExpectedExceptionCaught(exception12);
				// ProjectData.ClearProjectError();
			}
			catch (Exception exception26)
			{
				// ProjectData.SetProjectError(exception26);
				Exception exception13 = exception26;
				this.GHTSubTestUnexpectedExceptionCaught(exception13);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
		}
 
	}
}
