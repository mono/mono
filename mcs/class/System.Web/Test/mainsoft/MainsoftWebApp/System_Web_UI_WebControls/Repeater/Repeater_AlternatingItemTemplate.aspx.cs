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
	public class Repeater_AlternatingItemTemplate
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

		#region "Template Classes"
		public class t_DBLitTemplate : ITemplate
		{
			public void InstantiateIn(Control container)
			{
				Literal l = new Literal();
				l.Text = "<div>";
				l.DataBinding += new EventHandler(this.BindData);
				container.Controls.Add(l);
			}

			public void BindData(object sender, EventArgs e)
			{
				Literal lc = (Literal)sender;
				RepeaterItem container;
				container = (RepeaterItem)lc.NamingContainer;
				lc.Text = container.DataItem + "</div>";
			}

		}
		public class t_EmptyLitTemplate : ITemplate
		{
			public void InstantiateIn(Control container)
			{
				Literal l = new Literal();
				l.Text = "";
				container.Controls.Add(l);
			}

		}
		public class t_PlainTextLitTemplate : ITemplate
		{
			public void InstantiateIn(Control container)
			{
				Literal l = new Literal();
				l.Text = "Plain text template create at run time";
				container.Controls.Add(l);
			}

		}
		public class t_HtmlLitTemplate : ITemplate
		{
			public void InstantiateIn(Control container)
			{
				Literal l = new Literal();
				l.Text = "<div><b><i>Html template created at run time</i></b></div>";
				container.Controls.Add(l);
			}

		}
		public class t_ControlLitTemplate : ITemplate
		{
			public void InstantiateIn(Control container)
			{
				HtmlInputButton l = new HtmlInputButton();
				l.Value = "Control template";
				container.Controls.Add(l);
			}

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
				Repeater1.DataSource = GHTTests.GHDataSources.DSArrayList();;
				Repeater1.DataBind();
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTActiveSubTest = GHTSubTest2;
			try 
			{
				Repeater2.DataSource = GHTTests.GHDataSources.DSArrayList();;
				Repeater2.DataBind();
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTActiveSubTest = GHTSubTest3;
			try 
			{
				Repeater3.DataSource = GHTTests.GHDataSources.DSArrayList();;
				Repeater3.DataBind();
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTActiveSubTest = GHTSubTest4;
			try 
			{
				Repeater4.DataSource = GHTTests.GHDataSources.DSArrayList();;
				Repeater4.DataBind();
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTActiveSubTest = GHTSubTest5;
			try 
			{
				Repeater5.DataSource = GHTTests.GHDataSources.DSArrayList();;
				Repeater5.DataBind();
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTSubTestBegin("Code base template 1");
			try 
			{
				System.Web.UI.WebControls.Repeater Repeater6 = new System.Web.UI.WebControls.Repeater();
				GHTActiveSubTest.Controls.Add(Repeater6);
				Repeater6.DataSource = GHTTests.GHDataSources.DSArrayList();;
				Repeater6.AlternatingItemTemplate = new t_EmptyLitTemplate();
				Repeater6.ItemTemplate = new t_DBLitTemplate();
				Repeater6.DataBind();
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}
			GHTSubTestEnd();

			GHTSubTestBegin("Code base template 2");
			try 
			{
				System.Web.UI.WebControls.Repeater Repeater7 = new System.Web.UI.WebControls.Repeater();
				GHTActiveSubTest.Controls.Add(Repeater7);
				Repeater7.DataSource = GHTTests.GHDataSources.DSArrayList();;
				Repeater7.AlternatingItemTemplate = new t_PlainTextLitTemplate();
				Repeater7.ItemTemplate = new t_DBLitTemplate();
				Repeater7.DataBind();
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}
			GHTSubTestEnd();

			GHTSubTestBegin("Code base template 3");
			try 
			{
				System.Web.UI.WebControls.Repeater Repeater8 = new System.Web.UI.WebControls.Repeater();
				GHTActiveSubTest.Controls.Add(Repeater8);
				Repeater8.DataSource = GHTTests.GHDataSources.DSArrayList();;
				Repeater8.AlternatingItemTemplate = new t_HtmlLitTemplate();
				Repeater8.ItemTemplate = new t_DBLitTemplate();
				Repeater8.DataBind();
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}
			GHTSubTestEnd();

			GHTSubTestBegin("Code base template 4");
			try 
			{
				System.Web.UI.WebControls.Repeater Repeater9 = new System.Web.UI.WebControls.Repeater();
				GHTActiveSubTest.Controls.Add(Repeater9);
				Repeater9.DataSource = GHTTests.GHDataSources.DSArrayList();;
				Repeater9.AlternatingItemTemplate = new t_ControlLitTemplate();
				Repeater9.ItemTemplate = new t_DBLitTemplate();
				Repeater9.DataBind();
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}
			GHTSubTestEnd();

			GHTSubTestBegin("Code base template 5");
				try 
				{
					System.Web.UI.WebControls.Repeater Repeater10 = new System.Web.UI.WebControls.Repeater();
					GHTActiveSubTest.Controls.Add(Repeater10);
					Repeater10.DataSource = GHTTests.GHDataSources.DSArrayList();;
					Repeater10.AlternatingItemTemplate = new t_DBLitTemplate();
					Repeater10.ItemTemplate = new t_DBLitTemplate();
					Repeater10.DataBind();
				}
				catch (Exception ex) 
				{
					GHTSubTestUnexpectedExceptionCaught(ex);
				}
			GHTSubTestEnd();

			GHTTestEnd();
		}
	}
}
