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
using System.Web.Util;

namespace System.Web.UI.WebControls {

	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class TableStyle : Style {

		[Flags]
		enum TableStyles
		{
			BackImageUrl = 0x00010000,
			CellPadding = 0x00020000,
			CellSpacing = 0x00040000,
			GridLines = 0x00080000,
			HorizontalAlign = 0x00100000,
		}

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
				if (!CheckBit ((int) TableStyles.BackImageUrl))
					return String.Empty;
				return (string) ViewState ["BackImageUrl"];
			}
			set {
				if (value == null)
					throw new ArgumentNullException ("BackImageUrl");
				ViewState ["BackImageUrl"] = value;
				SetBit ((int) TableStyles.BackImageUrl);
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
				if (!CheckBit ((int) TableStyles.CellPadding))
					return -1;
				return (int) ViewState ["CellPadding"];
			}
			set {
				if (value < -1)
					throw new ArgumentOutOfRangeException ("< -1");
				ViewState ["CellPadding"] = value;
				SetBit ((int) TableStyles.CellPadding);
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
				if (!CheckBit ((int) TableStyles.CellSpacing))
					return -1;
				return (int) ViewState ["CellSpacing"];
			}
			set {
				if (value < -1)
					throw new ArgumentOutOfRangeException ("< -1");
				ViewState ["CellSpacing"] = value;
				SetBit ((int) TableStyles.CellSpacing);
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
				if (!CheckBit ((int) TableStyles.GridLines))
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
				SetBit ((int) TableStyles.GridLines);
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
				if (!CheckBit ((int) TableStyles.HorizontalAlign))
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
				SetBit ((int) TableStyles.HorizontalAlign);
			}
		}
#if NET_4_0
		[MonoTODO ("collapse style should be rendered only for browsers which support that.")]
#endif
		public override void AddAttributesToRender (HtmlTextWriter writer, WebControl owner)
		{
			base.AddAttributesToRender (writer, owner);
			if (writer == null)
				return;

			// note: avoid calling properties multiple times
			int i = CellSpacing;
			if (i != -1) {
				writer.AddAttribute (HtmlTextWriterAttribute.Cellspacing, i.ToString (Helpers.InvariantCulture), false);
				if (i == 0)
					writer.AddStyleAttribute(HtmlTextWriterStyle.BorderCollapse, "collapse");
			}

			i = CellPadding;
			if (i != -1)
				writer.AddAttribute (HtmlTextWriterAttribute.Cellpadding, i.ToString (Helpers.InvariantCulture), false);
			
			GridLines g = GridLines;
			switch (g) {
			case GridLines.Horizontal:
				writer.AddAttribute (HtmlTextWriterAttribute.Rules, "rows", false);
				break;
			case GridLines.Vertical:
				writer.AddAttribute (HtmlTextWriterAttribute.Rules, "cols", false);
				break;
			case GridLines.Both:
				writer.AddAttribute (HtmlTextWriterAttribute.Rules, "all", false);
				break;
			}

			// note: avoid ToString on the enum
			switch (HorizontalAlign) {
			case HorizontalAlign.Left:
				writer.AddAttribute (HtmlTextWriterAttribute.Align, "left", false);
				break;
			case HorizontalAlign.Center:
				writer.AddAttribute (HtmlTextWriterAttribute.Align, "center", false);
				break;
			case HorizontalAlign.Right:
				writer.AddAttribute (HtmlTextWriterAttribute.Align, "right", false);
				break;
			case HorizontalAlign.Justify:
				writer.AddAttribute (HtmlTextWriterAttribute.Align, "justify", false);
				break;
			}
#if NET_4_0
			if (g != GridLines.None && BorderWidth.IsEmpty)
				writer.AddAttribute (HtmlTextWriterAttribute.Border, "1", false);
#else
			// border (=0) is always present (and base class doesn't seems to add it)
			// but border is "promoted" to 1 if gridlines are present (with BorderWidth == 0)
			if (g == GridLines.None) {
				writer.AddAttribute (HtmlTextWriterAttribute.Border, "0", false);
			} else if (BorderWidth.IsEmpty) {
				writer.AddAttribute (HtmlTextWriterAttribute.Border, "1", false);
			} else {
				writer.AddAttribute (HtmlTextWriterAttribute.Border, BorderWidth.Value.ToString (Helpers.InvariantCulture));
			}
#endif
#if !NET_2_0
			string s = BackImageUrl;
			if (s.Length > 0) {
				if (owner != null)
					s = owner.ResolveClientUrl (s);
				s = String.Concat ("url(", s, ")");
				writer.AddStyleAttribute (HtmlTextWriterStyle.BackgroundImage, s);
			}
#endif
		}

		void Copy (string name, TableStyles s, Style source)
		{
			if (source.CheckBit ((int) s)) {
				object o = source.ViewState [name];
				if (o != null) {
					ViewState [name] = o;
					SetBit ((int) s);
				}
			}
		}

		public override void CopyFrom (Style s)
		{
			// note: styles is copied in base
			base.CopyFrom (s);
			if ((s != null) && !s.IsEmpty) {
				Copy ("BackImageUrl", TableStyles.BackImageUrl, s);
				Copy ("CellPadding", TableStyles.CellPadding, s);
				Copy ("CellSpacing", TableStyles.CellSpacing, s);
				Copy ("GridLines", TableStyles.GridLines, s);
				Copy ("HorizontalAlign", TableStyles.HorizontalAlign, s);
			}
		}

		void Merge (string name, TableStyles s, Style source)
		{
			if ((!CheckBit ((int) s)) && (source.CheckBit ((int) s))) {
				object o = source.ViewState [name];
				if (o != null) {
					ViewState [name] = o;
					SetBit ((int) s);
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
					Merge ("BackImageUrl", TableStyles.BackImageUrl, s);
					Merge ("CellPadding", TableStyles.CellPadding, s);
					Merge ("CellSpacing", TableStyles.CellSpacing, s);
					Merge ("GridLines", TableStyles.GridLines, s);
					Merge ("HorizontalAlign", TableStyles.HorizontalAlign, s);
				}
			}
		}

		public override void Reset ()
		{
			if (CheckBit ((int) TableStyles.BackImageUrl))
				ViewState.Remove ("BackImageUrl");
			if (CheckBit ((int) TableStyles.CellPadding))
				ViewState.Remove ("CellPadding");
			if (CheckBit ((int) TableStyles.CellSpacing))
				ViewState.Remove ("CellSpacing");
			if (CheckBit ((int) TableStyles.GridLines))
				ViewState.Remove ("GridLines");
			if (CheckBit ((int) TableStyles.HorizontalAlign))
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

	}
}
