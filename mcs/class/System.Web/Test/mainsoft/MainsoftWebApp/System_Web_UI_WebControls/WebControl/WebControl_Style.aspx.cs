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
	public class WebControl_Style
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

		protected System.Web.UI.WebControls.Button Button2;
		protected GHTWebControls.GHTSubTest GHTSubTest24;
		protected System.Web.UI.WebControls.CheckBox Checkbox2;
		protected GHTWebControls.GHTSubTest GHTSubTest25;
		protected System.Web.UI.WebControls.HyperLink Hyperlink2;
		protected GHTWebControls.GHTSubTest GHTSubTest26;
		protected System.Web.UI.WebControls.Image Image2;
		protected GHTWebControls.GHTSubTest GHTSubTest27;
		protected System.Web.UI.WebControls.ImageButton Imagebutton2;
		protected GHTWebControls.GHTSubTest GHTSubTest28;
		protected System.Web.UI.WebControls.Label Label2;
		protected GHTWebControls.GHTSubTest GHTSubTest29;
		protected System.Web.UI.WebControls.LinkButton Linkbutton2;
		protected GHTWebControls.GHTSubTest GHTSubTest30;
		protected System.Web.UI.WebControls.Panel Panel2;
		protected GHTWebControls.GHTSubTest GHTSubTest31;
		protected System.Web.UI.WebControls.RadioButton Radiobutton2;
		protected GHTWebControls.GHTSubTest GHTSubTest32;
		protected System.Web.UI.WebControls.TextBox Textbox2;
		protected GHTWebControls.GHTSubTest GHTSubTest33;
		protected System.Web.UI.WebControls.DropDownList Dropdownlist2;
		protected GHTWebControls.GHTSubTest GHTSubTest34;
		protected System.Web.UI.WebControls.ListBox Listbox2;
		protected GHTWebControls.GHTSubTest GHTSubTest35;
		protected System.Web.UI.WebControls.RadioButtonList Radiobuttonlist2;
		protected GHTWebControls.GHTSubTest GHTSubTest36;
		protected System.Web.UI.WebControls.CheckBoxList Checkboxlist2;
		protected GHTWebControls.GHTSubTest GHTSubTest37;
		protected GHTWebControls.GHTSubTest GHTSubTest38;
		protected GHTWebControls.GHTSubTest GHTSubTest39;
		protected GHTWebControls.GHTSubTest GHTSubTest40;
		protected GHTWebControls.GHTSubTest GHTSubTest41;
		protected GHTWebControls.GHTSubTest GHTSubTest42;
		protected GHTWebControls.GHTSubTest GHTSubTest43;
		protected System.Web.UI.WebControls.DataGrid Datagrid3;
		protected GHTWebControls.GHTSubTest GHTSubTest44;
		protected System.Web.UI.WebControls.DataGrid Datagrid4;
		protected GHTWebControls.GHTSubTest GHTSubTest45;
		protected System.Web.UI.WebControls.DataList Datalist3;
		protected GHTWebControls.GHTSubTest GHTSubTest46;
		protected System.Web.UI.WebControls.DataList Datalist4;
		protected GHTWebControls.GHTSubTest GHTSubTest47;
		protected System.Web.UI.WebControls.Table Table4;
		protected GHTWebControls.GHTSubTest GHTSubTest48;
		protected System.Web.UI.WebControls.Table Table6;
		protected GHTWebControls.GHTSubTest GHTSubTest49;
		protected System.Web.UI.WebControls.Table Table7;
		protected GHTWebControls.GHTSubTest GHTSubTest50;
		protected System.Web.UI.WebControls.Table Table8;
		protected GHTWebControls.GHTSubTest GHTSubTest51;

		protected static string [] m_dataSource = new String[] {"aaa", "bbb", "ccc", "ddd", "eee", "fff"};

		private void Page_Load(object sender, System.EventArgs e) 
		{
			HtmlForm frm  = (HtmlForm)FindControl("Form1");
			GHTTestBegin(frm);
			RunDesignTimeTests();

			foreach (Type currentType in TypesToTest)
			{
				GHTHeader(currentType.ToString());
				Test(currentType);
			}
			GHTTestEnd();
		}

		private void Test(Type ctrlType)
		{
			try
			{
				this.GHTSubTestBegin(ctrlType, "Default attributes");
				this.TestedControl.Style.Add("AddedStyleAttribute", "Value");
			}
			catch (Exception exception2)
			{
				// ProjectData.SetProjectError(exception2);
				Exception exception1 = exception2;
				this.GHTSubTestUnexpectedExceptionCaught(exception1);
				// ProjectData.ClearProjectError();
			}
		}
 

		private void RunDesignTimeTests()
		{
			try
			{
				this.Datagrid3.DataSource = WebControl_Style.m_dataSource;
				this.Datagrid4.DataSource = WebControl_Style.m_dataSource;
				this.Datalist3.DataSource = WebControl_Style.m_dataSource;
				this.Datalist4.DataSource = WebControl_Style.m_dataSource;
				this.Datagrid3.DataBind();
				this.Datagrid4.DataBind();
				this.Datalist3.DataBind();
				this.Datalist4.DataBind();
				GHTSubTest test1 = this.GHTSubTest24;
				WebControl control1 = this.Button2;
				this.AddAttributes(ref test1, ref control1);
				this.Button2 = (Button) control1;
				this.GHTSubTest24 = test1;
				test1 = this.GHTSubTest25;
				control1 = this.Checkbox2;
				this.AddAttributes(ref test1, ref control1);
				this.Checkbox2 = (CheckBox) control1;
				this.GHTSubTest25 = test1;
				test1 = this.GHTSubTest26;
				control1 = this.Hyperlink2;
				this.AddAttributes(ref test1, ref control1);
				this.Hyperlink2 = (HyperLink) control1;
				this.GHTSubTest26 = test1;
				test1 = this.GHTSubTest27;
				control1 = this.Image2;
				this.AddAttributes(ref test1, ref control1);
				this.Image2 = (Image) control1;
				this.GHTSubTest27 = test1;
				test1 = this.GHTSubTest28;
				control1 = this.Imagebutton2;
				this.AddAttributes(ref test1, ref control1);
				this.Imagebutton2 = (ImageButton) control1;
				this.GHTSubTest28 = test1;
				test1 = this.GHTSubTest29;
				control1 = this.Label2;
				this.AddAttributes(ref test1, ref control1);
				this.Label2 = (Label) control1;
				this.GHTSubTest29 = test1;
				test1 = this.GHTSubTest30;
				control1 = this.Linkbutton2;
				this.AddAttributes(ref test1, ref control1);
				this.Linkbutton2 = (LinkButton) control1;
				this.GHTSubTest30 = test1;
				test1 = this.GHTSubTest31;
				control1 = this.Panel2;
				this.AddAttributes(ref test1, ref control1);
				this.Panel2 = (Panel) control1;
				this.GHTSubTest31 = test1;
				test1 = this.GHTSubTest32;
				control1 = this.Radiobutton2;
				this.AddAttributes(ref test1, ref control1);
				this.Radiobutton2 = (RadioButton) control1;
				this.GHTSubTest32 = test1;
				test1 = this.GHTSubTest33;
				control1 = this.Textbox2;
				this.AddAttributes(ref test1, ref control1);
				this.Textbox2 = (TextBox) control1;
				this.GHTSubTest33 = test1;
				test1 = this.GHTSubTest34;
				control1 = this.Dropdownlist2;
				this.AddAttributes(ref test1, ref control1);
				this.Dropdownlist2 = (DropDownList) control1;
				this.GHTSubTest34 = test1;
				test1 = this.GHTSubTest35;
				control1 = this.Listbox2;
				this.AddAttributes(ref test1, ref control1);
				this.Listbox2 = (ListBox) control1;
				this.GHTSubTest35 = test1;
				test1 = this.GHTSubTest36;
				control1 = this.Radiobuttonlist2;
				this.AddAttributes(ref test1, ref control1);
				this.Radiobuttonlist2 = (RadioButtonList) control1;
				this.GHTSubTest36 = test1;
				test1 = this.GHTSubTest37;
				control1 = this.Checkboxlist2;
				this.AddAttributes(ref test1, ref control1);
				this.Checkboxlist2 = (CheckBoxList) control1;
				this.GHTSubTest37 = test1;
				test1 = this.GHTSubTest44;
				control1 = this.Datagrid3;
				this.AddAttributes(ref test1, ref control1);
				this.Datagrid3 = (DataGrid) control1;
				this.GHTSubTest44 = test1;
				test1 = this.GHTSubTest45;
				control1 = this.Datagrid4.Items[0];
				this.AddAttributes(ref test1, ref control1);
				this.GHTSubTest45 = test1;
				test1 = this.GHTSubTest46;
				control1 = this.Datalist3;
				this.AddAttributes(ref test1, ref control1);
				this.Datalist3 = (DataList) control1;
				this.GHTSubTest46 = test1;
				test1 = this.GHTSubTest47;
				control1 = this.Datalist4.Items[0];
				this.AddAttributes(ref test1, ref control1);
				this.GHTSubTest47 = test1;
				test1 = this.GHTSubTest48;
				control1 = this.Table4;
				this.AddAttributes(ref test1, ref control1);
				this.Table4 = (Table) control1;
				this.GHTSubTest48 = test1;
				test1 = this.GHTSubTest49;
				control1 = this.Table6;
				this.AddAttributes(ref test1, ref control1);
				this.Table6 = (Table) control1;
				this.GHTSubTest49 = test1;
				test1 = this.GHTSubTest50;
				control1 = this.Table7;
				this.AddAttributes(ref test1, ref control1);
				this.Table7 = (Table) control1;
				this.GHTSubTest50 = test1;
				test1 = this.GHTSubTest51;
				control1 = this.Table8;
				this.AddAttributes(ref test1, ref control1);
				this.Table8 = (Table) control1;
				this.GHTSubTest51 = test1;
			}
			catch (Exception exception2)
			{
				// ProjectData.SetProjectError(exception2);
				Exception exception1 = exception2;
				this.GHTSubTestBegin();
				this.GHTSubTestUnexpectedExceptionCaught(exception1);
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
					enumerator1 = a_toTest.Style.Keys.GetEnumerator();
					while (enumerator1.MoveNext())
					{
						string text1 = (string)(enumerator1.Current);
						this.GHTSubTestAddResult("Key: " + text1 + " Value: " + a_toTest.Style[text1]);
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
