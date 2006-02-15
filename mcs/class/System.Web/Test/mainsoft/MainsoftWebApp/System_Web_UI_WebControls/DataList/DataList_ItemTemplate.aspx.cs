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
using System.Drawing;

namespace GHTTests.System_Web_dll.System_Web_UI_WebControls
{
	public class DataList_ItemTemplate
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

		protected System.Web.UI.WebControls.DataList DataList1;
		protected GHTWebControls.GHTSubTest GHTSubTest1;
		protected System.Web.UI.WebControls.DataList Datalist2;
		protected GHTWebControls.GHTSubTest Ghtsubtest2;
		protected System.Web.UI.WebControls.DataList Datalist3;
		protected GHTWebControls.GHTSubTest Ghtsubtest3;
		protected System.Web.UI.WebControls.DataList DataList4;
		protected GHTWebControls.GHTSubTest GHTSubTest4;
		protected System.Web.UI.WebControls.DataList DataList5;
		protected GHTWebControls.GHTSubTest GHTSubTest5;
		private static string [] m_data = new String[] {"aaa", "bbb", "ccc", "ddd", "eee", "fff", "ggg"};

		private void Page_Load(object sender, System.EventArgs e) 
		{
			HtmlForm frm  = (HtmlForm)FindControl("Form1");
			GHTTestBegin(frm);

			GHTActiveSubTest = GHTSubTest1;
			try 
			{
				DataList1.DataSource = m_data;;
				DataList1.DataBind();;
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTActiveSubTest = Ghtsubtest2;
			try 
			{
				Datalist2.DataSource = m_data;
				Datalist2.DataBind();
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTActiveSubTest = Ghtsubtest3;
			try 
			{
				Datalist3.DataSource = m_data;
				Datalist3.DataBind();
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTActiveSubTest = GHTSubTest4;
			try 
			{
				DataList4.DataSource = m_data;
				DataList4.DataBind();
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTActiveSubTest = GHTSubTest5;
			try 
			{
				DataList5.ItemTemplate = new ItemTemplate();
				DataList5.DataSource = m_data;
				DataList5.DataBind();
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTTestEnd();
		}

		// Template for  item.
		private class ItemTemplate : ITemplate
		{
			private Label m_l;


			public void InstantiateIn(Control container)
			{
				this.m_l = new Label();
				this.m_l.BackColor = Color.Aquamarine;
				container.Controls.Add(this.m_l);
				m_l.DataBinding += new EventHandler(m_l_DataBinding);
			}
 
			private void m_l_DataBinding(object sender, EventArgs e)
			{
				DataListItem item1 = (DataListItem) ((Label) sender).NamingContainer;
				this.m_l.Text = (string)(item1.DataItem);
			}
 
		}
	}
}
