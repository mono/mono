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
using System.Collections;
using GHTWebControls;

namespace GHTTests.System_Web_dll.System_Web_UI_WebControls
{
	public class WebControl_Attributes
		: GHTWebControlBase
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

		protected System.Web.UI.WebControls.Button Button1;
		protected GHTWebControls.GHTSubTest GHTSubTest25;
		protected System.Web.UI.WebControls.CheckBox CheckBox1;
		protected GHTWebControls.GHTSubTest GHTSubTest26;
		protected System.Web.UI.WebControls.HyperLink HyperLink1;
		protected GHTWebControls.GHTSubTest GHTSubTest28;
		protected System.Web.UI.WebControls.Image Image1;
		protected GHTWebControls.GHTSubTest GHTSubTest30;
		protected System.Web.UI.WebControls.ImageButton ImageButton1;
		protected GHTWebControls.GHTSubTest GHTSubTest32;
		protected System.Web.UI.WebControls.Label Label1;
		protected GHTWebControls.GHTSubTest GHTSubTest34;
		protected System.Web.UI.WebControls.LinkButton LinkButton1;
		protected GHTWebControls.GHTSubTest GHTSubTest36;
		protected System.Web.UI.WebControls.Panel Panel1;
		protected GHTWebControls.GHTSubTest GHTSubTest37;
		protected System.Web.UI.WebControls.RadioButton RadioButton1;
		protected GHTWebControls.GHTSubTest GHTSubTest38;
		protected System.Web.UI.WebControls.TextBox TextBox1;
		protected GHTWebControls.GHTSubTest GHTSubTest39;
		protected System.Web.UI.WebControls.DropDownList DropDownList1;
		protected GHTWebControls.GHTSubTest GHTSubTest40;
		protected System.Web.UI.WebControls.ListBox ListBox1;
		protected GHTWebControls.GHTSubTest GHTSubTest41;
		protected System.Web.UI.WebControls.RadioButtonList RadioButtonList1;
		protected GHTWebControls.GHTSubTest GHTSubTest42;
		protected System.Web.UI.WebControls.CheckBoxList CheckBoxList1;
		protected GHTWebControls.GHTSubTest GHTSubTest43;
		protected System.Web.UI.WebControls.CompareValidator CompareValidator1;
		protected GHTWebControls.GHTSubTest GHT_SubTest44;
		protected System.Web.UI.WebControls.CustomValidator CustomValidator1;
		protected GHTWebControls.GHTSubTest GHT_SubTest45;
		protected System.Web.UI.WebControls.RangeValidator RangeValidator1;
		protected GHTWebControls.GHTSubTest GHT_SubTest46;
		protected System.Web.UI.WebControls.RegularExpressionValidator RegularExpressionValidator1;
		protected GHTWebControls.GHTSubTest GHT_SubTest47;
		protected System.Web.UI.WebControls.RequiredFieldValidator RequiredFieldValidator1;
		protected GHTWebControls.GHTSubTest GHT_SubTest48;
		protected System.Web.UI.WebControls.ValidationSummary ValidationSummary1;
		protected GHTWebControls.GHTSubTest GHT_SubTest49;
		protected System.Web.UI.WebControls.DataGrid DataGrid1;
		protected GHTWebControls.GHTSubTest GHTSubTest50;
		protected System.Web.UI.WebControls.DataGrid DataGrid2;
		protected GHTWebControls.GHTSubTest GHTSubTest51;
		protected System.Web.UI.WebControls.DataList DataList1;
		protected GHTWebControls.GHTSubTest GHTSubTest52;
		protected System.Web.UI.WebControls.DataList DataList2;
		protected GHTWebControls.GHTSubTest GHTSubTest53;
		protected System.Web.UI.WebControls.Table Table1;
		protected GHTWebControls.GHTSubTest GHTSubTest54;
		protected System.Web.UI.WebControls.Table Table5;
		protected GHTWebControls.GHTSubTest GHTSubTest55;
		protected System.Web.UI.WebControls.Table Table2;
		protected GHTWebControls.GHTSubTest GHTSubTest56;
		protected System.Web.UI.WebControls.Table Table3;
		protected GHTWebControls.GHTSubTest GHTSubTest57;

		protected static Item [] m_dataSource = new Item[] { 
															   new Item(1, "aaa"), 
															   new Item(2, "bbb"), 
															   new Item(3, "ccc"), 
															   new Item(4, "ddd"), 
															   new Item(5, "eee"), 
															   new Item(6, "fff")};

		private void Page_Load(object sender, System.EventArgs e) 
		{
			HtmlForm frm  = (HtmlForm)FindControl("Form1");
			GHTTestBegin(frm);
			foreach (Type currentType in TypesToTest)
			{
				GHTHeader(currentType.ToString());
				Test(currentType);
			}

			RunDesignTimeTests();

			GHTTestEnd();
		}

		private void Test(Type ctrlType)
		{
			try
			{
				IEnumerator enumerator1 = null;
				this.GHTSubTestBegin(ctrlType, "Description");
				this.TestedControl.Attributes.Add("Test", " Value");
				try
				{
					enumerator1 = this.TestedControl.Attributes.Keys.GetEnumerator();
					while (enumerator1.MoveNext())
					{
						string text1 = (string)(enumerator1.Current);
						this.GHTSubTestAddResult("Attribute - Name: " + text1 + " Value: " + this.TestedControl.Attributes[text1]);
					}
				}
				finally
				{
					if (enumerator1 is IDisposable)
					{
						((IDisposable) enumerator1).Dispose();
					}
				}
			}
			catch (Exception exception2)
			{
				// ProjectData.SetProjectError(exception2);
				Exception exception1 = exception2;
				this.GHTSubTestUnexpectedExceptionCaught(exception1);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
		}
 

		private void RunDesignTimeTests()
		{
			try
			{
				this.DataGrid1.DataSource = WebControl_Attributes.m_dataSource;
				this.DataGrid2.DataSource = WebControl_Attributes.m_dataSource;
				this.DataList1.DataSource = WebControl_Attributes.m_dataSource;
				this.DataList2.DataSource = WebControl_Attributes.m_dataSource;
				this.DataGrid1.DataBind();
				this.DataGrid2.DataBind();
				this.DataList1.DataBind();
				this.DataList2.DataBind();
				GHTSubTest test1 = this.GHTSubTest25;
				WebControl control1 = this.Button1;
				this.AddAttributes(ref test1, ref control1);
				this.Button1 = (Button) control1;
				this.GHTSubTest25 = test1;
				test1 = this.GHTSubTest26;
				control1 = this.CheckBox1;
				this.AddAttributes(ref test1, ref control1);
				this.CheckBox1 = (CheckBox) control1;
				this.GHTSubTest26 = test1;
				test1 = this.GHTSubTest28;
				control1 = this.HyperLink1;
				this.AddAttributes(ref test1, ref control1);
				this.HyperLink1 = (HyperLink) control1;
				this.GHTSubTest28 = test1;
				test1 = this.GHTSubTest30;
				control1 = this.Image1;
				this.AddAttributes(ref test1, ref control1);
				this.Image1 = (Image) control1;
				this.GHTSubTest30 = test1;
				test1 = this.GHTSubTest32;
				control1 = this.ImageButton1;
				this.AddAttributes(ref test1, ref control1);
				this.ImageButton1 = (ImageButton) control1;
				this.GHTSubTest32 = test1;
				test1 = this.GHTSubTest34;
				control1 = this.Label1;
				this.AddAttributes(ref test1, ref control1);
				this.Label1 = (Label) control1;
				this.GHTSubTest34 = test1;
				test1 = this.GHTSubTest36;
				control1 = this.LinkButton1;
				this.AddAttributes(ref test1, ref control1);
				this.LinkButton1 = (LinkButton) control1;
				this.GHTSubTest36 = test1;
				test1 = this.GHTSubTest37;
				control1 = this.Panel1;
				this.AddAttributes(ref test1, ref control1);
				this.Panel1 = (Panel) control1;
				this.GHTSubTest37 = test1;
				test1 = this.GHTSubTest38;
				control1 = this.RadioButton1;
				this.AddAttributes(ref test1, ref control1);
				this.RadioButton1 = (RadioButton) control1;
				this.GHTSubTest38 = test1;
				test1 = this.GHTSubTest39;
				control1 = this.TextBox1;
				this.AddAttributes(ref test1, ref control1);
				this.TextBox1 = (TextBox) control1;
				this.GHTSubTest39 = test1;
				test1 = this.GHTSubTest40;
				control1 = this.DropDownList1;
				this.AddAttributes(ref test1, ref control1);
				this.DropDownList1 = (DropDownList) control1;
				this.GHTSubTest40 = test1;
				test1 = this.GHTSubTest41;
				control1 = this.ListBox1;
				this.AddAttributes(ref test1, ref control1);
				this.ListBox1 = (ListBox) control1;
				this.GHTSubTest41 = test1;
				test1 = this.GHTSubTest42;
				control1 = this.RadioButtonList1;
				this.AddAttributes(ref test1, ref control1);
				this.RadioButtonList1 = (RadioButtonList) control1;
				this.GHTSubTest42 = test1;
				test1 = this.GHTSubTest43;
				control1 = this.CheckBoxList1;
				this.AddAttributes(ref test1, ref control1);
				this.CheckBoxList1 = (CheckBoxList) control1;
				this.GHTSubTest43 = test1;
				test1 = this.GHTSubTest50;
				control1 = this.DataGrid1;
				this.AddAttributes(ref test1, ref control1);
				this.DataGrid1 = (DataGrid) control1;
				this.GHTSubTest50 = test1;
				test1 = this.GHTSubTest51;
				control1 = this.DataGrid2.Items[0];
				this.AddAttributes(ref test1, ref control1);
				this.GHTSubTest51 = test1;
				test1 = this.GHTSubTest52;
				control1 = this.DataList1;
				this.AddAttributes(ref test1, ref control1);
				this.DataList1 = (DataList) control1;
				this.GHTSubTest52 = test1;
				test1 = this.GHTSubTest53;
				control1 = this.DataList2.Items[0];
				this.AddAttributes(ref test1, ref control1);
				this.GHTSubTest53 = test1;
				test1 = this.GHTSubTest54;
				control1 = this.Table1;
				this.AddAttributes(ref test1, ref control1);
				this.Table1 = (Table) control1;
				this.GHTSubTest54 = test1;
				test1 = this.GHTSubTest55;
				control1 = this.Table5;
				this.AddAttributes(ref test1, ref control1);
				this.Table5 = (Table) control1;
				this.GHTSubTest55 = test1;
				test1 = this.GHTSubTest56;
				control1 = this.Table2;
				this.AddAttributes(ref test1, ref control1);
				this.Table2 = (Table) control1;
				this.GHTSubTest56 = test1;
				test1 = this.GHTSubTest57;
				control1 = this.Table3;
				this.AddAttributes(ref test1, ref control1);
				this.Table3 = (Table) control1;
				this.GHTSubTest57 = test1;
			}
			catch (Exception exception2)
			{
				// ProjectData.SetProjectError(exception2);
				Exception exception1 = exception2;
				this.GHTSubTestBegin();
				string text1 = string.Empty + exception1.GetType().ToString();
				text1 = text1 + " caught during preperations for design time tests.";
				text1 = text1 + "<br>Message: ";
				text1 = text1 + exception1.Message;
				text1 = text1 + "<br>Trace: ";
				text1 = text1 + exception1.StackTrace;
				this.GHTSubTestAddResult(text1);
				this.GHTSubTestEnd();
				// ProjectData.ClearProjectError();
			}
		}
 

		private void AddAttributes(ref GHTSubTest a_currentSubTest, ref WebControl a_toTest)
		{
			try
			{
				IEnumerator enumerator1 = null;
				base.GHTActiveSubTest = a_currentSubTest;
				try
				{
					enumerator1 = a_toTest.Attributes.Keys.GetEnumerator();
					while (enumerator1.MoveNext())
					{
						string text1 = (string)(enumerator1.Current);
						this.GHTSubTestAddResult("Attribute - Name: " + text1 + " Value: " + a_toTest.Attributes[text1]);
					}
				}
				finally
				{
					if (enumerator1 is IDisposable)
					{
						((IDisposable) enumerator1).Dispose();
					}
				}
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
