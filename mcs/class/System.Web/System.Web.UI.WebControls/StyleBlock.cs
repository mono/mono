//
// Authors:
//	Marek Habersack <grendel@twistedcode.net>
//
// (C) 2010 Novell, Inc (http://novell.com)
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
using System;
using System.Collections.Generic;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	sealed class StyleBlock : Control
	{
		List <NamedCssStyleCollection> cssStyles;
		Dictionary <string, NamedCssStyleCollection> cssStyleIndex;
		string stylePrefix;
		
		List <NamedCssStyleCollection> CssStyles {
			get {
				if (cssStyles == null) {
					cssStyles = new List <NamedCssStyleCollection> ();
					cssStyleIndex = new Dictionary <string, NamedCssStyleCollection> (StringComparer.Ordinal);
				}
				
				return cssStyles;
			}
		}
		
		public StyleBlock (string stylePrefix)
		{
			if (String.IsNullOrEmpty (stylePrefix))
				throw new ArgumentNullException ("stylePrefix");

			this.stylePrefix = stylePrefix;
		}

		public NamedCssStyleCollection RegisterStyle (string name = null)
		{
			if (name == null)
				name = String.Empty;

			return GetStyle (name);
		}
		
		public NamedCssStyleCollection RegisterStyle (Style style, string name = null)
		{
			if (style == null)
				throw new ArgumentNullException ("style");
			
			if (name == null)
				name = String.Empty;

			NamedCssStyleCollection cssStyle = GetStyle (name);
			cssStyle.CopyFrom (style.GetStyleAttributes (null));

			return cssStyle;
		}

		public NamedCssStyleCollection RegisterStyle (HtmlTextWriterStyle key, string value, string styleName = null)
		{
			if (styleName == null)
				styleName = String.Empty;

			NamedCssStyleCollection style = GetStyle (styleName);
			style.Add (key, value);

			return style;
		}
		
		NamedCssStyleCollection GetStyle (string name)
		{
			List <NamedCssStyleCollection> cssStyles = CssStyles;
			NamedCssStyleCollection style;

			if (!cssStyleIndex.TryGetValue (name, out style)) {
				style = new NamedCssStyleCollection (name);
				cssStyleIndex.Add (name, style);
				cssStyles.Add (style);
			}

			if (style == null)
				throw new InvalidOperationException (String.Format ("Internal error. Stylesheet for style {0} is null.", name));
			
			return style;
		}
		
		protected internal override void Render (HtmlTextWriter writer)
		{
			if (cssStyles == null || cssStyles.Count == 0)
				return;

			writer.AddAttribute (HtmlTextWriterAttribute.Type, "text/css");
			writer.RenderBeginTag (HtmlTextWriterTag.Style);
			writer.WriteLine ("/* <![CDATA[ */");

			string name, value;
			foreach (var css in cssStyles) {
				value = css.Collection.Value;
				if (String.IsNullOrEmpty (value))
					continue;
				
				name = css.Name;
				if (name != String.Empty)
					name = name + " ";
				
				writer.WriteLine ("#{0} {1}{{ {2} }}", stylePrefix, name, value);
			}
			
			writer.WriteLine ("/* ]]> */");
			writer.RenderEndTag (); // </style>
		}
	}
}
