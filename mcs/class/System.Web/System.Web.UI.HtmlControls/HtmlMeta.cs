//
// System.Web.UI.HtmlControls.HtmlHead
//
// Authors:
// 	Chris Toshok (toshok@ximian.com)
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

#if NET_2_0

using System.ComponentModel;
using System.Collections;
using System.Security.Permissions;
using System.Web.UI.WebControls;
using System.Web.Configuration;

namespace System.Web.UI.HtmlControls
{
	[ControlBuilder (typeof (HtmlEmptyTagControlBuilder))]
	public class HtmlMeta: HtmlControl
	{
		public HtmlMeta () : base ("meta")
		{
		}

		[DefaultValue ("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public virtual string Content {
			get {
				string s = Attributes["content"];
				if (s == null)
					return "";
				return s;
			}
			set {
				if (value == null)
					Attributes.Remove ("content");
				else
					Attributes["content"] = value;
			}
		}

		[DefaultValue ("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public virtual string HttpEquiv {
			get {
				string s = Attributes["http-equiv"];
				if (s == null)
					return "";
				return s;
			}
			set {
				if (value == null)
					Attributes.Remove ("http-equiv");
				else
					Attributes["http-equiv"] = value;
			}
		}

		[DefaultValue ("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public virtual string Name {
			get {
				string s = Attributes["name"];
				if (s == null)
					return "";
				return s;
			}
			set {
				if (value == null)
					Attributes.Remove ("name");
				else
					Attributes["name"] = value;
			}
		}

		[DefaultValue ("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public virtual string Scheme {
			get {
				string s = Attributes["scheme"];
				if (s == null)
					return "";
				return s;
			}
			set {
				if (value == null)
					Attributes.Remove ("scheme");
				else
					Attributes["scheme"] = value;
			}
		}

		protected internal override void Render (HtmlTextWriter writer)
		{
			XhtmlConformanceSection xhtml = WebConfigurationManager.GetSection ("system.web/xhtmlConformance") as XhtmlConformanceSection;

			if (xhtml != null && xhtml.Mode == XhtmlConformanceMode.Legacy)
				base.Render (writer);
			else {
				writer.WriteBeginTag (TagName);
				RenderAttributes (writer);
				writer.Write ("/>");
			}
		}
	}
}

#endif
