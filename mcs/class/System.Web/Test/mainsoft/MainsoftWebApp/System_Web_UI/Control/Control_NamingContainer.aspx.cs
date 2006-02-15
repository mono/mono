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

namespace GHTTests.System_Web_dll.System_Web_UI
{
	public class Control_NamingContainer
		: GHTControlBase
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
			this.ID = "Control_NamingContainer";
			System.Web.UI.HtmlControls.HtmlForm frm = (HtmlForm)FindControl("Form1");
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
				this.GHTSubTestBegin(ctrlType, "Default naming container");
				this.GHTSubTestAddResult(this.TestedControl.NamingContainer.ID);
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
				this.GHTSubTestBegin(ctrlType, "Custom naming container", false);
				TestNamingContainer container1 = new TestNamingContainer();
				container1.ID = ctrlType.ToString() + "custom_test_naming_container";
				container1.Controls.Add(this.TestedControl);
				this.HandleValidationControls(this.TestedControl);
				base.GHTActiveForm.Controls.Add(container1);
				this.GHTSubTestAddResult("NamingContainer.UniqueID: " + this.TestedControl.NamingContainer.UniqueID);
				this.GHTSubTestAddResult("NamingContainer.Type: " + this.TestedControl.NamingContainer.GetType().ToString());
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
 

		public override string ToString()
		{
			return "Control_NamingContainer";
		}

	}

	public class TestNamingContainer : System.Web.UI.Control, INamingContainer
	{
		public TestNamingContainer()
		{
			TextBox m_tbToValidate = new TextBox();
			m_tbToValidate.ID = "m_tbToValidate";
			this.Controls.Add(m_tbToValidate);
		}
	}
}
