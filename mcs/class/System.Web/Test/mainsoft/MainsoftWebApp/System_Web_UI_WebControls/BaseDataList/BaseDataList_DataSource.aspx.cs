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
	public class BaseDataList_DataSource
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

			this.Test(typeof(DataGrid), new BaseDataList_DataSource.BuildDataListControl(this.GHTBuildUnboundSampleDataGrid));
			this.Test(typeof(DataList), new BaseDataList_DataSource.BuildDataListControl(this.GHTBuildUnboundSampleDataList));
  
			GHTTestEnd();
		}
		private void Test(Type CtlType, BaseDataList_DataSource.BuildDataListControl CtlBuilder)
		{
			BaseDataList list1;
			try
			{
				this.GHTSubTestBegin("BaseDataList_" + CtlType.Name + "_DataSource1");
				list1 = (BaseDataList) this.GHTElementClone(CtlType);
				base.GHTActiveSubTest.Controls.Add(list1);
				CtlBuilder(list1);
				list1.DataSource = GHTDataListBase.GHTGetSampleDataSourceDS();
				list1.DataMember = "Sample";
				list1.DataBind();
			}
			catch (Exception exception6)
			{
				this.GHTSubTestUnexpectedExceptionCaught(exception6);
			}
			this.GHTSubTestEnd();
			try
			{
				this.GHTSubTestBegin("BaseDataList_" + CtlType.Name + "_DataSource2");
				list1 = (BaseDataList) this.GHTElementClone(CtlType);
				base.GHTActiveSubTest.Controls.Add(list1);
				CtlBuilder(list1);
				list1.DataSource = GHTDataListBase.GHTGetSampleDataSource();
				list1.DataBind();
			}
			catch (Exception exception7)
			{
				this.GHTSubTestUnexpectedExceptionCaught(exception7);
			}
			this.GHTSubTestEnd();
			try
			{
				this.GHTSubTestBegin("BaseDataList_" + CtlType.Name + "_DataSource3");
				list1 = (BaseDataList) this.GHTElementClone(CtlType);
				base.GHTActiveSubTest.Controls.Add(list1);
				list1.DataSource = GHTDataListBase.GHTGetSampleDataSourceArray();
				list1.DataBind();
			}
			catch (Exception exception8)
			{
				this.GHTSubTestUnexpectedExceptionCaught(exception8);
			}
			this.GHTSubTestEnd();
			try
			{
				this.GHTSubTestBegin("BaseDataList_" + CtlType.Name + "_DataSource5");
				list1 = (BaseDataList) this.GHTElementClone(CtlType);
				base.GHTActiveSubTest.Controls.Add(list1);
				CtlBuilder(list1);
				list1.DataSource = "Unknown";
				list1.DataBind();
				this.GHTSubTestExpectedExceptionNotCaught("httpException");
			}
			catch (Exception exception9)
			{
				this.GHTSubTestExpectedExceptionCaught(exception9);
			}
			this.GHTSubTestEnd();
			try
			{
				this.GHTSubTestBegin("BaseDataList_" + CtlType.Name + "_DataSource6");
				list1 = (BaseDataList) this.GHTElementClone(CtlType);
				base.GHTActiveSubTest.Controls.Add(list1);
				CtlBuilder(list1);
				list1.DataSource = null;
				list1.DataBind();
			}
			catch (Exception exception10)
			{
				this.GHTSubTestUnexpectedExceptionCaught(exception10);
			}
			this.GHTSubTestEnd();
		}
 	}
}
