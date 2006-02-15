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

namespace GHTTests.System_Web_dll.System_Web_UI_WebControls
{
	public class ListItem_Attributes
		: GHTBaseWeb 
	{
		protected System.Web.UI.WebControls.ListBox ListBox1;
		protected GHTWebControls.GHTSubTest GHTSubTest1;
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

		private void Page_Load(object sender, EventArgs e)
		{
			HtmlForm form1 = (HtmlForm) (HtmlForm)this.FindControl("Form1");
			this.GHTTestBegin(form1);
			foreach (ListItem item1 in this.ListBox1.Items)
			{
				AttributeCollection collection1 = item1.Attributes;
				this.AddTitle(ref form1, item1.Text);
				this.RunSubTests(ref collection1);
			}
			this.GHTTestEnd();
		}
 

		private void RunSubTests(ref AttributeCollection attributes)
		{
			this.GHTSubTestBegin("Attributes Keys count");
			try
			{
				this.GHTSubTestAddResult(attributes.Count.ToString());
			}
			catch (Exception exception4)
			{
				// ProjectData.SetProjectError(exception4);
				Exception exception1 = exception4;
				this.GHTSubTestUnexpectedExceptionCaught(exception1);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("Attributes Values count");
			try
			{
				this.GHTSubTestAddResult(attributes[attributes.Count.ToString()]);
			}
			catch (Exception exception5)
			{
				// ProjectData.SetProjectError(exception5);
				Exception exception2 = exception5;
				this.GHTSubTestUnexpectedExceptionCaught(exception2);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			this.GHTSubTestBegin("Attributes Keys+Values ");
			try
			{
				IEnumerator enumerator1 = null;
				string text1 = string.Empty + "<AttributeContainer ";
				try
				{
					enumerator1 = attributes.Keys.GetEnumerator();
					while (enumerator1.MoveNext())
					{
						string text2 = (string)(enumerator1.Current);
						string[] textArray1 = new string[] { text1, " ", text2, "=", attributes[text2], " " } ;
						text1 = string.Concat(textArray1);
					}
				}
				finally
				{
					if (enumerator1 is IDisposable)
					{
						((IDisposable) enumerator1).Dispose();
					}
				}
				text1 = text1 + "></AttributeContainer>";
				this.GHTSubTestAddResult(text1);
			}
			catch (Exception exception6)
			{
				// ProjectData.SetProjectError(exception6);
				Exception exception3 = exception6;
				this.GHTSubTestUnexpectedExceptionCaught(exception3);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
		}
 
		private void AddTitle(ref HtmlForm frm, string text)
		{
			Label label1 = new Label();
			label1.Text = text;
			label1.Font.Bold = true;
			label1.Font.Underline = true;
			FontUnit unit1 = new FontUnit(FontSize.Larger);
			label1.Font.Size = unit1;
			frm.Controls.Add(label1);
		}
 
	}
}
