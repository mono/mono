//
// System.Web.UI.WebControls.TableHeaderCell.cs
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.ComponentModel;
using System.Security.Permissions;
using System.Text;

namespace System.Web.UI.WebControls {

	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class TableHeaderCell : TableCell {

		public TableHeaderCell ()
			: base (HtmlTextWriterTag.Th)
		{
		}

#if NET_2_0
		[DefaultValue ("")]
		public virtual string AbbreviatedText {
			get {
				object o = ViewState ["AbbreviatedText"];
				return (o == null) ? String.Empty : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("AbbreviatedText");
				else
					ViewState ["AbbreviatedText"] = value;
			}
		}

		[DefaultValue (null)]
		[TypeConverter (typeof (StringArrayConverter))]
		public virtual string[] CategoryText {
			get {
				object o = ViewState ["CategoryText"];
				return (o == null) ? new string [0] : (string[]) o;
			}
			set {
				ViewState ["CategoryText"] = value;
			}
		}

		[DefaultValue (TableHeaderScope.NotSet)]
		public virtual TableHeaderScope Scope {
			get {
				object o = ViewState ["Scope"];
				return (o == null) ? TableHeaderScope.NotSet : (TableHeaderScope) o;
			}
			set {
				ViewState ["Scope"] = (int) value;
			}
		}

		protected override void AddAttributesToRender (HtmlTextWriter writer)
		{
			base.AddAttributesToRender (writer);
			if (writer != null) {
				object o = ViewState ["AbbreviatedText"];
				if (o != null)
					writer.AddAttribute (HtmlTextWriterAttribute.Abbr, (string) o);

				switch (Scope) {
				case TableHeaderScope.Column:
					writer.AddAttribute (HtmlTextWriterAttribute.Scope, "column", false);
					break;
				case TableHeaderScope.Row:
					writer.AddAttribute (HtmlTextWriterAttribute.Scope, "row", false);
					break;
				}

				string[] cats = CategoryText;
				if (cats.Length == 1) {
					writer.AddAttribute (HtmlTextWriterAttribute.Axis, cats [0]);
				} else if (cats.Length > 1) {
					StringBuilder sb = new StringBuilder ();
					for (int i=0; i < cats.Length - 1; i++) {
						sb.Append (cats [i]);
						sb.Append (",");
					}
					sb.Append (cats [cats.Length - 1]);
					writer.AddAttribute (HtmlTextWriterAttribute.Axis, sb.ToString ());
				}
			}
		}
#endif
	}
}
