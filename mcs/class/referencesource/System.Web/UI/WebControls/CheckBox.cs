//------------------------------------------------------------------------------
// <copyright file="CheckBox.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;
    using AttributeCollection = System.Web.UI.AttributeCollection;


    /// <devdoc>
    ///    <para>Represents a Windows checkbox control.</para>
    /// </devdoc>
    [
    ControlValueProperty("Checked"),
    DataBindingHandler("System.Web.UI.Design.TextDataBindingHandler, " + AssemblyRef.SystemDesign),
    DefaultEvent("CheckedChanged"),
    Designer("System.Web.UI.Design.WebControls.CheckBoxDesigner, " + AssemblyRef.SystemDesign),
    DefaultProperty("Text"),
    SupportsEventValidation,
    ]
    public class CheckBox : WebControl, IPostBackDataHandler, ICheckBoxControl {
        internal AttributeCollection _inputAttributes;
        private StateBag _inputAttributesState;
        private AttributeCollection _labelAttributes;
        private StateBag _labelAttributesState;
        private string _valueAttribute = null;

        private static readonly object EventCheckedChanged = new object();


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.CheckBox'/> class.</para>
        /// </devdoc>
        public CheckBox() : base(HtmlTextWriterTag.Input) {
        }


        /// <devdoc>
        /// <para>Gets or sets a value indicating that the <see cref='System.Web.UI.WebControls.CheckBox'/> state is automatically posted back to
        ///    the
        ///    server.</para>
        /// </devdoc>
        [
        DefaultValue(false),
        WebCategory("Behavior"),
        WebSysDescription(SR.CheckBox_AutoPostBack),
        Themeable(false),
        ]
        public virtual bool AutoPostBack {
            get {
                object b = ViewState["AutoPostBack"];
                return((b == null) ? false : (bool)b);
            }
            set {
                ViewState["AutoPostBack"] = value;
            }
        }


        [
        DefaultValue(false),
        WebCategory("Behavior"),
        WebSysDescription(SR.AutoPostBackControl_CausesValidation),
        Themeable(false),
        ]
        public virtual bool CausesValidation {
            get {
                object b = ViewState["CausesValidation"];
                return((b == null) ? false : (bool)b);
            }
            set {
                ViewState["CausesValidation"] = value;
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets a value indicating the checked state of the
        ///    <see cref='System.Web.UI.WebControls.CheckBox'/>.</para>
        /// </devdoc>
        [
        Bindable(true, BindingDirection.TwoWay),
        DefaultValue(false),
        Themeable(false),
        WebSysDescription(SR.CheckBox_Checked),
        ]
        public virtual bool Checked {
            get {
                object b = ViewState["Checked"];
                return((b == null) ? false : (bool)b);
            }
            set {
                ViewState["Checked"] = value;
            }
        }


        /// <devdoc>
        ///    <para>Attribute collection for the rendered input element.</para>
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        WebSysDescription(SR.CheckBox_InputAttributes)
        ]
        public AttributeCollection InputAttributes {
            get {
                if (_inputAttributes == null) {

                    if (_inputAttributesState == null) {
                        _inputAttributesState = new StateBag(true);
                        if (IsTrackingViewState)
                            _inputAttributesState.TrackViewState();
                    }

                    _inputAttributes = new AttributeCollection(_inputAttributesState);
                }
                return _inputAttributes;
            }
        }


        /// <devdoc>
        ///    <para>Attribute collection for the rendered span element.</para>
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        WebSysDescription(SR.CheckBox_LabelAttributes)
        ]
        public AttributeCollection LabelAttributes {
            get {
                if (_labelAttributes == null) {

                    if (_labelAttributesState == null) {
                        _labelAttributesState = new StateBag(true);
                        if (IsTrackingViewState)
                            _labelAttributesState.TrackViewState();
                    }

                    _labelAttributes = new AttributeCollection(_labelAttributesState);
                }
                return _labelAttributes;
            }
        }


        internal override bool RequiresLegacyRendering {
            get {
                return true;
            }
        }


        /// <devdoc>
        ///   Controls whether the Checked property is saved in ViewState.
        ///   This is used for optimizing the size of the view state.
        /// </devdoc>
        private bool SaveCheckedViewState(bool autoPostBack) {
            // Must be saved when
            // 1. There is a registered event handler for SelectedIndexChanged
            // 2. Control is not enabled or visible, because the browser's post data will not include this control
            // 3. The instance is a derived instance, which might be overriding the OnSelectedIndexChanged method
            //    This is a bit hacky, since we have to cover all the four derived classes we have...
            // 4. AutoPostBack is true and Adapter doesn't support JavaScript
            //    For CheckBoxes to behave the same on mobile devices
            //    that simulate AutoPostBack by rendering a command button, we need to save
            //    state
            //    

            if ((Events[EventCheckedChanged] != null) ||
                (IsEnabled == false) ||
                (Visible == false) ||
                (autoPostBack == true && ((Page != null) && !Page.ClientSupportsJavaScript))) {

                return true;
            }

            Type t = this.GetType();
            if ((t == typeof(CheckBox)) || (t == typeof(RadioButton))) {
                return false;
            }

            return true;
        }


        /// <devdoc>
        /// <para>Gets or sets the text label associated with the <see cref='System.Web.UI.WebControls.CheckBox'/>.</para>
        /// </devdoc>
        [
        Bindable(true),
        Localizable(true),
        WebCategory("Appearance"),
        DefaultValue(""),
        WebSysDescription(SR.CheckBox_Text)
        ]
        public virtual string Text {
            get {
                string s = (string)ViewState["Text"];
                return((s == null) ? String.Empty : s);
            }
            set {
                ViewState["Text"] = value;
            }
        }


        /// <devdoc>
        /// <para>Gets or sets the alignment of the <see langword='Text'/> associated with the <see cref='System.Web.UI.WebControls.CheckBox'/>.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(TextAlign.Right),
        WebSysDescription(SR.WebControl_TextAlign)
        ]
        public virtual TextAlign TextAlign {
            get {
                object align = ViewState["TextAlign"];
                return((align == null) ? TextAlign.Right : (TextAlign)align);
            }
            set {
                if (value < TextAlign.Left || value > TextAlign.Right) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["TextAlign"] = value;
            }
        }


        [
        DefaultValue(""),
        Themeable(false),
        WebCategory("Behavior"),
        WebSysDescription(SR.PostBackControl_ValidationGroup),
        ]
        public virtual string ValidationGroup {
            get {
                string s = (string)ViewState["ValidationGroup"];
                return((s == null) ? String.Empty : s);
            }
            set {
                ViewState["ValidationGroup"] = value;
            }
        }


        /// <devdoc>
        /// <para>Occurs when the <see cref='System.Web.UI.WebControls.CheckBox'/> is clicked.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.Control_OnServerCheckChanged)
        ]
        public event EventHandler CheckedChanged {
            add {
                Events.AddHandler(EventCheckedChanged, value);
            }
            remove {
                Events.RemoveHandler(EventCheckedChanged, value);
            }
        }


        /// <devdoc>
        ///     Adds attributes to be rendered.
        /// </devdoc>
        protected override void AddAttributesToRender(HtmlTextWriter writer) {
            // VSWhidbey 460446
            AddDisplayInlineBlockIfNeeded(writer);

            // Everett's behavior is that we didn't call the base method.
        }


        /// <devdoc>
        ///     Loads the view state for the control.
        /// </devdoc>
        protected override void LoadViewState(object savedState) {
            if (savedState != null) {
                Triplet stateTriplet = (Triplet)savedState;
                base.LoadViewState(stateTriplet.First);
                if (stateTriplet.Second != null) {
                    if (_inputAttributesState == null) {
                        _inputAttributesState = new StateBag();
                        _inputAttributesState.TrackViewState();
                    }
                    _inputAttributesState.LoadViewState(stateTriplet.Second);
                }
                if (stateTriplet.Third != null) {
                    if (_labelAttributesState == null) {
                        _labelAttributesState = new StateBag();
                        _labelAttributesState.TrackViewState();
                    }
                    _labelAttributesState.LoadViewState(stateTriplet.Second);
                }
            }
        }


        /// <devdoc>
        ///    <para> Raises the
        ///    <see langword='CheckedChanged'/> event of the <see cref='System.Web.UI.WebControls.CheckBox'/>
        ///    controls.</para>
        /// </devdoc>
        protected virtual void OnCheckedChanged(EventArgs e) {
            EventHandler handler = (EventHandler)Events[EventCheckedChanged];
            if (handler != null) {
                handler(this, e);
            }
        }


        /// <devdoc>
        ///    <para>Registers client script for generating postback prior to
        ///       rendering on the client if <see cref='System.Web.UI.WebControls.CheckBox.AutoPostBack'/> is
        ///    <see langword='true'/>.</para>
        /// </devdoc>
        protected internal override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);
            bool autoPostBack = AutoPostBack;

            if (Page != null && IsEnabled) {
                // we always need to get post back data
                Page.RegisterRequiresPostBack(this);
                if (autoPostBack) {
                    Page.RegisterPostBackScript();
                    Page.RegisterFocusScript();

                    // VSWhidbey 489577: It would handle both CheckBox and RadioButton cases
                    if (CausesValidation && Page.GetValidators(ValidationGroup).Count > 0) {
                        Page.RegisterWebFormsScript();
                    }
                }
            }

            if (!SaveCheckedViewState(autoPostBack)) {
                ViewState.SetItemDirty("Checked", false);

                if ((Page != null) && IsEnabled) {
                    // Store a client-side array of enabled control, so we can re-enable them on
                    // postback (in case they are disabled client-side)
                    Page.RegisterEnabledControl(this);
                }
            }
        }


        /// <devdoc>
        ///     Saves the view state for the control.
        /// </devdoc>
        protected override object SaveViewState() {
            object baseState = base.SaveViewState();
            object inputState = null;
            object labelState = null;
            object myState = null;

            if (_inputAttributesState != null) {
                inputState = _inputAttributesState.SaveViewState();
            }
            if (_labelAttributesState != null) {
                labelState = _labelAttributesState.SaveViewState();
            }
            if (baseState != null || inputState != null || labelState != null) {
                myState = new Triplet(baseState, inputState, labelState);
            }
            return myState;
        }


        /// <devdoc>
        ///     Starts view state tracking.
        /// </devdoc>
        protected override void TrackViewState() {
            base.TrackViewState();
            if (_inputAttributesState != null) {
                _inputAttributesState.TrackViewState();
            }
            if (_labelAttributesState != null) {
                _labelAttributesState.TrackViewState();
            }
        }


        /// <internalonly/>
        /// <devdoc>
        /// <para>Displays the <see cref='System.Web.UI.WebControls.CheckBox'/> on the client.</para>
        /// </devdoc>
        protected internal override void Render(HtmlTextWriter writer) {
            AddAttributesToRender(writer);

            // Make sure we are in a form tag with runat=server.
            if (Page != null) {
                Page.VerifyRenderingInServerForm(this);
            }

            bool renderWrapper = false;

            // On wrapper, render ---- attribute and class according to RenderingCompatibility
            if (!IsEnabled) {
                if (RenderingCompatibility < VersionUtil.Framework40) {
                    writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
                    renderWrapper = true;
                }
                else if (!Enabled && !String.IsNullOrEmpty(DisabledCssClass)) {
                    if (String.IsNullOrEmpty(CssClass)) {
                        ControlStyle.CssClass = DisabledCssClass;
                    }
                    else {
                        ControlStyle.CssClass = DisabledCssClass + " " + CssClass;
                    }
                    renderWrapper = true;
                }
            }

            // And Style
            if (ControlStyleCreated) {
                Style controlStyle = ControlStyle;
                if (!controlStyle.IsEmpty) {
                    controlStyle.AddAttributesToRender(writer, this);
                    renderWrapper = true;
                }
            }
            // And ToolTip
            string toolTip = ToolTip;
            if (toolTip.Length > 0) {
                writer.AddAttribute(HtmlTextWriterAttribute.Title, toolTip);
                renderWrapper = true;
            }

            string onClick = null;
            // And other attributes
            if (HasAttributes) {
                AttributeCollection attribs = Attributes;

                // remove value from the attribute collection so it's not on the wrapper
                string val = attribs["value"];
                if (val != null)
                    attribs.Remove("value");

                // remove and save onclick from the attribute collection so we can move it to the input tag
                onClick = attribs["onclick"];
                if (onClick != null) {
                    onClick = Util.EnsureEndWithSemiColon(onClick);
                    attribs.Remove("onclick");
                }

                if (attribs.Count != 0)
                {
                    attribs.AddAttributes(writer);
                    renderWrapper = true;
                }

                if (val != null)
                    attribs["value"] = val;
            }

            // render begin tag of wrapper SPAN
            if (renderWrapper) {
                writer.RenderBeginTag(HtmlTextWriterTag.Span);
            }

            string text = Text;
            string clientID = ClientID;
            if (text.Length != 0) {
                if (TextAlign == TextAlign.Left) {
                    // render label to left of checkbox
                    RenderLabel(writer, text, clientID);
                    RenderInputTag(writer, clientID, onClick);
                }
                else {
                    // render label to right of checkbox
                    RenderInputTag(writer, clientID, onClick);
                    RenderLabel(writer, text, clientID);
                }
            }
            else
                RenderInputTag(writer, clientID, onClick);

            // render end tag of wrapper SPAN
            if (renderWrapper) {
                writer.RenderEndTag();
            }
        }

        private void RenderLabel(HtmlTextWriter writer, string text, string clientID) {
            writer.AddAttribute(HtmlTextWriterAttribute.For, clientID);

            if (_labelAttributes != null && _labelAttributes.Count != 0) {
                _labelAttributes.AddAttributes(writer);
            }

            writer.RenderBeginTag(HtmlTextWriterTag.Label);
            writer.Write(text);
            writer.RenderEndTag();
        }

        internal virtual void RenderInputTag(HtmlTextWriter writer, string clientID, string onClick) {
            if (clientID != null) {
                writer.AddAttribute(HtmlTextWriterAttribute.Id, clientID);
            }

            writer.AddAttribute(HtmlTextWriterAttribute.Type, "checkbox");

            if (UniqueID != null) {
                writer.AddAttribute(HtmlTextWriterAttribute.Name, UniqueID);
            }

            // Whidbey 20815
            if (_valueAttribute != null) {
                writer.AddAttribute(HtmlTextWriterAttribute.Value, _valueAttribute);
            }

            if (Checked)
                writer.AddAttribute(HtmlTextWriterAttribute.Checked, "checked");

            // ASURT 119141: Render ---- attribute on the INPUT tag (instead of the SPAN) so the checkbox actually gets disabled when Enabled=false
            if (!IsEnabled && SupportsDisabledAttribute) {
                writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
            }

            if (AutoPostBack && (Page != null) && Page.ClientSupportsJavaScript) {

                PostBackOptions options = new PostBackOptions(this, String.Empty);

                if (CausesValidation && Page.GetValidators(ValidationGroup).Count > 0) {
                    options.PerformValidation = true;
                    options.ValidationGroup = ValidationGroup;
                }

                if (Page.Form != null) {
                    options.AutoPostBack = true;
                }

                // ASURT 98368
                // Need to merge the autopostback script with the user script
                onClick = Util.MergeScript(onClick, Page.ClientScript.GetPostBackEventReference(options, true));
                writer.AddAttribute(HtmlTextWriterAttribute.Onclick, onClick);

                if (EnableLegacyRendering) {
                    writer.AddAttribute("language", "javascript", false);
                }
            }
            else {
                if (Page != null) {
                    Page.ClientScript.RegisterForEventValidation(this.UniqueID);
                }

                if (onClick != null) {
                    writer.AddAttribute(HtmlTextWriterAttribute.Onclick, onClick);
                }
            }

            string s = AccessKey;
            if (s.Length > 0)
                writer.AddAttribute(HtmlTextWriterAttribute.Accesskey, s);

            int i = TabIndex;
            if (i != 0)
                writer.AddAttribute(HtmlTextWriterAttribute.Tabindex, i.ToString(NumberFormatInfo.InvariantInfo));

            if (_inputAttributes != null && _inputAttributes.Count != 0) {
                _inputAttributes.AddAttributes(writer);
            }

            writer.RenderBeginTag(HtmlTextWriterTag.Input);
            writer.RenderEndTag();
        }

        /// <internalonly/>
        /// <devdoc>
        /// <para>Processes posted data for the <see cref='System.Web.UI.WebControls.CheckBox'/>
        /// control.</para>
        /// </devdoc>
        bool IPostBackDataHandler.LoadPostData(string postDataKey, NameValueCollection postCollection) {
            return LoadPostData(postDataKey, postCollection);
        }


        /// <internalonly/>
        /// <devdoc>
        /// <para>Processes posted data for the <see cref='System.Web.UI.WebControls.CheckBox'/>
        /// control.</para>
        /// </devdoc>
        protected virtual bool LoadPostData(string postDataKey, NameValueCollection postCollection) {
            bool dataChanged = false;

            string post = postCollection[postDataKey];

            bool isChecked = (!String.IsNullOrEmpty(post));

            if (isChecked) {
                ValidateEvent(postDataKey);
            }

            dataChanged = (isChecked != Checked);
            Checked = isChecked;

            return dataChanged;
        }


        /// <internalonly/>
        /// <devdoc>
        /// Raises when posted data for a control has changed.
        /// </devdoc>
        void IPostBackDataHandler.RaisePostDataChangedEvent() {
            RaisePostDataChangedEvent();
        }


        /// <internalonly/>
        /// <devdoc>
        /// Raises when posted data for a control has changed.
        /// </devdoc>
        protected virtual void RaisePostDataChangedEvent() {
            if (AutoPostBack && !Page.IsPostBackEventControlRegistered) {
                // VSWhidbey 204824
                Page.AutoPostBackControl = this;

                if (CausesValidation) {
                    Page.Validate(ValidationGroup);
                }
            }
            OnCheckedChanged(EventArgs.Empty);
        }
    }
}

