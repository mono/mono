//
// System.Web.UI.WebControls.Style.cs
//
// Authors:
//   Gaurav Vaish (gvaish@iitk.ac.in)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
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
using System.Text;
using System.Collections;
using System.Drawing;
using System.Globalization;
using System.ComponentModel;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	[ToolboxItem(false)]
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public class Style : Component , IStateManager
	{
		internal static int MARKED   	= (0x01 << 0);
		internal static int BACKCOLOR   = (0x01 << 1);
		internal static int BORDERCOLOR = (0x01 << 2);
		internal static int BORDERSTYLE = (0x01 << 3);
		internal static int BORDERWIDTH = (0x01 << 4);
		internal static int CSSCLASS    = (0x01 << 5);
		internal static int FORECOLOR   = (0x01 << 6);
		internal static int HEIGHT      = (0x01 << 7);
		internal static int WIDTH       = (0x01 << 8);
		internal static int FONT_BOLD   = (0x01 << 9);
		internal static int FONT_ITALIC = (0x01 << 10);
		internal static int FONT_NAMES  = (0x01 << 11);
		internal static int FONT_SIZE   = (0x01 << 12);
		internal static int FONT_STRIKE = (0x01 << 13);
		internal static int FONT_OLINE  = (0x01 << 14);
		internal static int FONT_ULINE  = (0x01 << 15);

		internal static string selectionBitString = "_SBS";

		StateBag viewState;
		int  selectionBits;
		bool selfStateBag;
		bool marked;

		private FontInfo font;

		public Style ()
		{
			Initialize(null);
			selfStateBag = true;
		}

		public Style (StateBag bag)
		{
			Initialize (bag);
			selfStateBag = false;
		}

		private void Initialize (StateBag bag)
		{
			viewState     = bag;
			selectionBits = 0x00;
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		protected internal StateBag ViewState {
			get {
				if (viewState == null) {
					viewState = new StateBag (false);
					if (IsTrackingViewState)
						viewState.TrackViewState ();
				}
				return viewState;
			}
		}

		internal bool IsSet (int bit)
		{
			return ((selectionBits & bit) != 0x00);
		}

		internal virtual void Set (int bit)
		{
			selectionBits |= bit;
			if (IsTrackingViewState)
				selectionBits |= MARKED;
		}

		[NotifyParentProperty (true)]
		[DefaultValue (null), Bindable (true), WebCategory ("Appearance")]
		[TypeConverter (typeof (WebColorConverter))]
		[WebSysDescription ("The background color for the WebControl.")]
		public Color BackColor {
			get {
				if(IsSet(BACKCOLOR))
					return (Color)ViewState["BackColor"];
				return Color.Empty;
			}
			set {
				ViewState["BackColor"] = value;
				Set(BACKCOLOR);
			}
		}

		[NotifyParentProperty (true)]
		[DefaultValue (null), Bindable (true), WebCategory ("Appearance")]
		[TypeConverter (typeof (WebColorConverter))]
		[WebSysDescription ("The border color for the WebControl.")]
		public Color BorderColor {
			get {
				if (IsSet (BORDERCOLOR))
					return (Color) ViewState ["BorderColor"];
				return Color.Empty;
			}
			set {
				ViewState ["BorderColor"] = value;
				Set (BORDERCOLOR);
			}
		}

		[NotifyParentProperty (true)]
		[DefaultValue (typeof(BorderStyle), "NotSet"), Bindable (true), WebCategory ("Appearance")]
		[WebSysDescription ("The style/type of the border used for the WebControl.")]
		public BorderStyle BorderStyle {
			get {
				if (IsSet (BORDERSTYLE))
					return (BorderStyle) ViewState ["BorderStyle"];
				return BorderStyle.NotSet;
			}
			set {
				ViewState ["BorderStyle"] = value;
				Set (BORDERSTYLE);
			}
		}

		[NotifyParentProperty (true)]
		[DefaultValue (null), Bindable (true), WebCategory ("Appearance")]
		[WebSysDescription ("The width of the border used for the WebControl.")]
		public Unit BorderWidth {
			get {
				if (IsSet (BORDERWIDTH))
					return (Unit) ViewState ["BorderWidth"];
				return Unit.Empty;
			}
			set {
				ViewState ["BorderWidth"] = value;
				Set (BORDERWIDTH);
			}
		}

		[NotifyParentProperty (true)]
		[DefaultValue (""), WebCategory ("Appearance")]
		[WebSysDescription ("The cascading stylesheet class that is associated with this WebControl.")]
		public string CssClass {
			get {
				if (IsSet (CSSCLASS))
					return (string) ViewState["CssClass"];
				return string.Empty;
			}
			set {
				ViewState ["CssClass"] = value;
				Set (CSSCLASS);
			}
		}

		[NotifyParentProperty (true)]
		[DefaultValue (null), Bindable (true), WebCategory ("Appearance")]
		[TypeConverter (typeof (WebColorConverter))]
		[WebSysDescription ("The color that is used to paint the primary display of the WebControl.")]
		public Color ForeColor {
			get {
				if (IsSet (FORECOLOR))
					return (Color) ViewState ["ForeColor"];
				return Color.Empty;
			}
			set {
				ViewState ["ForeColor"] = value;
				Set (FORECOLOR);
			}
		}

		[NotifyParentProperty (true)]
		[DefaultValue (null), Bindable (true), WebCategory ("Layout")]
		[WebSysDescription ("The height of this WebControl.")]
		public Unit Height {
			get {
				if (IsSet (HEIGHT))
					return (Unit) ViewState ["Height"];
				return Unit.Empty;
			}
			set {
				ViewState ["Height"] = value;
				Set (HEIGHT);
			}
		}

		[NotifyParentProperty (true)]
		[DefaultValue (null), Bindable (true), WebCategory ("Layout")]
		[WebSysDescription ("The width of this WebControl.")]
		public Unit Width {
			get {
				if (IsSet(WIDTH))
					return (Unit) ViewState ["Width"];
				return Unit.Empty;
			}
			set {
				ViewState ["Width"] = value;
				Set (WIDTH);
			}
		}

		[NotifyParentProperty (true)]
		[WebCategory ("Appearance")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[WebSysDescription ("The font of this WebControl.")]
		public FontInfo Font {
			get {
				if (font==null)
					font = new FontInfo (this);
				return font;
			}
		}

		protected internal virtual bool IsEmpty
		{
			get { return (selectionBits == 0); }
		}

		private void AddColor (CssStyleCollection attributes, HtmlTextWriterStyle style, Color color)
		{
			if (!color.IsEmpty)
				attributes.Add (style, ColorTranslator.ToHtml (color));
		}

		private static string StringArrayToString (string [] array, char separator)
		{
			if (array.Length == 0)
				return String.Empty;
			StringBuilder sb = new StringBuilder ();
			for (int i = 0; i < array.Length; i++) {
				if (i == 0) {
					sb.Append (array [0]);
				} else {
					sb.Append (separator);
					sb.Append (array [i]);
				}
			}
			return sb.ToString ();
		}

		public void AddAttributesToRender (HtmlTextWriter writer)
		{
			AddAttributesToRender (writer, null);
		}

		public virtual void AddAttributesToRender (HtmlTextWriter writer, WebControl owner)
		{
			if (IsSet (CSSCLASS)) {
				string cssClass = (string) ViewState ["CssClass"];
				if (cssClass.Length > 0)
					writer.AddAttribute (HtmlTextWriterAttribute.Class, cssClass);
			}

			CssStyleCollection ats = new CssStyleCollection ();
#if NET_2_0
			FillStyleAttributes (ats, owner);
#else
			FillAttributes (ats);
#endif
			foreach (string key in ats.Keys)
				writer.AddStyleAttribute (key, ats [key]);
		}

#if NET_2_0
		public CssStyleCollection FillStyleAttributes (IUrlResolutionService urlResolver)
		{
			CssStyleCollection ats = new CssStyleCollection ();
			FillStyleAttributes (ats, urlResolver);
			return ats;
		}
		protected virtual void FillStyleAttributes (CssStyleCollection attributes, IUrlResolutionService urlResolver)
		{
			FillAttributes (attributes);
		}
#endif

		void FillAttributes (CssStyleCollection attributes)
		{
			if (IsSet (BACKCOLOR))
				AddColor (attributes, HtmlTextWriterStyle.BackgroundColor, BackColor);

			if (IsSet(BORDERCOLOR))
				AddColor (attributes, HtmlTextWriterStyle.BorderColor, BorderColor);

			if (IsSet (FORECOLOR))
				AddColor (attributes, HtmlTextWriterStyle.Color, ForeColor);

			if (!BorderWidth.IsEmpty) {
				attributes.Add (HtmlTextWriterStyle.BorderWidth,
						BorderWidth.ToString (CultureInfo.InvariantCulture));

				if (BorderStyle != BorderStyle.NotSet) {
					attributes.Add (HtmlTextWriterStyle.BorderStyle,
							Enum.Format (typeof (BorderStyle), BorderStyle, "G"));
				} else {
					if (BorderWidth.Value != 0.0)
						attributes.Add (HtmlTextWriterStyle.BorderStyle, "solid");
				}
			} else {
				if (BorderStyle != BorderStyle.NotSet)
					attributes.Add (HtmlTextWriterStyle.BorderStyle,
							Enum.Format (typeof (BorderStyle), BorderStyle, "G"));
			}

			if (Font.Names.Length > 0)
				attributes.Add (HtmlTextWriterStyle.FontFamily,
							StringArrayToString (Font.Names, ','));

			if (!Font.Size.IsEmpty)
				attributes.Add (HtmlTextWriterStyle.FontSize,
							Font.Size.ToString (CultureInfo.InvariantCulture));

			if (Font.Bold)
				attributes.Add (HtmlTextWriterStyle.FontWeight, "bold");

			if (Font.Italic)
				attributes.Add (HtmlTextWriterStyle.FontStyle, "italic");

			string textDecoration = String.Empty;
			if (Font.Strikeout)
				textDecoration += " line-through";

			if (Font.Underline)
				textDecoration += " underline";

			if (Font.Overline)
				textDecoration += " overline";

			if (textDecoration.Length > 0)
				attributes.Add (HtmlTextWriterStyle.TextDecoration, textDecoration);

			Unit u = Unit.Empty;
			if (IsSet (HEIGHT)) {
				u = (Unit) ViewState ["Height"];
				if (!u.IsEmpty)
					attributes.Add (HtmlTextWriterStyle.Height,
								u.ToString (CultureInfo.InvariantCulture));
			}

			if (IsSet (WIDTH)) {
				u = (Unit) ViewState ["Width"];
				if (!u.IsEmpty)
					attributes.Add (HtmlTextWriterStyle.Width,
								u.ToString (CultureInfo.InvariantCulture));
			}
		}
		
		public virtual void CopyFrom (Style source)
		{
			if (source == null || source.IsEmpty)
				return;

			Font.CopyFrom (source.Font);
			if (source.IsSet (HEIGHT)&& (source.Height != Unit.Empty))
				Height = source.Height;

			if (source.IsSet (WIDTH)&& (source.Width != Unit.Empty))
				Width = source.Width;

			if (source.IsSet (BORDERCOLOR)&& (source.BorderColor != Color.Empty))
				BorderColor = source.BorderColor;

			if (source.IsSet (BORDERWIDTH)&& (source.BorderWidth != Unit.Empty))
				BorderWidth = source.BorderWidth;

			if (source.IsSet (BORDERSTYLE))
				BorderStyle = source.BorderStyle;

			if (source.IsSet (BACKCOLOR)&& (source.BackColor != Color.Empty))
				BackColor = source.BackColor;

			if (source.IsSet (CSSCLASS))
				CssClass = source.CssClass;

			if (source.IsSet (FORECOLOR)&& (source.ForeColor != Color.Empty))
				ForeColor = source.ForeColor;

		}

		public virtual void MergeWith (Style with)
		{
			if (with == null || with.IsEmpty)
				return;

			if (IsEmpty) {
				CopyFrom (with);
				return;
			}

			Font.MergeWith (with.Font);
			if (!IsSet (HEIGHT) && with.Height != Unit.Empty)
				Height = with.Height;

			if (!IsSet(WIDTH) && with.Width != Unit.Empty)
				Width = with.Width;

			if (!IsSet (BORDERCOLOR) && with.BorderColor != Color.Empty)
				BorderColor = with.BorderColor;

			if (!IsSet (BORDERWIDTH) && with.BorderWidth != Unit.Empty)
				BorderWidth = with.BorderWidth;

			if (!IsSet (BORDERSTYLE) && with.BorderStyle != BorderStyle.NotSet)
				BorderStyle = with.BorderStyle;

			if (!IsSet (BACKCOLOR) && with.BackColor != Color.Empty)
				BackColor = with.BackColor;

			if (!IsSet (CSSCLASS) && with.CssClass != String.Empty)
				CssClass = with.CssClass;

			if (!IsSet (FORECOLOR) && with.ForeColor != Color.Empty)
				ForeColor = with.ForeColor;
		}

		public virtual void Reset ()
		{
			if (IsSet (BACKCOLOR))
				ViewState.Remove ("BackColor");

			if (IsSet (BORDERCOLOR))
				ViewState.Remove ("BorderColor");

			if (IsSet (BORDERSTYLE))
				ViewState.Remove ("BorderStyle");

			if (IsSet (BORDERWIDTH))
				ViewState.Remove ("BorderWidth");

			if (IsSet (CSSCLASS))
				ViewState.Remove ("CssClass");

			if (IsSet (FORECOLOR))
				ViewState.Remove ("ForeColor");

			if (IsSet (HEIGHT))
				ViewState.Remove ("Height");

			if (IsSet (WIDTH))
				ViewState.Remove( "Width");

			if (font != null)
				font.Reset ();

			selectionBits = 0x00;
		}

		protected bool IsTrackingViewState {
			get { return marked; }
		}

		protected internal virtual void TrackViewState ()
		{
			if (selfStateBag)
				ViewState.TrackViewState ();

			marked = true;
		}

		protected internal virtual object SaveViewState ()
		{
			if (viewState != null) {
				if (marked && IsSet (MARKED))
					ViewState [selectionBitString] = selectionBits;

				if (selfStateBag)
					return ViewState.SaveViewState ();
			}

			return null;
		}

		protected internal void LoadViewState (object state)
		{
			if (state != null && selfStateBag)
				ViewState.LoadViewState (state);

			if (viewState != null) {
				object o = ViewState [selectionBitString];
				if (o != null)
					selectionBits = (int) o;
			}
		}

		void IStateManager.LoadViewState(object state)
		{
			LoadViewState(state);
		}

		object IStateManager.SaveViewState ()
		{
			return SaveViewState ();
		}

		void IStateManager.TrackViewState ()
		{
			TrackViewState ();
		}

		bool IStateManager.IsTrackingViewState {
			get { return IsTrackingViewState; }
		}

		public override string ToString ()
		{
			return String.Empty;
		}

		protected internal void SetBit (int bit)
		{
			Set (bit);
		}
		
#if NET_2_0
		public void SetDirty ()
		{
			if (selectionBits != 0x00)
				Set (MARKED);
			if (viewState != null)
				viewState.SetDirty ();
		}
#endif
	}
}

