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
//	Peter Bartok	(pbartok@novell.com)
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
	[ParseChildren (true)]
#if NET_2_0
	[PersistChildrenAttribute (false, false)]
	[Themeable (true)]
#else	
	[PersistChildrenAttribute (false)]
#endif		
	public class WebControl : Control, IAttributeAccessor
	{
#if NET_4_0
		const string DEFAULT_DISABLED_CSS_CLASS = "aspNetDisabled";
#endif
		Style style;
		HtmlTextWriterTag tag;
		string tag_name;
		AttributeCollection attributes;
		StateBag attribute_state;
		bool enabled;
		bool track_enabled_state;
#if NET_4_0
		static WebControl ()
		{
			DisabledCssClass = DEFAULT_DISABLED_CSS_CLASS;
		}
#endif
		public WebControl (HtmlTextWriterTag tag) 
		{
			this.tag = tag;
			this.enabled = true;
		}

		protected WebControl () : this (HtmlTextWriterTag.Span) 
		{
		}

		protected WebControl (string tag) 
		{
			this.tag = HtmlTextWriterTag.Unknown;
			this.tag_name = tag;
			this.enabled = true;
		}

#if ONLY_1_1
		[Bindable(true)]
#endif		
		[DefaultValue("")]
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		public virtual string AccessKey {
			get {
				return ViewState.GetString ("AccessKey", string.Empty);
			}
			set {
				if (value == null || value.Length < 2)
					ViewState ["AccessKey"] = value;
				else
					throw new ArgumentException ("AccessKey can only be null, empty or a single character", "value");
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		public AttributeCollection Attributes {
			get {
				if (attributes == null) {
					attribute_state = new StateBag (true);
					if (IsTrackingViewState)
						attribute_state.TrackViewState ();
					
					attributes = new AttributeCollection (attribute_state);
				}
				return attributes;
			}
		}

#if ONLY_1_1
		[Bindable(true)]
#endif		
		[DefaultValue(typeof (Color), "")]
		[TypeConverter(typeof(System.Web.UI.WebControls.WebColorConverter))]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public virtual Color BackColor {
			get {
				if (style == null) 
					return Color.Empty;
				
				return style.BackColor;
			}
			set {
				ControlStyle.BackColor = value;
			}
		}

#if ONLY_1_1
		[Bindable(true)]
#endif		
		[DefaultValue(typeof (Color), "")]
		[TypeConverter(typeof(System.Web.UI.WebControls.WebColorConverter))]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public virtual Color BorderColor {
			get {
				if (style == null) 
					return Color.Empty;

				return style.BorderColor;
			}

			set {
				ControlStyle.BorderColor = value;
			}
		}

#if ONLY_1_1
		[Bindable(true)]
#endif		
		[DefaultValue(BorderStyle.NotSet)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public virtual BorderStyle BorderStyle {
			get {
				if (style == null) 
					return BorderStyle.NotSet;
				
				return style.BorderStyle;
			}
			set {
                                if (value < BorderStyle.NotSet || value > BorderStyle.Outset)
                                        throw new ArgumentOutOfRangeException ("value");

				ControlStyle.BorderStyle = value;
			}
		}

#if ONLY_1_1
		[Bindable(true)]
#endif		
		[DefaultValue(typeof (Unit), "")]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public virtual Unit BorderWidth {
			get {
				if (style == null) 
					return Unit.Empty;

				return style.BorderWidth;
			}
			set { ControlStyle.BorderWidth = value; }
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public Style ControlStyle {
			get {
				if (style == null) {
					style = this.CreateControlStyle ();

					if (IsTrackingViewState)
						style.TrackViewState ();
				}

				return style;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#if NET_2_0
		[EditorBrowsable (EditorBrowsableState.Never)]
#endif
		public bool ControlStyleCreated {
			get {
				return style != null;
			}
		}

#if ONLY_1_1
		[Bindable(true)]
#endif		
		[DefaultValue("")]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public virtual string CssClass {
			get {
				if (style == null) 
					return string.Empty;
				
				return style.CssClass;
			}
			set {
				ControlStyle.CssClass = value;
			}
		}

		[Bindable(true)]
		[DefaultValue(true)]
#if NET_2_0
		[Themeable (false)]
#endif		
		public virtual bool Enabled {
			get {
				return enabled;
			}

			set {
				if (enabled != value) {
					if (IsTrackingViewState)
						track_enabled_state = true;
					enabled = value;
				}
			}
		}

#if NET_2_0
		[Browsable (true)]
		public virtual new bool EnableTheming
		{
			get { return base.EnableTheming; }
			set { base.EnableTheming = value; }
		}
#endif		

#if ONLY_1_1
		[DefaultValue(null)]
#endif		
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[NotifyParentProperty(true)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public virtual FontInfo Font {
			get {
				// Oddly enough, it looks like we have to let it create the style
				// since we can't create a FontInfo without a style owner
				return ControlStyle.Font;
			}
		}

#if ONLY_1_1
		[Bindable(true)]
#endif		
		[DefaultValue(typeof (Color), "")]
		[TypeConverter(typeof(System.Web.UI.WebControls.WebColorConverter))]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public virtual Color ForeColor {
			get {
				if (style == null) 
					return Color.Empty;
				
				return style.ForeColor;
			}
			set {
				ControlStyle.ForeColor = value;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
#if NET_2_0
		public
#else
		internal
#endif
		bool HasAttributes 
		{
			get {
				return (attributes != null && attributes.Count > 0);
			}
		}
		
#if ONLY_1_1
		[Bindable(true)]
#endif		
		[DefaultValue(typeof (Unit), "")]
		[WebSysDescription ("")]
		[WebCategory ("Layout")]
		public virtual Unit Height {
			get {
				if (style == null) 
					return Unit.Empty;
				
				return style.Height;
			}
			set {
				ControlStyle.Height = value;
			}
		}

#if NET_2_0
		[Browsable (true)]
		public virtual new string SkinID
		{
			get { return base.SkinID; }
			set { base.SkinID = value; }
		}
#endif		
		
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[WebSysDescription ("")]
		[WebCategory ("Style")]
		public CssStyleCollection Style {
			get {
				return Attributes.CssStyle;
			}
		}

		[DefaultValue((short)0)]
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		public virtual short TabIndex {
			get {
				return ViewState.GetShort ("TabIndex", 0);
			}
			set {
				ViewState ["TabIndex"] = value;
			}
		}

#if ONLY_1_1
		[Bindable(true)]
#endif		
		[DefaultValue("")]
#if NET_2_0
		[Localizable (true)]
#endif		
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		public virtual string ToolTip {
			get {
				return ViewState.GetString ("ToolTip", string.Empty);
			}
			set {
				ViewState ["ToolTip"] = value;
			}
		}

#if ONLY_1_1
		[Bindable(true)]
#endif		
		[DefaultValue(typeof (Unit), "")]
		[WebSysDescription ("")]
		[WebCategory ("Layout")]
		public virtual Unit Width {
			get {
				if (style == null) 
					return Unit.Empty;
				
				return style.Width;
			}
			set {
				ControlStyle.Width = value;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected virtual HtmlTextWriterTag TagKey {
			get {
				return tag;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected virtual string TagName {
			get {
				// do this here to avoid potentially costly lookups on every control
				if (tag_name == null)
					tag_name = HtmlTextWriter.StaticGetTagName (TagKey);
				
				return tag_name;
			}
		}

#if NET_2_0
		protected
#endif
		internal bool IsEnabled	
		{
			get {
#if NET_2_0
				WebControl wc = this;
				while (wc != null) {
					if (!wc.Enabled)
						return false;
					wc = wc.Parent as WebControl;
				}
				return true;
#else
				return Enabled;
#endif
			}
		}
#if NET_4_0
		public static string DisabledCssClass {
			get;
			set;
		}
		
		[Browsable (false)]
		public virtual bool SupportsDisabledAttribute {
			get { return true; }
		}
#endif
		public void ApplyStyle (Style s) 
		{
			if (s != null && !s.IsEmpty)
				ControlStyle.CopyFrom(s);
		}

		public void CopyBaseAttributes (WebControl controlSrc) 
		{
			object o;

			if (controlSrc == null) 
				return;

			Enabled = controlSrc.Enabled;

			o = controlSrc.ViewState ["AccessKey"];
			if (o != null)
				ViewState ["AccessKey"] = o;

			o = controlSrc.ViewState ["TabIndex"];
			if (o != null)
				ViewState ["TabIndex"] = o;

			o = controlSrc.ViewState ["ToolTip"];
			if (o != null)
				ViewState ["ToolTip"] = o;

			if (controlSrc.attributes != null) {
				AttributeCollection attributes = Attributes;
				
				foreach (string s in controlSrc.attributes.Keys)
					attributes [s] = controlSrc.attributes [s];
			}
		}

		public void MergeStyle (Style s) 
		{
			if (s != null && !s.IsEmpty)
				ControlStyle.MergeWith(s);
		}

		public virtual void RenderBeginTag (HtmlTextWriter writer)
		{
			AddAttributesToRender (writer);
			
			if (TagKey == HtmlTextWriterTag.Unknown)
				writer.RenderBeginTag (TagName);
			else
				writer.RenderBeginTag (TagKey);
			
		}

		public virtual void RenderEndTag (HtmlTextWriter writer) 
		{
			writer.RenderEndTag ();
		}

		static char[] _script_trim_chars = {';'};
		internal string BuildScriptAttribute (string name, string tail)
		{
			AttributeCollection attrs = Attributes;
			string attr = attrs [name];
			
			if (attr == null || attr.Length == 0)
				return tail;
			if (attr [attr.Length - 1] == ';')
				attr = attr.TrimEnd (_script_trim_chars);
			
			attr = String.Concat (attr, ";", tail);
			attrs.Remove (name);
			
			return attr;
		}
		
#if NET_2_0
		internal void AddDisplayStyleAttribute (HtmlTextWriter writer)
		{
			if (!ControlStyleCreated)
				return;

			if (!ControlStyle.BorderWidth.IsEmpty ||
			    (ControlStyle.BorderStyle != BorderStyle.None && ControlStyle.BorderStyle != BorderStyle.NotSet) ||
			    !ControlStyle.Height.IsEmpty ||
			    !ControlStyle.Width.IsEmpty)
				writer.AddStyleAttribute (HtmlTextWriterStyle.Display, "inline-block");
		}
#endif
		void RenderDisabled (HtmlTextWriter writer)
		{
			if (!IsEnabled) {
#if NET_4_0
				if (!SupportsDisabledAttribute)
					ControlStyle.PrependCssClass (DisabledCssClass);
				else
#endif
					writer.AddAttribute (HtmlTextWriterAttribute.Disabled, "disabled", false);
			}

		}
		
		protected virtual void AddAttributesToRender (HtmlTextWriter writer) 
		{
#if NET_4_0
			RenderDisabled (writer);
#endif
			if (ID != null)
				writer.AddAttribute(HtmlTextWriterAttribute.Id, ClientID);
#if !NET_4_0
			RenderDisabled (writer);
#endif
			if (AccessKey != string.Empty)
				writer.AddAttribute (HtmlTextWriterAttribute.Accesskey, AccessKey);
			
			if (ToolTip != string.Empty)
				writer.AddAttribute (HtmlTextWriterAttribute.Title, ToolTip);

			if (TabIndex != 0)
				writer.AddAttribute (HtmlTextWriterAttribute.Tabindex, TabIndex.ToString ());

			if (style != null && !style.IsEmpty) {
#if NET_2_0
				//unbelievable, but see WebControlTest.RenderBeginTag_BorderWidth_xxx
				if (TagKey == HtmlTextWriterTag.Span)
					AddDisplayStyleAttribute (writer);
#endif
				style.AddAttributesToRender(writer, this);
			}

			if (attributes != null)
				foreach(string s in attributes.Keys)
					writer.AddAttribute (s, attributes [s]);
		}

		protected virtual Style CreateControlStyle() 
		{
			return new Style (ViewState);
		}

		protected override void LoadViewState (object savedState) 
		{
			if (savedState == null || !(savedState is Pair)) {
				base.LoadViewState (null);
				return;
			}

			Pair pair = (Pair) savedState;
			
			base.LoadViewState (pair.First);
			if (ViewState [System.Web.UI.WebControls.Style.BitStateKey] != null)
				ControlStyle.LoadBitState ();

			if (pair.Second != null) {
				if (attribute_state == null) {
					attribute_state = new StateBag ();
					if (IsTrackingViewState) 
						attribute_state.TrackViewState ();
				}

				attribute_state.LoadViewState (pair.Second);
				attributes = new AttributeCollection(attribute_state);
			}

			enabled = ViewState.GetBool ("Enabled", enabled);
		}

#if NET_2_0
		protected internal
#else		
		protected
#endif		
		override void Render (HtmlTextWriter writer)
		{
#if NET_2_0
			if (Adapter != null) {
				Adapter.Render(writer);
				return;
			}
#endif
			RenderBeginTag (writer);
			RenderContents (writer);
			RenderEndTag (writer);
		}

#if NET_2_0
		protected internal
#else		
		protected
#endif		
		virtual void RenderContents (HtmlTextWriter writer)
		{
			base.Render (writer);
		}

		protected override object SaveViewState () 
		{
			if (track_enabled_state)
				ViewState ["Enabled"] = enabled;

			object view_state;
			object attr_view_state = null;

			if (style != null)
				style.SaveBitState ();
			view_state = base.SaveViewState ();

			if (attribute_state != null)
				attr_view_state = attribute_state.SaveViewState ();
		
			if (view_state == null && attr_view_state == null)
				return null;

			return new Pair (view_state, attr_view_state);
		}

		protected override void TrackViewState() 
		{
			if (style != null)
				style.TrackViewState ();

			if (attribute_state != null) {
				attribute_state.TrackViewState ();
				attribute_state.SetDirty (true);
			}

			base.TrackViewState ();
		}

		string IAttributeAccessor.GetAttribute (string key) 
		{
			if (attributes != null)
				return attributes [key];

			return null;
		}

		void IAttributeAccessor.SetAttribute (string key, string value) 
		{
			Attributes [key] = value;
		}
	}
}
