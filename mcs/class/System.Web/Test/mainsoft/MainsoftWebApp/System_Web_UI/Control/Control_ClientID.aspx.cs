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
	public class Control_ClientID
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

			//ClientId - server generated.
			//Because the id is random, then it cannot be compared between two servers,
			//There for I only test that a value is automatically set, and does
			//not add the control to the subtest.
			try
			{
				this.GHTSubTestBegin(ctrlType, "ClientId - server generated", false);
				this.GHTAddToActiveForm(this.TestedControl);
				if (this.TestedControl.ClientID != string.Empty)
				{
					this.GHTSubTestAddResult("Passed, ClientID is set by the server.");
				}
				else
				{
					this.GHTSubTestAddResult("Failed, ClientID is not set by the server.");
				}
			}
			catch (Exception exception4)
			{
				// ProjectData.SetProjectError(exception4);
				Exception exception1 = exception4;
				this.GHTSubTestUnexpectedExceptionCaught(exception1);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();

			//ClientId - Id property set to String.empty
			//As in previous subtest, the ClientID is a random number, and cannot be compared,
			//There for I only test that the value is not string.empty, 
			//and does not add the control to the subtest.
			try
			{
				this.GHTSubTestBegin(ctrlType, "ClientId - Id property set to String.empty", false);
				this.GHTAddToActiveForm(this.TestedControl);
				this.TestedControl.ID = string.Empty;
				if (this.TestedControl.ClientID != string.Empty)
				{
					this.GHTSubTestAddResult("Passed, ClientID is set by the server.");
				}
				else
				{
					this.GHTSubTestAddResult("Failed, ClientID is not set by the server.");
				}
			}
			catch (Exception exception5)
			{
				// ProjectData.SetProjectError(exception5);
				Exception exception2 = exception5;
				this.GHTSubTestUnexpectedExceptionCaught(exception2);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();


			//ClientId - overriden by Id property:
			try
			{
				this.GHTSubTestBegin(ctrlType, "ClientId - overriden by Id property");
				this.TestedControl.ID = ctrlType.ToString() + "_Id";
			}
			catch (Exception exception6)
			{
				// ProjectData.SetProjectError(exception6);
				Exception exception3 = exception6;
				this.GHTSubTestUnexpectedExceptionCaught(exception3);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();

		}
	}
}
