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
	public class TableRowCollection_AddAt_IT
		: GHTBaseWeb 
	{
		protected GHTWebControls.GHTSubTest GHTSubTest1;
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
			base.GHTTestBegin((HtmlForm)this.FindControl("Form1"));

			System.Web.UI.WebControls.Table tbl1 = new System.Web.UI.WebControls.Table();
			tbl1.Rows.Add(new System.Web.UI.WebControls.TableRow());

			System.Web.UI.WebControls.Table tbl = new System.Web.UI.WebControls.Table();
			tbl.Rows.Add(new System.Web.UI.WebControls.TableRow());

			try 
			{
				base.GHTSubTestBegin("AddAt rows");
				base.GHTActiveSubTest.Controls.Add(tbl);
				base.GHTActiveSubTest.Controls.Add(tbl1);

				// add new cell
				tbl1.Rows.AddAt(0, new System.Web.UI.WebControls.TableRow());
				tbl.Rows.AddAt(0, new System.Web.UI.WebControls.TableRow());
				System.Web.UI.WebControls.TableRow tblRow = new System.Web.UI.WebControls.TableRow();
				tbl.Rows.AddAt(1, tblRow);

				// add cell from one table to another
				tbl.Rows.AddAt(1, tbl1.Rows[0]);
			}
			catch (Exception ex) 
			{
				base.GHTSubTestUnexpectedExceptionCaught(ex);
			}
			base.GHTSubTestEnd();
			base.GHTTestEnd();
		}
	}
}
