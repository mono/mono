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
    public class WebControl_Style_Font_Names
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
			string[] textArray2;
			try
			{
				this.GHTSubTestBegin(ctrlType, "Set legal value");
				textArray2 = new string[] { "David", "Times New Roman", "Courier New" } ;
				this.TestedControl.ControlStyle.Font.Names = textArray2;
			}
			catch (Exception exception8)
			{
				// ProjectData.SetProjectError(exception8);
				Exception exception1 = exception8;
				this.GHTSubTestUnexpectedExceptionCaught(exception1);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			try
			{
				this.GHTSubTestBegin(ctrlType, "Change Order");
				FontInfo info1 = this.TestedControl.ControlStyle.Font;
				textArray2 = new string[] { "David", "Times New Roman", "Courier New" } ;
				info1.Names = textArray2;
				string text1 = info1.Names[1];
				info1.Names[1] = info1.Names[0];
				info1.Names[0] = text1;
				info1 = null;
			}
			catch (Exception exception9)
			{
				// ProjectData.SetProjectError(exception9);
				Exception exception2 = exception9;
				this.GHTSubTestUnexpectedExceptionCaught(exception2);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			try
			{
				this.GHTSubTestBegin(ctrlType, "Setting Name changes Names.");
				textArray2 = new string[] { "David", "Times New Roman", "Courier New" } ;
				this.TestedControl.ControlStyle.Font.Names = textArray2;
				this.TestedControl.ControlStyle.Font.Name = "Arial";
				string text2 = string.Empty;
				string[] textArray1 = this.TestedControl.ControlStyle.Font.Names;
				for (int num1 = 0; num1 < textArray1.Length; num1++)
				{
					string text3 = textArray1[num1];
					text2 = text2 + text3;
				}
				this.Compare(text2, this.TestedControl.ControlStyle.Font.Name);
			}
			catch (Exception exception10)
			{
				// ProjectData.SetProjectError(exception10);
				Exception exception3 = exception10;
				this.GHTSubTestUnexpectedExceptionCaught(exception3);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			try
			{
				this.GHTSubTestBegin(ctrlType, "Set Names in code + Names in CssClass.");
				this.TestedControl.ControlStyle.CssClass = "NamesCssClass";
				textArray2 = new string[] { "Times New Roman", "Arial", "Courier New" } ;
				this.TestedControl.ControlStyle.Font.Names = textArray2;
			}
			catch (ArgumentException exception11)
			{
				// ProjectData.SetProjectError(exception11);
				ArgumentException exception4 = exception11;
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
			try
			{
				this.GHTSubTestBegin(ctrlType, "Set Names in code + Name in CssClass.");
				this.TestedControl.ControlStyle.CssClass = "NameCssClass";
				textArray2 = new string[] { "Times New Roman", "Arial", "Courier New" } ;
				this.TestedControl.ControlStyle.Font.Names = textArray2;
			}
			catch (ArgumentException exception13)
			{
				// ProjectData.SetProjectError(exception13);
				ArgumentException exception6 = exception13;
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
