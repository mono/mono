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
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Data;
using System.Text;

namespace GHTTests.System_Web_dll.System_Web_UI_WebControls
{
	public class BaseDataList_DataBind_
		: GHTDataListBase
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
		private delegate void BuildDataListControl(BaseDataList ctl);

		private void Page_Load(object sender, System.EventArgs e) 
		{
			HtmlForm frm = (HtmlForm)FindControl("form1");

			GHTTestBegin(frm);

			this.Test(typeof(DataGrid), new BaseDataList_DataBind_.BuildDataListControl(this.GHTBuildSampleDataGrid));
			this.Test(typeof(DataList), new BaseDataList_DataBind_.BuildDataListControl(this.GHTBuildSampleDataList));
			testDataSetBind();

			GHTTestEnd();
		}

		private void Test(Type CtlType, BaseDataList_DataBind_.BuildDataListControl CtlBuilder)
		{
			BaseDataList list1;
			try
			{
				this.GHTSubTestBegin("BaseDataList_" + CtlType.Name + "_DataBind1");
				list1 = (BaseDataList) this.GHTElementClone(CtlType);
				base.GHTActiveSubTest.Controls.Add(list1);
				CtlBuilder(list1);
				list1.DataSource = GHTDataListBase.GHTGetSampleDataSource();
				list1.DataBind();
			}
			catch (Exception exception3)
			{
				this.GHTSubTestUnexpectedExceptionCaught(exception3);
			}
			try
			{
				this.GHTSubTestBegin("BaseDataList_" + CtlType.Name + "_DataBind2");
				list1 = (BaseDataList) this.GHTElementClone(CtlType);
				base.GHTActiveSubTest.Controls.Add(list1);
				CtlBuilder(list1);
				list1.DataSource = "Unknown";
				list1.DataBind();
				this.GHTSubTestExpectedExceptionNotCaught("??????");
			}
			catch (Exception exception4)
			{
				this.GHTSubTestExpectedExceptionCaught(exception4);
				return;
			}
		}

		private void testDataSetBind()
		{
			try
			{
				this.GHTSubTestBegin("BaseDataList_DataGrid_DataBind3");
				DataGrid grid1 = new DataGrid();
				base.GHTActiveSubTest.Controls.Add(grid1);
				DataSet set1 = new DataSet();
				string text1 = "<NewDataSet><TestRunDetails><Test_x0020_Id>13266</Test_x0020_Id><Name>OleDbCommand_Transaction</Name></TestRunDetails></NewDataSet>";
				MemoryStream stream1 = new MemoryStream();
				byte[] buffer1 = Encoding.Default.GetBytes(text1);
				stream1.Write(buffer1, 0, buffer1.Length);
				stream1.Seek(0, SeekOrigin.Begin);
				set1.ReadXml(stream1);
				set1.Tables[0].Columns["Test Id"].ColumnName = "TestId";
				grid1.DataSource = set1;
				grid1.DataBind();
			}
			catch (Exception exception2)
			{
				// ProjectData.SetProjectError(exception2);
				Exception exception1 = exception2;
				this.GHTSubTestUnexpectedExceptionCaught(exception1);
				// ProjectData.ClearProjectError();
			}
		}
 

	}
}