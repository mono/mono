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
	public class RepeaterItem_DataItem
		: GHTBaseWeb 
	{
		protected System.Web.UI.WebControls.Repeater Repeater1;
		protected GHTWebControls.GHTSubTest GHTSubTest1;
		protected System.Web.UI.WebControls.Repeater Repeater2;
		protected GHTWebControls.GHTSubTest Ghtsubtest2;
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
			this.Repeater1.ItemCreated += new RepeaterItemEventHandler(Repeater1_ItemCreated);
			this.Repeater2.ItemCreated += new RepeaterItemEventHandler(Repeater2_ItemCreated);
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

				IEnumerator items = Repeater1.Items.GetEnumerator();
				System.Web.UI.WebControls.RepeaterItem item;

				while ( items.MoveNext() )
				{
					item = (System.Web.UI.WebControls.RepeaterItem)items.Current;
					GHTSubTestAddResult(item.DataItem.ToString());
				}
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTActiveSubTest = Ghtsubtest2;
			try 
			{
				Repeater2.DataSource = GHTTests.GHDataSources.DSArrayList();
				Repeater2.DataBind();
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTTestEnd();
		}

		private void Repeater1_ItemCreated(object sender, System.Web.UI.WebControls.RepeaterItemEventArgs e)
		{
			if (e.Item.ItemIndex >= 0)
			{
				e.Item.Controls.Add(new LiteralControl("<div>"));
				e.Item.Controls.Add(new LiteralControl(e.Item.DataItem.GetType().ToString()));
				e.Item.Controls.Add(new LiteralControl("</div>"));
				e.Item.Controls.Add(new LiteralControl("<div>"));
				e.Item.Controls.Add(new LiteralControl(e.Item.DataItem.ToString()));
				e.Item.Controls.Add(new LiteralControl("</div>"));
			}
		}

		private void Repeater2_ItemCreated(object sender, System.Web.UI.WebControls.RepeaterItemEventArgs e)
		{
			if (e.Item.ItemIndex >= 0)
				e.Item.DataItem = "eee";
		}
	}
}
