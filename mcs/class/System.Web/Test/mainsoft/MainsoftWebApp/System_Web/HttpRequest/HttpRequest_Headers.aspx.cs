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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Collections.Specialized;

namespace GHTTests.System_Web_dll.System_Web
{
	public class HttpRequest_Headers
		: GHTBaseWeb 
	{

		private void Page_Load(object sender, System.EventArgs e) 
		{
			System.Web.UI.HtmlControls.HtmlForm frm = (HtmlForm)FindControl("Form1");
			GHTTestBegin(frm);
			// ===================================
			// testing if the headers object is set
			// ===================================
			GHTSubTestBegin("Request.Headers1");
				try 
				{
					if (Request.Headers == null)
						GHTSubTestAddResult("Failed");
					else
						GHTSubTestAddResult("Success");
				}
				catch (Exception ex) 
				{
					GHTSubTestAddResult("unxpected " + ex.GetType().Name + " exception was caught-" + ex.Message);
				}

			GHTSubTestEnd();

			// ===================================
			// testing if the objects returned is from the 
			// correct type
			// ===================================
			GHTSubTestBegin("Request.Headers2");
			try 
			{
				NameValueCollection cookie = Request.Headers;
				GHTSubTestAddResult("success");
			}
			catch (Exception ex) 
			{
				GHTSubTestAddResult("Unxpected " + ex.GetType().Name + " exception was caught-" + ex.Message);
			}
			GHTSubTestEnd();

			// ===================================
			// testing if the Headers collection contains the
			// right context. 
			// ===================================
			GHTSubTestBegin("Request.Headers3");
			try 
			{
				NameValueCollection Headers = Request.Headers;
				foreach (string current in Headers.Keys)
					GHTSubTestAddResult(current);
			}

			catch (Exception ex) 
			{
				GHTSubTestAddResult("Unxpected " + ex.GetType().Name + " exception was caught-" + ex.Message);
			}

			GHTSubTestEnd();

			// ===================================
			// testing if the Headers collection contains the
			// right context. 
			// ===================================
			GHTSubTestBegin("Request.Headers4");
			try 
			{
				NameValueCollection Headers = this.Request.Headers;
				foreach (string header in Headers.AllKeys)
					GHTSubTestAddResult(header);
			}
			catch (Exception ex) 
			{
				GHTSubTestAddResult("Unxpected " + ex.GetType().Name + " exception was caught-" + ex.Message);
			}
			GHTSubTestEnd();

			GHTSubTestBegin("Request.Headers5");
			try
			{
				int num1 = 0;
				string[] textArray1 = this.Request.Headers.AllKeys;
				int num2 = textArray1.Length - 1;
				for (num1 = 0; num1 <= num2; num1++)
				{
					if (textArray1[num1].ToLower().CompareTo("host") != 0)
					{
						this.GHTSubTestAddResult(textArray1[num1] + "=" + this.Request.Headers.Get(textArray1[num1]) + "<br>");
					}
				}
			}
			catch (Exception ex) 
			{
				GHTSubTestAddResult("unxpected " + ex.GetType().Name + " exception was caught-" + ex.Message);
				}
			GHTSubTestEnd();
			GHTTestEnd();
		}

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

	}
}
