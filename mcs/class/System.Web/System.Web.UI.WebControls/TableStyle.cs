//
// System.Web.UI.WebControls.TableStyle.cs
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
using System.Globalization;
using System.Security.Permissions;
using System.Web.UI;

namespace System.Web.UI.WebControls {

	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class TableStyle : Style {

		public TableStyle ()
		{
		}

		public TableStyle (StateBag bag)
			: base (bag)
		{
		}


#if NET_2_0
		[NotifyParentProperty (true)]
		[UrlProperty]
#else
		[Bindable (true)]
#endif
		[DefaultValue ("")]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public virtual string BackImageUrl {
			get {
				if ((styles & Styles.BackImageUrl) == 0)
					return String.Empty;
				return (string) ViewState ["BackImageUrl"];
			}
			set {
				if (value == null)
					throw new ArgumentNullException ("BackImageUrl");
				ViewState ["BackImageUrl"] = value;
				styles |= Styles.BackImageUrl;
			}
		}

#if NET_2_0
		[NotifyParentProperty (true)]
#else
		[Bindable (true)]
#endif
		[DefaultValue (-1)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public virtual int CellPadding {
			get {
				if ((styles & Styles.CellPadding) == 0)
					return -1;
				return (int) ViewState ["CellPadding"];
			}
			set {
				if (value < -1)
					throw new ArgumentOutOfRangeException ("< -1");
				ViewState ["CellPadding"] = value;
				styles |= Styles.CellPadding;
			}
		}

#if NET_2_0
		[NotifyParentProperty (true)]
#else
		[Bindable (true)]
#endif
		[DefaultValue (-1)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public virtual int CellSpacing {
			get {
				if ((styles & Styles.CellSpacing) == 0)
					return -1;
				return (int) ViewState ["CellSpacing"];
			}
			set {
				if (value < -1)
					throw new ArgumentOutOfRangeException ("< -1");
				ViewState ["CellSpacing"] = value;
				styles |= Styles.CellSpacing;
			}
		}

		// LAMESPEC: default is documented to be Both
#if NET_2_0
		[NotifyParentProperty (true)]
#else
		[Bindable (true)]
#endif
		[DefaultValue (GridLines.None)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public virtual GridLines GridLines {
			get {
				if ((styles & Styles.GridLines) == 0)
					return GridLines.None;
				return (GridLines) ViewState ["GridLines"];
			}
			set {
				// avoid reflection
				if ((value < GridLines.None) || (value > GridLines.Both)) {
					// LAMESPEC: documented as ArgumentException
					throw new ArgumentOutOfRangeException (Locale.GetText ("Invalid GridLines value."));
				}
				ViewState ["GridLines"] = value;
				styles |= Styles.GridLines;
			}
		}

#if NET_2_0
		[NotifyParentProperty (true)]
#else
		[Bindable (true)]
#endif
		[DefaultValue (HorizontalAlign.NotSet)]
		[WebSysDescription ("")]
		[WebCategory ("Layout")]
		public virtual HorizontalAlign HorizontalAlign {
			get {
				if ((styles & Styles.HorizontalAlign) == 0)
					return HorizontalAlign.NotSet;
				return (HorizontalAlign) ViewState ["HorizontalAlign"];
			}
			set {
				// avoid reflection
				if ((value < HorizontalAlign.NotSet) || (value > HorizontalAlign.Justify)) {
					// LAMESPEC: documented as ArgumentException
					throw new ArgumentOutOfRangeException (Locale.GetText ("Invalid HorizontalAlign value."));
				}
				ViewState ["HorizontalAlign"] = value;
				styles |= Styles.HorizontalAlign;
			}
		}


		public override void AddAttributesToRender (HtmlTextWriter writer, WebControl owner)
		{
			base.AddAttributesToRender (writer, owner);
			if (writer == null)
				return;

			// note: avoid calling properties multiple times
			int i = CellPadding;
			if (i != -1)
				writer.AddAttribute (HtmlTextWriterAttribute.Cellpadding, i.ToString (CultureInfo.InvariantCulture));
			
			i = CellSpacing;
			if (i != -1) {
				writer.AddAttribute (HtmlTextWriterAttribute.Cellspacing, i.ToString (CultureInfo.InvariantCulture));
				if (i == 0) {
					writer.AddStyleAttribute(HtmlTextWriterStyle.BorderCollapse, "collapse");
				}
			}

			GridLines g = GridLines;
			switch (g) {
			case GridLines.Horizontal:
				writer.AddAttribute (HtmlTextWriterAttribute.Rules, "rows");
				break;
			case GridLines.Vertical:
				writer.AddAttribute (HtmlTextWriterAttribute.Rules, "cols");
				break;
			case GridLines.Both:
				writer.AddAttribute (HtmlTextWriterAttribute.Rules, "all");
				break;
			}

			// note: avoid ToString on the enum
			switch (HorizontalAlign) {
			case HorizontalAlign.Left:
				writer.AddAttribute (HtmlTextWriterAttribute.Align, "Left");
				break;
			case HorizontalAlign.Center:
				writer.AddAttribute (HtmlTextWriterAttribute.Align, "Center");
				break;
			case HorizontalAlign.Right:
				writer.AddAttribute (HtmlTextWriterAttribute.Align, "Right");
				break;
			case HorizontalAlign.Justify:
				writer.AddAttribute (HtmlTextWriterAttribute.Align, "Justify");
				break;
			}

			// border (=0) is always present (and base class doesn't seems to add it)
			// but border is "promoted" to 1 if gridlines are present (with BorderWidth == 0)
			if (g == GridLines.None) {
				writer.AddAttribute (HtmlTextWriterAttribute.Border, "0");
			} else if (BorderWidth.IsEmpty) {
				writer.AddAttribute (HtmlTextWriterAttribute.Border, "1");
			} else {
				writer.AddAttribute (HtmlTextWriterAttribute.Border, BorderWidth.Value.ToString (CultureInfo.InvariantCulture));
			}

			string s = BackImageUrl;
			if (s.Length > 0) {
				if (owner != null)
					s = owner.ResolveUrl (s);
#if ONLY_1_1
				s = String.Concat ("url(", s, ")");
#endif
				writer.AddStyleAttribute (HtmlTextWriterStyle.BackgroundImage, s);
			}
		}

		private void Copy (string name, Styles s, Style source)
		{
			if ((source.styles & s) != 0) {
				object o = source.ViewState [name];
				if (o != null) {
					ViewState [name] = o;
					styles |= s;
				}
			}
		}

		public override void CopyFrom (Style s)
		{
			// note: styles is copied in base
			base.CopyFrom (s);
			if ((s != null) && !s.IsEmpty) {
				Copy ("BackImageUrl", Styles.BackImageUrl, s);
				Copy ("CellPadding", Styles.CellPadding, s);
				Copy ("CellSpacing", Styles.CellSpacing, s);
				Copy ("GridLines", Styles.GridLines, s);
				Copy ("HorizontalAlign", Styles.HorizontalAlign, s);
			}
		}

		private void Merge (string name, Styles s, Style source)
		{
			if ((styles & s) == 0 && (source.styles & s) != 0) {
				object o = source.ViewState [name];
				if (o != null) {
					ViewState [name] = o;
					styles |= s;
				}
			}
		}

		public override void MergeWith (Style s)
		{
			// if we're empty then it's like a copy
			if (IsEmpty) {
				CopyFrom (s);
			} else {
				base.MergeWith (s);
				if ((s != null) && !s.IsEmpty) {
					Merge ("BackImageUrl", Styles.BackImageUrl, s);
					Merge ("CellPadding", Styles.CellPadding, s);
					Merge ("CellSpacing", Styles.CellSpacing, s);
					Merge ("GridLines", Styles.GridLines, s);
					Merge ("HorizontalAlign", Styles.HorizontalAlign, s);
				}
			}
		}

		public override void Reset ()
		{
			if ((styles & Styles.BackImageUrl) != 0)
				ViewState.Remove ("BackImageUrl");
			if ((styles & Styles.CellPadding) != 0)
				ViewState.Remove ("CellPadding");
			if ((styles & Styles.CellSpacing) != 0)
				ViewState.Remove ("CellSpacing");
			if ((styles & Styles.GridLines) != 0)
				ViewState.Remove ("GridLines");
			if ((styles & Styles.HorizontalAlign) != 0)
				ViewState.Remove ("HorizontalAlign");
			// call base at the end because "styles" will reset there
			base.Reset ();
		}
#if NET_2_0
		protected override void FillStyleAttributes (CssStyleCollection attributes, IUrlResolutionService urlResolver)
		{
			if (attributes != null) {
				string url = BackImageUrl;
				if (url.Length > 0) {
					if (urlResolver != null)
						url = urlResolver.ResolveClientUrl (url);
					attributes.Add (HtmlTextWriterStyle.BackgroundImage, url);
				}
			}
			base.FillStyleAttributes (attributes, urlResolver);
		}
#endif

		internal override void LoadViewStateInternal()
		{
			if (viewstate["BackImageUrl"] != null) {
				styles |= Styles.BackImageUrl;
			}
			if (viewstate["CellPadding"] != null) {
				styles |= Styles.CellPadding;
			}
			if (viewstate["CellSpacing"] != null) {
				styles |= Styles.CellSpacing;
			}
			if (viewstate["GridLines"] != null) {
				styles |= Styles.GridLines;
			}
			if (viewstate["HorizontalAlign"] != null) {
				styles |= Styles.HorizontalAlign;
			}

			base.LoadViewStateInternal();
		}
	}
}
