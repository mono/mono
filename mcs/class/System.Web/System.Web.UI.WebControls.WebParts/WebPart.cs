//
// System.Web.UI.WebControls.WebParts.Part.cs
//
// Authors:
//   Gaurav Vaish (gaurav[DOT]vaish[AT]gmail[DOT]com)
//   Sanjay Gupta (gsanjay@novell.com)
//
// (C) 2004 Gaurav Vaish (http://www.mastergaurav.org)
// (C) 2004 Novell Inc., (http://www.novell.com)
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

#if NET_2_0

using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI.WebControls;

namespace System.Web.UI.WebControls.WebParts
{
	[DesignerAttribute ("System.Web.UI.Design.WebControls.WebParts.WebPartDesigner, System.Design", 
		"System.ComponentModel.Design.IDesigner")]			
	public class WebPart : Part, IWebPart, IWebActionable
	{
		private bool allowClose      = true;
		private bool allowEdit       = true;
		private bool allowHide       = true;
		private bool allowMinimize   = true;
		private bool allowZoneChange = true;
		private bool allowHelp	     = true;

		private bool isStatic = true;
		private bool isStandalone = true;
		private bool isClosed = true;

		private PartChromeState chromeState = PartChromeState.Normal;
		private PartChromeType chromeType = PartChromeType.Default;
		private WebPartExportMode exportMode = WebPartExportMode.None;
		private WebPartHelpMode   helpMode   = WebPartHelpMode.Navigate;

		private string subtitle;
		private string catalogIconImageUrl;
		private string description;
		private string titleIconImageUrl;
		private string title;
		private string titleUrl;
		private WebPartVerbCollection verbCollection;
		
		protected WebPart()
		{
		}
		
		[WebSysDescriptionAttribute ("Determines Whether the Web Part can be closed."),
		DefaultValueAttribute (true), WebCategoryAttribute ("Behavior of Web Part")]
		//, PersonalizableAttribute 
		public virtual bool AllowClose {
			get { return allowClose; }
			set { allowClose = value; }
		}

		[WebSysDescriptionAttribute ("Determines Whether properties of the Web Part can be changed using the EditorZone."),
		DefaultValueAttribute (true), WebCategoryAttribute ("Behavior of Web Part")]
		//, PersonalizableAttribute 
		public virtual bool AllowEdit {
			get { return allowEdit; }
			set { allowEdit = value; }
		}

		[WebSysDescriptionAttribute ("Determines Whether properties of the Web Part can be changed using the EditorZone."),
		DefaultValueAttribute (true), WebCategoryAttribute ("Behavior of Web Part")]
		//, PersonalizableAttribute 
		public virtual bool AllowHelp {
			get { return AllowHelp; }
			set { allowHelp = value; }
		}

		[WebSysDescriptionAttribute ("Determines Whether the Web Part can be minimized."),
		DefaultValueAttribute (true), WebCategoryAttribute ("Behavior of Web Part")]
		//, PersonalizableAttribute 
		public virtual bool AllowMinimize {
			get { return allowMinimize; }
			set { allowMinimize = value; }
		}

		[WebSysDescriptionAttribute ("Determines Whether the Web Part can be moved to some other zone."),
		DefaultValueAttribute (true), WebCategoryAttribute ("Behavior of Web Part")]
		//, PersonalizableAttribute 
		public virtual bool AllowZoneChange {
			get { return allowZoneChange; }
			set { allowZoneChange = value; }
		}

		[BrowsableAttribute (false), 
		DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		public bool IsClosed {
			get { return isClosed; }
		}

		[BrowsableAttribute (false),
		DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		public bool IsStandalone
		{
			get { return isStandalone; }
		}
		
		//[PersonalizableAttribute ]
		public override PartChromeState ChromeState {
			get { return chromeState; }
			set {
				if(!Enum.IsDefined (typeof (PartChromeState), value))
					throw new ArgumentException ("value");
				chromeState = value;
			}
		}
		
		//[PersonalizableAttribute ]
		public override PartChromeType ChromeType {
			get { return chromeType; }
			set {
				if(!Enum.IsDefined (typeof (PartChromeType), value))
					throw new ArgumentException ("value");
				chromeType = value;
			}
		}

		[BrowsableAttribute (false), 
		DesignerSerializationVisibilityAttribute (System.ComponentModel.DesignerSerializationVisibility.Hidden),
		LocalizableAttribute (true)]			
		string IWebPart.Subtitle { 
			get { return subtitle; }
		}
	
		[DefaultValueAttribute (String.Empty), 
		EditorAttribute ("System.Web.UI.Design.ImageUrlEditor, System.Design", 
				"System.Drawing.Design.UITypeEditor, System.Drawing") , 
		WebCategoryAttribute ("Appearance of the Web Part"),
		WebSysDescriptionAttribute ("Specifies URL of image which is displayed in WebPart's Catalog.")]
		//UrlPropertyAttribute, PersonalizableAttribute
		string IWebPart.CatalogIconImageUrl { 
			get { return catalogIconImageUrl; }
			set { catalogIconImageUrl = value; }
		}

		string IWebPart.Description { 
			get { return description; }
			set { description = value; }
		}

		string IWebPart.Title { 
			get { return title; }
			set { title = value; }
		}

		[DefaultValueAttribute (String.Empty),
		EditorAttribute ("System.Web.UI.Design.ImageUrlEditor, System.Design",
				"System.Drawing.Design.UITypeEditor, System.Drawing"),
		WebCategoryAttribute ("Appearance of the Web Part"),
		WebSysDescriptionAttribute ("Specifies URL of image which is displayed in WebPart's title bar.")]
		//UrlPropertyAttribute, PersonalizableAttribute
		string IWebPart.TitleIconImageUrl
		{
			get { return titleIconImageUrl; }
			set { titleIconImageUrl = value; }
		}

		[DefaultValueAttribute (String.Empty),
		EditorAttribute ("System.Web.UI.Design.UrlEditor, System.Design",
				"System.Drawing.Design.UITypeEditor, System.Drawing"),
		WebCategoryAttribute ("Behavior of the Web Part"),
		WebSysDescriptionAttribute ("Specifies URL of page, containing additional information about this WebPart.")]
		//UrlPropertyAttribute, PersonalizableAttribute
		string IWebPart.TitleUrl { 
			get { return titleUrl; }
			set { titleUrl = value; }
		}

		[BrowsableAttribute (false),
		DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		WebPartVerbCollection IWebActionable.Verbs {
			get {
				if (verbCollection == null) {
					verbCollection = new WebPartVerbCollection ();
				}
				return verbCollection;
			}
		}
	}
}
#endif
