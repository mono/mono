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
using System.Data;
using System.Collections;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace GHTTests.System_Web_dll.System_Web_UI_WebControls
{
	public class Repeater_DataSource
		: GHTBaseWeb 
	{
		protected System.Web.UI.WebControls.Repeater Repeater1;
		protected GHTWebControls.GHTSubTest GHTSubTest1;
		protected System.Web.UI.WebControls.Repeater Repeater2;
		protected GHTWebControls.GHTSubTest Ghtsubtest2;
		protected System.Web.UI.WebControls.Repeater Repeater3;
		protected GHTWebControls.GHTSubTest Ghtsubtest3;
		protected System.Web.UI.WebControls.Repeater Repeater4;
		protected GHTWebControls.GHTSubTest Ghtsubtest4;
		protected System.Web.UI.WebControls.Repeater Repeater5;
		protected GHTWebControls.GHTSubTest Ghtsubtest5;
		protected System.Web.UI.WebControls.Repeater Repeater6;
		protected GHTWebControls.GHTSubTest GHTSubTest6;
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
			//Put user code to initialize the page here

			System.Web.UI.HtmlControls.HtmlForm frm = (HtmlForm)this.FindControl("Form1");
			GHTTestBegin(frm);

			GHTActiveSubTest = GHTSubTest1;
			try 
			{
				Repeater1.DataSource = GHTTests.GHDataSources.DSArrayList();
				Repeater1.DataBind();

				GHTSubTestAddResult(Repeater1.DataSource.GetType().ToString());
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTActiveSubTest = Ghtsubtest2;
			try 
			{
				Repeater2.DataBind();
				GHTSubTestAddResult(Repeater2.DataSource.GetType().ToString());
				GHTSubTestExpectedExceptionNotCaught("NullReferenceException");
			}
			catch (NullReferenceException eex) 
			{
				GHTSubTestExpectedExceptionCaught(eex); 
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTActiveSubTest = Ghtsubtest3;
			try 
			{
				Repeater3.DataSource = 123;
				Repeater3.DataBind();

				GHTSubTestAddResult(Repeater3.DataSource.GetType().ToString());
				GHTSubTestExpectedExceptionNotCaught("ArgumentException");
			}
			catch (ArgumentException eex) 
			{
				GHTSubTestExpectedExceptionCaught(eex); 
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTActiveSubTest = Ghtsubtest4;
			try 
			{
				Repeater4.DataBind();
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTActiveSubTest = Ghtsubtest5;
			try 
			{
				ArrayList a = new ArrayList();
				Repeater5.DataSource = a;
				Repeater5.DataBind();
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}


			GHTActiveSubTest = GHTSubTest6;
			try 
			{
				Repeater6.DataSource = null;
				Repeater6.DataBind();
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}


			GHTTestEnd();
		}
	}
}
