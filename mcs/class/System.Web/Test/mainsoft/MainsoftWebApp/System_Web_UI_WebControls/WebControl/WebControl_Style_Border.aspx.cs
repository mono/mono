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
using System.Drawing;

namespace GHTTests.System_Web_dll.System_Web_UI_WebControls
{
    public class WebControl_Style_Border
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
			Unit unit1;
			try
			{
				this.GHTSubTestBegin(ctrlType, "Normal Values.");
				this.TestedControl.ControlStyle.BorderStyle = BorderStyle.Ridge;
				this.TestedControl.ControlStyle.BorderColor = Color.Sienna;
				unit1 = new Unit(9);
				this.TestedControl.ControlStyle.BorderWidth = unit1;
			}
			catch (Exception exception10)
			{
				// ProjectData.SetProjectError(exception10);
				Exception exception1 = exception10;
				this.GHTSubTestUnexpectedExceptionCaught(exception1);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			try
			{
				this.GHTSubTestBegin(ctrlType, "BorderStyle = none.");
				this.TestedControl.ControlStyle.BorderStyle = BorderStyle.None;
				this.TestedControl.ControlStyle.BorderColor = Color.Turquoise;
				unit1 = new Unit(12);
				this.TestedControl.ControlStyle.BorderWidth = unit1;
			}
			catch (Exception exception11)
			{
				// ProjectData.SetProjectError(exception11);
				Exception exception2 = exception11;
				this.GHTSubTestUnexpectedExceptionCaught(exception2);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			try
			{
				this.GHTSubTestBegin(ctrlType, "BorderStyle = NotSet.");
				this.TestedControl.ControlStyle.BorderStyle = BorderStyle.NotSet;
				this.TestedControl.ControlStyle.BorderColor = Color.Fuchsia;
				unit1 = new Unit(20);
				this.TestedControl.ControlStyle.BorderWidth = unit1;
			}
			catch (Exception exception12)
			{
				// ProjectData.SetProjectError(exception12);
				Exception exception3 = exception12;
				this.GHTSubTestUnexpectedExceptionCaught(exception3);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			try
			{
				this.GHTSubTestBegin(ctrlType, "BorderColor = Empty.");
				this.TestedControl.ControlStyle.BorderStyle = BorderStyle.Inset;
				this.TestedControl.ControlStyle.BorderColor = Color.Empty;
				unit1 = new Unit(12);
				this.TestedControl.ControlStyle.BorderWidth = unit1;
			}
			catch (Exception exception13)
			{
				// ProjectData.SetProjectError(exception13);
				Exception exception4 = exception13;
				this.GHTSubTestUnexpectedExceptionCaught(exception4);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			try
			{
				this.GHTSubTestBegin(ctrlType, "BorderColor = Transparent.");
				this.TestedControl.ControlStyle.BorderStyle = BorderStyle.Groove;
				this.TestedControl.ControlStyle.BorderColor = Color.Transparent;
				unit1 = new Unit(12);
				this.TestedControl.ControlStyle.BorderWidth = unit1;
			}
			catch (Exception exception14)
			{
				// ProjectData.SetProjectError(exception14);
				Exception exception5 = exception14;
				this.GHTSubTestUnexpectedExceptionCaught(exception5);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			try
			{
				this.GHTSubTestBegin(ctrlType, "BorderWidth = 0.");
				this.TestedControl.ControlStyle.BorderStyle = BorderStyle.Double;
				this.TestedControl.ControlStyle.BorderColor = Color.Tomato;
				unit1 = new Unit(0);
				this.TestedControl.ControlStyle.BorderWidth = unit1;
			}
			catch (Exception exception15)
			{
				// ProjectData.SetProjectError(exception15);
				Exception exception6 = exception15;
				this.GHTSubTestUnexpectedExceptionCaught(exception6);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			try
			{
				this.GHTSubTestBegin(ctrlType, "Set BorderWidth and CssClass.");
				this.TestedControl.ControlStyle.CssClass = "CssClass1";
				unit1 = new Unit(20);
				this.TestedControl.ControlStyle.BorderWidth = unit1;
			}
			catch (Exception exception16)
			{
				// ProjectData.SetProjectError(exception16);
				Exception exception7 = exception16;
				this.GHTSubTestUnexpectedExceptionCaught(exception7);
				// ProjectData.ClearProjectError();
			}
			try
			{
				this.GHTSubTestBegin(ctrlType, "Set BorderStyle and CssClass.");
				this.TestedControl.ControlStyle.CssClass = "CssClass1";
				this.TestedControl.ControlStyle.BorderStyle = BorderStyle.Dotted;
			}
			catch (Exception exception17)
			{
				// ProjectData.SetProjectError(exception17);
				Exception exception8 = exception17;
				this.GHTSubTestUnexpectedExceptionCaught(exception8);
				// ProjectData.ClearProjectError();
			}
			try
			{
				this.GHTSubTestBegin(ctrlType, "Set BorderColor and CssClass.");
				this.TestedControl.ControlStyle.CssClass = "CssClass1";
				this.TestedControl.ControlStyle.BorderColor = Color.Silver;
			}
			catch (Exception exception18)
			{
				// ProjectData.SetProjectError(exception18);
				Exception exception9 = exception18;
				this.GHTSubTestUnexpectedExceptionCaught(exception9);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
		}
 
	}
}
