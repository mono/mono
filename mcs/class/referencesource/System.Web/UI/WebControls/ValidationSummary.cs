//------------------------------------------------------------------------------
// <copyright file="ValidationSummary.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System.ComponentModel;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Globalization;
    using System.Web;
    using System.Web.ModelBinding;
    using System.Web.Util;

    /// <devdoc>
    ///    <para>Displays a summary of all validation errors of
    ///       a page in a list, bulletted list, or single paragraph format. The errors can be displayed inline
    ///       and/or in a popup message box.</para>
    /// </devdoc>
    [Designer("System.Web.UI.Design.WebControls.ValidationSummaryDesigner, " + AssemblyRef.SystemDesign)]
    public class ValidationSummary : WebControl {

        private const String breakTag = "b";

        private bool renderUplevel;
        private bool wasForeColorSet = false;

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.ValidationSummary'/> class.</para>
        /// </devdoc>
        public ValidationSummary() : base(HtmlTextWriterTag.Div) {
            renderUplevel = false;
        }

        private bool IsUnobtrusive {
            get {
                return (Page != null && Page.UnobtrusiveValidationMode != UnobtrusiveValidationMode.None);
            }
        }

        /// <devdoc>
        ///    <para>Gets or sets the display mode of the validation summary.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(ValidationSummaryDisplayMode.BulletList),
        WebSysDescription(SR.ValidationSummary_DisplayMode)
        ]
        public ValidationSummaryDisplayMode DisplayMode {
            get {
                object o = ViewState["DisplayMode"];
                return((o == null) ? ValidationSummaryDisplayMode.BulletList : (ValidationSummaryDisplayMode)o);
            }
            set {
                if (value < ValidationSummaryDisplayMode.List || value > ValidationSummaryDisplayMode.SingleParagraph) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["DisplayMode"] = value;
            }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        Themeable(false),
        DefaultValue(true),
        WebSysDescription(SR.ValidationSummary_EnableClientScript)
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
        ///    <para>Gets or sets a value indicating whether the validation
        ///       summary from validators should be shown. Default value is true.</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        Themeable(false),
        DefaultValue(true),
        WebSysDescription(SR.ValidationSummary_ShowValidationErrors)
        ]
        public bool ShowValidationErrors {
            get {
                object o = ViewState["ShowValidationErrors"];
                return ((o == null) ? true : (bool)o);
            }
            set {
                ViewState["ShowValidationErrors"] = value;
            }
        }

        /// <devdoc>
        ///    <para>Gets or sets a value indicating whether the model state
        ///       errors from a data operation should be shown. Default value is true.</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        Themeable(false),
        DefaultValue(true),
        WebSysDescription(SR.ValidationSummary_ShowModelStateErrors)
        ]
        public bool ShowModelStateErrors {
            get {
                object o = ViewState["ShowModelStateErrors"];
                return ((o == null) ? true : (bool)o);
            }
            set {
                ViewState["ShowModelStateErrors"] = value;
            }
        }

        /// <devdoc>
        ///    <para>Gets or sets the foreground color
        ///       (typically the color of the text) of the control.</para>
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
        ///    <para>Gets or sets the header text to be displayed at the top
        ///       of the summary.</para>
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        DefaultValue(""),
        WebSysDescription(SR.ValidationSummary_HeaderText)
        ]
        public string HeaderText {
            get {
                object o = ViewState["HeaderText"];
                return((o == null) ? String.Empty : (string)o);
            }
            set {
                ViewState["HeaderText"] = value;
            }
        }

        public override bool SupportsDisabledAttribute {
            get {
                return RenderingCompatibility < VersionUtil.Framework40;
            }
        }

        /// <devdoc>
        ///    <para>Gets or sets a value indicating whether the validation
        ///       summary is to be displayed in a pop-up message box.</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(false),
        WebSysDescription(SR.ValidationSummary_ShowMessageBox)
        ]
        public bool ShowMessageBox {
            get {
                object o = ViewState["ShowMessageBox"];
                return((o == null) ? false : (bool)o);
            }
            set {
                ViewState["ShowMessageBox"] = value;
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets a value indicating whether the validation
        ///       summary is to be displayed inline.</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(true),
        WebSysDescription(SR.ValidationSummary_ShowSummary)
        ]
        public bool ShowSummary {
            get {
                object o = ViewState["ShowSummary"];
                return((o == null) ? true : (bool)o);
            }
            set {
                ViewState["ShowSummary"] = value;
            }
        }


        [
        WebCategory("Behavior"),
        Themeable(false),
        DefaultValue(""),
        WebSysDescription(SR.ValidationSummary_ValidationGroup)
        ]
        public virtual string ValidationGroup {
            get {
                string s = (string)ViewState["ValidationGroup"];
                return((s == null) ? string.Empty : s);
            }
            set {
                ViewState["ValidationGroup"] = value;
            }
        }


        /// <internalonly/>
        /// <devdoc>
        ///    AddAttributesToRender method.
        /// </devdoc>
        protected override void AddAttributesToRender(HtmlTextWriter writer) {
            if (renderUplevel) {
                // We always want validation cotnrols to have an id on the client
                EnsureID();
                string id = ClientID;

                // DevDiv 33149: A backward compat. switch for Everett rendering
                HtmlTextWriter expandoAttributeWriter = (EnableLegacyRendering || IsUnobtrusive) ? writer : null;

                if (IsUnobtrusive) {
                    Attributes["data-valsummary"] = "true";
                }

                if (HeaderText.Length > 0 ) {
                    BaseValidator.AddExpandoAttribute(this, expandoAttributeWriter, id, "headertext", HeaderText, true);
                }
                if (ShowMessageBox) {
                    BaseValidator.AddExpandoAttribute(this, expandoAttributeWriter, id, "showmessagebox", "True", false);
                }
                if (!ShowSummary) {
                    BaseValidator.AddExpandoAttribute(this, expandoAttributeWriter, id, "showsummary", "False", false);
                }
                if (DisplayMode != ValidationSummaryDisplayMode.BulletList) {
                    BaseValidator.AddExpandoAttribute(this, expandoAttributeWriter, id, "displaymode", PropertyConverter.EnumToString(typeof(ValidationSummaryDisplayMode), DisplayMode), false);
                }
                if (ValidationGroup.Length > 0) {
                    BaseValidator.AddExpandoAttribute(this, expandoAttributeWriter, id, "validationGroup", ValidationGroup, true);
                }
            }

            base.AddAttributesToRender(writer);
        }

        internal String[] GetErrorMessages(out bool inError) {
            // Fetch errors from the Page
            List<string> errorDescriptions = new List<string>();
            inError = false;

            if (ShowValidationErrors) {
                // see if we are in error and how many messages there are
                ValidatorCollection validators = Page.GetValidators(ValidationGroup);
                for (int i = 0; i < validators.Count; i++) {
                    IValidator val = validators[i];
                    if (!val.IsValid) {
                        inError = true;
                        if (!String.IsNullOrEmpty(val.ErrorMessage)) {
                            errorDescriptions.Add(String.Copy(val.ErrorMessage));
                        }
                        else {
                            Debug.Assert(true, "Not all messages were found!");
                        }
                    }
                }
            }

            if (ShowModelStateErrors) {
                ModelStateDictionary modelState = Page.ModelState;
                if (!modelState.IsValid) {
                    inError = true;
                    foreach (KeyValuePair<string, ModelState> pair in modelState) {
                        foreach (ModelError error in pair.Value.Errors) {
                            if (!String.IsNullOrEmpty(error.ErrorMessage)) {
                                errorDescriptions.Add(error.ErrorMessage);
                            }
                        }
                    }
                }
            }

            return errorDescriptions.ToArray();
        }

        /// <internalonly/>
        /// <devdoc>
        ///    <para> Dynamically setting the Default ForeColor</para>
        /// </devdoc>
        protected internal override void OnInit(EventArgs e) {
            base.OnInit(e);

            if (!wasForeColorSet && (RenderingCompatibility < VersionUtil.Framework40)) {
                // If the ForeColor wasn't already set, try to set our dynamic default value
                ForeColor = Color.Red;
            }
        }

        /// <internalonly/>
        /// <devdoc>
        ///    PreRender method.
        /// </devdoc>
        protected internal override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);

            // Act like invisible if disabled
            if (!Enabled) {
                return;
            }

            // work out uplevelness now
            Page page = Page;
            if (page != null && page.RequestInternal != null) {
                renderUplevel = (EnableClientScript && ShowValidationErrors
                                 && page.Request.Browser.W3CDomVersion.Major >= 1
                                 && page.Request.Browser.EcmaScriptVersion.CompareTo(new Version(1, 2)) >= 0);
            }
            if (renderUplevel && !IsUnobtrusive) {
                const string arrayName = "Page_ValidationSummaries";
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
                    ValidatorCompatibilityHelper.RegisterStartupScript(this, typeof(ValidationSummary), ClientID + "_DisposeScript",
                        String.Format(
                            CultureInfo.InvariantCulture,
                            @"
(function(id) {{
    var e = document.getElementById(id);
    if (e) {{
        e.dispose = function() {{
            Array.remove({1}, document.getElementById(id));
        }}
        e = null;
    }}
}})('{0}');
",
                            ClientID, arrayName), true);
                }
            }
        }


        /// <internalonly/>
        /// <devdoc>
        ///    Render method.
        /// </devdoc>
        protected internal override void Render(HtmlTextWriter writer) {
            string [] errorDescriptions;
            bool displayContents;

            if (DesignMode) {
                // Dummy Error state
                errorDescriptions = new string [] {
                    SR.GetString(SR.ValSummary_error_message_1),
                    SR.GetString(SR.ValSummary_error_message_2),
                };
                displayContents = true;
                renderUplevel = false;
            }
            else {
                // Act like invisible if disabled
                if (!Enabled) {
                    return;
                }

                bool inError;
                errorDescriptions = GetErrorMessages(out inError);
                displayContents = (ShowSummary && inError);

                // Make sure tags are hidden if there are no contents
                if (!displayContents && renderUplevel) {
                    Style["display"] = "none";
                }
            }

            // Make sure we are in a form tag with runat=server.
            if (Page != null) {
                Page.VerifyRenderingInServerForm(this);
            }

            bool displayTags = renderUplevel ? true : displayContents;

            if (displayTags) {
                RenderBeginTag(writer);
            }

            if (displayContents) {

                string headerSep;
                string first;
                string pre;
                string post;
                string final;

                switch (DisplayMode) {
                    case ValidationSummaryDisplayMode.List:
                        headerSep = breakTag;
                        first = String.Empty;
                        pre = String.Empty;
                        post = breakTag;
                        final = String.Empty;
                        break;

                    case ValidationSummaryDisplayMode.BulletList:
                        headerSep = String.Empty;
                        first = "<ul>";
                        pre = "<li>";
                        post = "</li>";
                        final = "</ul>";
                        break;

                    case ValidationSummaryDisplayMode.SingleParagraph:
                        headerSep = " ";
                        first = String.Empty;
                        pre = String.Empty;
                        post = " ";
                        final = breakTag;
                        break;

                    default:
                        Debug.Fail("Invalid DisplayMode!");
                        goto
                    case ValidationSummaryDisplayMode.BulletList;
                }
                if (HeaderText.Length > 0) {
                    writer.Write(HeaderText);
                    WriteBreakIfPresent(writer, headerSep);
                }
                if (errorDescriptions != null) {
                    writer.Write(first);
                    for (int i = 0; i < errorDescriptions.Length; i++) {
                        Debug.Assert(errorDescriptions[i] != null && errorDescriptions[i].Length > 0, "Bad Error Messages");
                        writer.Write(pre);
                        writer.Write(errorDescriptions[i]);
                        WriteBreakIfPresent(writer, post);
                    }
                    WriteBreakIfPresent(writer, final);
                }
            }
            if (displayTags) {
                RenderEndTag(writer);
            }
        }

        internal bool ShouldSerializeForeColor() {
            Color defaultForeColor = (RenderingCompatibility < VersionUtil.Framework40) ? Color.Red : Color.Empty;
            return defaultForeColor != ForeColor;
        }

        private void WriteBreakIfPresent(HtmlTextWriter writer, String text) {
            if (text == breakTag) {
                if (EnableLegacyRendering) {
                    writer.WriteObsoleteBreak();
                }
                else {
                    writer.WriteBreak();
                }
            }
            else {
                writer.Write(text);
            }
        }
    }
}

