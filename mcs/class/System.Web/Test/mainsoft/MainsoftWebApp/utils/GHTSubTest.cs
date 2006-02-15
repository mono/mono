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
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;

namespace GHTWebControls
{
	/// <summary>
	/// Summary description for WebCustomControl1.
	/// </summary>
	[DefaultProperty("Text"),
	ToolboxData("<{0}:GHTSubTest runat=server></{0}:GHTSubTest>")]
	// since we have a bug 4020, For FireFox GH render Panel as DIV instead where .Net render as TABLE,
	// we replaced the use of Panel in Table.	public class GHTSubTest : System.Web.UI.WebControls.Table//.HtmlGenericControl
	public class GHTSubTest : System.Web.UI.WebControls.Panel//System.Web.UI.HtmlControls.HtmlGenericControl //
	{
		string mDescription="";
		[Browsable(true)]
		public string Description
		{
			get 
			{
				return mDescription;
			}
			set
			{
				mDescription = value;
			}
		}
		protected override void Render(HtmlTextWriter writer)
		{
			writer.WriteFullBeginTag("br");
			writer.WriteFullBeginTag("u");
			writer.WriteFullBeginTag("b");
			if (mDescription != "") 
				writer.Write(mDescription);
			else if (this.ID !=null)
				writer.Write(this.ID.ToString());
			else
				writer.Write("GHTSubTest.ID not set. Can not display the sub test id");
			writer.WriteEndTag("b");
			writer.WriteEndTag("u");

			base.Render (writer);
			Literal lbl = new Literal();
			lbl.Text=this.ID;
			this.Controls.Add (lbl);
		}
  	}
}
