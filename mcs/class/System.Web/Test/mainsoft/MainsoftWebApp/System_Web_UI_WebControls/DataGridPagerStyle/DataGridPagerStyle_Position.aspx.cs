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
	public class DataGridPagerStyle_Position
		: GHTBaseWeb 
	{
		protected System.Web.UI.WebControls.DataGrid DataGrid1;
		protected GHTWebControls.GHTSubTest GHTSubTest1;
		protected System.Web.UI.WebControls.DataGrid DataGrid2;
		protected GHTWebControls.GHTSubTest GHTSubTest2;
		protected System.Web.UI.WebControls.DataGrid DataGrid3;
		protected GHTWebControls.GHTSubTest GHTSubTest3;
		protected System.Web.UI.WebControls.DataGrid DataGrid4;
		protected GHTWebControls.GHTSubTest Ghtsubtest4;
		protected System.Web.UI.WebControls.DataGrid DataGrid5;
		protected GHTWebControls.GHTSubTest Ghtsubtest5;
		protected System.Web.UI.WebControls.DataGrid DataGrid6;
		protected GHTWebControls.GHTSubTest Ghtsubtest6;
		protected System.Web.UI.WebControls.DataGrid DataGrid7;
		protected GHTWebControls.GHTSubTest Ghtsubtest7;
		protected System.Web.UI.WebControls.DataGrid DataGrid8;
		protected GHTWebControls.GHTSubTest Ghtsubtest8;
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
				DataGrid1.DataSource = GHTTests.GHDataSources.DSDataTable(1, 3);
				DataGrid1.AllowPaging = true;
				DataGrid1.DataBind();;
				GHTSubTestAddResult(DataGrid1.PagerStyle.Position.ToString());
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTActiveSubTest = GHTSubTest2;
			try 
			{
				DataGrid2.DataSource = GHTTests.GHDataSources.DSDataTable(1, 3);
				DataGrid2.AllowPaging = true;
				DataGrid2.PagerStyle.Position = PagerPosition.Bottom;
				DataGrid2.DataBind();;
				GHTSubTestAddResult(DataGrid2.PagerStyle.Position.ToString());
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTActiveSubTest = GHTSubTest3;
			try 
			{
				DataGrid3.DataSource = GHTTests.GHDataSources.DSDataTable(1, 3);
				DataGrid3.AllowPaging = true;
				DataGrid3.PagerStyle.Position = PagerPosition.Top;
				DataGrid3.DataBind();;
				GHTSubTestAddResult(DataGrid3.PagerStyle.Position.ToString());
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTActiveSubTest = Ghtsubtest4;
			try 
			{
				DataGrid4.DataSource = GHTTests.GHDataSources.DSDataTable(1, 3);
				DataGrid4.AllowPaging = true;
				DataGrid4.PagerStyle.Position = PagerPosition.TopAndBottom;
				DataGrid4.DataBind();;
				GHTSubTestAddResult(DataGrid4.PagerStyle.Position.ToString());
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTActiveSubTest = Ghtsubtest5;
			try 
			{
				DataGrid5.DataSource = GHTTests.GHDataSources.DSDataTable(1, 3);
				DataGrid5.AllowPaging = true;
				DataGrid5.PagerStyle.Mode = PagerMode.NumericPages;
				DataGrid5.DataBind();
				GHTSubTestAddResult(DataGrid5.PagerStyle.Position.ToString());
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTActiveSubTest = Ghtsubtest6;
			try 
			{
				DataGrid6.DataSource = GHTTests.GHDataSources.DSDataTable(1, 3);
				DataGrid6.AllowPaging = true;
				DataGrid6.PagerStyle.Position = PagerPosition.Bottom;
				DataGrid6.PagerStyle.Mode = PagerMode.NumericPages;
				DataGrid6.DataBind();
				GHTSubTestAddResult(DataGrid6.PagerStyle.Position.ToString());
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTActiveSubTest = Ghtsubtest7;
			try 
			{
				DataGrid7.DataSource = GHTTests.GHDataSources.DSDataTable(1, 3);
				DataGrid7.AllowPaging = true;
				DataGrid7.PagerStyle.Position = PagerPosition.Top;
				DataGrid7.PagerStyle.Mode = PagerMode.NumericPages;
				DataGrid7.DataBind();
				GHTSubTestAddResult(DataGrid7.PagerStyle.Position.ToString());
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTActiveSubTest = Ghtsubtest8;
			try 
			{
				DataGrid8.DataSource = GHTTests.GHDataSources.DSDataTable(1, 3);
				DataGrid8.AllowPaging = true;
				DataGrid8.PagerStyle.Position = PagerPosition.TopAndBottom;
				DataGrid8.PagerStyle.Mode = PagerMode.NumericPages;
				DataGrid8.DataBind();
				GHTSubTestAddResult(DataGrid8.PagerStyle.Position.ToString());
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTTestEnd();
		}
	}
}
