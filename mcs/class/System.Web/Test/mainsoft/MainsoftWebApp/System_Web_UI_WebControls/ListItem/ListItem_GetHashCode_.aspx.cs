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
	public class ListItem_GetHashCode_
		: GHTBaseWeb 
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
			HtmlForm frm = (HtmlForm)FindControl("Form1");
			GHTTestBegin(frm);
      
			// Call twice with the same instance should provide the same result.
			this.GHTSubTestBegin("Two calls by the same object.");
			try
			{
				bool flag1;
				ListItem item1 = new ListItem("Text", "Value");
				if (item1.GetHashCode() == item1.GetHashCode())
				{
					flag1 = true;
				}
				else
				{
					flag1 = false;
				}
				this.GHTSubTestAddResult(flag1.ToString());
			}
			catch (Exception exception5)
			{
				this.GHTSubTestUnexpectedExceptionCaught(exception5);
			}
			this.GHTSubTestEnd();

			// Equal items (value equality, and not ref equality) should provide the same hashcode.
			this.GHTSubTestBegin("Equal items ");
			try
			{
				bool flag2;
				ListItem item2 = new ListItem("Text", "Value");
				ListItem item3 = new ListItem("Text", "Value");
				if (item2.GetHashCode() == item3.GetHashCode())
				{
					flag2 = true;
				}
				else
				{
					flag2 = false;
				}
				this.GHTSubTestAddResult(flag2.ToString());
			}
			catch (Exception exception6)
			{
				this.GHTSubTestUnexpectedExceptionCaught(exception6);
			}
			this.GHTSubTestEnd();

			// Not equal items (different text) should provide the diffrent hashcode.
			this.GHTSubTestBegin("Not equal items ");
			try
			{
				bool flag3;
				ListItem item4 = new ListItem("Text1", "Value");
				ListItem item5 = new ListItem("Text2", "Value");
				if (item4.GetHashCode() == item5.GetHashCode())
				{
					flag3 = true;
				}
				else
				{
					flag3 = false;
				}
				this.GHTSubTestAddResult(flag3.ToString());
			}
			catch (Exception exception7)
			{
				// ProjectData.SetProjectError(exception7);
				Exception exception3 = exception7;
				this.GHTSubTestUnexpectedExceptionCaught(exception3);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();

			// Not equal items (different value) should provide the diffrent hashcode.
			this.GHTSubTestBegin("Not equal items ");
			try
			{
				bool flag4;
				ListItem item6 = new ListItem("Text", "Value1");
				ListItem item7 = new ListItem("Text", "Value2");
				if (item6.GetHashCode() == item7.GetHashCode())
				{
					flag4 = true;
				}
				else
				{
					flag4 = false;
				}
				this.GHTSubTestAddResult(flag4.ToString());
			}
			catch (Exception exception8)
			{
				// ProjectData.SetProjectError(exception8);
				Exception exception4 = exception8;
				this.GHTSubTestUnexpectedExceptionCaught(exception4);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTTestEnd();

		}
	}
}
