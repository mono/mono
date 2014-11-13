//------------------------------------------------------------------------------
// <copyright file="BaseValidator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;

    /// <devdoc>
    ///    <para> Serves as the abstract base
    ///       class for validator objects.</para>
    /// </devdoc>
    [
    DefaultProperty("ErrorMessage"),
    Designer("System.Web.UI.Design.WebControls.BaseValidatorDesigner, " + AssemblyRef.SystemDesign),
    ]
    public abstract class BaseValidator : Label, IValidator {

        private const string UnobtrusivePrefix = "data-val-";
        private const string jqueryScriptKey = "jquery";

        // constants for Validation script library
        private const string ValidatorFileName = "WebUIValidation.js";
        private const string ValidatorIncludeScriptKey = "ValidatorIncludeScript";
        private const string ValidatorStartupScript = @"
var Page_ValidationActive = false;
if (typeof(ValidatorOnLoad) == ""function"") {
    ValidatorOnLoad();
}

function ValidatorOnSubmit() {
    if (Page_ValidationActive) {
        return ValidatorCommonOnSubmit();
    }
    else {
        return true;
    }
}
        ";

        private bool preRenderCalled;
        private bool isValid;
        private bool propertiesChecked;
        private bool propertiesValid;
        private bool renderUplevel;
        private bool wasForeColorSet = false;

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.BaseValidator'/> class.</para>
        /// </devdoc>
        protected BaseValidator() {
            isValid = true;
            propertiesChecked = false;
            propertiesValid = true;
            renderUplevel = false;
        }


        protected bool IsUnobtrusive {
            get {
                return (Page != null && Page.UnobtrusiveValidationMode != UnobtrusiveValidationMode.None);
            }
        }

        [
        Browsable(false),
        EditorBrowsable(EditorBrowsableState.Never)
        ]
        public override string AssociatedControlID {
            get {
                return base.AssociatedControlID;
            }
            set {
                throw new NotSupportedException(
                    SR.GetString(SR.Property_Not_Supported,
                                                     "AssociatedControlID",
                                                     this.GetType().ToString()));
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets
        ///       the text color of validation messages.</para>
        /// </devdoc>
        [
        DefaultValue(typeof(Color), "Red")
        ]
        public override Color ForeColor {
            get {
                return base.ForeColor;
            }
            set {
                wasForeColorSet = true;
                base.ForeColor = value;
            }
        }

        /// <devdoc>
        ///    <para>Gets or sets the control to validate.</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        Themeable(false),
        DefaultValue(""),
        IDReferenceProperty(),
        WebSysDescription(SR.BaseValidator_ControlToValidate),
        TypeConverter(typeof(ValidatedControlConverter))
        ]
        public string ControlToValidate {
            get {
                object o = ViewState["ControlToValidate"];
                return((o == null) ? String.Empty : (string)o);
            }
            set {
                ViewState["ControlToValidate"] = value;
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the text for the error message.</para>
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        DefaultValue(""),
        WebSysDescription(SR.BaseValidator_ErrorMessage)
        ]
        public string ErrorMessage {
            get {
                object o = ViewState["ErrorMessage"];
                return((o == null) ? String.Empty : (string)o);
            }
            set {
                ViewState["ErrorMessage"] = value;
            }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        Themeable(false),
        DefaultValue(true),
        WebSysDescription(SR.BaseValidator_EnableClientScript)
        ]
        public bool EnableClientScript {
            get {
                object o = ViewState["EnableClientScript"];
                return((o == null) ? true : (bool)o);
            }
            set {
                ViewState["EnableClientScript"] = value;
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets a value that indicates whether the validation for the control is
        ///       enabled.</para>
        /// </devdoc>
        public override bool Enabled {
            get {
                return base.Enabled;
            }
            set {
                base.Enabled= value;
                // When disabling a validator, it would almost always be intended for that validator
                // to not make the page invalid for that round-trip.
                if (!value) {
                    isValid = true;
                }
            }
        }

        // VSWhidbey 244999
        internal override bool IsReloadable {
            get {
                return true;
            }
        }


        /// <devdoc>
        ///    <para>Gets
        ///       or sets a flag to indicate if the referenced control passed
        ///       validation.</para>
        /// </devdoc>
        [
        Browsable(false),
        WebCategory("Behavior"),
        Themeable(false),
        DefaultValue(true),
        WebSysDescription(SR.BaseValidator_IsValid),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public bool IsValid {
            get {
                return isValid;
            }
            set {
                isValid = value;
            }
        }


        /// <devdoc>
        ///    <para>Gets a value that indicates whether the property of the control is valid. This property is read-only.</para>
        /// </devdoc>
        protected bool PropertiesValid {
            get {
                if (!propertiesChecked) {
                    propertiesValid = ControlPropertiesValid();
                    propertiesChecked = true;
                }
                return propertiesValid;
            }
        }


        /// <devdoc>
        ///    <para>Gets a value that indicates whether the client's browser supports uplevel rendering. This
        ///       property is read-only.</para>
        /// </devdoc>
        protected bool RenderUplevel {
            get {
                return renderUplevel;
            }
        }

        // This property should be Themeable. (DevDiv Bugs 156845)
        /// <devdoc>
        ///    <para>Gets or sets the display behavior of the
        ///       validator control.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        Themeable(true),
        DefaultValue(ValidatorDisplay.Static),
        WebSysDescription(SR.BaseValidator_Display)
        ]
        public ValidatorDisplay Display {
            get {
                object o = ViewState["Display"];
                return((o == null) ? ValidatorDisplay.Static : (ValidatorDisplay)o);
            }
            set {
                if (value < ValidatorDisplay.None || value > ValidatorDisplay.Dynamic) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["Display"] = value;
            }
        }


        /// <devdoc>
        ///    Whether the validator sets focus on the control when invalid
        /// </devdoc>
        [
        WebCategory("Behavior"),
        Themeable(false),
        DefaultValue(false),
        WebSysDescription(SR.BaseValidator_SetFocusOnError)
        ]
        public bool SetFocusOnError {
            get {
                object o = ViewState["SetFocusOnError"];
                return((o == null) ? false : (bool)o);
            }
            set {
                ViewState["SetFocusOnError"] = value;
            }
        }


        /// <devdoc>
        /// <para>Text to display for the validator when the validated control is invalid.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(""),
        WebSysDescription(SR.BaseValidator_Text),
        PersistenceMode(PersistenceMode.InnerDefaultProperty)
        ]
        public override string Text {
            // VSWhidbey 83105: Override the property only to override the description
            get {
                return base.Text;
            }
            set {
                base.Text = value;
            }
        }


        [
        WebCategory("Behavior"),
        Themeable(false),
        DefaultValue(""),
        WebSysDescription(SR.BaseValidator_ValidationGroup)
        ]
        public virtual string ValidationGroup {
            get {
                object o = ViewState["ValidationGroup"];
                return((o == null) ? string.Empty : (string)o);
            }
            set {
                ViewState["ValidationGroup"] = value;
            }
        }


        /// <internalonly/>
        /// <devdoc>
        ///    <para>Adds the attributes of this control to the output stream for rendering on the
        ///       client.</para>
        /// </devdoc>
        protected override void AddAttributesToRender(HtmlTextWriter writer) {
            // Validators do not render the "disabled" attribute, instead they are invisible when disabled.
            bool disabled = !Enabled;
            if (disabled) {
                Enabled = true;
            }

            try {
                if (RenderUplevel) {
                    // We always want validators to have an id on the client
                    EnsureID();
                    string id = ClientID;

                    // DevDiv Schedule 33075: Expando attributes are added through client-side JavaScript

                    // DevDiv 33149: A backward compat. switch for Everett rendering
                    HtmlTextWriter expandoAttributeWriter = (EnableLegacyRendering || IsUnobtrusive) ? writer : null;

                    if (IsUnobtrusive) {
                        Attributes["data-val"] = "true";
                    }

                    if (ControlToValidate.Length > 0) {
                        AddExpandoAttribute(expandoAttributeWriter, id, "controltovalidate", GetControlRenderID(ControlToValidate));
                    }
                    if (SetFocusOnError) {
                        AddExpandoAttribute(expandoAttributeWriter, id, "focusOnError", "t", false);
                    }
                    if (ErrorMessage.Length > 0) {
                        AddExpandoAttribute(expandoAttributeWriter, id, "errormessage", ErrorMessage);
                    }
                    ValidatorDisplay display = Display;
                    if (display != ValidatorDisplay.Static) {
                        AddExpandoAttribute(expandoAttributeWriter, id, "display", PropertyConverter.EnumToString(typeof(ValidatorDisplay), display), false);
                    }
                    if (!IsValid) {
                        AddExpandoAttribute(expandoAttributeWriter, id, "isvalid", "False", false);
                    }
                    if (disabled) {
                        AddExpandoAttribute(expandoAttributeWriter, id, "enabled", "False", false);
                    }
                    if (ValidationGroup.Length > 0) {
                        AddExpandoAttribute(expandoAttributeWriter, id, "validationGroup", ValidationGroup);
                    }
                }

                base.AddAttributesToRender(writer);
            }
            finally {
                // If exception happens above, we can still reset the property if needed
                if (disabled) {
                    Enabled = false;
                }
            }
        }

        internal void AddExpandoAttribute(HtmlTextWriter writer, string controlId, string attributeName, string attributeValue) {
            AddExpandoAttribute(writer, controlId, attributeName, attributeValue, true);
        }

        internal void AddExpandoAttribute(HtmlTextWriter writer, string controlId, string attributeName, string attributeValue, bool encode) {
            AddExpandoAttribute(this, writer, controlId, attributeName, attributeValue, encode);
        }

        internal static void AddExpandoAttribute(Control control, HtmlTextWriter writer, string controlId, string attributeName, string attributeValue, bool encode) {
            Debug.Assert(control != null);
            Page page = control.Page;
            Debug.Assert(page != null);

            // if writer is not null, assuming the expando attribute is written out explicitly
            if (writer != null) {
                if (page.UnobtrusiveValidationMode != UnobtrusiveValidationMode.None) {
                    attributeName = UnobtrusivePrefix + attributeName;
                }
                writer.AddAttribute(attributeName, attributeValue, encode);
            }
            else {
                Debug.Assert(page.UnobtrusiveValidationMode == UnobtrusiveValidationMode.None, "The writer must have been passed in the Unobtrusive mode");

                // Cannot use the overload of RegisterExpandoAttribute that takes a Control, since that method only works with AJAX 3.5,
                // and we need to support Validators in AJAX 1.0 (Windows OS Bugs 2015831).
                if (!page.IsPartialRenderingSupported) {
                    // Fall back to ASP.NET 2.0 behavior
                    page.ClientScript.RegisterExpandoAttribute(controlId, attributeName, attributeValue, encode);
                }
                else {
                    // Atlas Partial Rendering support
                    // ScriptManager exists, so call its instance' method for script registration
                    ValidatorCompatibilityHelper.RegisterExpandoAttribute(control, controlId, attributeName, attributeValue, encode);
                }
            }
        }


        /// <devdoc>
        ///    <para> Determines if the referenced control
        ///       has a validation property.</para>
        /// </devdoc>
        protected void CheckControlValidationProperty(string name, string propertyName) {
            // get the control using the relative name
            Control c = NamingContainer.FindControl(name);
            if (c == null) {
                throw new HttpException(
                                       SR.GetString(SR.Validator_control_not_found, name, propertyName, ID));
            }

            // get its validation property
            PropertyDescriptor prop = GetValidationProperty(c);
            if (prop == null) {
                throw new HttpException(
                                       SR.GetString(SR.Validator_bad_control_type, name, propertyName, ID));
            }

        }


        /// <devdoc>
        ///    <para>Determines if the properties are valid so that validation
        ///       is meaningful.</para>
        /// </devdoc>
        protected virtual bool ControlPropertiesValid() {
            // Check for blank control to validate
            string controlToValidate = ControlToValidate;
            if (controlToValidate.Length == 0) {
                throw new HttpException(
                                       SR.GetString(SR.Validator_control_blank, ID));
            }

            // Check that the property points to a valid control. Will throw and exception if not found
            CheckControlValidationProperty(controlToValidate, "ControlToValidate");

            return true;
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected virtual bool DetermineRenderUplevel() {

            // must be on a page
            Page page = Page;
            if (page == null || page.RequestInternal == null) {
                return false;
            }

            // Check the browser capabilities
            return (EnableClientScript
                        && page.Request.Browser.W3CDomVersion.Major >= 1
                        && page.Request.Browser.EcmaScriptVersion.CompareTo(new Version(1, 2)) >= 0);
        }


        /// <devdoc>
        ///    <para> TDB. Not
        ///       coded yet.</para>
        /// </devdoc>
        protected abstract bool EvaluateIsValid();


        /// <devdoc>
        ///    Gets the control indicated by the relative name and
        ///    returns an ID that can be used on the client.
        /// </devdoc>
        protected string GetControlRenderID(string name) {

            // get the control using the relative name
            Control c = FindControl(name);
            if (c == null) {
                Debug.Fail("We should have already checked for the presence of this");
                return String.Empty;
            }
            return c.ClientID;
        }



        /// <devdoc>
        ///    <para> Gets the validation value of the control
        ///       named relative to the validator.</para>
        /// </devdoc>
        protected string GetControlValidationValue(string name) {

            // get the control using the relative name
            Control c = NamingContainer.FindControl(name);
            if (c == null) {
                return null;
            }

            // get its validation property
            PropertyDescriptor prop = GetValidationProperty(c);
            if (prop == null) {
                return null;
            }

            // get its value as a string
            object value = prop.GetValue(c);
            if (value is ListItem) {
                return((ListItem) value).Value;
            }
            else if (value != null) {
                return value.ToString();
            }
            else {
                return string.Empty;
            }
        }


        /// <devdoc>
        ///    <para>Helper function to get the validation
        ///       property of a control if it exists.</para>
        /// </devdoc>
        public static PropertyDescriptor GetValidationProperty(object component) {
            ValidationPropertyAttribute valProp = (ValidationPropertyAttribute)TypeDescriptor.GetAttributes(component)[typeof(ValidationPropertyAttribute)];
            if (valProp != null && valProp.Name != null) {
                return TypeDescriptor.GetProperties(component, null)[valProp.Name];
            }
            return null;
        }


        /// <internalonly/>
        /// <devdoc>
        ///    <para> Registers the validator on the page.</para>
        /// </devdoc>
        protected internal override void OnInit(EventArgs e) {
            base.OnInit(e);

            if (!wasForeColorSet && (RenderingCompatibility < VersionUtil.Framework40 )) {
                // If the ForeColor wasn't already set, try to set our dynamic default value
                ForeColor = Color.Red;
            }

            Page.Validators.Add(this);
        }


        /// <internalonly/>
        /// <devdoc>
        ///    <para> Un-registers the validator on the page.</para>
        /// </devdoc>
        protected internal override void OnUnload(EventArgs e) {
            if (Page != null) {
                Page.Validators.Remove(this);
            }
            base.OnUnload(e);
        }


        /// <internalonly/>
        /// <devdoc>
        ///    <para>Checks the client brower and configures the
        ///       validator for compatibility prior to rendering. </para>
        /// </devdoc>

        protected internal override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);
            preRenderCalled = true;

            // force a requery of properties for render
            propertiesChecked = false;

            // VSWhidbey 83130, we should check properties during PreRender so
            // the checking applies to all deviecs.
            if (!PropertiesValid) {
                // In practice the call to the property PropertiesValid would
                // throw if bad things happen.
                Debug.Assert(false, "Exception should have been thrown if properties are invalid");
            }

            // work out uplevelness now
            renderUplevel = DetermineRenderUplevel();

            if (IsUnobtrusive && EnableClientScript) {
                RegisterUnobtrusiveScript();
            } 

            if (renderUplevel) {
                RegisterValidatorCommonScript();
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Registers code on the page for client-side validation.
        ///    </para>
        /// </devdoc>
        protected void RegisterValidatorCommonScript() {
            const string onSubmitScriptKey = "ValidatorOnSubmit";
            const string onSubmitScript = "if (typeof(ValidatorOnSubmit) == \"function\" && ValidatorOnSubmit() == false) return false;";

            // Cannot use the overloads of Register* that take a Control, since these methods only work with AJAX 3.5,
            // and we need to support Validators in AJAX 1.0 (Windows OS Bugs 2015831).
            if (!Page.IsPartialRenderingSupported) {
                if (Page.ClientScript.IsClientScriptBlockRegistered(typeof(BaseValidator), ValidatorIncludeScriptKey)) {
                    return;
                }

                Page.ClientScript.RegisterClientScriptResource(typeof(BaseValidator), ValidatorFileName);
                Page.ClientScript.RegisterOnSubmitStatement(typeof(BaseValidator), onSubmitScriptKey, onSubmitScript);
                if (!IsUnobtrusive) {
                    Page.ClientScript.RegisterStartupScript(typeof(BaseValidator), ValidatorIncludeScriptKey, ValidatorStartupScript, addScriptTags: true);
                }
            }
            else {
                // Register the original validation scripts but through the new ScriptManager APIs
                ValidatorCompatibilityHelper.RegisterClientScriptResource(this, typeof(BaseValidator), ValidatorFileName);
                ValidatorCompatibilityHelper.RegisterOnSubmitStatement(this, typeof(BaseValidator), onSubmitScriptKey, onSubmitScript);
                if (!IsUnobtrusive) {
                    ValidatorCompatibilityHelper.RegisterStartupScript(this, typeof(BaseValidator), ValidatorIncludeScriptKey, ValidatorStartupScript, addScriptTags: true);
                }
            }
        }

        internal void RegisterUnobtrusiveScript() {
            ClientScriptManager.EnsureJqueryRegistered();
            ValidatorCompatibilityHelper.RegisterClientScriptResource(this, jqueryScriptKey);
        }

        /// <devdoc>
        /// <para>Registers array declarations using the default array, <see langword='Page_Validators'/> .</para>
        /// </devdoc>
        protected virtual void RegisterValidatorDeclaration() {
            const string arrayName = "Page_Validators";
            string element = "document.getElementById(\"" + ClientID + "\")";
            
            // Cannot use the overloads of Register* that take a Control, since these methods only work with AJAX 3.5,
            // and we need to support Validators in AJAX 1.0 (Windows OS Bugs 2015831).
            if (!Page.IsPartialRenderingSupported) {
                Page.ClientScript.RegisterArrayDeclaration(arrayName, element);
            }
            else {
                ValidatorCompatibilityHelper.RegisterArrayDeclaration(this, arrayName, element);

                // Register a dispose script to make sure we clean up the page if we get destroyed
                // during an async postback.
                // We should technically use the ScriptManager.RegisterDispose() method here, but the original implementation
                // of Validators in AJAX 1.0 manually attached a dispose expando.  We added this code back in the product
                // late in the Orcas cycle, and we didn't want to take the risk of using RegisterDispose() instead.
                // (Windows OS Bugs 2015831)
                ValidatorCompatibilityHelper.RegisterStartupScript(this, typeof(BaseValidator), ClientID + "_DisposeScript",
                    String.Format(
                        CultureInfo.InvariantCulture,
                        @"
document.getElementById('{0}').dispose = function() {{
    Array.remove({1}, document.getElementById('{0}'));
}}
",
                        ClientID, arrayName), true);
            }
        }


        /// <internalonly/>
        /// <devdoc>
        ///    <para>Displays the control on the client.</para>
        /// </devdoc>
        protected internal override void Render(HtmlTextWriter writer) {
            bool shouldBeVisible;

            // VSWhidbey 347677, 398978: Backward Compat.: Skip property checking if the
            // validator doesn't have PreRender called and it is not in page control tree.
            if (DesignMode || (!preRenderCalled && Page == null)) {
                // This is for design time. In this case we don't want any expandos
                // created, don't want property checks and always want to be visible.
                propertiesChecked = true;
                propertiesValid = true;
                renderUplevel = false;
                shouldBeVisible = true;
            }
            else {
                shouldBeVisible = Enabled && !IsValid;
            }

            // No point rendering if we have errors
            if (!PropertiesValid) {
                return;
            }

            // Make sure we are in a form tag with runat=server.
            if (Page != null) {
                Page.VerifyRenderingInServerForm(this);
            }

            // work out what we are displaying
            ValidatorDisplay display = Display;
            bool displayContents;
            bool displayTags;
            if (RenderUplevel) {
                displayTags = true;
                displayContents = (display != ValidatorDisplay.None);
            }
            else {
                displayContents = (display != ValidatorDisplay.None && shouldBeVisible);
                displayTags = displayContents;
            }

            if (displayTags && RenderUplevel) {

                if (!IsUnobtrusive) {
                    // Put ourselves in the array
                    RegisterValidatorDeclaration();
                }

                // Set extra uplevel styles
                if (display == ValidatorDisplay.None
                    || (!shouldBeVisible && display == ValidatorDisplay.Dynamic)) {
                    Style["display"] = "none";
                }
                else if (!shouldBeVisible) {
                    Debug.Assert(display == ValidatorDisplay.Static, "Unknown Display Type");
                    Style["visibility"] = "hidden";
                }
            }

            // Display it
            if (displayTags) {
                RenderBeginTag(writer);
            }
            if (displayContents) {
                if (Text.Trim().Length > 0) {
                    RenderContents(writer);
                }
                else if (HasRenderingData()) {
                    base.RenderContents(writer);
                }
                else {
                    writer.Write(ErrorMessage);
                }
            }
            else if (!RenderUplevel && display == ValidatorDisplay.Static) {
                // For downlevel in static mode, render a space so that table cells do not render as empty
                writer.Write("&nbsp;");
            }
            if (displayTags) {
                RenderEndTag(writer);
            }
        }

        internal bool ShouldSerializeForeColor() {
            Color defaultForeColor = (RenderingCompatibility < VersionUtil.Framework40) ? Color.Red : Color.Empty;
            return defaultForeColor != ForeColor;
        }

        /// <devdoc>
        /// <para>Evaluates validity and updates the <see cref='System.Web.UI.WebControls.BaseValidator.IsValid'/> property.</para>
        /// </devdoc>
        public void Validate() {
            IsValid = true;
            if (!Visible || !Enabled) {
                return;
            }
            propertiesChecked = false;
            if (!PropertiesValid) {
                return;
            }
            IsValid = EvaluateIsValid();
            Debug.Trace("BaseValidator.Validate", "id:" + ID + ", evaluateIsValid = " + IsValid.ToString());
            if (!IsValid) {
                Page page = Page;
                if (page != null && SetFocusOnError) {
                    // Dev10 584609 Need to render ClientID not control id for auto focus to work
                    string validateId = ControlToValidate;
                    Control c = NamingContainer.FindControl(validateId);
                    if (c != null) {
                        validateId = c.ClientID;
                    }

                    Page.SetValidatorInvalidControlFocus(validateId);
                }
            }
        }
    }
}
