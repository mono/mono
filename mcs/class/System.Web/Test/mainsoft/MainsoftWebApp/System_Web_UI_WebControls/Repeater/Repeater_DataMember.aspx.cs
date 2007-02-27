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
	public class Repeater_DataMember
		: GHTBaseWeb 
	{
		protected System.Web.UI.WebControls.Repeater Repeater1;
		protected GHTWebControls.GHTSubTest GHTSubTest1;
		protected System.Web.UI.WebControls.Repeater Repeater2;
		protected GHTWebControls.GHTSubTest GHTSubTest2;
		protected System.Web.UI.WebControls.Repeater Repeater3;
		protected GHTWebControls.GHTSubTest GHTSubTest3;
		protected System.Web.UI.WebControls.Repeater Repeater4;
		protected GHTWebControls.GHTSubTest GHTSubTest4;
		protected System.Web.UI.WebControls.Repeater Repeater5;
		protected GHTWebControls.GHTSubTest GHTSubTest5;
		protected System.Web.UI.WebControls.Repeater Repeater6;
		protected GHTWebControls.GHTSubTest GHTSubTest6;
		protected System.Web.UI.WebControls.Repeater Repeater7;
		protected GHTWebControls.GHTSubTest GHTSubTest7;
		protected System.Web.UI.WebControls.Repeater Repeater8;
		protected GHTWebControls.GHTSubTest GHTSubTest8;
		protected System.Web.UI.WebControls.Repeater Repeater9;
		protected GHTWebControls.GHTSubTest GHTSubTest9;
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
			System.Web.UI.HtmlControls.HtmlForm frm = (HtmlForm)this.FindControl("Form1");
			GHTTestBegin(frm);

			GHTActiveSubTest = GHTSubTest1;
			try 
			{
				Repeater1.DataSource = GHTTests.GHDataSources.DSArrayList();
				Repeater1.DataBind();
				GHTSubTestAddResult(Repeater1.DataMember);
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTActiveSubTest = GHTSubTest2;
			try 
			{
				Repeater2.DataSource = GHTTests.GHDataSources.DSArrayList();
				Repeater2.DataMember = "";
				Repeater2.DataBind();
				GHTSubTestAddResult(Repeater2.DataMember);
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTActiveSubTest = GHTSubTest3;
			try 
			{
				Repeater3.DataSource = GHTTests.GHDataSources.DSArrayList();
				Repeater3.DataMember = "NonExistValue";
				Repeater3.DataBind();
				GHTSubTestAddResult(Repeater3.DataMember);
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTActiveSubTest = GHTSubTest4;
			try 
			{
				Repeater4.DataSource = GHTTests.GHDataSources.DSDataTable();
				Repeater4.DataMember = "Not Name Of Table";
				Repeater4.DataBind();
				GHTSubTestAddResult(Repeater4.DataMember);
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTActiveSubTest = GHTSubTest5;
			try 
			{
				Repeater5.DataSource = GHTTests.GHDataSources.DSDataTable();
				Repeater5.DataMember = "Customers";
				Repeater5.DataBind();
				GHTSubTestAddResult(Repeater5.DataMember);
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTActiveSubTest = GHTSubTest6;
			try 
			{
				Repeater6.DataSource = GHTTests.GHDataSources.DSDataSet();;
				Repeater6.DataBind();
				GHTSubTestAddResult(Repeater6.DataMember);
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTActiveSubTest = GHTSubTest7;
			try 
			{
				Repeater7.DataSource = GHTTests.GHDataSources.DSDataSet();
				Repeater7.DataMember = "Customers";
				Repeater7.DataBind();
				GHTSubTestAddResult(Repeater7.DataMember);
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTActiveSubTest = GHTSubTest8;
			try 
			{
				Repeater8.DataSource = GHTTests.GHDataSources.DSDataSet();
				Repeater8.DataMember = "Orders";
				Repeater8.DataBind();
				GHTSubTestAddResult(Repeater8.DataMember);
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}
#if !NET_2_0
			GHTActiveSubTest = GHTSubTest9;
			try 
			{
				Repeater9.DataSource = GHTTests.GHDataSources.DSDataSet();
				Repeater9.DataMember = "NonExist";
				Repeater9.DataBind();
				GHTSubTestAddResult(Repeater9.DataMember);
				GHTSubTestExpectedExceptionNotCaught("HttpException");
			}
			catch (HttpException ex) 
			{
				GHTSubTestExpectedExceptionCaught(ex);
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}
#endif
			GHTTestEnd();
		}
	}
}
