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
using System.Drawing;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace GHTTests.System_Web_dll.System_Web_UI_WebControls
{
	public class WebControl_ApplyStyle_S
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

		#region "Data members"
		private Style m_newStyle;
		private Style m_oldStyle;
		private Style m_blankStyle;
		#endregion

		private void Page_Load(object sender, System.EventArgs e) 
		{
			HtmlForm frm  = (HtmlForm)FindControl("Form1");
			try 
			{
				InitStyles();
			}
			catch (Exception ex) 
			{
				GHTSubTestBegin("Styles initialization");
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

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
				this.GHTSubTestBegin(ctrlType, "Apply a new style:");
				this.TestedControl.ApplyStyle(this.m_newStyle);
			}
			catch (Exception exception3)
			{
				// ProjectData.SetProjectError(exception3);
				Exception exception1 = exception3;
				this.GHTSubTestUnexpectedExceptionCaught(exception1);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			try
			{
				this.GHTSubTestBegin(ctrlType, "Apply a blank style:");
				this.TestedControl.ApplyStyle(this.m_oldStyle);
				this.TestedControl.ApplyStyle(this.m_blankStyle);
			}
			catch (Exception exception4)
			{
				// ProjectData.SetProjectError(exception4);
				Exception exception2 = exception4;
				this.GHTSubTestUnexpectedExceptionCaught(exception2);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
		}
 

		private void InitStyles()
		{
			this.m_blankStyle = new Style();
			this.m_oldStyle = new Style();
			Style style2 = this.m_oldStyle;
			style2.BackColor = Color.Wheat;
			style2.BorderColor = Color.DarkGreen;
			style2.BorderStyle = BorderStyle.Double;
			Unit unit2 = new Unit(5);
			style2.BorderWidth = unit2;
			style2.ForeColor = Color.DarkMagenta;
			unit2 = new Unit(60);
			style2.Height = unit2;
			unit2 = new Unit(60);
			style2.Width = unit2;
			FontInfo info2 = style2.Font;
			info2.Bold = false;
			info2.Italic = false;
			info2.Name = "Times New Roman";
			info2.Overline = false;
			FontUnit unit1 = new FontUnit(FontSize.Small);
			info2.Size = unit1;
			info2.Strikeout = false;
			info2.Underline = false;
			info2 = null;
			style2 = null;
			this.m_newStyle = new Style();
			Style style1 = this.m_newStyle;
			style1.BackColor = Color.AliceBlue;
			style1.BorderColor = Color.Red;
			style1.BorderStyle = BorderStyle.Dashed;
			unit2 = new Unit(2);
			style1.BorderWidth = unit2;
			style1.ForeColor = Color.RoyalBlue;
			unit2 = new Unit(40);
			style1.Height = unit2;
			unit2 = new Unit(40);
			style1.Width = unit2;
			FontInfo info1 = style1.Font;
			info1.Bold = true;
			info1.Italic = true;
			info1.Name = "Arial";
			info1.Overline = true;
			unit1 = new FontUnit(FontSize.Large);
			info1.Size = unit1;
			info1.Strikeout = true;
			info1.Underline = true;
			info1 = null;
			style1 = null;
		}
	}
}
