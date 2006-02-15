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
	public class WebControl_BorderStyle
		: GHTWebControlBase
	{
		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e) 
		{
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
		private void InitializeComponent() 
		{    
			this.Load += new System.EventHandler(this.Page_Load);
		}
		#endregion

		private void Page_Load(object sender, System.EventArgs e) 
		{
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
				this.GHTSubTestBegin(ctrlType, "Default:");
				this.TestedControl.BorderColor = Color.Black;
				unit1 = new Unit(2);
				this.TestedControl.BorderWidth = unit1;
				this.TestedControl.BorderStyle = BorderStyle.Dashed;
			}
			catch (Exception exception12)
			{
				// ProjectData.SetProjectError(exception12);
				Exception exception1 = exception12;
				this.GHTSubTestUnexpectedExceptionCaught(exception1);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			try
			{
				this.GHTSubTestBegin(ctrlType, "Dashed:");
				this.TestedControl.BorderColor = Color.Black;
				unit1 = new Unit(2);
				this.TestedControl.BorderWidth = unit1;
				this.TestedControl.BorderStyle = BorderStyle.Dashed;
			}
			catch (Exception exception13)
			{
				// ProjectData.SetProjectError(exception13);
				Exception exception2 = exception13;
				this.GHTSubTestUnexpectedExceptionCaught(exception2);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			try
			{
				this.GHTSubTestBegin(ctrlType, "Dotted:");
				this.TestedControl.BorderColor = Color.Black;
				unit1 = new Unit(2);
				this.TestedControl.BorderWidth = unit1;
				this.TestedControl.BorderStyle = BorderStyle.Dotted;
			}
			catch (Exception exception14)
			{
				// ProjectData.SetProjectError(exception14);
				Exception exception3 = exception14;
				this.GHTSubTestUnexpectedExceptionCaught(exception3);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			try
			{
				this.GHTSubTestBegin(ctrlType, "Double:");
				this.TestedControl.BorderColor = Color.Black;
				unit1 = new Unit(2);
				this.TestedControl.BorderWidth = unit1;
				this.TestedControl.BorderStyle = BorderStyle.Double;
			}
			catch (Exception exception15)
			{
				// ProjectData.SetProjectError(exception15);
				Exception exception4 = exception15;
				this.GHTSubTestUnexpectedExceptionCaught(exception4);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			try
			{
				this.GHTSubTestBegin(ctrlType, "Groove:");
				this.TestedControl.BorderColor = Color.Black;
				unit1 = new Unit(2);
				this.TestedControl.BorderWidth = unit1;
				this.TestedControl.BorderStyle = BorderStyle.Groove;
			}
			catch (Exception exception16)
			{
				// ProjectData.SetProjectError(exception16);
				Exception exception5 = exception16;
				this.GHTSubTestUnexpectedExceptionCaught(exception5);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			try
			{
				this.GHTSubTestBegin(ctrlType, "Inset:");
				this.TestedControl.BorderColor = Color.Black;
				unit1 = new Unit(2);
				this.TestedControl.BorderWidth = unit1;
				this.TestedControl.BorderStyle = BorderStyle.Inset;
			}
			catch (Exception exception17)
			{
				// ProjectData.SetProjectError(exception17);
				Exception exception6 = exception17;
				this.GHTSubTestUnexpectedExceptionCaught(exception6);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			try
			{
				this.GHTSubTestBegin(ctrlType, "None:");
				this.TestedControl.BorderColor = Color.Black;
				unit1 = new Unit(2);
				this.TestedControl.BorderWidth = unit1;
				this.TestedControl.BorderStyle = BorderStyle.None;
			}
			catch (Exception exception18)
			{
				// ProjectData.SetProjectError(exception18);
				Exception exception7 = exception18;
				this.GHTSubTestUnexpectedExceptionCaught(exception7);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			try
			{
				this.GHTSubTestBegin(ctrlType, "NotSet:");
				this.TestedControl.BorderColor = Color.Black;
				unit1 = new Unit(2);
				this.TestedControl.BorderWidth = unit1;
				this.TestedControl.BorderStyle = BorderStyle.NotSet;
			}
			catch (Exception exception19)
			{
				// ProjectData.SetProjectError(exception19);
				Exception exception8 = exception19;
				this.GHTSubTestUnexpectedExceptionCaught(exception8);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			try
			{
				this.GHTSubTestBegin(ctrlType, "Outset:");
				this.TestedControl.BorderColor = Color.Black;
				unit1 = new Unit(2);
				this.TestedControl.BorderWidth = unit1;
				this.TestedControl.BorderStyle = BorderStyle.Outset;
			}
			catch (Exception exception20)
			{
				// ProjectData.SetProjectError(exception20);
				Exception exception9 = exception20;
				this.GHTSubTestUnexpectedExceptionCaught(exception9);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			try
			{
				this.GHTSubTestBegin(ctrlType, "Ridge:");
				this.TestedControl.BorderColor = Color.Black;
				unit1 = new Unit(2);
				this.TestedControl.BorderWidth = unit1;
				this.TestedControl.BorderStyle = BorderStyle.Ridge;
			}
			catch (Exception exception21)
			{
				// ProjectData.SetProjectError(exception21);
				Exception exception10 = exception21;
				this.GHTSubTestUnexpectedExceptionCaught(exception10);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			try
			{
				this.GHTSubTestBegin(ctrlType, "Solid:");
				this.TestedControl.BorderColor = Color.Black;
				unit1 = new Unit(2);
				this.TestedControl.BorderWidth = unit1;
				this.TestedControl.BorderStyle = BorderStyle.Solid;
			}
			catch (Exception exception22)
			{
				// ProjectData.SetProjectError(exception22);
				Exception exception11 = exception22;
				this.GHTSubTestUnexpectedExceptionCaught(exception11);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
		}
 
	}
}
