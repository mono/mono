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
{public class DataListItem_DataItem
    : GHTBaseWeb {
	 #region Web Form Designer generated code
	 override protected void OnInit(EventArgs e) {
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
	 private void InitializeComponent() {    
		 this.Load += new System.EventHandler(this.Page_Load);
		 this.DataList1.ItemCreated += new DataListItemEventHandler(DataList1_ItemCreated);

	 }
	 #endregion

 	protected System.Web.UI.WebControls.DataList DataList1;
 	protected GHTWebControls.GHTSubTest GHTSubTest1;
 	protected GHTWebControls.GHTSubTest Ghtsubtest2;
 	protected GHTWebControls.GHTSubTest Ghtsubtest3;
 	protected GHTWebControls.GHTSubTest Ghtsubtest4;
 	protected GHTWebControls.GHTSubTest Ghtsubtest5;
 	protected GHTWebControls.GHTSubTest Ghtsubtest6;
 	protected GHTWebControls.GHTSubTest Ghtsubtest7;
 	protected GHTWebControls.GHTSubTest Ghtsubtest8;

        protected static string [] m_data = new string[] {"aaa", "bbb", "ccc", "ddd"};

        private void Page_Load(object sender, System.EventArgs e) {
            HtmlForm frm = (HtmlForm)FindControl("form1");
            GHTTestBegin(frm);
            DataList1.DataBind();;

        }


	 private void DataList1_ItemCreated(object sender, DataListItemEventArgs e)
	 {
		 switch (e.Item.ItemType)
		 {
			 case ListItemType.Header:
			 {
				 try
				 {
					 this.GHTSubTestAddResult(this.Ghtsubtest4, (string)(e.Item.DataItem));
				 }
				 catch (Exception exception12)
				 {
					 // ProjectData.SetProjectError(exception12);
					 Exception exception4 = exception12;
					 this.GHTSubTestUnexpectedExceptionCaught(exception4);
					 // ProjectData.ClearProjectError();
				 }
				 this.GHTSubTestEnd();
				 return;
			 }
			 case ListItemType.Footer:
			 {
				 try
				 {
					 this.GHTSubTestAddResult(this.Ghtsubtest3, (string)(e.Item.DataItem));
				 }
				 catch (Exception exception11)
				 {
					 // ProjectData.SetProjectError(exception11);
					 Exception exception3 = exception11;
					 this.GHTSubTestUnexpectedExceptionCaught(exception3);
					 // ProjectData.ClearProjectError();
				 }
				 this.GHTSubTestEnd();
				 return;
			 }
			 case ListItemType.Item:
			 {
				 try
				 {
					 this.GHTSubTestAddResult(this.Ghtsubtest5, (string)(e.Item.DataItem));
				 }
				 catch (Exception exception13)
				 {
					 // ProjectData.SetProjectError(exception13);
					 Exception exception5 = exception13;
					 this.GHTSubTestUnexpectedExceptionCaught(exception5);
					 // ProjectData.ClearProjectError();
				 }
				 this.GHTSubTestEnd();
				 return;
			 }
			 case ListItemType.AlternatingItem:
			 {
				 try
				 {
					 this.GHTSubTestAddResult(this.GHTSubTest1, (string)(e.Item.DataItem));
				 }
				 catch (Exception exception9)
				 {
					 // ProjectData.SetProjectError(exception9);
					 Exception exception1 = exception9;
					 this.GHTSubTestUnexpectedExceptionCaught(exception1);
					 // ProjectData.ClearProjectError();
				 }
				 return;
			 }
			 case ListItemType.SelectedItem:
			 {
				 try
				 {
					 this.GHTSubTestAddResult(this.Ghtsubtest7, (string)(e.Item.DataItem));
				 }
				 catch (Exception exception15)
				 {
					 // ProjectData.SetProjectError(exception15);
					 Exception exception7 = exception15;
					 this.GHTSubTestUnexpectedExceptionCaught(exception7);
					 // ProjectData.ClearProjectError();
				 }
				 this.GHTSubTestEnd();
				 return;
			 }
			 case ListItemType.EditItem:
			 {
				 try
				 {
					 this.GHTSubTestAddResult(this.Ghtsubtest2, (string)(e.Item.DataItem));
				 }
				 catch (Exception exception10)
				 {
					 // ProjectData.SetProjectError(exception10);
					 Exception exception2 = exception10;
					 this.GHTSubTestUnexpectedExceptionCaught(exception2);
					 // ProjectData.ClearProjectError();
				 }
				 return;
			 }
			 case ListItemType.Separator:
			 {
				 try
				 {
					 this.GHTSubTestAddResult(this.Ghtsubtest8, (string)(e.Item.DataItem));
				 }
				 catch (Exception exception16)
				 {
					 // ProjectData.SetProjectError(exception16);
					 Exception exception8 = exception16;
					 this.GHTSubTestUnexpectedExceptionCaught(exception8);
					 // ProjectData.ClearProjectError();
				 }
				 this.GHTSubTestEnd();
				 return;
			 }
			 case ListItemType.Pager:
			 {
				 try
				 {
					 this.GHTSubTestAddResult(this.Ghtsubtest6, (string)(e.Item.DataItem));
				 }
				 catch (Exception exception14)
				 {
					 // ProjectData.SetProjectError(exception14);
					 Exception exception6 = exception14;
					 this.GHTSubTestUnexpectedExceptionCaught(exception6);
					 // ProjectData.ClearProjectError();
				 }
				 this.GHTSubTestEnd();
				 return;
			 }
		 }
	 }
 
 }
}
