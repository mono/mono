//
// System.Web.UI.WebControls.DataControlFieldHeaderCell.cs
//
// Authors:
//  Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2005-2010 Novell, Inc. (http://www.novell.com)
//

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
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public class DataControlFieldHeaderCell : DataControlFieldCell
	{
		TableHeaderScope scope;
		
		public DataControlFieldHeaderCell (DataControlField containingField): base (HtmlTextWriterTag.Th, containingField)
		{
		}
		
		internal DataControlFieldHeaderCell (DataControlField containerField, TableHeaderScope scope): this (containerField)
		{
			this.scope = scope;
		}
		
		public virtual TableHeaderScope Scope {
			get {
				object ob = ViewState ["Scope"];
				if (ob != null)
					return (TableHeaderScope) ob;
				else
					return TableHeaderScope.NotSet;
			}
			set { ViewState ["Scope"] = value; }
		}
		
		public virtual string AbbreviatedText {
			get {
				object ob = ViewState ["AbbreviatedText"];
				if (ob != null)
					return (string) ob;
				else
					return String.Empty;
			}
			set { ViewState ["AbbreviatedText"] = value;}
		}
		
		protected override void AddAttributesToRender (HtmlTextWriter writer)
		{
			base.AddAttributesToRender (writer);
			switch (scope) {
				case TableHeaderScope.Column:
					writer.AddAttribute (HtmlTextWriterAttribute.Scope, "col", false);
					break;
				case TableHeaderScope.Row:
					writer.AddAttribute (HtmlTextWriterAttribute.Scope, "row", false);
					break;
			}
			if (AbbreviatedText.Length > 0)
				writer.AddAttribute (HtmlTextWriterAttribute.Abbr, AbbreviatedText);
		}
	}
}

