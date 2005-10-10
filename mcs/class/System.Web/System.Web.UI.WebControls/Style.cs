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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Dennis Bartok	(pbartok@novell.com)
//
//

using System.ComponentModel;
using System.Drawing;
using System.Security.Permissions;

namespace System.Web.UI.WebControls {

	// CAS
	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
#if NET_2_0
// Not until we actually have StyleConverter
//	[TypeConverter(typeof(System.Web.UI.WebControls.StyleConverter))]
#else
	[TypeConverter(typeof(System.ComponentModel.ExpandableObjectConverter))]
#endif
	[ToolboxItem("")]
	public class Style : System.ComponentModel.Component, System.Web.UI.IStateManager 
	{
		[Flags]
		internal enum Styles 
		{
			None		= 0,
			BackColor	= 0x00000001,
			BorderColor	= 0x00000002,
			BorderStyle	= 0x00000004,
			BorderWidth	= 0x00000008,
			CssClass	= 0x00000010,
			Font		= 0x00000020,
			ForeColor	= 0x00000040,
			Height		= 0x00000080,
			Width		= 0x00000100,

			// from TableStyle (which doesn't override IsEmpty)
			BackImageUrl	= 0x00000200,
			CellPadding	= 0x00000400,
			CellSpacing	= 0x00000800,
			GridLines	= 0x00001000,
			HorizontalAlign	= 0x00002000,

			// from TableItemStyle (which doesn't override IsEmpty neither)
			VerticalAlign	= 0x00004000,
			Wrap		= 0x00008000,

			// from DataGridPagerStyle (and, once again, no IsEmpty override)
			Mode		= 0x00010000,
			NextPageText	= 0x00020000,
			PageButtonCount	= 0x00040000,
			Position	= 0x00080000,
			PrevPageText	= 0x00100000,
			Visible		= 0x00200000
			
		}

		#region Fields
		internal Styles		styles;
		internal StateBag	viewstate;
		private FontInfo	fontinfo;
		private bool		tracking;
#if NET_2_0
		private string		registered_class;
#endif
		#endregion	// Fields

		#region Public Constructors
		public Style() : this(new StateBag()) 
		{
		}

		public Style(System.Web.UI.StateBag bag) 
		{
			if (bag != null) {
				viewstate = bag;
			} else {
				viewstate = new StateBag();
			}
			tracking = false;
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
#if !NET_2_0
		[Bindable(true)]
#endif
		[DefaultValue(typeof (Color), "")]
		[NotifyParentProperty(true)]
		[TypeConverter(typeof(System.Web.UI.WebControls.WebColorConverter))]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public Color BackColor 
		{
			get 
			{
				if ((styles & Styles.BackColor) == 0) 
				{
					return Color.Empty;
				}

				return (Color)viewstate["BackColor"];
			}

			set 
			{
				viewstate["BackColor"] = value;
				styles |= Styles.BackColor;
			}
		}

#if !NET_2_0
		[Bindable(true)]
#endif
		[DefaultValue(typeof (Color), "")]
		[NotifyParentProperty(true)]
		[TypeConverter(typeof(System.Web.UI.WebControls.WebColorConverter))]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public Color BorderColor 
		{
			get 
			{
				if ((styles & Styles.BorderColor) == 0) 
				{
					return Color.Empty;
				}

				return (Color)viewstate["BorderColor"];
			}

			set 
			{
				viewstate["BorderColor"] = value;
				styles |= Styles.BorderColor;
			}
		}

#if !NET_2_0
		[Bindable(true)]
#endif
		[DefaultValue(BorderStyle.NotSet)]
		[NotifyParentProperty(true)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public BorderStyle BorderStyle 
		{
			get 
			{
				if ((styles & Styles.BorderStyle) == 0) 
				{
					return BorderStyle.NotSet;
				}

				return (BorderStyle)viewstate["BorderStyle"];
			}

			set 
			{
				viewstate["BorderStyle"] = value;
				styles |= Styles.BorderStyle;
			}
		}

#if !NET_2_0
		[Bindable(true)]
#endif
		[DefaultValue(typeof (Unit), "")]
		[NotifyParentProperty(true)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public Unit BorderWidth 
		{
			get 
			{
				if ((styles & Styles.BorderWidth) == 0) 
				{
					return Unit.Empty;
				}

				return (Unit)viewstate["BorderWidth"];
			}

			set 
			{
				if (value.Value < 0) 
				{
					throw new ArgumentOutOfRangeException("Value", value.Value, "BorderWidth must not be negative");
				}

				viewstate["BorderWidth"] = value;
				styles |= Styles.BorderWidth;
			}
		}

		[DefaultValue("")]
		[NotifyParentProperty(true)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public string CssClass 
		{
			get 
			{
				if ((styles & Styles.CssClass) == 0) 
				{
					return string.Empty;
				}

				return (string)viewstate["CssClass"];
			}

			set 
			{
				viewstate["CssClass"] = value;
				styles |= Styles.CssClass;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[NotifyParentProperty(true)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public FontInfo Font 
		{
			get 
			{
				if (fontinfo == null) 
				{
					fontinfo = new FontInfo(this);
				}
				return fontinfo;
			}
		}

#if !NET_2_0
		[Bindable(true)]
#endif
		[DefaultValue(typeof (Color), "")]
		[NotifyParentProperty(true)]
		[TypeConverter(typeof(System.Web.UI.WebControls.WebColorConverter))]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public Color ForeColor 
		{
			get 
			{
				if ((styles & Styles.ForeColor) == 0) 
				{
					return Color.Empty;
				}

				return (Color)viewstate["ForeColor"];
			}

			set 
			{
				viewstate["ForeColor"] = value;
				styles |= Styles.ForeColor;
			}
		}

#if !NET_2_0
		[Bindable(true)]
#endif
		[DefaultValue(typeof (Unit), "")]
		[NotifyParentProperty(true)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public Unit Height 
		{
			get 
			{
				if ((styles & Styles.Height) == 0) 
				{
					return Unit.Empty;
				}

				return (Unit)viewstate["Height"];
			}

			set 
			{
				if (value.Value < 0) 
				{
					throw new ArgumentOutOfRangeException("Value", value.Value, "Height must not be negative");
				}

				viewstate["Height"] = value;
				styles |= Styles.Height;
			}
		}

#if !NET_2_0
		[Bindable(true)]
#endif
		[DefaultValue(typeof (Unit), "")]
		[NotifyParentProperty(true)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public Unit Width 
		{
			get 
			{
				if ((styles & Styles.Width) == 0) 
				{
					return Unit.Empty;
				}

				return (Unit)viewstate["Width"];
			}

			set 
			{
				if (value.Value < 0) 
				{
					throw new ArgumentOutOfRangeException("Value", value.Value, "Width must not be negative");
				}

				viewstate["Width"] = value;
				styles |= Styles.Width;
			}
		}
		#endregion	// Public Instance Properties

		#region Protected Instance Properties
#if NET_2_0
		[Browsable (false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public virtual bool IsEmpty 
#else
		protected internal virtual bool IsEmpty 
#endif
		{
			get 
			{
				return (styles == 0 && (fontinfo == null || fontinfo.IsEmpty));
			}
		}

		protected bool IsTrackingViewState 
		{
			get 
			{
				return tracking;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected internal StateBag ViewState 
		{
			get 
			{
				return viewstate;
			}
		}
		#endregion	// Protected Instance Properties

		#region Public Instance Methods
		public void AddAttributesToRender(System.Web.UI.HtmlTextWriter writer) 
		{
			AddAttributesToRender(writer, null);
		}

		public virtual void AddAttributesToRender(System.Web.UI.HtmlTextWriter writer, WebControl owner)
		{
			if ((styles & Styles.CssClass) != 0) 
			{
				string s = (string)viewstate["CssClass"];
				if (s != string.Empty)
					writer.AddAttribute (HtmlTextWriterAttribute.Class, s);
			}

			WriteStyleAttributes (writer);
		}

		void WriteStyleAttributes (HtmlTextWriter writer) 
		{
			string		s;
			Color		color;
			BorderStyle	bs;
			Unit		u;

			if ((styles & Styles.BackColor) != 0) {
				color = (Color)viewstate["BackColor"];
				if (!color.IsEmpty)
					writer.AddStyleAttribute (HtmlTextWriterStyle.BackgroundColor, ColorTranslator.ToHtml(color));
			}

			if ((styles & Styles.BorderColor) != 0) {
				color = (Color)viewstate["BorderColor"];
				if (!color.IsEmpty)
					writer.AddStyleAttribute (HtmlTextWriterStyle.BorderColor, ColorTranslator.ToHtml(color));
			}

			bool have_width = false;
			if ((styles & Styles.BorderWidth) != 0) {
				u = (Unit)viewstate["BorderWidth"];
				if (!u.IsEmpty) {
					have_width = true;
					writer.AddStyleAttribute (HtmlTextWriterStyle.BorderWidth, u.ToString());
				}
			}

			if ((styles & Styles.BorderStyle) != 0) {
				bs = (BorderStyle)viewstate["BorderStyle"];
				if (bs != BorderStyle.NotSet) 
					writer.AddStyleAttribute (HtmlTextWriterStyle.BorderStyle, bs.ToString());
			} else if (have_width) {
				writer.AddStyleAttribute (HtmlTextWriterStyle.BorderStyle, "solid");
			}

			if ((styles & Styles.ForeColor) != 0) {
				color = (Color)viewstate["ForeColor"];
				if (!color.IsEmpty)
					writer.AddStyleAttribute (HtmlTextWriterStyle.Color, ColorTranslator.ToHtml(color));
			}

			if ((styles & Styles.Height) != 0) {
				u = (Unit)viewstate["Height"];
				if (!u.IsEmpty)
					writer.AddStyleAttribute (HtmlTextWriterStyle.Height, u.ToString());
			}

			if ((styles & Styles.Width) != 0) {
				u = (Unit)viewstate["Width"];
				if (!u.IsEmpty)
					writer.AddStyleAttribute (HtmlTextWriterStyle.Width, u.ToString());
			}

			if (fontinfo != null) {
				// Fonts are a bit weird
				if (fontinfo.Name != string.Empty) {
					s = fontinfo.Names[0];
					for (int i = 1; i < fontinfo.Names.Length; i++)
						s += "," + fontinfo.Names[i];
					writer.AddStyleAttribute (HtmlTextWriterStyle.FontFamily, s);
				}

				if (fontinfo.Bold)
					writer.AddStyleAttribute (HtmlTextWriterStyle.FontWeight, "bold");

				if (fontinfo.Italic)
					writer.AddStyleAttribute (HtmlTextWriterStyle.FontStyle, "italic");

				if (!fontinfo.Size.IsEmpty)
					writer.AddStyleAttribute (HtmlTextWriterStyle.FontSize, fontinfo.Size.ToString());

				// These styles are munged into a attribute decoration
				s = string.Empty;

				if (fontinfo.Overline)
					s += "overline ";

				if (fontinfo.Strikeout)
					s += "line-through ";

				if (fontinfo.Underline)
					s += "underline ";

				if (s != string.Empty)
					writer.AddStyleAttribute (HtmlTextWriterStyle.TextDecoration, s);
			}
		}

		void FillStyleAttributes (CssStyleCollection attributes) 
		{
			string		s;
			Color		color;
			BorderStyle	bs;
			Unit		u;

			if ((styles & Styles.BackColor) != 0)
			{
				color = (Color)viewstate["BackColor"];
				if (!color.IsEmpty)
					attributes.Add (HtmlTextWriterStyle.BackgroundColor, ColorTranslator.ToHtml(color));
			}

			if ((styles & Styles.BorderColor) != 0) 
			{
				color = (Color)viewstate["BorderColor"];
				if (!color.IsEmpty)
					attributes.Add (HtmlTextWriterStyle.BorderColor, ColorTranslator.ToHtml(color));
			}

			if ((styles & Styles.BorderStyle) != 0) 
			{
				bs = (BorderStyle)viewstate["BorderStyle"];
				if (bs != BorderStyle.NotSet) 
					attributes.Add (HtmlTextWriterStyle.BorderStyle, bs.ToString());
			}

			if ((styles & Styles.BorderWidth) != 0) 
			{
				u = (Unit)viewstate["BorderWidth"];
				if (!u.IsEmpty)
					attributes.Add (HtmlTextWriterStyle.BorderWidth, u.ToString());
			}

			if ((styles & Styles.ForeColor) != 0) 
			{
				color = (Color)viewstate["ForeColor"];
				if (!color.IsEmpty)
					attributes.Add (HtmlTextWriterStyle.Color, ColorTranslator.ToHtml(color));
			}

			if ((styles & Styles.Height) != 0) 
			{
				u = (Unit)viewstate["Height"];
				if (!u.IsEmpty)
					attributes.Add (HtmlTextWriterStyle.Height, u.ToString());
			}

			if ((styles & Styles.Width) != 0) 
			{
				u = (Unit)viewstate["Width"];
				if (!u.IsEmpty)
					attributes.Add (HtmlTextWriterStyle.Width, u.ToString());
			}

			if (fontinfo != null) {
				// Fonts are a bit weird
				if (fontinfo.Name != string.Empty) 
				{
					s = fontinfo.Names[0];
					for (int i = 1; i < fontinfo.Names.Length; i++) 
					{
						s += "," + fontinfo.Names[i];
					}
					attributes.Add (HtmlTextWriterStyle.FontFamily, s);
				}

				if (fontinfo.Bold) 
				{
					attributes.Add (HtmlTextWriterStyle.FontWeight, "bold");
				}

				if (fontinfo.Italic) 
				{
					attributes.Add (HtmlTextWriterStyle.FontStyle, "italic");
				}

				if (!fontinfo.Size.IsEmpty) 
				{
					attributes.Add (HtmlTextWriterStyle.FontSize, fontinfo.Size.ToString());
				}

				// These styles are munged into a attribute decoration
				s = string.Empty;

				if (fontinfo.Overline) 
				{
					s += "overline ";
				}

				if (fontinfo.Strikeout) 
				{
					s += "line-through ";
				}

				if (fontinfo.Underline) 
				{
					s += "underline ";
				}

				if (s != string.Empty) 
				{
					attributes.Add (HtmlTextWriterStyle.TextDecoration, s);
				}
			}
		}

		public virtual void CopyFrom(Style s) 
		{
			if ((s == null) || s.IsEmpty) 
			{
				return;
			}

			if (s.fontinfo != null) 
			{
				Font.CopyFrom(s.fontinfo);
			}

			if (((s.styles & Styles.BackColor) != 0) && (s.BackColor != Color.Empty))
			{
				this.BackColor = s.BackColor;
			}
			if (((s.styles & Styles.BorderColor) != 0) && (s.BorderColor != Color.Empty))
			{
				this.BorderColor = s.BorderColor;
			}
			if (((s.styles & Styles.BorderStyle) != 0) && (s.BorderStyle != BorderStyle.NotSet))
			{
				this.BorderStyle = s.BorderStyle;
			}
			if (((s.styles & Styles.BorderWidth) != 0) && (s.BorderWidth != Unit.Empty))
			{
				this.BorderWidth = s.BorderWidth;
			}
			if (((s.styles & Styles.CssClass) != 0) && (s.CssClass != string.Empty))
			{
				this.CssClass = s.CssClass;
			}
			if (((s.styles & Styles.ForeColor) != 0) && (s.ForeColor != Color.Empty))
			{
				this.ForeColor = s.ForeColor;
			}
			if (((s.styles & Styles.Height) != 0) && (s.Height != Unit.Empty))
			{
				this.Height = s.Height;
			}
			if (((s.styles & Styles.Width) != 0) && (s.Width != Unit.Empty))
			{
				this.Width = s.Width;
			}
		}

		public virtual void MergeWith(Style s) 
		{
			if ((s == null) || (s.IsEmpty))
			{
				return;
			}

			if (s.fontinfo != null) 
			{
				Font.MergeWith(s.fontinfo);
			}

			if (((styles & Styles.BackColor) == 0) && ((s.styles & Styles.BackColor) != 0) && (s.BackColor != Color.Empty))
			{
				this.BackColor = s.BackColor;
			}
			if (((styles & Styles.BorderColor) == 0) && ((s.styles & Styles.BorderColor) != 0) && (s.BorderColor != Color.Empty)) 
			{
				this.BorderColor = s.BorderColor;
			}
			if (((styles & Styles.BorderStyle) == 0) && ((s.styles & Styles.BorderStyle) != 0) && (s.BorderStyle != BorderStyle.NotSet)) 
			{
				this.BorderStyle = s.BorderStyle;
			}
			if (((styles & Styles.BorderWidth) == 0) && ((s.styles & Styles.BorderWidth) != 0) && (s.BorderWidth != Unit.Empty)) 
			{
				this.BorderWidth = s.BorderWidth;
			}
			if (((styles & Styles.CssClass) == 0) && ((s.styles & Styles.CssClass) != 0) && (s.CssClass != string.Empty)) 
			{
				this.CssClass = s.CssClass;
			}
			if (((styles & Styles.ForeColor) == 0) && ((s.styles & Styles.ForeColor) != 0) && (s.ForeColor != Color.Empty)) 
			{
				this.ForeColor = s.ForeColor;
			}
			if (((styles & Styles.Height) == 0) && ((s.styles & Styles.Height) != 0) && (s.Height != Unit.Empty)) 
			{
				this.Height = s.Height;
			}
			if (((styles & Styles.Width) == 0) && ((s.styles & Styles.Width) != 0) && (s.Width != Unit.Empty)) 
			{
				this.Width = s.Width;
			}
		}

		/*
		internal void Print ()
		{
			Console.WriteLine ("BackColor: {0}", BackColor);
			Console.WriteLine ("BorderColor: {0}", BorderColor);
			Console.WriteLine ("BorderStyle: {0}", BorderStyle);
			Console.WriteLine ("BorderWidth: {0}", BorderWidth);
			Console.WriteLine ("CssClass: {0}", CssClass);
			Console.WriteLine ("ForeColor: {0}", ForeColor);
			Console.WriteLine ("Height: {0}", Height);
			Console.WriteLine ("Width: {0}", Width);
		}
		*/

		public virtual void Reset() 
		{
			viewstate.Remove("BackColor");
			viewstate.Remove("BorderColor");
			viewstate.Remove("BorderStyle");
			viewstate.Remove("BorderWidth");
			viewstate.Remove("CssClass");
			viewstate.Remove("ForeColor");
			viewstate.Remove("Height");
			viewstate.Remove("Width");
			if (fontinfo != null) 
			{
				fontinfo.Reset();
			}
			styles = Styles.None;
		}
#if ONLY_1_1
		public override string ToString() 
		{
			return string.Empty;
		}
#endif
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		protected internal void LoadViewState(object state) 
		{
			viewstate.LoadViewState(state);

			// Update our style
			this.styles = Styles.None;

			if (viewstate["BackColor"] != null) 
			{
				styles |= Styles.BackColor;
			}
			if (viewstate["BorderColor"] != null) 
			{
				styles |= Styles.BorderColor;
			}
			if (viewstate["BorderStyle"] != null) 
			{
				styles |= Styles.BorderStyle;
			}
			if (viewstate["BorderWidth"] != null) 
			{
				styles |= Styles.BorderWidth;
			}
			if (viewstate["CssClass"] != null) 
			{
				styles |= Styles.CssClass;
			}
			if (viewstate["ForeColor"] != null) 
			{
				styles |= Styles.ForeColor;
			}
			if (viewstate["Height"] != null) 
			{
				styles |= Styles.Height;
			}
			if (viewstate["Width"] != null) 
			{
				styles |= Styles.Width;
			}
			if (fontinfo != null) {
				fontinfo.LoadViewState();
			}

			LoadViewStateInternal();
		}

		internal virtual void LoadViewStateInternal()
		{
			// Override me
		}

		protected internal virtual object SaveViewState () 
		{
			if (styles != Styles.None) 
			{
				return viewstate.SaveViewState();
			}
			return null;
		}

		[MonoTODO]
		protected internal virtual void SetBit( int bit ) 
		{
			throw new NotImplementedException();
		}

		protected internal virtual void TrackViewState() 
		{
			tracking = true;
			viewstate.TrackViewState();
		}
		#endregion	// Protected Instance Methods

		#region IStateManager Properties & Methods
		void IStateManager.LoadViewState(object state) 
		{
			LoadViewState(state);
		}

		object IStateManager.SaveViewState() 
		{
			return SaveViewState();
		}

		void IStateManager.TrackViewState() 
		{
			TrackViewState();
		}

		bool IStateManager.IsTrackingViewState 
		{
			get 
			{
				return this.IsTrackingViewState;
			}
		}
		#endregion	// IStateManager Properties & Methods

#if NET_2_0
		protected virtual void FillStyleAttributes (CssStyleCollection attributes, IUrlResolutionService urlResolver)
		{
			FillStyleAttributes (attributes);
		}

		internal void SetRegisteredCssClass (string name)
		{
			registered_class = name;
		}

		public CssStyleCollection GetStyleAttributes (IUrlResolutionService resolver)
		{
			CssStyleCollection col = new CssStyleCollection (new StateBag ());
			FillStyleAttributes (col, resolver);
			return col;
		}

		[MonoTODO]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public string RegisteredCssClass {
			get {
				if (registered_class == null)
					registered_class = String.Empty;
				return registered_class;
			}
		}

		internal virtual void CopyTextStylesFrom (Style source)
		{
			// Need to ask lluis if we need fonts, too
			if ((styles & Styles.ForeColor) != 0) {
				ForeColor = source.ForeColor;
			}
			if ((styles & Styles.BackColor) != 0) {
				BackColor = source.BackColor;
			}
		}

		public void SetDirty ()
		{
			if (viewstate != null)
				viewstate.SetDirty (true);
		}
#endif
	}
}
