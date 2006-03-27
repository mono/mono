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
	public class WebControl_CopyBaseAttributes_W
		: GHTWebControlBase
	{
		protected System.Web.UI.WebControls.Button btnAccessKey;
		protected System.Web.UI.WebControls.Button btnEnabled;
		protected System.Web.UI.WebControls.Button btnToolTip;
		protected System.Web.UI.WebControls.Button btnTabIndex;
		protected System.Web.UI.WebControls.Button btnAttributes;
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
			HtmlForm frm  = (HtmlForm)FindControl("Form1");
			GHTTestBegin(frm);
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
				this.GHTSubTestBegin(ctrlType, "AccessKey:");
				this.TestedControl.CopyBaseAttributes(this.btnAccessKey);
				this.Compare(this.TestedControl.AccessKey, this.btnAccessKey.AccessKey);
			}
			catch (Exception exception6)
			{
				// ProjectData.SetProjectError(exception6);
				Exception exception1 = exception6;
				this.GHTSubTestUnexpectedExceptionCaught(exception1);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			try
			{
				if ((TestedControl is System.Web.UI.WebControls.Image) || 
					(TestedControl is System.Web.UI.WebControls.ImageButton)) {
					this.GHTSubTestBegin(ctrlType, "Enabled:");
					this.GHTSubTestAddResult("Not supported for this control");
				} else {
					this.GHTSubTestBegin(ctrlType, "Enabled:");
					this.TestedControl.CopyBaseAttributes(this.btnEnabled);
					this.Compare(this.TestedControl.Enabled, this.btnEnabled.Enabled);
				}
			}
			catch (Exception exception7)
			{
				// ProjectData.SetProjectError(exception7);
				Exception exception2 = exception7;
				this.GHTSubTestUnexpectedExceptionCaught(exception2);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			try
			{
				if ((TestedControl is System.Web.UI.WebControls.ListBox) ||
					(TestedControl is System.Web.UI.WebControls.DropDownList)) {
					this.GHTSubTestBegin(ctrlType, "ToolTip:");
					this.GHTSubTestAddResult("Not supported for this control");
				} else {
					this.GHTSubTestBegin(ctrlType, "ToolTip:");
					this.TestedControl.CopyBaseAttributes(this.btnToolTip);
					this.Compare(this.TestedControl.ToolTip, this.btnToolTip.ToolTip);
				}
			}
			catch (Exception exception8)
			{
				// ProjectData.SetProjectError(exception8);
				Exception exception3 = exception8;
				this.GHTSubTestUnexpectedExceptionCaught(exception3);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			try
			{
				this.GHTSubTestBegin(ctrlType, "TabIndex:");
				this.TestedControl.CopyBaseAttributes(this.btnTabIndex);
				this.Compare(this.TestedControl.TabIndex, this.btnTabIndex.TabIndex);
			}
			catch (Exception exception9)
			{
				// ProjectData.SetProjectError(exception9);
				Exception exception4 = exception9;
				this.GHTSubTestUnexpectedExceptionCaught(exception4);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
			try
			{
				IEnumerator enumerator1 = null;
				this.GHTSubTestBegin(ctrlType, "Attributes:");
				this.TestedControl.CopyBaseAttributes(this.btnAttributes);
				try
				{
					enumerator1 = this.TestedControl.Attributes.Keys.GetEnumerator();
					while (enumerator1.MoveNext())
					{
						string text1 = (string)(enumerator1.Current);
						this.GHTSubTestAddResult("key = " + text1 + " Value = " + this.TestedControl.Attributes[text1]);
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
			catch (Exception exception10)
			{
				// ProjectData.SetProjectError(exception10);
				Exception exception5 = exception10;
				this.GHTSubTestUnexpectedExceptionCaught(exception5);
				// ProjectData.ClearProjectError();
			}
			this.GHTSubTestEnd();
		}
	}
}
