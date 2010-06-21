//
// System.Web.UI.WebControls.CheckBox.cs
//
// Author:
//      Dick Porter  <dick@ximian.com>
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

using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Security.Permissions;
using System.Web.Util;

namespace System.Web.UI.WebControls {
	// CAS
	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[Designer ("System.Web.UI.Design.WebControls.CheckBoxDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[DataBindingHandler ("System.Web.UI.Design.TextDataBindingHandler, " + Consts.AssemblySystem_Design)]
	[DefaultEvent ("CheckedChanged")]
	[DefaultProperty ("Text")]
#if NET_2_0
	[ControlValueProperty ("Checked", null)]
	[SupportsEventValidation]
#endif		
	public class CheckBox : WebControl, IPostBackDataHandler
#if NET_2_0
	, ICheckBoxControl
#endif
	{
		string render_type;
		AttributeCollection common_attrs;

#if NET_2_0
		AttributeCollection inputAttributes;
		StateBag inputAttributesState;
		AttributeCollection labelAttributes;
		StateBag labelAttributesState;
#endif
		
		public CheckBox () : base (HtmlTextWriterTag.Input)
		{
			render_type = "checkbox";
		}

		internal CheckBox (string render_type) : base (HtmlTextWriterTag.Input)
		{
			this.render_type = render_type;
		}

		[DefaultValue (false)]
#if NET_2_0
		[Themeable (false)]
#endif		
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		public virtual bool AutoPostBack 
		{
			get {
				return (ViewState.GetBool ("AutoPostBack",
							   false));
			}
			set {
				ViewState["AutoPostBack"] = value;
			}
		}

#if NET_2_0
		[DefaultValue (false)]
		[Themeable (false)]
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		public virtual bool CausesValidation 
		{
			get { return ViewState.GetBool ("CausesValidation", false); }
			set { ViewState ["CausesValidation"] = value; }
		}
#endif		
		

		[DefaultValue (false)]
#if NET_2_0
		[Bindable (true, BindingDirection.TwoWay)]
		[Themeable (false)]
#else		
		[Bindable (true)]
#endif		
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		public virtual bool Checked 
		{
			get {
				return (ViewState.GetBool ("Checked", false));
			}
			set {
				ViewState["Checked"] = value;
			}
		}

#if NET_2_0
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public AttributeCollection InputAttributes 
		{
			get {
				if (inputAttributes == null) {
					if (inputAttributesState == null) {
						inputAttributesState = new StateBag (true);
						if (IsTrackingViewState)
							inputAttributesState.TrackViewState();
					}
					inputAttributes = new AttributeCollection (inputAttributesState);
				}
				return inputAttributes;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public AttributeCollection LabelAttributes
		{
			get {
				if (labelAttributes == null) {
					if (labelAttributesState == null) {
						labelAttributesState = new StateBag (true);
						if (IsTrackingViewState)
							labelAttributesState.TrackViewState();
					}
					labelAttributes = new AttributeCollection (labelAttributesState);
				}
				return labelAttributes;
			}
		}
#endif		

		[DefaultValue ("")]
		[Bindable (true)]
#if NET_2_0
		[Localizable (true)]
#endif		
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public virtual string Text 
		{
			get {
				return (ViewState.GetString ("Text",
							     String.Empty));
			}
			set {
				ViewState["Text"] = value;
			}
		}

		[DefaultValue (TextAlign.Right)]
#if ONLY_1_1
		[Bindable (true)]
#endif		
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public virtual TextAlign TextAlign
		{
			get { return (TextAlign) ViewState.GetInt ("TextAlign", (int)TextAlign.Right); }
			set {
				if (value != TextAlign.Left &&
				    value != TextAlign.Right) {
					throw new ArgumentOutOfRangeException ("value");
				}
				
				ViewState["TextAlign"] = value;
			}
		}

#if NET_2_0
		[Themeable (false)]
		[DefaultValue ("")]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Behavior")]
		public virtual string ValidationGroup
		{
			get { return ViewState.GetString ("ValidationGroup", String.Empty); }
			set { ViewState["ValidationGroup"] = value; }
		}
#endif		

		static readonly object EventCheckedChanged = new object ();
		[WebSysDescription ("")]
		[WebCategory ("Action")]
		public event EventHandler CheckedChanged 
		{
			add {
				Events.AddHandler (EventCheckedChanged, value);
			}
			remove {
				Events.RemoveHandler (EventCheckedChanged, value);
			}
		}

		protected virtual void OnCheckedChanged (EventArgs e)
		{
			EventHandler handler = (EventHandler)Events[EventCheckedChanged];
			
			if (handler != null) {
				handler (this, e);
			}
		}

		internal virtual string NameAttribute 
		{
			get {
				return (this.UniqueID);
			}
		}
		
#if NET_2_0
		protected override void LoadViewState (object savedState)
		{
			if (savedState == null) {
				base.LoadViewState (null);
				return;
			}

			Triplet saved = (Triplet) savedState;
			base.LoadViewState (saved.First);

			if (saved.Second != null) {
				if (inputAttributesState == null) {
					inputAttributesState = new StateBag(true);
					inputAttributesState.TrackViewState ();
				}
				inputAttributesState.LoadViewState (saved.Second);
			}

			if (saved.Third != null) {
				if (labelAttributesState == null) {
					labelAttributesState = new StateBag(true);
					labelAttributesState.TrackViewState ();
				}
				labelAttributesState.LoadViewState (saved.Third);
			}
		}

		protected override object SaveViewState ()
		{
			object baseView = base.SaveViewState ();
			object inputAttrView = null;
			object labelAttrView = null;

			if (inputAttributesState != null)
				inputAttrView = inputAttributesState.SaveViewState ();

			if (labelAttributesState != null)
				labelAttrView = labelAttributesState.SaveViewState ();

			if (baseView == null && inputAttrView == null && labelAttrView == null)
				return null;

			return new Triplet (baseView, inputAttrView, labelAttrView);		
		}

		protected override void TrackViewState ()
		{
			base.TrackViewState();
			if (inputAttributesState != null)
				inputAttributesState.TrackViewState ();
			if (labelAttributesState != null)
				labelAttributesState.TrackViewState ();
		}
#endif		

#if NET_2_0
		protected internal
#else		
		protected
#endif		
		override void OnPreRender (EventArgs e)
		{
			base.OnPreRender (e);
			Page page = Page;
			
			if (page != null && IsEnabled) {
				page.RegisterRequiresPostBack (this);
#if NET_2_0
				page.RegisterEnabledControl (this);
#endif
			}
		}

		static bool IsInputOrCommonAttr (string attname)
		{
			attname = attname.ToUpper (Helpers.InvariantCulture);
			switch (attname) {
			case "VALUE":
			case "CHECKED":
			case "SIZE":
			case "MAXLENGTH":
			case "SRC":
			case "ALT":
			case "USEMAP":
			case "DISABLED":
			case "READONLY":
			case "ACCEPT":
			case "ACCESSKEY":
			case "TABINDEX":
			case "ONFOCUS":
			case "ONBLUR":
			case "ONSELECT":
			case "ONCHANGE":
			case "ONCLICK":
			case "ONDBLCLICK":
			case "ONMOUSEDOWN":
			case "ONMOUSEUP":
			case "ONMOUSEOVER":
			case "ONMOUSEMOVE":
			case "ONMOUSEOUT":
			case "ONKEYPRESS":
			case "ONKEYDOWN":
			case "ONKEYUP":
				return true;
			default:
				return false;
			}
		}

		bool AddAttributesForSpan (HtmlTextWriter writer)
		{
			if (HasAttributes) {
				AttributeCollection attributes = Attributes;
				ICollection k = attributes.Keys;
				string [] keys = new string [k.Count];
				k.CopyTo (keys, 0);
				foreach (string key in keys) {
					if (!IsInputOrCommonAttr (key))
						continue;
					if (common_attrs == null)
						common_attrs = new AttributeCollection (new StateBag ());
					common_attrs [key] = Attributes [key];
					attributes.Remove (key);
				}
			
				if (attributes.Count > 0) {
					attributes.AddAttributes (writer);
					return true;
				}
			}
			
			return false;
		}
#if NET_2_0
		protected internal
#else		
		protected
#endif		
		override void Render (HtmlTextWriter w)
		{
			Page page = Page;
			if (page != null) {
				page.VerifyRenderingInServerForm (this);
#if NET_2_0
				page.ClientScript.RegisterForEventValidation (UniqueID);
#endif
			}
			
			bool need_span = ControlStyleCreated && !ControlStyle.IsEmpty;
			if (need_span) {
#if NET_2_0
				AddDisplayStyleAttribute (w);
#endif
				ControlStyle.AddAttributesToRender (w, this);
			}

			bool enabled = IsEnabled;
			if (!enabled) {
				w.AddAttribute (HtmlTextWriterAttribute.Disabled, "disabled", false);
				need_span = true;
			}

			string tt = ToolTip;
			if (tt != null && tt.Length > 0){
				w.AddAttribute ("title", tt);
				need_span = true;
			}

#if NET_2_0
			if (HasAttributes && AddAttributesForSpan (w))
				need_span = true;
#else
			if (Attributes.Count > 0 && AddAttributesForSpan (w))
				need_span = true;
#endif

			if (need_span)
				w.RenderBeginTag (HtmlTextWriterTag.Span);

			TextAlign align = TextAlign;
			if (align == TextAlign.Right) {
				RenderInput (w, enabled);
				RenderLabel (w);
			} else {
				RenderLabel (w);
				RenderInput (w, enabled);
			}

			if (need_span)
				w.RenderEndTag ();
		}

		void RenderInput (HtmlTextWriter w, bool enabled)
		{
			if (ClientID != null && ClientID.Length > 0)
				w.AddAttribute (HtmlTextWriterAttribute.Id, ClientID);
			w.AddAttribute (HtmlTextWriterAttribute.Type, render_type);
			string nameAttr = NameAttribute;
			if (nameAttr != null && nameAttr.Length > 0)
				w.AddAttribute (HtmlTextWriterAttribute.Name, nameAttr);
			InternalAddAttributesToRender (w, enabled);
#if NET_2_0
			AddAttributesToRender (w);
			if (inputAttributes != null)
				inputAttributes.AddAttributes (w);
#endif
			if (Checked)
				w.AddAttribute (HtmlTextWriterAttribute.Checked, "checked", false);

			if (AutoPostBack) {
#if NET_2_0
				string onclick = Page.ClientScript.GetPostBackEventReference (GetPostBackOptions (), true);
				onclick = String.Concat ("setTimeout('", onclick.Replace ("\\", "\\\\").Replace ("'", "\\'"), "', 0)");
				w.AddAttribute (HtmlTextWriterAttribute.Onclick, BuildScriptAttribute ("onclick", onclick));
#else
					w.AddAttribute (HtmlTextWriterAttribute.Onclick,
							BuildScriptAttribute ("onclick", Page.ClientScript.GetPostBackEventReference (this, String.Empty)));
					w.AddAttribute ("language", "javascript", false);
#endif
			}

			if (AccessKey.Length > 0)
				w.AddAttribute (HtmlTextWriterAttribute.Accesskey, AccessKey);

			if (TabIndex != 0)
				w.AddAttribute (HtmlTextWriterAttribute.Tabindex,
							 TabIndex.ToString (NumberFormatInfo.InvariantInfo));

			if (common_attrs != null)
				common_attrs.AddAttributes (w);
			w.RenderBeginTag (HtmlTextWriterTag.Input);
			w.RenderEndTag ();
		}

		void RenderLabel (HtmlTextWriter w) {
			string text = Text;
			if (text.Length > 0) {
#if NET_2_0
				if (labelAttributes != null)
					labelAttributes.AddAttributes (w);
#endif
				w.AddAttribute (HtmlTextWriterAttribute.For, ClientID);
				w.RenderBeginTag (HtmlTextWriterTag.Label);
				w.Write (text);
				w.RenderEndTag ();
			}
		}

#if NET_2_0
		protected virtual
#endif
		bool LoadPostData (string postDataKey, NameValueCollection postCollection)
		{
			if (!IsEnabled)
				return false;

			string postedValue = postCollection[postDataKey];
			bool postedBool = ((postedValue != null) &&
					   (postedValue.Length > 0));
			
			if (Checked != postedBool) {
				Checked = postedBool;
				return (true);
			}

			return (false);
		}

#if NET_2_0
		protected virtual
#endif
		void RaisePostDataChangedEvent ()
		{
#if NET_2_0
			ValidateEvent (UniqueID, String.Empty);
			if (CausesValidation)
				Page.Validate (ValidationGroup);
#endif
			OnCheckedChanged (EventArgs.Empty);
		}

		bool IPostBackDataHandler.LoadPostData (string postDataKey, NameValueCollection postCollection)
		{
			return LoadPostData (postDataKey, postCollection);
		}
		
		void IPostBackDataHandler.RaisePostDataChangedEvent ()
		{
			RaisePostDataChangedEvent ();
		}

#if NET_2_0
		PostBackOptions GetPostBackOptions () {
			PostBackOptions options = new PostBackOptions (this);
			options.ActionUrl = null;
			options.ValidationGroup = null;
			options.Argument = String.Empty;
			options.RequiresJavaScriptProtocol = false;
			options.ClientSubmit = true;
			options.PerformValidation = CausesValidation && Page != null && Page.AreValidatorsUplevel (ValidationGroup);
			if (options.PerformValidation)
				options.ValidationGroup = ValidationGroup;

			return options;
		}

		protected override void AddAttributesToRender (HtmlTextWriter writer)
		{
		}
#endif

		internal virtual void InternalAddAttributesToRender (HtmlTextWriter w, bool enabled)
		{
			if (!enabled)
				w.AddAttribute (HtmlTextWriterAttribute.Disabled, "disabled", false);
		}
	}
}
