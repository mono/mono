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
using System.Collections;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace GHTTests.System_Web_dll.System_Web_UI_HtmlControls
{
	public class HtmlControl_Attributes
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


		private void TestAttributeseControl(Type HTMLCtlType, string TestName)
		{

			System.Web.UI.HtmlControls.HtmlControl ctl;
			string attributesStr = "";

			GHTSubTestBegin(TestName);
			try 
			{
				ctl = (HtmlControl)GHTElementClone(HTMLCtlType);
				GHTActiveSubTest.Controls.Add(ctl);
				ctl.Attributes.Add("id", "ctl:" + TestName);
				ctl.Attributes.Add("notid", "ctl");
				ctl.Attributes.Add("no_value", "");
				ctl.Attributes.Add("space space", "sp a ce");

				IEnumerator keys = ctl.Attributes.Keys.GetEnumerator();
				while ( keys.MoveNext() )
				{
					string key = (string)keys.Current;
					if (attributesStr != "") attributesStr += ",";
					attributesStr += key + "=" + ctl.Attributes[key];
				}

				GHTSubTestAddResult(attributesStr);
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
				ctl = null;
			}
			GHTSubTestEnd();
		}

		private void Page_Load(object sender, System.EventArgs e) 
		{
			try 
			{
				//Put user code to initialize the page here
				System.Web.UI.HtmlControls.HtmlForm frm = (HtmlForm)this.FindControl("Form1");
				GHTTestBegin(frm);

				TestAttributeseControl(typeof(System.Web.UI.HtmlControls.HtmlInputButton), "HTMLInputButton_Attributes");
				TestAttributeseControl(typeof(System.Web.UI.HtmlControls.HtmlButton), "HTMLButton_Attributes");
				TestAttributeseControl(typeof(System.Web.UI.HtmlControls.HtmlAnchor), "HTMLAnchor_Attributes");
				TestAttributeseControl(typeof(System.Web.UI.HtmlControls.HtmlInputHidden), "HtmlInputHidden_Attributes");
				TestAttributeseControl(typeof(System.Web.UI.HtmlControls.HtmlInputImage), "HtmlInputImage_Attributes");
				TestAttributeseControl(typeof(System.Web.UI.HtmlControls.HtmlTextArea), "HtmlTextArea_Attributes");
				TestAttributeseControl(typeof(System.Web.UI.HtmlControls.HtmlGenericControl), "HtmlGenericControl_Attributes");
			}

			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}
			GHTTestEnd();
		}
	}
}
