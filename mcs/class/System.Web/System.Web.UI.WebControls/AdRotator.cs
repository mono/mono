// 
// System.Web.UI.WebControls.AdRotator
//
// Author:
//        Ben Maurer <bmaurer@novell.com>
//
// Copyright (C) 2005-2010 Novell, Inc (http://www.novell.com)
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

using System.Xml;
using System.Collections;
using System.ComponentModel;
using System.Security.Permissions;
using System.Web.Util;

namespace System.Web.UI.WebControls
{
	// CAS
	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[DefaultEvent("AdCreated")]
	[DefaultProperty("AdvertisementFile")]
	[Designer("System.Web.UI.Design.WebControls.AdRotatorDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[ToolboxData("<{0}:AdRotator runat=\"server\"></{0}:AdRotator>")]
	public class AdRotator : DataBoundControl
	{
		AdCreatedEventArgs createdargs;
		ArrayList ads = new ArrayList ();
		string ad_file = String.Empty;

		protected internal override void OnInit (EventArgs e)
		{
			base.OnInit(e);
		}

		protected internal override void OnPreRender (EventArgs eee)
		{
			Hashtable ht = null;
			
			if (!String.IsNullOrEmpty (ad_file)) {
				ReadAdsFromFile (GetPhysicalFilePath (ad_file));
				ht = ChooseAd ();
			}

		 	AdCreatedEventArgs e = new AdCreatedEventArgs (ht);
			OnAdCreated (e);
			createdargs = e;
			
		}

		[MonoTODO ("Not implemented")]
		protected internal override void PerformDataBinding (IEnumerable data)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Not implemented")]
		protected override void PerformSelect ()
		{
			throw new NotImplementedException ();
		}

		protected internal override void Render (HtmlTextWriter w)
		{
			AdCreatedEventArgs e = createdargs;

			base.AddAttributesToRender (w);

			if (e.NavigateUrl != null && e.NavigateUrl.Length > 0)
				w.AddAttribute (HtmlTextWriterAttribute.Href, ResolveAdUrl (e.NavigateUrl));
			if (Target != null && Target.Length > 0)
				w.AddAttribute (HtmlTextWriterAttribute.Target, Target);
			
			w.RenderBeginTag (HtmlTextWriterTag.A);

			if (e.ImageUrl != null && e.ImageUrl.Length > 0)
				w.AddAttribute (HtmlTextWriterAttribute.Src, ResolveAdUrl (e.ImageUrl));

			w.AddAttribute (HtmlTextWriterAttribute.Alt, e.AlternateText == null ? String.Empty : e.AlternateText);
			w.AddAttribute (HtmlTextWriterAttribute.Border, "0", false);
			w.RenderBeginTag (HtmlTextWriterTag.Img);
			w.RenderEndTag (); // img
			w.RenderEndTag (); // a
		}

		string ResolveAdUrl (string url)
		{
			string path = url;

			if (AdvertisementFile != null && AdvertisementFile.Length > 0 && path [0] != '/' && path [0] != '~')
				try {
					new Uri (path);
				}
				catch {
					return UrlUtils.Combine (UrlUtils.GetDirectory (ResolveUrl (AdvertisementFile)), path);
				}
			
			return ResolveUrl (path);
		}

		//
		// We take all the ads in the ad file and add up their
		// impression weight. We then take a random number [0,
		// TotalImpressions). We then choose the ad that is
		// that number or less impressions for the beginning
		// of the file. This lets us respect the weights
		// given.
		//
		Hashtable ChooseAd ()
		{
			// cache for performance
			string KeywordFilter = this.KeywordFilter;
			
			int total_imp = 0;
			int cur_imp = 0;
			bool keywordFilterEmpty = KeywordFilter.Length == 0;
			
			foreach (Hashtable a in ads) {
				if (keywordFilterEmpty || KeywordFilter == (string) a ["Keyword"])
					total_imp += a ["Impressions"] != null ? int.Parse ((string) a ["Impressions"]) : 1;
			}

			int r = new Random ().Next (total_imp);

			foreach (Hashtable a in ads) {
				if (!keywordFilterEmpty && KeywordFilter != (string) a ["Keyword"])
					continue;
				cur_imp += a ["Impressions"] != null ? int.Parse ((string) a ["Impressions"]) : 1;
				
				if (cur_imp > r)
					return a;
			}

			if (total_imp != 0)
				throw new Exception ("I should only get here if no ads matched");
			
			return null;
		}
		
		void ReadAdsFromFile (string s)
		{
			XmlDocument d = new XmlDocument ();
			try {
				d.Load (s);
			} catch (Exception e) {
				throw new HttpException ("AdRotator could not parse the xml file", e);
			}
			
			ads.Clear ();
			
			foreach (XmlNode n in d.DocumentElement.ChildNodes) {

				Hashtable ad = new Hashtable ();
				
				foreach (XmlNode nn in n.ChildNodes)
					ad.Add (nn.Name, nn.InnerText);
				
				ads.Add (ad);
			}
		}


		[UrlProperty]
		[Bindable(true)]
		[DefaultValue("")]
		[Editor("System.Web.UI.Design.XmlUrlEditor, " + Consts.AssemblySystem_Design, typeof(System.Drawing.Design.UITypeEditor))]
		[WebSysDescription("")]
		[WebCategory("Behavior")]
		public string AdvertisementFile {
			get {
				return ad_file;
			}
			set {
				ad_file = value;
			}
			
		}

		[DefaultValue ("AlternateText")]
		[WebSysDescriptionAttribute ("")]
		[WebCategoryAttribute ("Behavior")]
		[MonoTODO ("Not implemented")]
		public string AlternateTextField 
		{
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override FontInfo Font {
			get {
				return base.Font;
			}
		}

		[DefaultValue ("ImageUrl")]
		[MonoTODO ("Not implemented")]
		[WebSysDescriptionAttribute ("")]
		[WebCategoryAttribute ("Behavior")]
		public string ImageUrlField 
		{
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[Bindable(true)]
		[DefaultValue("")]
		[WebSysDescription("")]
		[WebCategory("Behavior")]
		public string KeywordFilter {
			get {
				return ViewState.GetString ("KeywordFilter", String.Empty);
			}
			set {
				ViewState ["KeywordFilter"] = value;
			}
			
		}

		[DefaultValue ("NavigateUrl")]
		[MonoTODO ("Not implemented")]
		[WebSysDescriptionAttribute ("")]
		[WebCategoryAttribute ("Behavior")]
		public string NavigateUrlField 
		{
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		[Bindable(true)]
		[DefaultValue("_top")]
		[TypeConverter(typeof(System.Web.UI.WebControls.TargetConverter))]
		[WebSysDescription("")]
		[WebCategory("Behavior")]
		public string Target {
			get {
				return ViewState.GetString ("Target", "_top");
			}
			set {
				ViewState ["Target"] = value;
			}
			
		}

		/* all these are listed in corcompare */
		public override string UniqueID
		{
			get {
				return base.UniqueID;
			}
		}

		protected override HtmlTextWriterTag TagKey 
		{
			get {
				return base.TagKey;
			}
		}
	
		protected virtual void OnAdCreated (AdCreatedEventArgs e)
		{
			AdCreatedEventHandler h = (AdCreatedEventHandler) Events [AdCreatedEvent];
			if (h != null)
				h (this, e);
		}

		static readonly object AdCreatedEvent = new object ();

		[WebSysDescription("")]
		[WebCategory("Action")]
		public event AdCreatedEventHandler AdCreated {
			add { Events.AddHandler (AdCreatedEvent, value); }
			remove { Events.RemoveHandler (AdCreatedEvent, value); }
		}
	}
}
