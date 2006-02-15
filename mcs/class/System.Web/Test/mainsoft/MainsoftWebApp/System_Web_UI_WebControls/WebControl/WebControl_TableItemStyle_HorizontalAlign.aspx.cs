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

namespace GHTTests.System_Web_dll.System_Web_UI_WebControls
{
    public class WebControl_TableItemStyle_HorizontalAlign
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
        //Overrides GHTWebControlBase.InitTypes
        //This method initializes the types that will be contained in the TypesToTest array.
        //This only controls that are relevant for TableItemStyle will be tested.
		protected override void InitTypes()
		{
			base.m_derivedTypes = new ArrayList();
			base.m_derivedTypes.Add(typeof(TableRow));
			base.m_derivedTypes.Add(typeof(TableCell));
			base.m_derivedTypes.Add(typeof(DataListItem));
		}
 

		private void Page_Load(object sender, System.EventArgs e) 
		{
			HtmlForm form1 = (HtmlForm) (HtmlForm)this.FindControl("Form1");
			this.GHTTestBegin(form1);
			Type[] typeArray1 = this.TypesToTest;
			for (int num1 = 0; num1 < typeArray1.Length; num1++)
			{
				Type type1 = typeArray1[num1];
				this.GHTHeader(type1.ToString());
				this.Test(type1);
			}
			this.GHTTestEnd();
		}

		private void Test(Type ctrlType)
		{
			TableItemStyle style1;
			try
			{
				this.GHTSubTestBegin(ctrlType, "Test default value.");
				style1 = (TableItemStyle) this.TestedControl.ControlStyle;
				this.GHTSubTestAddResult(((int) style1.HorizontalAlign).ToString());
			}
			catch (Exception exception9)
			{
				this.GHTSubTestUnexpectedExceptionCaught(exception9);
			}
			this.GHTSubTestEnd();
			try
			{
				this.GHTSubTestBegin(ctrlType, "Set 'Center'.");
				style1 = (TableItemStyle) this.TestedControl.ControlStyle;
				style1.HorizontalAlign = HorizontalAlign.Center;
			}
			catch (Exception exception10)
			{
				this.GHTSubTestUnexpectedExceptionCaught(exception10);
			}
			this.GHTSubTestEnd();
			try
			{
				this.GHTSubTestBegin(ctrlType, "Set 'Justify'.");
				style1 = (TableItemStyle) this.TestedControl.ControlStyle;
				style1.HorizontalAlign = HorizontalAlign.Justify;
			}
			catch (Exception exception11)
			{
				// ProjectData.SetProjectError(exception11);
				Exception exception3 = exception11;
				this.GHTSubTestUnexpectedExceptionCaught(exception3);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			try
			{
				this.GHTSubTestBegin(ctrlType, "Set 'Left'.");
				style1 = (TableItemStyle) this.TestedControl.ControlStyle;
				style1.HorizontalAlign = HorizontalAlign.Left;
			}
			catch (Exception exception12)
			{
				// ProjectData.SetProjectError(exception12);
				Exception exception4 = exception12;
				this.GHTSubTestUnexpectedExceptionCaught(exception4);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			try
			{
				this.GHTSubTestBegin(ctrlType, "Set 'NotSet'.");
				style1 = (TableItemStyle) this.TestedControl.ControlStyle;
				style1.HorizontalAlign = HorizontalAlign.NotSet;
			}
			catch (Exception exception13)
			{
				// ProjectData.SetProjectError(exception13);
				Exception exception5 = exception13;
				this.GHTSubTestUnexpectedExceptionCaught(exception5);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			try
			{
				this.GHTSubTestBegin(ctrlType, "Set 'Right'.");
				style1 = (TableItemStyle) this.TestedControl.ControlStyle;
				style1.HorizontalAlign = HorizontalAlign.Right;
			}
			catch (Exception exception14)
			{
				// ProjectData.SetProjectError(exception14);
				Exception exception6 = exception14;
				this.GHTSubTestUnexpectedExceptionCaught(exception6);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			try
			{
				this.GHTSubTestBegin(ctrlType, "Throws exception for illegal HorizontalAlign values.");
				style1 = (TableItemStyle) this.TestedControl.ControlStyle;
				style1.HorizontalAlign = (HorizontalAlign) 13;
				this.GHTSubTestExpectedExceptionNotCaught("ArgumentException");
			}
			catch (ArgumentException exception15)
			{
				// ProjectData.SetProjectError(exception15);
				ArgumentException exception7 = exception15;
				this.GHTSubTestAddResult("Test passed. Expected ArgumentException exception was caught.");
				// ProjectData.ClearProjectError();
			}
			catch (Exception exception16)
			{
				// ProjectData.SetProjectError(exception16);
				Exception exception8 = exception16;
				this.GHTSubTestUnexpectedExceptionCaught(exception8);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
		}
 
	}
}
