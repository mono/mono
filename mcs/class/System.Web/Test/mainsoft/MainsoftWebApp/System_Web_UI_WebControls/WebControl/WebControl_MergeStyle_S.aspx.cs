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
    public class WebControl_MergeStyle_S
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

#region "Data members"
        Style [] m_styles = new Style[3];
		enum EStyledescription 
		{
			blank = 0,
			nonBlankOld,
			nonBlankNew
		}
#endregion

        private void Page_Load(object sender, System.EventArgs e) {
            HtmlForm frm  = (HtmlForm)FindControl("Form1");
            GHTTestBegin(frm);
			try
			{
				this.InitStyles();
				Type[] typeArray1 = this.TypesToTest;
				for (int num1 = 0; num1 < typeArray1.Length; num1++)
				{
					Type type1 = typeArray1[num1];
					this.GHTHeader(type1.ToString());
					this.Test(type1);
				}
				this.GHTTestEnd();
			}
			catch (Exception exception2)
			{
				// ProjectData.SetProjectError(exception2);
				Exception exception1 = exception2;
				this.GHTSubTestBegin();
				this.GHTSubTestAddResult("Failed to initialize styles needed for the test. Caught " + exception1.GetType().ToString() + " - " + exception1.Message);
				this.GHTSubTestEnd();
				// ProjectData.ClearProjectError();
				return;
			}
			finally
			{
				this.GHTTestEnd();
			}
		}

		private void Test(Type ctrlType)
		{
			try
			{
				this.GHTSubTestBegin(ctrlType, "Empty style + Empty style:");
				this.TestedControl.ApplyStyle(this.m_styles[0]);
				this.TestedControl.MergeStyle(this.m_styles[0]);
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
				this.GHTSubTestBegin(ctrlType, "Empty style + Non-Empty style:");
				this.TestedControl.ApplyStyle(this.m_styles[0]);
				this.TestedControl.MergeStyle(this.m_styles[2]);
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
				this.GHTSubTestBegin(ctrlType, "Non-Empty style + Empty style:");
				this.TestedControl.ApplyStyle(this.m_styles[1]);
				this.TestedControl.MergeStyle(this.m_styles[0]);
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
				this.GHTSubTestBegin(ctrlType, "Non-Empty old style + Non-Empty new style:");
				this.TestedControl.ApplyStyle(this.m_styles[1]);
				this.TestedControl.MergeStyle(this.m_styles[2]);
			}
			catch (Exception exception8)
			{
				// ProjectData.SetProjectError(exception8);
				Exception exception4 = exception8;
				this.GHTSubTestUnexpectedExceptionCaught(exception4);
				// ProjectData.ClearProjectError();
				return;
			}
		}
 


		private void InitStyles()
		{
			this.m_styles[0] = new Style();
			this.m_styles[1] = new Style();
			Style style2 = this.m_styles[1];
			style2.BackColor = Color.AliceBlue;
			style2.BorderColor = Color.AliceBlue;
			style2.BorderStyle = BorderStyle.Dashed;
			Unit unit1 = new Unit(1);
			style2.BorderWidth = unit1;
			style2.ForeColor = Color.AliceBlue;
			unit1 = new Unit(40);
			style2.Height = unit1;
			unit1 = new Unit(40);
			style2.Width = unit1;
			FontInfo info2 = style2.Font;
			info2.Bold = false;
			info2.Italic = false;
			info2.Name = "Arial";
			string[] textArray1 = new string[] { "Arial", "verdana" } ;
			info2.Names = textArray1;
			info2.Overline = false;
			info2.Size = FontUnit.Small;
			info2.Strikeout = false;
			info2.Underline = false;
			info2 = null;
			style2 = null;
			this.m_styles[2] = new Style();
			Style style1 = this.m_styles[2];
			style1.BackColor = Color.YellowGreen;
			style1.BorderColor = Color.YellowGreen;
			style1.BorderStyle = BorderStyle.Solid;
			unit1 = new Unit(10);
			style1.BorderWidth = unit1;
			style1.ForeColor = Color.YellowGreen;
			unit1 = new Unit(80);
			style1.Height = unit1;
			unit1 = new Unit(80);
			style1.Width = unit1;
			FontInfo info1 = style1.Font;
			info1.Bold = true;
			info1.Italic = true;
			info1.Name = "Courier";
			textArray1 = new string[] { "Courier", "David" } ;
			info1.Names = textArray1;
			info1.Overline = true;
			info1.Size = FontUnit.XXLarge;
			info1.Strikeout = true;
			info1.Underline = true;
			info1 = null;
			style1 = null;
		}
 

    }
}
