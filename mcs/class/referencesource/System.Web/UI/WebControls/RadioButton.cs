//------------------------------------------------------------------------------
// <copyright file="RadioButton.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Web;
    using System.Web.UI;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;

    /// <devdoc>
    ///    <para>Constructs a radio button and defines its
    ///       properties.</para>
    /// </devdoc>
    [
    Designer("System.Web.UI.Design.WebControls.CheckBoxDesigner, " + AssemblyRef.SystemDesign),
    SupportsEventValidation,
    ]
    public class RadioButton : CheckBox, IPostBackDataHandler {

        private string _uniqueGroupName = null;


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.RadioButton'/> class.</para>
        /// </devdoc>
        public RadioButton() {
        }


        /// <devdoc>
        ///    <para>Gets or
        ///       sets the name of the group that the radio button belongs to.</para>
        /// </devdoc>
        [
        DefaultValue(""),
        WebCategory("Behavior"),
        WebSysDescription(SR.RadioButton_GroupName),
        Themeable(false),
        ]
        public virtual string GroupName {
            get {
                string s = (string)ViewState["GroupName"];
                return((s == null) ? String.Empty : s);
            }
            set {
                ViewState["GroupName"] = value;
            }
        }

        // Fully qualified GroupName for rendering purposes, to take care of conflicts
        // between different naming containers
        internal string UniqueGroupName {
            get {
                if (_uniqueGroupName == null) {
                    // For radio buttons, we must make the groupname unique, but can't just use the
                    // UniqueID because all buttons in a group must have the same name.  So
                    // we replace the last part of the UniqueID with the group Name.
                    string name = GroupName;
                    string uid = UniqueID;

                    if (uid != null) {
                        int lastColon = uid.LastIndexOf(IdSeparator);
                        if (lastColon >= 0) {
                            if (name.Length > 0) {
                                name = uid.Substring(0, lastColon+1) + name;
                            }
                            else if (NamingContainer is RadioButtonList) {
                                // If GroupName is not set we simply use the naming
                                // container as the group name
                                name = uid.Substring(0, lastColon);
                            }
                        }

                        if (name.Length == 0) {
                            name = uid;
                        }
                    }

                    _uniqueGroupName = name;
                }
                return _uniqueGroupName;
            }
        }


        /// <devdoc>
        /// </devdoc>
        internal string ValueAttribute {
            get {
                string valueAttr = Attributes["value"];
                if (valueAttr == null) {

                    // VSWhidbey 146829. Always EnsureID so the valueAttribute will not change by
                    // call to ClientID which happens during Render.
                    EnsureID();

                    if (ID != null)
                        valueAttr = ID;
                    else
                        valueAttr = UniqueID;
                }

                return valueAttr;
            }
        }


        /// <internalonly/>
        /// <devdoc>
        ///    <para>Method of IPostBackDataHandler interface to process posted data.
        ///       RadioButton determines the posted radio group state.</para>
        /// </devdoc>
        bool IPostBackDataHandler.LoadPostData(String postDataKey, NameValueCollection postCollection) {
            return LoadPostData(postDataKey, postCollection);
        }


        /// <internalonly/>
        /// <devdoc>
        ///    <para>Method of IPostBackDataHandler interface to process posted data.
        ///       RadioButton determines the posted radio group state.</para>
        /// </devdoc>
        protected override bool LoadPostData(String postDataKey, NameValueCollection postCollection) {
            string post = postCollection[UniqueGroupName];
            bool valueChanged = false;
            if ((post != null) && post.Equals(ValueAttribute)) {

                ValidateEvent(UniqueGroupName, post);

                if (Checked == false) {
                    Checked = true;
                    // only fire change event for RadioButton that is being checked
                    valueChanged = true;
                }
            }
            else {
                if (Checked == true) {
                    Checked = false;
                }
            }

            return valueChanged;
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
        protected override void RaisePostDataChangedEvent() {
            if (AutoPostBack && !Page.IsPostBackEventControlRegistered) {
                // VSWhidbey 204824
                Page.AutoPostBackControl = this;

                if (CausesValidation) {
                    Page.Validate(ValidationGroup);
                }
            }
            OnCheckedChanged(EventArgs.Empty);
        }


        /// <internalonly/>
        /// <devdoc>
        ///    This method is invoked just prior to rendering.
        ///    Register client script for handling postback if onChangeHandler is set.
        /// </devdoc>
        protected internal override void OnPreRender(EventArgs e) {
            // must call CheckBox PreRender
            base.OnPreRender(e);

            if (Page != null && !Checked && Enabled) {
                Page.RegisterRequiresPostBack(this);
            }
        }

        internal override void RenderInputTag(HtmlTextWriter writer, string clientID, string onClick) {

            writer.AddAttribute(HtmlTextWriterAttribute.Id, clientID);
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "radio");
            writer.AddAttribute(HtmlTextWriterAttribute.Name, UniqueGroupName);
            writer.AddAttribute(HtmlTextWriterAttribute.Value, ValueAttribute);

            if (Page != null) {
                Page.ClientScript.RegisterForEventValidation(UniqueGroupName, ValueAttribute);
            }

            if (Checked)
                writer.AddAttribute(HtmlTextWriterAttribute.Checked, "checked");

            // ASURT 119141: Render ---- attribute on the INPUT tag (instead of the SPAN) so the checkbox actually gets disabled when Enabled=false
            if (!IsEnabled && SupportsDisabledAttribute) {
                writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
            }

            // We don't do autopostback if the radio button has been selected.
            // This is to make it consistent that it only posts back if its
            // state has been changed.  Also, it avoids the problem of missing
            // validation since the data changed event would not be fired if the
            // selected radio button was posting back.
            if (AutoPostBack && !Checked && Page != null) {

                PostBackOptions options = new PostBackOptions(this, String.Empty);

                // ASURT 98368
                // Need to merge the autopostback script with the user script
                if (CausesValidation) {
                    options.PerformValidation = true;
                    options.ValidationGroup = ValidationGroup;
                }

                if (Page.Form != null) {
                    options.AutoPostBack = true;
                }

                onClick = Util.MergeScript(onClick, Page.ClientScript.GetPostBackEventReference(options));
                writer.AddAttribute(HtmlTextWriterAttribute.Onclick, onClick);
                if (EnableLegacyRendering) {
                    writer.AddAttribute("language", "javascript", false);
                }
            }
            else {
                if (onClick != null) {
                    writer.AddAttribute(HtmlTextWriterAttribute.Onclick, onClick);
                }
            }

            string s = AccessKey;
            if (s.Length > 0)
                writer.AddAttribute(HtmlTextWriterAttribute.Accesskey, s);

            int i = TabIndex;
            if (i != 0) {
                writer.AddAttribute(HtmlTextWriterAttribute.Tabindex, i.ToString(NumberFormatInfo.InvariantInfo));
            }

            if (_inputAttributes != null && _inputAttributes.Count != 0) {
                _inputAttributes.AddAttributes(writer);
            }

            writer.RenderBeginTag(HtmlTextWriterTag.Input);
            writer.RenderEndTag();
        }
    }
}

