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
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace GHTTests.System_Web_dll.System_Web_UI_WebControls
{
	public class DataListItem_RenderItem_HBB
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

		protected static string [] m_data = new string[] {"aaa", "bbb", "ccc", "ddd"};
		private StringBuilder m_sb;
		protected System.Web.UI.WebControls.DataList DataList1;
		protected GHTWebControls.GHTSubTest GHTSubTest1;
		protected GHTWebControls.GHTSubTest Ghtsubtest2;
		protected GHTWebControls.GHTSubTest Ghtsubtest3;
		protected GHTWebControls.GHTSubTest Ghtsubtest4;
		protected GHTWebControls.GHTSubTest Ghtsubtest5;
		protected GHTWebControls.GHTSubTest Ghtsubtest6;
		protected GHTWebControls.GHTSubTest Ghtsubtest7;
		protected GHTWebControls.GHTSubTest Ghtsubtest8;
		protected GHTWebControls.GHTSubTest Ghtsubtest9;
		protected GHTWebControls.GHTSubTest Ghtsubtest10;
		protected GHTWebControls.GHTSubTest Ghtsubtest11;
		protected GHTWebControls.GHTSubTest Ghtsubtest12;
		protected GHTWebControls.GHTSubTest Ghtsubtest13;
		protected GHTWebControls.GHTSubTest Ghtsubtest14;
		protected GHTWebControls.GHTSubTest Ghtsubtest15;
		protected GHTWebControls.GHTSubTest Ghtsubtest16;
		protected GHTWebControls.GHTSubTest Ghtsubtest17;
		protected GHTWebControls.GHTSubTest Ghtsubtest18;
		protected GHTWebControls.GHTSubTest Ghtsubtest19;
		protected GHTWebControls.GHTSubTest Ghtsubtest20;
		protected GHTWebControls.GHTSubTest Ghtsubtest21;
		protected GHTWebControls.GHTSubTest Ghtsubtest22;
		protected GHTWebControls.GHTSubTest Ghtsubtest23;
		protected GHTWebControls.GHTSubTest Ghtsubtest24;
		protected GHTWebControls.GHTSubTest Ghtsubtest25;
		protected GHTWebControls.GHTSubTest Ghtsubtest26;
		protected GHTWebControls.GHTSubTest Ghtsubtest27;
		protected GHTWebControls.GHTSubTest Ghtsubtest28;
		protected GHTWebControls.GHTSubTest Ghtsubtest29;
		protected GHTWebControls.GHTSubTest Ghtsubtest30;
		protected GHTWebControls.GHTSubTest Ghtsubtest31;
		protected GHTWebControls.GHTSubTest Ghtsubtest32;
		HtmlTextWriter m_hw;

		private void Page_Load(object sender, System.EventArgs e) 
		{
			m_sb = new StringBuilder();
			System.IO.StringWriter sw = new System.IO.StringWriter(m_sb);
			m_hw = new HtmlTextWriter(sw);
			HtmlForm frm = (HtmlForm)FindControl("form1");
			GHTTestBegin(frm);
			DataList1.DataBind();;
		}

		/*
				private void DataList1_ItemCreated(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.DataListItemEventArgs) Handles DataList1.ItemCreated
					Select Case e.Item.ItemType
						Case ListItemType.AlternatingItem
							try {
								GHTActiveSubTest = GHTSubTest1;
								e.Item.RenderItem(m_hw, true, true)
								GHTSubTestAddResult(m_sb.ToString(), true)
							catch (Exception ex) {
								GHTSubTestUnexpectedExceptionCaught(ex);
							Finally
								m_sb.Remove(0, m_sb.Length)
							}

							try {
								GHTActiveSubTest = Ghtsubtest2;
								e.Item.RenderItem(m_hw, False, true)
								GHTSubTestAddResult(m_sb.ToString(), true)
							catch (Exception ex) {
								GHTSubTestUnexpectedExceptionCaught(ex);
							Finally
								m_sb.Remove(0, m_sb.Length)
							}

							try {
								GHTActiveSubTest = Ghtsubtest3;
								e.Item.RenderItem(m_hw, true, False)
								GHTSubTestAddResult(m_sb.ToString(), true)
							catch (Exception ex) {
								GHTSubTestUnexpectedExceptionCaught(ex);
							Finally
								m_sb.Remove(0, m_sb.Length)
							}

							try {
								GHTActiveSubTest = Ghtsubtest4;
								e.Item.RenderItem(m_hw, False, False)
								GHTSubTestAddResult(m_sb.ToString(), true)
							catch (Exception ex) {
								GHTSubTestUnexpectedExceptionCaught(ex);
							Finally
								m_sb.Remove(0, m_sb.Length)
							}
						Case ListItemType.EditItem
							try {
								GHTActiveSubTest = GHTSubTest5;
								e.Item.RenderItem(m_hw, true, true)
								GHTSubTestAddResult(m_sb.ToString(), true)
							catch (Exception ex) {
								GHTSubTestUnexpectedExceptionCaught(ex);
							Finally
								m_sb.Remove(0, m_sb.Length)
							}

							try {
								GHTActiveSubTest = Ghtsubtest6;
								e.Item.RenderItem(m_hw, False, true)
								GHTSubTestAddResult(m_sb.ToString(), true)
							catch (Exception ex) {
								GHTSubTestUnexpectedExceptionCaught(ex);
							Finally
								m_sb.Remove(0, m_sb.Length)
							}

							try {
								GHTActiveSubTest = Ghtsubtest7
								e.Item.RenderItem(m_hw, true, False)
								GHTSubTestAddResult(m_sb.ToString(), true)
							catch (Exception ex) {
								GHTSubTestUnexpectedExceptionCaught(ex);
							Finally
								m_sb.Remove(0, m_sb.Length)
							}

							try {
								GHTActiveSubTest = Ghtsubtest8
								e.Item.RenderItem(m_hw, False, False)
								GHTSubTestAddResult(m_sb.ToString(), true)
							catch (Exception ex) {
								GHTSubTestUnexpectedExceptionCaught(ex);
							Finally
								m_sb.Remove(0, m_sb.Length)
							}
						Case ListItemType.Footer
							try {
								GHTActiveSubTest = Ghtsubtest9
								e.Item.RenderItem(m_hw, true, true)
								GHTSubTestAddResult(m_sb.ToString(), true)
							catch (Exception ex) {
								GHTSubTestUnexpectedExceptionCaught(ex);
							Finally
								m_sb.Remove(0, m_sb.Length)
							}

							try {
								GHTActiveSubTest = Ghtsubtest1;0
								e.Item.RenderItem(m_hw, False, true)
								GHTSubTestAddResult(m_sb.ToString(), true)
							catch (Exception ex) {
								GHTSubTestUnexpectedExceptionCaught(ex);
							Finally
								m_sb.Remove(0, m_sb.Length)
							}

							try {
								GHTActiveSubTest = Ghtsubtest1;1
								e.Item.RenderItem(m_hw, true, False)
								GHTSubTestAddResult(m_sb.ToString(), true)
							catch (Exception ex) {
								GHTSubTestUnexpectedExceptionCaught(ex);
							Finally
								m_sb.Remove(0, m_sb.Length)
							}

							try {
								GHTActiveSubTest = Ghtsubtest1;2
								e.Item.RenderItem(m_hw, False, False)
								GHTSubTestAddResult(m_sb.ToString(), true)
							catch (Exception ex) {
								GHTSubTestUnexpectedExceptionCaught(ex);
							Finally
								m_sb.Remove(0, m_sb.Length)
							}
						Case ListItemType.Header
							try {
								GHTActiveSubTest = Ghtsubtest1;3
								e.Item.RenderItem(m_hw, true, true)
								GHTSubTestAddResult(m_sb.ToString(), true)
							catch (Exception ex) {
								GHTSubTestUnexpectedExceptionCaught(ex);
							Finally
								m_sb.Remove(0, m_sb.Length)
							}

							try {
								GHTActiveSubTest = Ghtsubtest1;4
								e.Item.RenderItem(m_hw, False, true)
								GHTSubTestAddResult(m_sb.ToString(), true)
							catch (Exception ex) {
								GHTSubTestUnexpectedExceptionCaught(ex);
							Finally
								m_sb.Remove(0, m_sb.Length)
							}

							try {
								GHTActiveSubTest = Ghtsubtest1;5
								e.Item.RenderItem(m_hw, true, False)
								GHTSubTestAddResult(m_sb.ToString(), true)
							catch (Exception ex) {
								GHTSubTestUnexpectedExceptionCaught(ex);
							Finally
								m_sb.Remove(0, m_sb.Length)
							}

							try {
								GHTActiveSubTest = Ghtsubtest1;6
								e.Item.RenderItem(m_hw, False, False)
								GHTSubTestAddResult(m_sb.ToString(), true)
							catch (Exception ex) {
								GHTSubTestUnexpectedExceptionCaught(ex);
							Finally
								m_sb.Remove(0, m_sb.Length)
							}
						Case ListItemType.Item
							try {
								GHTActiveSubTest = Ghtsubtest1;7
								e.Item.RenderItem(m_hw, true, true)
								GHTSubTestAddResult(m_sb.ToString(), true)
							catch (Exception ex) {
								GHTSubTestUnexpectedExceptionCaught(ex);
							Finally
								m_sb.Remove(0, m_sb.Length)
							}

							try {
								GHTActiveSubTest = Ghtsubtest1;8
								e.Item.RenderItem(m_hw, False, true)
								GHTSubTestAddResult(m_sb.ToString(), true)
							catch (Exception ex) {
								GHTSubTestUnexpectedExceptionCaught(ex);
							Finally
								m_sb.Remove(0, m_sb.Length)
							}

							try {
								GHTActiveSubTest = Ghtsubtest1;9
								e.Item.RenderItem(m_hw, true, False)
								GHTSubTestAddResult(m_sb.ToString(), true)
							catch (Exception ex) {
								GHTSubTestUnexpectedExceptionCaught(ex);
							Finally
								m_sb.Remove(0, m_sb.Length)
							}

							try {
								GHTActiveSubTest = Ghtsubtest2;0
								e.Item.RenderItem(m_hw, False, False)
								GHTSubTestAddResult(m_sb.ToString(), true)
							catch (Exception ex) {
								GHTSubTestUnexpectedExceptionCaught(ex);
							Finally
								m_sb.Remove(0, m_sb.Length)
							}
						Case ListItemType.Pager
							try {
								GHTActiveSubTest = Ghtsubtest2;1
								e.Item.RenderItem(m_hw, true, true)
								GHTSubTestAddResult(m_sb.ToString(), true)
							catch (Exception ex) {
								GHTSubTestUnexpectedExceptionCaught(ex);
							Finally
								m_sb.Remove(0, m_sb.Length)
							}

							try {
								GHTActiveSubTest = Ghtsubtest2;2
								e.Item.RenderItem(m_hw, False, true)
								GHTSubTestAddResult(m_sb.ToString(), true)
							catch (Exception ex) {
								GHTSubTestUnexpectedExceptionCaught(ex);
							Finally
								m_sb.Remove(0, m_sb.Length)
							}

							try {
								GHTActiveSubTest = Ghtsubtest2;3
								e.Item.RenderItem(m_hw, true, False)
								GHTSubTestAddResult(m_sb.ToString(), true)
							catch (Exception ex) {
								GHTSubTestUnexpectedExceptionCaught(ex);
							Finally
								m_sb.Remove(0, m_sb.Length)
							}

							try {
								GHTActiveSubTest = Ghtsubtest2;4
								e.Item.RenderItem(m_hw, False, False)
								GHTSubTestAddResult(m_sb.ToString(), true)
							catch (Exception ex) {
								GHTSubTestUnexpectedExceptionCaught(ex);
							Finally
								m_sb.Remove(0, m_sb.Length)
							}
						Case ListItemType.SelectedItem
							try {
								GHTActiveSubTest = Ghtsubtest2;5
								e.Item.RenderItem(m_hw, true, true)
								GHTSubTestAddResult(m_sb.ToString(), true)
							catch (Exception ex) {
								GHTSubTestUnexpectedExceptionCaught(ex);
							Finally
								m_sb.Remove(0, m_sb.Length)
							}

							try {
								GHTActiveSubTest = Ghtsubtest2;6
								e.Item.RenderItem(m_hw, False, true)
								GHTSubTestAddResult(m_sb.ToString(), true)
							catch (Exception ex) {
								GHTSubTestUnexpectedExceptionCaught(ex);
							Finally
								m_sb.Remove(0, m_sb.Length)
							}

							try {
								GHTActiveSubTest = Ghtsubtest2;7
								e.Item.RenderItem(m_hw, true, False)
								GHTSubTestAddResult(m_sb.ToString(), true)
							catch (Exception ex) {
								GHTSubTestUnexpectedExceptionCaught(ex);
							Finally
								m_sb.Remove(0, m_sb.Length)
							}

							try {
								GHTActiveSubTest = Ghtsubtest2;8
								e.Item.RenderItem(m_hw, False, False)
								GHTSubTestAddResult(m_sb.ToString(), true)
							catch (Exception ex) {
								GHTSubTestUnexpectedExceptionCaught(ex);
							Finally
								m_sb.Remove(0, m_sb.Length)
							}
						Case ListItemType.Separator
							try {
								GHTActiveSubTest = Ghtsubtest2;9
								e.Item.RenderItem(m_hw, true, true)
								GHTSubTestAddResult(m_sb.ToString(), true)
							catch (Exception ex) {
								GHTSubTestUnexpectedExceptionCaught(ex);
							Finally
								m_sb.Remove(0, m_sb.Length)
							}

							try {
								GHTActiveSubTest = Ghtsubtest3;0
								e.Item.RenderItem(m_hw, False, true)
								GHTSubTestAddResult(m_sb.ToString(), true)
							catch (Exception ex) {
								GHTSubTestUnexpectedExceptionCaught(ex);
							Finally
								m_sb.Remove(0, m_sb.Length)
							}

							try {
								GHTActiveSubTest = Ghtsubtest3;1
								e.Item.RenderItem(m_hw, true, False)
								GHTSubTestAddResult(m_sb.ToString(), true)
							catch (Exception ex) {
								GHTSubTestUnexpectedExceptionCaught(ex);
							Finally
								m_sb.Remove(0, m_sb.Length)
							}

							try {
								GHTActiveSubTest = Ghtsubtest3;2
								e.Item.RenderItem(m_hw, False, False)
								GHTSubTestAddResult(m_sb.ToString(), true)
							catch (Exception ex) {
								GHTSubTestUnexpectedExceptionCaught(ex);
							Finally
								m_sb.Remove(0, m_sb.Length)
							}
					End Select
				}
				*/
	}
}
