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
 
namespace GHTTests.System_Web_dll.System_Web
{
	public class HttpRequest_MapPath_S
		: GHTBaseWeb 
	{
		private void Page_Load(object sender, System.EventArgs e) 
		{
			System.Web.UI.HtmlControls.HtmlForm frm = (HtmlForm)FindControl("Form1");
			GHTTestBegin(frm);
			// trying full path
			GHTSubTestBegin("Request.MapPath1");
			try 
			{
				GHTSubTestAddResult(Request.MapPath("/"));
			}
			catch (ArgumentException ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}
			GHTSubTestEnd();

			// trying invalid path
			GHTSubTestBegin("Request.MapPath2");
			try 
			{
				GHTSubTestAddResult(Request.MapPath("Http://dummy"));
				GHTSubTestExpectedExceptionNotCaught("HttpException");
			}
			catch (ArgumentException ex) 
			{
				GHTSubTestExpectedExceptionCaught(ex);
			}
			GHTSubTestEnd();

			// another invalid path
			GHTSubTestBegin("Request.MapPath3");
			try 
			{
				// due to bug limitation 2846 we compare only the lenght of the MapPath
				GHTSubTestAddResult((Request.MapPath("").Length > 0).ToString());
			}
			catch (ArgumentException ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}
			GHTSubTestEnd();

			// trying a sub directory with a "/"
			GHTSubTestBegin("Request.MapPath4");
			try 
			{
				// due to bug limitation 2846 we compare only the lenght of the MapPath
				GHTSubTestAddResult((Request.MapPath("/System_Web_dll").Length > 0).ToString());
			}
			catch (ArgumentException ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}
			GHTSubTestEnd();

			// trying a sub directory 
			GHTSubTestBegin("Request.MapPath5");
			try 
			{
				// due to bug limitation 2846 we compare only the lenght of the MapPath
				GHTSubTestAddResult((Request.MapPath("/System_Web_dll/").Length > 0).ToString());
			}
			catch (ArgumentException ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
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
