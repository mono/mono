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
	public class BaseDataList_ctor_
		: GHTDataListBase
	{
		private delegate void BuildDataListControl(BaseDataList ctl);

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
			HtmlForm form1 = (HtmlForm) this.FindControl("form1");
			this.GHTTestBegin(form1);
			this.Test(typeof(DataGrid), new BaseDataList_ctor_.BuildDataListControl(this.GHTBuildSampleDataGrid));
			this.Test(typeof(DataList), new BaseDataList_ctor_.BuildDataListControl(this.GHTBuildSampleDataList));
			this.GHTTestEnd();
		}
		private void Test(Type CtlType, BaseDataList_ctor_.BuildDataListControl CtlBuilder)
		{
			try
			{
				this.GHTSubTestBegin("BaseDataList_" + CtlType.Name + "_DataBind1");
				BaseDataList list1 = (BaseDataList) this.GHTElementClone(CtlType);
				base.GHTActiveSubTest.Controls.Add(list1);
				CtlBuilder(list1);
				list1.DataSource = GHTDataListBase.GHTGetSampleDataSource();
				list1.DataBind();
				list1.Caption = "Caption Text";
				list1.CaptionAlign = TableCaptionAlign.Right;
				list1.UseAccessibleHeader = true;
			}
			catch (Exception exception2)
			{
				this.GHTSubTestUnexpectedExceptionCaught(exception2);
			}
		}
 
	}
}
