//
// System.Web.UI.WebControls.AdRotator.cs
//
// Authors:
//   Gaurav Vaish (gvaish@iitk.ac.in)
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (c) 2002 Ximian, Inc. (http://www.ximian.com)
// (C) Gaurav Vaish (2002)
// (C) 2003 Andreas Nahr
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
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Web;
using System.Web.Caching;
using System.Web.UI;
using System.Xml;
using System.Web.Util;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace System.Web.UI.WebControls
{
	[DefaultEvent("AdCreated")]
	[DefaultProperty("AdvertisementFile")]
	[Designer ("System.Web.UI.Design.WebControls.AdRotatorDesigner, " + Consts.AssemblySystem_Design, typeof (IDesigner))]
	[ToolboxData("<{0}:AdRotator runat=\"server\" Height=\"60px\" "
	+ "Width=\"468\"></{0}:AdRotator>")]
	public class AdRotator: WebControl
	{
		string advertisementFile;
		static readonly object AdCreatedEvent = new object();

		// Will be set values during (On)PreRender-ing
		string alternateText;
		string imageUrl;
		string navigateUrl;
		string fileDirectory;
		Random random;

		public AdRotator ()
		{
			advertisementFile = "";
			fileDirectory     = null;
		}

		AdRecord[] LoadAdFile (string file)
		{
			Stream fStream;
			try {
				fStream = new FileStream (file, FileMode.Open, FileAccess.Read, FileShare.Read);
			} catch (Exception e) {
				throw new HttpException("AdRotator: Unable to open file", e);
			}

			ArrayList list = new ArrayList ();
			try {
				IDictionary hybridDict = null;
				XmlDocument document = new XmlDocument ();
				document.Load (fStream);

				XmlElement docElem = document.DocumentElement;

				if (docElem == null)
					throw new HttpException ("No advertisements found");

				if (docElem.LocalName != "Advertisements")
					throw new HttpException ("No advertisements found: invalid document element");

				XmlNode node = docElem.FirstChild;
				while (node != null) {
					if (node.LocalName == "Ad") {
						XmlNode innerNode = node.FirstChild;
						while (innerNode != null) {
							if (node.NodeType == XmlNodeType.Element) {
								if (hybridDict == null)
									hybridDict = new HybridDictionary ();

								hybridDict.Add (innerNode.LocalName, innerNode.InnerText);
							}
							innerNode = innerNode.NextSibling;
						}

						if (hybridDict != null) {
							list.Add (hybridDict);
							hybridDict = null;
						}
					}
					node = node.NextSibling;
				}

			} catch(Exception e) {
				throw new HttpException("Parse error:" + file, e);
			} finally {
				if (fStream != null)
					fStream.Close();
			}

			if (list.Count == 0)
				throw new HttpException ("No advertisements found");

			AdRecord [] adsArray = new AdRecord [list.Count];
			int count = list.Count;
			for (int i = 0; i < count; i++)
				adsArray [i] = new AdRecord ((IDictionary) list [i]);

			return adsArray;
		}

		AdRecord [] GetData (string file)
		{
			string physPath = MapPathSecure (file);
			string AdKey = "AdRotatorCache: " + physPath;
			fileDirectory = UrlUtils.GetDirectory (UrlUtils.Combine (TemplateSourceDirectory, file));
			Cache cache = HttpRuntime.Cache;
			AdRecord[] records = (AdRecord[]) cache [AdKey];
			if (records == null) {
				records = LoadAdFile (physPath);
				cache.Insert (AdKey, records, new CacheDependency (physPath));
			}

			return records;
		}

		IDictionary SelectAd ()
		{
			AdRecord[] records = GetData (AdvertisementFile);
			if (records == null || records.Length ==0)
				return null;

			int impressions = 0;
			int rlength = records.Length;
			for (int i=0 ; i < rlength; i++) {
				if (IsAdMatching (records [i]))
					impressions += records [i].Hits;
			}

			if (impressions == 0)
				return null;

			if (random == null)
				random = new Random ();

			int rnd = random.Next (impressions) + 1;
			int counter = 0;
			int index = 0;
			for (int i = 0; i < rlength; i++) {
				if(IsAdMatching(records[i])) {
					if (rnd <= (counter + records [i].Hits)) {
						index = i;
						break;
					}
					counter += records [i].Hits;
				}
			}

			return records [index].Properties;
		}

		private bool IsAdMatching (AdRecord currAd)
		{
			if (KeywordFilter != String.Empty)
				return (0 == String.Compare (currAd.Keyword, KeywordFilter, true));

			return true;
		}

		private string ResolveAdUrl (string relativeUrl)
		{
			if (relativeUrl.Length==0 || !UrlUtils.IsRelativeUrl (relativeUrl))
				return relativeUrl;

			string fullUrl;
			if (fileDirectory != null)
				fullUrl = fileDirectory;
			else
				fullUrl = TemplateSourceDirectory;

			if (fullUrl.Length == 0)
				return relativeUrl;

			return UrlUtils.Combine (fullUrl, relativeUrl);
		}

		[WebCategory("Action")]
		[WebSysDescription("AdRotator_OnAdCreated")]
		public event AdCreatedEventHandler AdCreated {
			add { Events.AddHandler (AdCreatedEvent, value); }
			remove { Events.RemoveHandler (AdCreatedEvent, value); }
		}

		[Bindable(true)]
		[DefaultValue("")]
		[Editor ("System.Web.UI.Design.XmlUrlEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		[WebCategory("Behavior")]
		[WebSysDescription("AdRotator_AdvertisementFile")]
		public string AdvertisementFile {
			get { return ((advertisementFile != null) ? advertisementFile : ""); }
			set { advertisementFile = value; }
		}

		[Browsable (false), EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override FontInfo Font {
			get { return base.Font; }
		}

		[Bindable(true)]
		[DefaultValue("")]
		[WebCategory("Behavior")]
		[WebSysDescription("AdRotator_KeywordFilter")]
		public string KeywordFilter {
			get {
				object o = ViewState ["KeywordFilter"];
				if (o != null)
					return (string) o;

				return String.Empty;
			}
			set {
				if(value != null)
					ViewState ["KeywordFilter"] = value.Trim ();
			}
		}

		[Bindable(true)]
		[DefaultValue("")]
		[TypeConverter(typeof(TargetConverter))]
		[WebCategory("Behavior")]
		[WebSysDescription("AdRotator_Target")]
		public string Target {
			get {
				object o = ViewState ["Target"];
				if (o != null)
					return (string) o;

				return "_top";
			}
			set {
				ViewState["Target"] = value;
			}
		}

		protected override ControlCollection CreateControlCollection ()
		{
			return new EmptyControlCollection (this);
		}

		protected virtual void OnAdCreated (AdCreatedEventArgs e)
		{
			if (Events == null)
				return;

			AdCreatedEventHandler aceh = (AdCreatedEventHandler) Events [AdCreatedEvent];
			if (aceh != null)
				aceh (this, e);
		}

		protected override void OnPreRender (EventArgs e)
		{
			if(AdvertisementFile == String.Empty)
				return;

			AdCreatedEventArgs acea = new AdCreatedEventArgs (SelectAd ());
			OnAdCreated (acea);
			imageUrl = acea.ImageUrl;
			navigateUrl = acea.NavigateUrl;
			alternateText = acea.AlternateText;
		}

		protected override void Render (HtmlTextWriter writer)
		{
			HyperLink hLink = new HyperLink ();
			Image adImage = new Image();
			foreach (string current in Attributes.Keys)
				hLink.Attributes [current] = Attributes [current];

			if (ID != null && ID.Length > 0)
				hLink.ID = ID;

			hLink.Target = Target;
			hLink.AccessKey = AccessKey;
			hLink.Enabled  = Enabled;
			hLink.TabIndex = TabIndex;
			if (navigateUrl != null && navigateUrl.Length != 0)
				hLink.NavigateUrl = ResolveAdUrl (navigateUrl);

			hLink.RenderBeginTag (writer);
			if (ControlStyleCreated)
				adImage.ApplyStyle(ControlStyle);

			if(imageUrl!=null && imageUrl.Length > 0)
				adImage.ImageUrl = ResolveAdUrl (imageUrl);

			adImage.AlternateText = alternateText;
			adImage.ToolTip = ToolTip;
			adImage.RenderControl (writer);
			hLink.RenderEndTag (writer);
		}

		class AdRecord
		{
			public IDictionary Properties;
			public int Hits; // or impressions or clicks
			public string Keyword;

			public AdRecord (IDictionary adProps)
			{
				this.Properties = adProps;
				Keyword = Properties ["Keyword"] as string;
				if (Keyword == null)
					Keyword = "";

				string imp = Properties ["Impressions"] as string;
				Hits = 1;
				if (imp != null) {
					try {
						Hits = Int32.Parse (imp);
					} catch {
					}
				}
			}
		}
	}
}

