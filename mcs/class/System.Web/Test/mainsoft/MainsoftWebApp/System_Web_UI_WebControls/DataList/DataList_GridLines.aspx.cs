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
using System.Data;

namespace GHTTests.System_Web_dll.System_Web_UI_WebControls
{
	public class DataList_GridLines
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
		protected System.Web.UI.WebControls.DataList DataList2;
		protected GHTWebControls.GHTSubTest GHTSubTest2;
		protected System.Web.UI.WebControls.DataList DataList3;
		protected GHTWebControls.GHTSubTest GHTSubTest3;
		protected System.Web.UI.WebControls.DataList DataList4;
		protected GHTWebControls.GHTSubTest GHTSubTest4;
		protected System.Web.UI.WebControls.DataList DataList5;
		protected GHTWebControls.GHTSubTest GHTSubTest5;
		protected static DataTable m_data; // Will be used as datasource for the datalists in this test.

		// initializes m_data.
		static DataList_GridLines()
		{
			DataList_GridLines.m_data = new DataTable("Items");
			DataList_GridLines.m_data.Columns.Add(new DataColumn("StringValue", typeof(string)));
			DataList_GridLines.m_data.Columns.Add(new DataColumn("PriceValue", typeof(string)));
			DataList_GridLines.m_data.Columns.Add(new DataColumn("DescriptionValue", typeof(string)));
			DataList_GridLines.m_data.Columns.Add(new DataColumn("ExponentialValue", typeof(string)));
			int num1 = 0;
			do
			{
				DataRow row1 = DataList_GridLines.m_data.NewRow();
				row1[0] = "Item " + num1.ToString();
				double num3 = 1.23 * (num1 + 1);
				row1[1] = num3.ToString("C");
				row1[2] = "Description for Item " + num1.ToString();
				int num2 = 0x75bcd15 * (num1 + 1);
				row1[3] = num2.ToString("E");
				DataList_GridLines.m_data.Rows.Add(row1);
				num1++;
			}
			while (num1 <= 3);
		}
 

		private void Page_Load(object sender, System.EventArgs e) 
		{
			HtmlForm frm  = (HtmlForm)FindControl("Form1");
			GHTTestBegin(frm);

			GHTActiveSubTest = GHTSubTest1;
			try 
			{
				DataList1.DataBind();;
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTActiveSubTest = GHTSubTest2;
			try 
			{
				DataList2.DataBind();
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTActiveSubTest = GHTSubTest3;
			try 
			{
				DataList3.DataBind();
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTActiveSubTest = GHTSubTest4;
			try 
			{
				DataList4.DataBind();
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			GHTActiveSubTest = GHTSubTest5;
			try 
			{
				DataList5.DataBind();
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}

			// Check default value
			try
			{
				this.GHTSubTestBegin("Default value");
				DataList list1 = new DataList();
				base.GHTActiveSubTest.Controls.Add(list1);
				DataList list2 = list1;
				list2.DataSource = DataList_GridLines.m_data;
				list2.HeaderTemplate = this.DataList1.HeaderTemplate;
				list2.ItemTemplate = this.DataList1.ItemTemplate;
				list2.HeaderStyle.CopyFrom(this.DataList1.HeaderStyle);
				list2.ItemStyle.CopyFrom(this.DataList1.ItemStyle);
				list2.RepeatLayout = RepeatLayout.Table;
				list2.DataBind();
				list2 = null;
			}
			catch (Exception exception12)
			{
				// ProjectData.SetProjectError(exception12);
				Exception exception6 = exception12;
				this.GHTSubTestUnexpectedExceptionCaught(exception6);
				// ProjectData.ClearProjectError();
			}
			this.GHTTestEnd();

		}
	}
}
