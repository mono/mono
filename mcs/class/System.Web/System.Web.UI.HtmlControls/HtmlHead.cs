//
// System.Web.UI.HtmlControls.HtmlHead
//
// Authors:
// 	Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2004 Novell, Inc.

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

using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;

namespace System.Web.UI.HtmlControls
{
	[ControlBuilder (typeof(HtmlHeadBuilder))]
	public class HtmlHead: HtmlContainerControl, IPageHeader
	{
		HtmlTitle title;
		Hashtable metadata;
		ArrayList styleSheets;
		StyleSheetBag styleSheet;
		
		public HtmlHead(): base("head") {}

		protected override void OnInit (EventArgs e)
		{
			Page.SetHeader (this);
		}
		
		protected override void RenderChildren (HtmlTextWriter writer)
		{
			base.RenderChildren (writer);
			if (metadata != null) {
				foreach (DictionaryEntry entry in metadata) {
					writer.AddAttribute ("name", entry.Key.ToString ());
					writer.AddAttribute ("content", entry.Value.ToString ());
					writer.RenderBeginTag (HtmlTextWriterTag.Meta);
					writer.RenderEndTag ();
				}
			}
			
			if (styleSheet != null)
				styleSheet.Render (writer);
		}
		
		protected override void AddParsedSubObject (object ob)
		{
			if (ob is HtmlTitle)
				title = (HtmlTitle) ob;
			
			base.AddParsedSubObject (ob);
		}
		
		protected internal override void AddedControl (Control control, int index)
		{
			base.AddedControl (control, index);
		}
		
		IList IPageHeader.LinkedStyleSheets {
			get {
				if (styleSheets == null) styleSheets = new ArrayList ();
				return styleSheets;
			}
		} 
		
		IDictionary IPageHeader.Metadata {
			get {
				if (metadata == null) metadata = new Hashtable ();
				return metadata;
			}
		}
		
		IStyleSheet IPageHeader.StyleSheet {
			get {
				if (styleSheet == null) styleSheet = new StyleSheetBag (Page);
				return styleSheet;
			}
		}
		
		string IPageHeader.Title {
			get { return title.Text; }
			set { title.Text = value; }
		}
	}
	
	internal class StyleSheetBag: IStyleSheet
	{
		ArrayList entries = new ArrayList ();
		Page page;
		
		internal class StyleEntry
		{
			public Style Style;
			public string Selection;
			public IUrlResolutionService UrlResolver;
		}
		
		public StyleSheetBag (Page page)
		{
			this.page = page;
		}
		
		public void CreateStyleRule (Style style, string selection, IUrlResolutionService urlResolver)
		{
			StyleEntry entry = new StyleEntry ();
			entry.Style = style;
			entry.UrlResolver = urlResolver;
			entry.Selection = selection;
			entries.Add (entry);
		}
		
		public void RegisterStyle (Style style, IUrlResolutionService urlResolver)
		{
			for (int n=0; n<entries.Count; n++) {
				if (((StyleEntry)entries[n]).Style == style)
					return;
			}
			
			string name = "aspnet_" + entries.Count;
			style.SetRegisteredCssClass (name);
			CreateStyleRule (style, "." + name, urlResolver);
		}
		
		public void Render (HtmlTextWriter writer)
		{
			writer.AddAttribute ("type", "text/css");
			writer.RenderBeginTag (HtmlTextWriterTag.Style);

			foreach (StyleEntry entry in entries) {
				CssStyleCollection sts = entry.Style.FillStyleAttributes (entry.UrlResolver);
				writer.Write ("\n" + entry.Selection + " {" + sts.BagToString () + "}");
			}

			writer.RenderEndTag ();
		}
	}
}

#endif
