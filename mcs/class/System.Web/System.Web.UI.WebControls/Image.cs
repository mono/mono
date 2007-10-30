//
// System.Web.UI.WebControls.Image.cs
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

namespace System.Web.UI.WebControls {

	// Note: this control can live inside or outside a <form> element

	// CAS
	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[DefaultProperty ("ImageUrl")]
#if NET_2_0
	[Designer ("System.Web.UI.Design.WebControls.PreviewControlDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
#endif
	public class Image : WebControl {

		public Image ()
			: base (HtmlTextWriterTag.Img)
		{
		}


		[Bindable (true)]
		[DefaultValue ("")]
#if NET_2_0
		[Localizable (true)]
#endif
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public virtual string AlternateText {
			get {
				string s = (string) ViewState ["AlternateText"];
				return (s == null) ? String.Empty : s;
			}
			set {
				if (value == null)
					ViewState.Remove ("AlternateText");
				else
					ViewState ["AlternateText"] = value;
			}
		}

		// not applicable to Image
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override bool Enabled {
			get { return base.Enabled; }
			set {base.Enabled = value; }
		}

		// not applicable to Image
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override FontInfo Font {
			get { return base.Font; }
		}

#if ONLY_1_1
		[Bindable (true)]
#endif
		[DefaultValue (ImageAlign.NotSet)]
		[WebSysDescription ("")]
		[WebCategory ("Layout")]
		public virtual ImageAlign ImageAlign {
			get {
				object o = ViewState ["ImageAlign"];
				return (o == null) ? ImageAlign.NotSet : (ImageAlign) o;
			}
			set {
				// avoid reflection
				if ((value < ImageAlign.NotSet) || (value > ImageAlign.TextTop)) {
					// invalid ImageAlign (note: 2.0 beta2 documents ArgumentException)
					throw new ArgumentOutOfRangeException (Locale.GetText ("Invalid ImageAlign value."));
				}
				ViewState ["ImageAlign"] = value;
			}
		}

		[Bindable (true)]
		[DefaultValue ("")]
#if NET_2_0
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[UrlProperty]
#else
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
#endif
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public virtual string ImageUrl {
			get {
				string s = (string) ViewState ["ImageUrl"];
				return (s == null) ? String.Empty : s;
			}
			set {
				if (value == null)
					ViewState.Remove ("ImageUrl");
				else
					ViewState ["ImageUrl"] = value;
			}
		}

		// this was added in Fx 1.1 SP1
		[DefaultValue ("")]
#if NET_2_0
		[Editor ("System.Web.UI.Design.UrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[UrlProperty]
#endif
		[WebSysDescription ("")]
		[WebCategory ("Accessibility")]
		public virtual string DescriptionUrl {
			get {
				string s = (string) ViewState ["DescriptionUrl"];
				return (s == null) ? String.Empty : s;
			}
			set {
				if (value == null)
					ViewState.Remove ("DescriptionUrl");
				else
					ViewState ["DescriptionUrl"] = value;
			}
		}

#if NET_2_0
		[DefaultValue (false)]
		[WebSysDescription ("")]
		[WebCategory ("Accessibility")]
		public virtual bool GenerateEmptyAlternateText {
			get {
				object o = ViewState ["GenerateEmptyAlternateText"];
				return (o == null) ? false : (bool) o;
			}
			set {
				ViewState ["GenerateEmptyAlternateText"] = value;
			}
		}
#endif


		protected override void AddAttributesToRender (HtmlTextWriter writer)
		{
			base.AddAttributesToRender (writer);
#if NET_2_0
			// src is always present, even if empty, in 2.0
			writer.AddAttribute (HtmlTextWriterAttribute.Src, ResolveClientUrl (ImageUrl));
			string s = AlternateText;
			if ((s.Length > 0) || GenerateEmptyAlternateText)
				writer.AddAttribute (HtmlTextWriterAttribute.Alt, s);
			s = DescriptionUrl;
			if (s.Length > 0)
				writer.AddAttribute (HtmlTextWriterAttribute.Longdesc, ResolveClientUrl (s));
#else
			string s = ImageUrl;
			if (s.Length > 0)
				writer.AddAttribute (HtmlTextWriterAttribute.Src, ResolveClientUrl (s));
			// alt is always present, even if empty, in 1.x
			writer.AddAttribute (HtmlTextWriterAttribute.Alt, AlternateText);
			// added in Fx 1.1 SP1 but the HtmlTextWriterAttribute wasn't
			s = DescriptionUrl;
			if (s.Length > 0)
				writer.AddAttribute ("longdesc", s);
#endif
			switch (ImageAlign) {
			case ImageAlign.Left:
				writer.AddAttribute (HtmlTextWriterAttribute.Align, "left", false);
				break;
			case ImageAlign.Right:
				writer.AddAttribute (HtmlTextWriterAttribute.Align, "right", false);
				break;
			case ImageAlign.Baseline:
				writer.AddAttribute (HtmlTextWriterAttribute.Align, "baseline", false);
				break;
			case ImageAlign.Top:
				writer.AddAttribute (HtmlTextWriterAttribute.Align, "top", false);
				break;
			case ImageAlign.Middle:
				writer.AddAttribute (HtmlTextWriterAttribute.Align, "middle", false);
				break;
			case ImageAlign.Bottom:
				writer.AddAttribute (HtmlTextWriterAttribute.Align, "bottom", false);
				break;
			case ImageAlign.AbsBottom:
				writer.AddAttribute (HtmlTextWriterAttribute.Align, "absbottom", false);
				break;
			case ImageAlign.AbsMiddle:
				writer.AddAttribute (HtmlTextWriterAttribute.Align, "absmiddle", false);
				break;
			case ImageAlign.TextTop:
				writer.AddAttribute (HtmlTextWriterAttribute.Align, "texttop", false);
				break;
			}
#if NET_2_0
#if BUG_78875_FIXED
			if (Context.Request.Browser.SupportsCss)
#endif
			if (BorderWidth.IsEmpty)
				writer.AddStyleAttribute (HtmlTextWriterStyle.BorderWidth, "0px");
#if BUG_78875_FIXED
			else
#endif
#else
			// if border-with is not specified in style or 
			// no style is defined - set image to no border
			if (!ControlStyleCreated || ControlStyle.BorderWidth.IsEmpty)
				writer.AddAttribute (HtmlTextWriterAttribute.Border, "0");
#endif
		}

#if NET_2_0
		protected internal
#else		
		protected
#endif		
		override void RenderContents (HtmlTextWriter writer)
		{
			base.RenderContents (writer);
		}
	}
}
