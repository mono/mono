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
	public class Repeater_DataBind_
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


		#region Datasource
		private ArrayList DSArrayHashObject()
		{
			ArrayList list1 = new ArrayList();
			ArrayList list3 = GHTTests.GHDataSources.DSArrayList();
			IEnumerator enumerator1 = list3.GetEnumerator();
			while (enumerator1.MoveNext())
			{
				Hashtable hashtable1 = new Hashtable();
				hashtable1.Add("color", new Repeater_DataBind_.CustomColor("1", 1).ToString());
				hashtable1.Add("numberObj", new Repeater_DataBind_.CustomColor("1", 1));
				list1.Add(hashtable1);
			}
			return list1;
		}
		#endregion

		private class CustomColor
		{
			private string color_name;
			private byte color_value;
			public CustomColor(string name, byte value)
			{
				color_name = name;
				color_value = value;
			}
			public override string ToString()
			{
				return "Color: " + color_name;
			}
		}

		private void Page_Load(object sender, System.EventArgs e) 
		{
			System.Web.UI.HtmlControls.HtmlForm frm = (HtmlForm)this.FindControl("Form1");
			GHTTestBegin(frm);

			GHTActiveSubTest = GHTSubTest1;
			try 
			{
				Repeater1.DataSource = GHTTests.GHDataSources.DSArray();;
				Repeater1.DataBind();
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTActiveSubTest = GHTSubTest2;
			try 
			{
				Repeater2.DataSource = GHTTests.GHDataSources.DSArrayList();
				Repeater2.DataBind();
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTActiveSubTest = GHTSubTest3;
			try 
			{
				Repeater3.DataSource = GHTTests.GHDataSources.DSArrayHash();
				Repeater3.DataBind();
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTActiveSubTest = GHTSubTest4;
			try 
			{
				Repeater4.DataSource = GHTTests.GHDataSources.DSDataSet();
				Repeater4.DataBind();
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTActiveSubTest = GHTSubTest5;
			try 
			{
				Repeater5.DataSource = GHTTests.GHDataSources.DSDataTable();
				Repeater5.DataBind();
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTActiveSubTest = GHTSubTest6;
			try 
			{
				Repeater6.DataSource = DSArrayHashObject();
				Repeater6.DataBind();
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTActiveSubTest = GHTSubTest7;
			try 
			{
				Repeater7.DataSource = GHTTests.GHDataSources.DSArray();
					Repeater7.DataBind();
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTTestEnd();
		}
	}
}
