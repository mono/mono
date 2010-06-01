//
// System.Web.UI.HtmlControls.HtmlHead
//
// Authors:
// 	Lluis Sanchez Gual (lluis@novell.com)
//
// Copyright (C) 2004-2010 Novell, Inc (http://www.novell.com)
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
using System.Collections;
using System.Security.Permissions;
using System.Web.UI.WebControls;

namespace System.Web.UI.HtmlControls
{
	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[ControlBuilder (typeof(HtmlHeadBuilder))]
	public sealed class HtmlHead: HtmlGenericControl, IParserAccessor
	{
#if NET_4_0
		string descriptionText;
		string keywordsText;
		HtmlMeta descriptionMeta;
		HtmlMeta keywordsMeta;
#endif
		string titleText;
		HtmlTitle title;
		//Hashtable metadata;
		StyleSheetBag styleSheet;
		
		public HtmlHead(): base("head") {}

		public HtmlHead (string tag) : base (tag)
		{
		}
		
		protected internal override void OnInit (EventArgs e)
		{
			base.OnInit (e);
			Page page = Page;
			
			if (page == null)
				throw new HttpException ("The <head runat=\"server\"> control requires a page.");
			
			//You can only have one <head runat="server"> control on a page.
			if(page.Header != null)
				throw new HttpException ("You can only have one <head runat=\"server\"> control on a page.");
			page.SetHeader (this);
		}
		
		protected internal override void RenderChildren (HtmlTextWriter writer)
		{
			EnsureTitleControl ();

			base.RenderChildren (writer);
#if NET_4_0
			if (descriptionMeta == null && descriptionText != null) {
				writer.AddAttribute ("name", "description");
				writer.AddAttribute ("content", HttpUtility.HtmlAttributeEncode (descriptionText));
				writer.RenderBeginTag (HtmlTextWriterTag.Meta);
				writer.RenderEndTag ();
			}

			if (keywordsMeta == null && keywordsText != null) {
				writer.AddAttribute ("name", "keywords");
				writer.AddAttribute ("content", HttpUtility.HtmlAttributeEncode (keywordsText));
				writer.RenderBeginTag (HtmlTextWriterTag.Meta);
				writer.RenderEndTag ();
			}
#endif
			if (styleSheet != null)
				styleSheet.Render (writer);
		}
		
		protected internal override void AddedControl (Control control, int index)
		{
			//You can only have one <title> element within the <head> element.
			HtmlTitle t = control as HtmlTitle;
			if (t != null) {
				if (title != null)
					throw new HttpException ("You can only have one <title> element within the <head> element.");
				title = t;
			}

#if NET_4_0
			HtmlMeta meta = control as HtmlMeta;
			if (meta != null) {
				if (String.Compare ("keywords", meta.Name, StringComparison.OrdinalIgnoreCase) == 0)
					keywordsMeta = meta;
				else if (String.Compare ("description", meta.Name, StringComparison.OrdinalIgnoreCase) == 0)
					descriptionMeta = meta;
			}
#endif
			base.AddedControl (control, index);
		}

		protected internal override void RemovedControl (Control control)
		{
			if (title == control)
				title = null;

#if NET_4_0
			if (keywordsMeta == control)
				keywordsMeta = null;
			else if (descriptionMeta == control)
				descriptionMeta = null;
#endif
			base.RemovedControl (control);
		}
		
		void EnsureTitleControl () {
			if (title != null)
				return;

			HtmlTitle t = new HtmlTitle ();
			t.Text = titleText;
			Controls.Add (t);
		}
#if NET_4_0
		public string Description {
			get {
				if (descriptionMeta != null)
					return descriptionMeta.Content;
				return descriptionText;
			}
			
			set {
				if (descriptionMeta != null)
					descriptionMeta.Content = value;
				else
					descriptionText = value;
			}
		}

		public string Keywords {
			get {
				if (keywordsMeta != null)
					return keywordsMeta.Content;
				return keywordsText;
			}
			
			set {
				if (keywordsMeta != null)
					keywordsMeta.Content = value;
				else
					keywordsText = value;
			}
		}
#endif
		
		public IStyleSheet StyleSheet {
			get {
				if (styleSheet == null) styleSheet = new StyleSheetBag ();
				return styleSheet;
			}
		}
		
		public string Title {
			get {
				if (title != null)
					return title.Text;
				else
					return titleText;
			}
			set {
				if (title != null)
					title.Text = value;
				else
					titleText = value;
			}
		}
	}
	
	internal class StyleSheetBag: IStyleSheet
	{
		ArrayList entries = new ArrayList ();
		
		internal class StyleEntry
		{
			public Style Style;
			public string Selection;
			public IUrlResolutionService UrlResolver;
		}
		
		public StyleSheetBag ()
		{
		}
		
		public void CreateStyleRule (Style style, IUrlResolutionService urlResolver, string selection)
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
			CreateStyleRule (style, urlResolver, "." + name);
		}
		
		public void Render (HtmlTextWriter writer)
		{
			writer.AddAttribute ("type", "text/css", false);
			writer.RenderBeginTag (HtmlTextWriterTag.Style);

			foreach (StyleEntry entry in entries) {
				CssStyleCollection sts = entry.Style.GetStyleAttributes (entry.UrlResolver);
				writer.Write ("\n" + entry.Selection + " {" + sts.Value + "}");
			}

			writer.RenderEndTag ();
		}
	}
}

