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
	public class ListItem_Equals_O
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

		private void Page_Load(object sender, System.EventArgs e) 
		{
			HtmlForm frm = (HtmlForm) (HtmlForm)this.FindControl("Form1");
			this.GHTTestBegin(frm);


			// SubTest Reference equality
			this.GHTSubTestBegin("Reference equality");
			try 
			{
				System.Web.UI.WebControls.ListItem item1 = new System.Web.UI.WebControls.ListItem("Text", "Value");
				System.Web.UI.WebControls.ListItem item2 = item1;

				Compare(item1.Equals(item2), true);
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}
			GHTSubTestEnd();

			GHTTestBegin(frm);

			// SubTest Text & Value equal, but selected is not equal
			GHTSubTestBegin("Text & Value equal, but selected is not equal");
				try 
				{
					System.Web.UI.WebControls.ListItem item3 = new System.Web.UI.WebControls.ListItem("Text", "Value");
					item3.Selected = true;
					System.Web.UI.WebControls.ListItem item4 = new System.Web.UI.WebControls.ListItem("Text", "Value");
					item4.Selected = false;
					Compare(item3.Equals(item4), true);
				}
				catch (Exception ex) 
				{
					GHTSubTestUnexpectedExceptionCaught(ex);
				}
			GHTSubTestEnd();

			// SubTest Values are not equal
			GHTSubTestBegin("Values are not equal");
			try 
			{
				System.Web.UI.WebControls.ListItem item5 = new System.Web.UI.WebControls.ListItem("Text", "Value1");
				System.Web.UI.WebControls.ListItem item6 = new System.Web.UI.WebControls.ListItem("Text", "Value2");
				Compare(item5.Equals(item6), false);
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}
			GHTSubTestEnd();

			// SubTest Texts are not equal
			GHTSubTestBegin("Texts are not equal");
				try 
				{
					System.Web.UI.WebControls.ListItem item7 = new System.Web.UI.WebControls.ListItem("Text1", "Value");
					System.Web.UI.WebControls.ListItem item8 = new System.Web.UI.WebControls.ListItem("Text2", "Value");
					Compare(item7.Equals(item8), false);
				}
				catch (Exception ex) 
				{
					GHTSubTestUnexpectedExceptionCaught(ex);
				}
			GHTSubTestEnd();

			// SubTest FakeListItem
			GHTSubTestBegin("FakeListItem");
			try 
			{
				System.Web.UI.WebControls.ListItem item9 = new System.Web.UI.WebControls.ListItem("Text", "Value");
				FakeListItem item10 = new FakeListItem("Text", "Value");
				Compare(item9.Equals(item10), false);
			}
			catch (Exception ex) 
			{
				GHTSubTestUnexpectedExceptionCaught(ex);
			}
			GHTSubTestEnd();

			GHTTestEnd();
		}
	}


	class FakeListItem
	{
		// Methods
		public FakeListItem(string a_text, string a_value)
		{
			this.m_text = a_text;
			this.m_value = a_value;
		}
		// Properties
		public bool Selected
		{
			get
			{
				return this.m_selected;
			}
			set
			{
				this.m_selected = value;
			}
		}
		public string Text
		{
			get
			{
				return this.m_text;
			}
			set
			{
				this.m_text = value;
			}
		}
		public string Value
		{
			get
			{
				return this.m_value;
			}
			set
			{
				this.m_value = value;
			}
		}
		// Fields
		private bool m_selected;
		private string m_text;
		private string m_value;
	}
}

