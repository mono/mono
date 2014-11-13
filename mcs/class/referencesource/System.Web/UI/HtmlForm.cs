//------------------------------------------------------------------------------
// <copyright file="HtmlForm.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

// HtmlForm.cs
//

namespace System.Web.UI.HtmlControls {
    using System.ComponentModel;
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Web.Configuration;
    using System.Web.Util;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Web.Security;
    using System.Security.Permissions;


    /// <devdoc>
    ///    <para>
    ///       The <see langword='HtmlForm'/> class defines the methods, properties, and
    ///       events for the HtmlForm control. This class provides programmatic access to the
    ///       HTML &lt;form&gt; element on the server.
    ///    </para>
    /// </devdoc>
    public class HtmlForm : HtmlContainerControl {
        private string _defaultFocus;
        private string _defaultButton;
        private bool _submitDisabledControls;
        private const string _aspnetFormID = "aspnetForm";


        /// <devdoc>
        /// </devdoc>
        public HtmlForm()
            : base("form") {
        }

        [
        WebCategory("Behavior"),
        DefaultValue(""),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public string Action {
            get {
                string s = Attributes["action"];
                return ((s != null) ? s : String.Empty);
            }
            set {
                Attributes["action"] = MapStringAttributeToString(value);
            }
        }

        /// <devdoc>
        ///     Gets or sets default button for the form
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(""),
        ]
        public string DefaultButton {
            get {
                if (_defaultButton == null) {
                    return String.Empty;
                }
                return _defaultButton;
            }
            set {
                _defaultButton = value;
            }
        }


        /// <devdoc>
        ///     Gets or sets default focused control for the form
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(""),
        ]
        public string DefaultFocus {
            get {
                if (_defaultFocus == null) {
                    return String.Empty;
                }
                return _defaultFocus;
            }
            set {
                _defaultFocus = value;
            }
        }

        /*
         * Encode Type property.
         */

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the Enctype attribute of the form. This is
        ///       the encoding type that browsers
        ///       use when posting the form's data to the server.
        ///    </para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(""),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public string Enctype {
            get {
                string s = Attributes["enctype"];
                return ((s != null) ? s : String.Empty);
            }
            set {
                Attributes["enctype"] = MapStringAttributeToString(value);
            }
        }

        /*
         * Method property.
         */

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the Method attribute for the form. This defines how a browser
        ///       posts form data to the server for processing. The two common methods supported
        ///       by all browsers are GET and POST.
        ///    </para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(""),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public string Method {
            get {
                string s = Attributes["method"];
                return ((s != null) ? s : "post");
            }
            set {
                Attributes["method"] = MapStringAttributeToString(value);
            }
        }

        /*
         * Name property.
         */

        /// <devdoc>
        ///    <para>
        ///       Gets the value of the HTML Name attribute that will be rendered to the
        ///       browser.
        ///    </para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(""),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public virtual string Name {
            get {
                return UniqueID;
            }
            set {
                // no-op setter to prevent the name from being set
            }
        }


        /// <devdov>
        /// If true, forces controls disabled on the client to submit their values (thus preserving their previous postback state)
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(false)
        ]
        public virtual bool SubmitDisabledControls {
            get {
                return _submitDisabledControls;
            }
            set {
                _submitDisabledControls = value;
            }
        }

        /*
         * Target property.
         */

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the Uri of the frame or window to render the results of a Form
        ///       POST request. Developers can use this property to redirect these results to
        ///       another browser window or frame.
        ///    </para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(""),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public string Target {
            get {
                string s = Attributes["target"];
                return ((s != null) ? s : String.Empty);
            }

            set {
                Attributes["target"] = MapStringAttributeToString(value);
            }
        }


        /// <devdoc>
        /// Overridden to return a constant value or tack the ID onto the same constant value.
        /// This fixes a bug in PocketPC which doesn't allow the name and ID of a form to be different
        /// </devdoc>
        public override string UniqueID {
            get {
                if (NamingContainer == Page) {
                    return base.UniqueID;
                }
                else if (this.EffectiveClientIDMode != ClientIDMode.AutoID) {
                    return ID ?? _aspnetFormID;
                }

                return _aspnetFormID;
            }
        }

        public override string ClientID {
            get {
                if (this.EffectiveClientIDMode != ClientIDMode.AutoID) {
                    return ID;
                }
                return base.ClientID;
            }
        }

        protected internal override void Render(HtmlTextWriter output) {
            Page p = Page;
            if (p == null)
                throw new HttpException(SR.GetString(SR.Form_Needs_Page));

#pragma warning disable 0618    // To avoid deprecation warning
            if (p.SmartNavigation) {
#pragma warning restore 0618
                ((IAttributeAccessor)this).SetAttribute("__smartNavEnabled", "true");

                // Output the IFrame
                StringBuilder sb = new StringBuilder("<IFRAME id=\"__hifSmartNav\" name=\"__hifSmartNav\" style=\"display:none\" src=\"");
                sb.Append(HttpEncoderUtility.UrlEncodeSpaces(HttpUtility.HtmlAttributeEncode(Page.ClientScript.GetWebResourceUrl(typeof(HtmlForm), "SmartNav.htm"))));
                sb.Append("\"></IFRAME>");
                output.WriteLine(sb.ToString());
            }

            base.Render(output);
        }

        private string GetActionAttribute() {
            // If the Action property is nonempty, we use it instead of the current page.  This allows the developer
            // to support scenarios like PathInfo, UrlMapping, etc. (DevDiv Bugs 164390)
            string actionProperty = Action;
            if (!String.IsNullOrEmpty(actionProperty)) {
                return actionProperty;
            }

            string action;
            VirtualPath clientFilePath = Context.Request.ClientFilePath;

            // ASURT 15075/11054/59970: always set the action to the current page.
            // DevDiv Servicing 215795/Dev10 567580: The IIS URL Rewrite module and other rewrite
            // scenarios need the postback action to be the original URL.  Note however, if Server.Transfer/Execute
            // is used, the action will be set to the transferred/executed page, that is, the value of
            // CurrentExecutionFilePathObject.  This is because of ASURT 59970 and the document attached to
            // that bug, which indirectly states that things should behave this way when Transfer/Execute is used.
            if (Context.ServerExecuteDepth == 0) {
                // There hasn't been any Server.Transfer or RewritePath.
                // ASURT 15979: need to use a relative path, not absolute
                action = clientFilePath.VirtualPathString;
                int iPos = action.LastIndexOf('/');
                if (iPos >= 0) {
                    action = action.Substring(iPos + 1);
                }
            }
            else {
                VirtualPath currentFilePath = Context.Request.CurrentExecutionFilePathObject;
                // Server.Transfer or RewritePath case.  We need to make the form action relative
                // to the original ClientFilePath (since that's where the browser thinks we are).
                currentFilePath = clientFilePath.MakeRelative(currentFilePath);
                action = currentFilePath.VirtualPathString;
            }

            // VSWhidbey 202380: If cookieless is on, we need to add the app path modifier to the form action
            bool cookieless = CookielessHelperClass.UseCookieless(Context, false, FormsAuthentication.CookieMode);
            if (cookieless && Context.Request != null && Context.Response != null) {
                action = Context.Response.ApplyAppPathModifier(action);
            }

            // Dev11 406986: <form> elements must have non-empty 'action' attributes to pass W3 validation.
            // The only time this might happen is that the current file path is "", which meant that the
            // incoming URL ended in a slash, so we can just point 'action' back to the current directory.
            if (String.IsNullOrEmpty(action) && RenderingCompatibility >= VersionUtil.Framework45) {
                action = "./";
            }

            // Dev11 177096: The action may be empty if the RawUrl does not point to a file (for e.g. http://localhost:8080/) but is never null. 
            // Empty action values work fine since the form does not emit the action attribute.
            Debug.Assert(action != null);

            string queryString = Page.ClientQueryString;
            // ASURT 15355: Don't lose the query string if there is one.
            // In scriptless mobile HTML, we prepend __EVENTTARGET, et. al. to the query string.  These have to be
            // removed from the form action.  Use new HttpValueCollection to leverage ToString(bool encoded).
            if (!String.IsNullOrEmpty(queryString)) {
                action += "?" + queryString;
            }

            return action;
        }


        /// <internalonly/>
        /// <devdoc>
        ///    <para> Call RegisterViewStateHandler().</para>
        /// </devdoc>
        protected internal override void OnInit(EventArgs e) {
            base.OnInit(e);

            Page.SetForm(this);

            // Make sure view state is calculated (see ASURT 73020)
            Page.RegisterViewStateHandler();
        }


        /// <devdoc>
        /// Overridden to handle focus stuff
        /// </devdoc>
        protected internal override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);

#pragma warning disable 0618    // To avoid deprecation warning
            if (Page.SmartNavigation) {
#pragma warning restore 0618
                // Register the smartnav script file reference so it gets rendered
                Page.ClientScript.RegisterClientScriptResource(typeof(HtmlForm), "SmartNav.js");
            }
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected override void RenderAttributes(HtmlTextWriter writer) {
            ArrayList invalidAttributes = new ArrayList();
            foreach (String key in Attributes.Keys) {
                if (!writer.IsValidFormAttribute(key)) {
                    invalidAttributes.Add(key);
                }
            }

            foreach (String key in invalidAttributes) {
                Attributes.Remove(key);
            }

            bool enableLegacyRendering = EnableLegacyRendering;

            Page page = Page;
            if (writer.IsValidFormAttribute("name")) {
                // DevDiv 27328 Do not render name attribute for uplevel browser
                if (page != null && page.RequestInternal != null &&
                    RenderingCompatibility < VersionUtil.Framework40 &&
                    (page.RequestInternal.Browser.W3CDomVersion.Major == 0 ||
                     page.XhtmlConformanceMode != XhtmlConformanceMode.Strict)) {
                    writer.WriteAttribute("name", Name);
                }
                Attributes.Remove("name");
            }

            writer.WriteAttribute("method", Method);
            Attributes.Remove("method");

            // Encode the action attribute - ASURT 66784
            writer.WriteAttribute("action", GetActionAttribute(), true /*encode*/);
            Attributes.Remove("action");

            // see if the page has a submit event
            if (page != null) {
                string onSubmit = page.ClientOnSubmitEvent;
                if (!String.IsNullOrEmpty(onSubmit)) {
                    if (Attributes["onsubmit"] != null) {
                        // If there was an onsubmit on the form, register it as an onsubmit statement and remove it from the attribute collection
                        string formOnSubmit = Attributes["onsubmit"];
                        if (formOnSubmit.Length > 0) {
                            if (!StringUtil.StringEndsWith(formOnSubmit, ';')) {
                                formOnSubmit += ";";
                            }
                            if (page.ClientSupportsJavaScript || !formOnSubmit.ToLower(CultureInfo.CurrentCulture).Contains("javascript")) {
                                page.ClientScript.RegisterOnSubmitStatement(typeof(HtmlForm), "OnSubmitScript", formOnSubmit);
                            }
                            Attributes.Remove("onsubmit");
                        }
                    }

                    // Don't render the on submit if it contains javascript and the page doesn't support it
                    if (page.ClientSupportsJavaScript || !onSubmit.ToLower(CultureInfo.CurrentCulture).Contains("javascript")) {
                        if (enableLegacyRendering) {
                            writer.WriteAttribute("language", "javascript", false);
                        }
                        writer.WriteAttribute("onsubmit", onSubmit);
                    }
                }

                if ((page.RequestInternal != null) &&
                    (page.RequestInternal.Browser.EcmaScriptVersion.Major > 0) &&
                    (page.RequestInternal.Browser.W3CDomVersion.Major > 0)) {
                    if (DefaultButton.Length > 0) {
                        // Find control from the page if it's a hierarchical ID.
                        // Dev11 bug 19915
                        Control c = FindControlFromPageIfNecessary(DefaultButton);

                        if (c is IButtonControl) {
                            page.ClientScript.RegisterDefaultButtonScript(c, writer, false /* UseAddAttribute */);
                        }
                        else {
                            throw new InvalidOperationException(SR.GetString(SR.HtmlForm_OnlyIButtonControlCanBeDefaultButton, ID));
                        }
                    }
                }
            }


            // We always want the form to have an id on the client
            // base.RenderAttributes takes care of actually rendering it.
            EnsureID();

            base.RenderAttributes(writer);
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected internal override void RenderChildren(HtmlTextWriter writer) {
            // We need to register the script here since other controls might register
            // for focus during PreRender
            Page page = Page;
            if (page != null) {
                page.OnFormRender();
                page.BeginFormRender(writer, UniqueID);
            }

            // DevDiv Bugs 154630: move custom hidden fields to the begining of the form
            HttpWriter httpWriter = writer.InnerWriter as HttpWriter;
            if (page != null && httpWriter != null && RuntimeConfig.GetConfig(Context).Pages.RenderAllHiddenFieldsAtTopOfForm) {
                // If the response is flushed or cleared during render, we won't be able
                // to move the hidden fields.  Set HasBeenClearedRecently to false and
                // then check again when we're ready to move the fields.
                httpWriter.HasBeenClearedRecently = false;

                // Remember the index where the form begins
                int formBeginIndex = httpWriter.GetResponseBufferCountAfterFlush();

                base.RenderChildren(writer);

                // Remember the index where the custom hidden fields begin
                int fieldsBeginIndex = httpWriter.GetResponseBufferCountAfterFlush();

                page.EndFormRenderHiddenFields(writer, UniqueID);

                // we can only move the hidden fields if the response has not been flushed or cleared
                if (!httpWriter.HasBeenClearedRecently) {
                    int fieldsEndIndex = httpWriter.GetResponseBufferCountAfterFlush();
                    httpWriter.MoveResponseBufferRangeForward(fieldsBeginIndex, fieldsEndIndex - fieldsBeginIndex, formBeginIndex);
                }

                page.EndFormRenderArrayAndExpandoAttribute(writer, UniqueID);
                page.EndFormRenderPostBackAndWebFormsScript(writer, UniqueID);
                page.OnFormPostRender(writer);
            }
            else {
                base.RenderChildren(writer);

                if (page != null) {
                    page.EndFormRender(writer, UniqueID);
                    page.OnFormPostRender(writer);
                }
            }
        }

        public override void RenderControl(HtmlTextWriter writer) {
            if (DesignMode) {
                // User Control Designer scenario
                base.RenderChildren(writer);
            }
            else {
                base.RenderControl(writer);
            }
        }

        protected override ControlCollection CreateControlCollection() {
            return new ControlCollection(this, 100, 2);
        }
    }
}
