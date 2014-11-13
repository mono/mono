//------------------------------------------------------------------------------
// <copyright file="WebControl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;
    using AttributeCollection = System.Web.UI.AttributeCollection;
    
    /// <devdoc>
    ///    <para> The base class for all Web controls. Defines the
    ///       methods, properties and events common to all controls within the
    ///       System.Web.UI.WebControls namespace.</para>
    /// </devdoc>
    [
    ParseChildren(true),
    PersistChildren(false),
    Themeable(true),
    ]
    public class WebControl : Control, IAttributeAccessor {

        private static string _disabledCssClass = "aspNetDisabled";
        private string tagName;
        private HtmlTextWriterTag tagKey;
        private AttributeCollection attrColl;
        private StateBag attrState;
        private Style controlStyle;
        #pragma warning disable 0649
        private SimpleBitVector32 _webControlFlags;
        #pragma warning restore 0649

        // const mask into the BitVector32
        // do not change without verifying other uses
        private const int deferStyleLoadViewState = 0x00000001;
        private const int disabledDirty           = 0x00000002;
        private const int accessKeySet            = 0x00000004;
        private const int toolTipSet              = 0x00000008;
        private const int tabIndexSet             = 0x00000010;


        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Web.UI.WebControls.WebControl'/> class and renders
        ///       it as a SPAN tag.
        ///    </para>
        /// </devdoc>
        protected WebControl() : this(HtmlTextWriterTag.Span) {
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public WebControl(HtmlTextWriterTag tag) {
            tagKey = tag;
        }


        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Web.UI.WebControls.WebControl'/> class and renders
        ///       it as the specified tag.
        ///    </para>
        /// </devdoc>
        protected WebControl(string tag) {
            tagKey = HtmlTextWriterTag.Unknown;
            tagName = tag;
        }



        /// <devdoc>
        ///    <para>Gets or sets the keyboard shortcut key (AccessKey) for setting focus to the
        ///       Web control.</para>
        /// </devdoc>
        [
        DefaultValue(""),
        WebCategory("Accessibility"),
        WebSysDescription(SR.WebControl_AccessKey)
        ]
        public virtual string AccessKey {
            get {
                if (_webControlFlags[accessKeySet]) {
                    string s = (string)ViewState["AccessKey"];
                    if (s != null) return s;
                }
                return String.Empty;
            }
            set {
                // Valid values are null, String.Empty, and single character strings
                if ((value != null) && (value.Length > 1)) {
                    throw new ArgumentOutOfRangeException("value", SR.GetString(SR.WebControl_InvalidAccessKey));
                }

                ViewState["AccessKey"] = value;
                _webControlFlags.Set(accessKeySet);
            }
        }


        /// <devdoc>
        ///    <para>Gets the collection of attribute name/value pairs expressed on a Web control but
        ///       not supported by the control's strongly typed properties.</para>
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        WebSysDescription(SR.WebControl_Attributes)
        ]
        public AttributeCollection Attributes {
            get {
                if (attrColl == null) {

                    if (attrState == null) {
                        attrState = new StateBag(true);
                        if (IsTrackingViewState)
                            attrState.TrackViewState();
                    }

                    attrColl = new AttributeCollection(attrState);
                }
                return attrColl;
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the background color of the Web control.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(typeof(Color), ""),
        WebSysDescription(SR.WebControl_BackColor),
        TypeConverterAttribute(typeof(WebColorConverter))
        ]
        public virtual Color BackColor {
            get {
                if (ControlStyleCreated == false) {
                    return Color.Empty;
                }
                return ControlStyle.BackColor;
            }
            set {
                ControlStyle.BackColor = value;
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the border color of the Web control.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(typeof(Color), ""),
        WebSysDescription(SR.WebControl_BorderColor),
        TypeConverterAttribute(typeof(WebColorConverter))
        ]
        public virtual Color BorderColor {
            get {
                if (ControlStyleCreated == false) {
                    return Color.Empty;
                }
                return ControlStyle.BorderColor;
            }
            set {
                ControlStyle.BorderColor = value;
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the border width of the Web control.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(typeof(Unit), ""),
        WebSysDescription(SR.WebControl_BorderWidth)
        ]
        public virtual Unit BorderWidth {
            get {
                if (ControlStyleCreated == false) {
                    return Unit.Empty;
                }
                return ControlStyle.BorderWidth;
            }
            set {
                ControlStyle.BorderWidth = value;
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the border style of the Web control.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(BorderStyle.NotSet),
        WebSysDescription(SR.WebControl_BorderStyle)
        ]
        public virtual BorderStyle BorderStyle {
            get {
                if (ControlStyleCreated == false) {
                    return BorderStyle.NotSet;
                }
                return ControlStyle.BorderStyle;
            }
            set {
                ControlStyle.BorderStyle = value;
            }
        }


        /// <devdoc>
        ///    <para>Gets the style of the Web control.</para>
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        WebSysDescription(SR.WebControl_ControlStyle)
        ]
        public Style ControlStyle {
            get {
                if (controlStyle == null) {
                    controlStyle = CreateControlStyle();
                    if (IsTrackingViewState) {
                        controlStyle.TrackViewState();
                    }
                    if (_webControlFlags[deferStyleLoadViewState]) {
                        _webControlFlags.Clear(deferStyleLoadViewState);
                        controlStyle.LoadViewState(null);
                    }
                }
                return controlStyle;
            }
        }


        /// <devdoc>
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        EditorBrowsable(EditorBrowsableState.Advanced),
        WebSysDescription(SR.WebControl_ControlStyleCreated)
        ]
        public bool ControlStyleCreated {
            get {
                return (controlStyle != null);
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the CSS class rendered by the Web control.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(""),
        WebSysDescription(SR.WebControl_CSSClassName),
        CssClassProperty()
        ]
        public virtual string CssClass {
            get {
                if (ControlStyleCreated == false) {
                    return String.Empty;
                }
                return ControlStyle.CssClass;
            }
            set {
                ControlStyle.CssClass = value;
            }
        }

        public static string DisabledCssClass {
            get {
                return _disabledCssClass ?? String.Empty;
            }
            set {
                _disabledCssClass = value;
            }
        }

        /// <devdoc>
        ///    <para> Gets a collection of text attributes that will be rendered as a style
        ///       attribute on the outer tag of the Web control.</para>
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        WebSysDescription(SR.WebControl_Style)
        ]
        public CssStyleCollection Style {
            get {
                return Attributes.CssStyle;
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets a value indicating whether the Web control is enabled.</para>
        /// </devdoc>
        [
        Bindable(true),
        WebCategory("Behavior"),
        Themeable(false),
        DefaultValue(true),
        WebSysDescription(SR.WebControl_Enabled)
        ]
        public virtual bool Enabled {
            [System.Runtime.TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get {
                return !flags[isWebControlDisabled];
            }
            set {
                bool enabled = !flags[isWebControlDisabled];
                if (enabled != value) {
                    if(!value) {
                        flags.Set(isWebControlDisabled);
                    }
                    else {
                        flags.Clear(isWebControlDisabled);
                    }

                    if (IsTrackingViewState) {
                        _webControlFlags.Set(disabledDirty);
                    }
                }
            }
        }


        /// <devdoc>
        ///    <para>Gets and sets a value indicating whether theme is enabled.</para>
        /// </devdoc>
        [
        Browsable(true)
        ]
        public override bool EnableTheming {
            get {
                return base.EnableTheming;
            }
            set {
                base.EnableTheming = value;
            }
        }


        /// <devdoc>
        ///    <para>Gets font information of the Web control.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        WebSysDescription(SR.WebControl_Font),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true)
        ]
        public virtual FontInfo Font {
            get {
                return ControlStyle.Font;
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the foreground color (typically the color of the text) of the
        ///       Web control.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(typeof(Color), ""),
        WebSysDescription(SR.WebControl_ForeColor),
        TypeConverterAttribute(typeof(WebColorConverter))
        ]
        public virtual Color ForeColor {
            get {
                if (ControlStyleCreated == false) {
                    return Color.Empty;
                }
                return ControlStyle.ForeColor;
            }
            set {
                ControlStyle.ForeColor = value;
            }
        }


        /// <devdoc>
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public bool HasAttributes {
            get {
                return (((attrColl != null) && (attrColl.Count > 0)) || ((attrState != null) && (attrState.Count > 0)));
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the height of the Web control.</para>
        /// </devdoc>
        [
        WebCategory("Layout"),
        DefaultValue(typeof(Unit), ""),
        WebSysDescription(SR.WebControl_Height)
        ]
        public virtual Unit Height {
            get {
                if (ControlStyleCreated == false) {
                    return Unit.Empty;
                }
                return ControlStyle.Height;
            }
            set {
                ControlStyle.Height = value;
            }
        }


        /// <devdoc>
        /// The effective enabled property value taking into account that a parent control maybe
        /// disabled.
        /// </devdoc>
        protected internal bool IsEnabled {
            get {
                Control current = this;
                while (current != null) {
                    if (current.flags[isWebControlDisabled]) {
                        return false;
                    }

                    current = current.Parent;
                }

                return true;
            }
        }

        [
        Browsable(false)
        ]
        public virtual bool SupportsDisabledAttribute {
            get {
                return true;
            }
        }

        internal virtual bool RequiresLegacyRendering {
            get {
                return false;
            }
        }


        /// <devdoc>
        ///    <para>Gets and sets the skinID of the control.</para>
        /// </devdoc>
        [
        Browsable(true)
        ]
        public override string SkinID {
            get {
                return base.SkinID;
            }
            set {
                base.SkinID = value;
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Gets or
        ///       sets the tab index of the Web control.</para>
        /// </devdoc>
        [
        DefaultValue((short)0),
        WebCategory("Accessibility"),
        WebSysDescription(SR.WebControl_TabIndex)
        ]
        public virtual short TabIndex {
            get {
                if (_webControlFlags[tabIndexSet]) {
                    object o = ViewState["TabIndex"];
                    if (o != null) return (short) o;
                }
                return (short)0;
            }
            set {
                ViewState["TabIndex"] = value;
                _webControlFlags.Set(tabIndexSet);
            }
        }


        /// <devdoc>
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        protected virtual HtmlTextWriterTag TagKey {
            [System.Runtime.TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get {
                return tagKey;
            }
        }


        /// <devdoc>
        ///    <para> A protected property. Gets the name of the control tag.</para>
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        protected virtual string TagName {
            get {
                if (tagName == null && TagKey != HtmlTextWriterTag.Unknown) {
                    // perf: this enum.format wasn't changed to a switch because the TagKey is normally used, not the TagName.
                    tagName = Enum.Format(typeof(HtmlTextWriterTag), TagKey, "G").ToLower(CultureInfo.InvariantCulture);
                }
                return tagName;
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the tool tip for the Web control to be displayed when the mouse
        ///       cursor is over the control.</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        Localizable(true),
        DefaultValue(""),
        WebSysDescription(SR.WebControl_Tooltip)
        ]
        public virtual string ToolTip {
            get {
                if (_webControlFlags[toolTipSet]) {
                    string s = (string)ViewState["ToolTip"];
                    if (s != null) return s;
                }
                return String.Empty;
            }
            set {
                ViewState["ToolTip"] = value;
                _webControlFlags.Set(toolTipSet);
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the width of the Web control.</para>
        /// </devdoc>
        [
        WebCategory("Layout"),
        DefaultValue(typeof(Unit), ""),
        WebSysDescription(SR.WebControl_Width)
        ]
        public virtual Unit Width {
            get {
                if (ControlStyleCreated == false) {
                    return Unit.Empty;
                }
                return ControlStyle.Width;
            }
            set {
                ControlStyle.Width = value;
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Adds to the specified writer those HTML attributes and styles that need to be
        ///       rendered.
        ///    </para>
        /// </devdoc>
        protected virtual void AddAttributesToRender(HtmlTextWriter writer) {
            if (this.ID != null) {
                writer.AddAttribute(HtmlTextWriterAttribute.Id, ClientID);
            }

            if (_webControlFlags[accessKeySet]) {
                string s = AccessKey;
                if (s.Length > 0) {
                    writer.AddAttribute(HtmlTextWriterAttribute.Accesskey, s);
                }
            }
            if (!Enabled) {
                if (SupportsDisabledAttribute) {
                    writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
                }
                if ((RenderingCompatibility >= VersionUtil.Framework40) && !String.IsNullOrEmpty(DisabledCssClass)) {
                    if (String.IsNullOrEmpty(CssClass)) {
                        ControlStyle.CssClass = DisabledCssClass;
                    }
                    else {
                        ControlStyle.CssClass = DisabledCssClass + " " + CssClass;
                    }
                }
            }
            if (_webControlFlags[tabIndexSet]) {
                int n = TabIndex;
                if (n != 0) {
                    writer.AddAttribute(HtmlTextWriterAttribute.Tabindex, n.ToString(NumberFormatInfo.InvariantInfo));
                }
            }
            if (_webControlFlags[toolTipSet]) {
                string s = ToolTip;
                if (s.Length > 0) {
                    writer.AddAttribute(HtmlTextWriterAttribute.Title, s);
                }
            }

            // VSWhidbey 496445: Setting the specific style display:inline-block common to <span> and <a> tag
            if (TagKey == HtmlTextWriterTag.Span || TagKey == HtmlTextWriterTag.A) {
                AddDisplayInlineBlockIfNeeded(writer);
            }

            if (ControlStyleCreated && !ControlStyle.IsEmpty) {
                // let the style add attributes
                ControlStyle.AddAttributesToRender(writer, this);
            }

            // add expando attributes
            if (attrState != null) {
                AttributeCollection atrColl = Attributes;
                IEnumerator keys = atrColl.Keys.GetEnumerator();
                while (keys.MoveNext()) {
                    string attrName = (string)(keys.Current);
                    writer.AddAttribute(attrName, atrColl[attrName]);
                }
            }
        }

        internal void AddDisplayInlineBlockIfNeeded(HtmlTextWriter writer) {
            // VSWhidbey 68250 and 460446
            if ((!RequiresLegacyRendering || !EnableLegacyRendering) &&
                (BorderStyle != BorderStyle.NotSet || !BorderWidth.IsEmpty || !Height.IsEmpty || !Width.IsEmpty)) {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "inline-block");
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Copies any non-blank elements of the specified style to the Web control,
        ///       overwriting any existing style elements of the control.
        ///    </para>
        /// </devdoc>
        public void ApplyStyle(Style s) {
            if ((s != null) && (s.IsEmpty == false)) {
                ControlStyle.CopyFrom(s);
            }
        }


        /// <devdoc>
        /// <para>Copies the <see cref='System.Web.UI.WebControls.WebControl.AccessKey'/>, <see cref='System.Web.UI.WebControls.WebControl.Enabled'/>, ToolTip, <see cref='System.Web.UI.WebControls.WebControl.TabIndex'/>, and <see cref='System.Web.UI.WebControls.WebControl.Attributes'/> properties onto the
        ///    Web control from the specified source control.</para>
        /// </devdoc>
        public void CopyBaseAttributes(WebControl controlSrc) {
            if (controlSrc == null) {
                throw new ArgumentNullException("controlSrc");
            }

            if (controlSrc._webControlFlags[accessKeySet]) {
                this.AccessKey = controlSrc.AccessKey;
            }
            if (!controlSrc.Enabled) {
                this.Enabled = false;
            }
            if (controlSrc._webControlFlags[toolTipSet]) {
                this.ToolTip = controlSrc.ToolTip;
            }
            if (controlSrc._webControlFlags[tabIndexSet]) {
                this.TabIndex = controlSrc.TabIndex;
            }

            if (controlSrc.HasAttributes) {
                foreach(string key in controlSrc.Attributes.Keys) {
                    this.Attributes[key] = controlSrc.Attributes[key];
                }
            }
        }


        /// <devdoc>
        ///    <para> A protected method. Creates the style object that is used internally
        ///       to implement all style-related properties. Controls may override to create an
        ///       appropriately typed style.</para>
        /// </devdoc>
        protected virtual Style CreateControlStyle() {
            return new Style(ViewState);
        }

        /// <internalonly/>
        /// <devdoc>
        ///    <para>Loads previously saved state.
        ///       Overridden to handle ViewState, Style, and Attributes.</para>
        /// </devdoc>
        protected override void LoadViewState(object savedState) {
            if (savedState != null) {
                Pair myState = (Pair)savedState;
                base.LoadViewState(myState.First);

                if (ControlStyleCreated || (ViewState[System.Web.UI.WebControls.Style.SetBitsKey] != null)) {
                    // the style shares the StateBag of its owner WebControl
                    // call LoadViewState to let style participate in state management
                    ControlStyle.LoadViewState(null);
                }
                else {
                    _webControlFlags.Set(deferStyleLoadViewState);
                }

                if (myState.Second != null) {
                    if (attrState == null) {
                        attrState = new StateBag(true);
                        attrState.TrackViewState();
                    }
                    attrState.LoadViewState(myState.Second);
                }
            }

            // Load values cached out of view state
            object enabled = ViewState["Enabled"];
            if (enabled != null) {
                if(!(bool)enabled) {
                    flags.Set(isWebControlDisabled);
                }
                else {
                    flags.Clear(isWebControlDisabled);
                }
                _webControlFlags.Set(disabledDirty);
            }

            if (((IDictionary)ViewState).Contains("AccessKey")) {
                _webControlFlags.Set(accessKeySet);
            }

            if (((IDictionary)ViewState).Contains("TabIndex")) {
                _webControlFlags.Set(tabIndexSet);
            }

            if (((IDictionary)ViewState).Contains("ToolTip")) {
                _webControlFlags.Set(toolTipSet);
            }

        }


        /// <internalonly/>
        /// <devdoc>
        ///    <para>Marks the beginning for tracking state changes on the control.
        ///       Any changes made after "mark" will be tracked and
        ///       saved as part of the control viewstate.</para>
        /// </devdoc>
        protected override void TrackViewState() {
            base.TrackViewState();

            if (ControlStyleCreated) {
                // the style shares the StateBag of its owner WebControl
                // call TrackState to let style participate in state management
                ControlStyle.TrackViewState();
            }

            if (attrState != null) {
                attrState.TrackViewState();
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Copies any non-blank elements of the specified style to the Web control, but
        ///       will not overwrite any existing style elements of the control.
        ///    </para>
        /// </devdoc>
        public void MergeStyle(Style s) {
            if ((s != null) && (s.IsEmpty == false)) {
                ControlStyle.MergeWith(s);
            }
        }

        // Renders the control into the specified writer.
        protected internal override void Render(HtmlTextWriter writer) {
            RenderBeginTag(writer);
            RenderContents(writer);
            RenderEndTag(writer);
        }

        public virtual void RenderBeginTag(HtmlTextWriter writer) {
            AddAttributesToRender(writer);

            HtmlTextWriterTag tagKey = TagKey;
            if (tagKey != HtmlTextWriterTag.Unknown) {
                writer.RenderBeginTag(tagKey);
            }
            else {
                writer.RenderBeginTag(this.TagName);
            }
        }

        // Renders the HTML end tag of the control into the specified writer.
        public virtual void RenderEndTag(HtmlTextWriter writer) {
            writer.RenderEndTag();
        }


        protected internal virtual void RenderContents(HtmlTextWriter writer) {
            base.Render(writer);
        }


        /// <internalonly/>
        /// <devdoc>
        ///    <para>A protected method. Saves any
        ///       state that was modified after the TrackViewState method was invoked.</para>
        /// </devdoc>
        protected override object SaveViewState() {
            Pair myState = null;

            // Save values cached out of view state
            if (_webControlFlags[disabledDirty]) {
                ViewState["Enabled"] = !flags[isWebControlDisabled];
            }

            if (ControlStyleCreated) {
                // the style shares the StateBag of its owner WebControl
                // call SaveViewState to let style participate in state management
                ControlStyle.SaveViewState();
            }

            object baseState = base.SaveViewState();
            object aState = null;
            if (attrState != null) {
                aState = attrState.SaveViewState();
            }

            if (baseState != null || aState != null) {
                myState = new Pair(baseState, aState);
            }
            return myState;
        }

        /// <internalonly/>
        /// <devdoc>
        /// Returns the attribute value of the Web control having
        /// the specified attribute name.
        /// </devdoc>
        string IAttributeAccessor.GetAttribute(string name) {
            return((attrState != null) ? (string)attrState[name] : null);
        }

        /// <internalonly/>
        /// <devdoc>
        /// <para>Sets an attribute of the Web control with the specified
        /// name and value.</para>
        /// </devdoc>
        void IAttributeAccessor.SetAttribute(string name, string value) {
            Attributes[name] = value;
        }
    }
}

