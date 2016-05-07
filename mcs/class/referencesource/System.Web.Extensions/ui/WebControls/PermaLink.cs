//------------------------------------------------------------------------------
// <copyright file="PermaLink.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

// Permalink temporarily removed

//namespace System.Web.UI {

//    using System;
//    using System.Collections.Generic;
//    using System.ComponentModel;
//    using System.Diagnostics.CodeAnalysis;
//    using System.Drawing.Design;
//    using System.Security.Permissions;
//    using System.Web;
//    using System.Web.UI.WebControls;

//    /// <devdoc>
//    ///    <para>Creates a link to the current page that automatically updates its url as history state changes.</para>
//    /// </devdoc>
//    [
//    ControlBuilderAttribute(typeof(HyperLinkControlBuilder)),
//    DefaultProperty("Text"),
//    ToolboxData("<{0}:PermaLink runat=\"server\">PermaLink</{0}:PermaLink>"),
//    ParseChildren(false),
//    AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal),
//    AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)
//    ]
//    public class PermaLink : ScriptControl {

//        private string _url;

//        /// <devdoc>
//        ///    <para>Gets or sets the URL reference to an image to display as an alternative to plain text for the
//        ///       Permalink.</para>
//        /// </devdoc>
//        [
//        Bindable(true),
//        Category("Appearance"),
//        DefaultValue(""),
//        Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
//        UrlProperty(),
//        ResourceDescription("PermaLink_ImageUrl"),
//        SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Consistent with other asp.net url properties.")
//        ]
//        public virtual string ImageUrl {
//            get {
//                return ((string)ViewState["ImageUrl"]) ?? String.Empty;
//            }
//            set {
//                ViewState["ImageUrl"] = value;
//            }
//        }

//        /// <devdoc>
//        ///    <para>Gets the URL to navigate to when the Permalink is clicked.</para>
//        /// </devdoc>
//        [
//        UrlProperty(),
//        ResourceDescription("PermaLink_NavigateUrl"),
//        SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Consistent with other asp.net url properties.")
//        ]
//        public string NavigateUrl {
//            get {
//                if ((Context == null) || (Context.Request == null)) {
//                    return String.Empty;
//                }

//                if (String.IsNullOrEmpty(_url)) {
//                    // Logic to figure out a reliable path to the current page as copied from HtmlForm.
//                    VirtualPath clientFilePath = Context.Request.ClientFilePath;

//                    // ASURT 15075/11054/59970: always set the action to the current page.
//                    VirtualPath currentFilePath = Context.Request.CurrentExecutionFilePathObject;
//                    if (Object.ReferenceEquals(currentFilePath, clientFilePath)) {
//                        // There hasn't been any Server.Transfer or RewritePath.
//                        // ASURT 15979: need to use a relative path, not absolute
//                        _url = currentFilePath.VirtualPathString;
//                        int iPos = _url.LastIndexOf('/');
//                        if (iPos >= 0) {
//                            _url = _url.Substring(iPos + 1);
//                        }
//                    }
//                    else {
//                        // Server.Transfer or RewritePath case.  We need to make the form action relative
//                        // to the original ClientFilePath (since that's where the browser thinks we are).
//                        currentFilePath = clientFilePath.MakeRelative(currentFilePath);
//                        _url = currentFilePath.VirtualPathString;
//                    }

//                    // Note: PermaLink url does not contain cookieless session information.

//                    string queryString = Page.ClientQueryString;
//                    // ASURT 15355: Don't lose the query string if there is one.
//                    // In scriptless mobile HTML, we prepend __EVENTTARGET, et. al. to the query string.  These have to be
//                    // removed from the form action.  Use new HttpValueCollection to leverage ToString(bool encoded).
//                    if (!String.IsNullOrEmpty(queryString)) {
//                        _url += "?" + queryString;
//                    }
//                }

//                return _url;
//            }
//        }

//        protected override HtmlTextWriterTag TagKey {
//            get {
//                return HtmlTextWriterTag.A;
//            }
//        }

//        /// <devdoc>
//        ///    <para>Gets or sets the target window or frame the contents of
//        ///       the <see cref='System.Web.UI.WebControls.PermaLink'/> will be displayed into when clicked.</para>
//        /// </devdoc>
//        [
//        Category("Navigation"),
//        DefaultValue(""),
//        ResourceDescription("PermaLink_Target"),
//        TypeConverter(typeof(TargetConverter))
//        ]
//        public string Target {
//            get {
//                return ((string)ViewState["Target"]) ?? String.Empty;
//            }
//            set {
//                ViewState["Target"] = value;
//            }
//        }


//        /// <devdoc>
//        ///    <para>
//        ///       Gets or sets the text displayed for the <see cref='System.Web.UI.WebControls.PermaLink'/>.</para>
//        /// </devdoc>
//        [
//        Localizable(true),
//        Bindable(true),
//        Category("Appearance"),
//        DefaultValue(""),
//        ResourceDescription("PermaLink_Text"),
//        PersistenceMode(PersistenceMode.InnerDefaultProperty)
//        ]
//        public virtual string Text {
//            get {
//                return ((string)ViewState["Text"]) ?? String.Empty;
//            }
//            set {
//                if (HasControls()) {
//                    Controls.Clear();
//                }
//                ViewState["Text"] = value;
//            }
//        }

//        /// <internalonly/>
//        /// <devdoc>
//        /// <para>Adds the attribututes of the a <see cref='System.Web.UI.WebControls.PermaLink'/> to the output
//        ///    stream for rendering.</para>
//        /// </devdoc>
//        protected override void AddAttributesToRender(HtmlTextWriter writer) {
//            if (Enabled && !IsEnabled) {
//                // We need to do the cascade effect on the server, because the browser
//                // only renders as disabled, but doesn't disable the functionality.
//                writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
//            }

//            base.AddAttributesToRender(writer);

//            string s = NavigateUrl;
//            if (!String.IsNullOrEmpty(s) && IsEnabled) {
//                string resolvedUrl = ResolveClientUrl(s);
//                writer.AddAttribute(HtmlTextWriterAttribute.Href, resolvedUrl);
//            }
//            s = Target;
//            if (!String.IsNullOrEmpty(s)) {
//                writer.AddAttribute(HtmlTextWriterAttribute.Target, s);
//            }
//        }

//        protected override void AddParsedSubObject(object obj) {
//            if (HasControls()) {
//                base.AddParsedSubObject(obj);
//            }
//            else {
//                if (obj is LiteralControl) {
//                    Text = ((LiteralControl)obj).Text;
//                }
//                else {
//                    string currentText = Text;
//                    if (!String.IsNullOrEmpty(currentText)) {
//                        Text = String.Empty;
//                        base.AddParsedSubObject(new LiteralControl(currentText));
//                    }
//                    base.AddParsedSubObject(obj);
//                }
//            }
//        }

//        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
//            Justification = "Matches IScriptControl interface.")]
//        protected override IEnumerable<ScriptDescriptor> GetScriptDescriptors() {
//            // Don't render any scripts when history is not enabled
//            if (Page != null) {
//                ScriptManager sm = ScriptManager.GetCurrent(Page);
//                if (sm.EnableHistory && Visible) {
//                    ScriptControlDescriptor desc = new ScriptControlDescriptor("Sys.UI.PermaLink", ClientID);
//                    yield return desc;
//                }
//            }

//            yield break;
//        }

//        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
//            Justification = "Matches IScriptControl interface.")]
//        protected override IEnumerable<ScriptReference> GetScriptReferences() {
//            yield break;
//        }

//        /// <internalonly/>
//        /// <devdoc>
//        ///    Load previously saved state.
//        ///    Overridden to synchronize Text property with LiteralContent.
//        /// </devdoc>
//        protected override void LoadViewState(object savedState) {
//            if (savedState != null) {
//                base.LoadViewState(savedState);
//                string s = (string)ViewState["Text"];
//                if (s != null)
//                    Text = s;
//            }
//        }

//        /// <internalonly/>
//        /// <devdoc>
//        /// <para>Displays the <see cref='System.Web.UI.WebControls.PermaLink'/> on a page.</para>
//        /// </devdoc>
//        protected internal override void RenderContents(HtmlTextWriter writer) {
//            string s = ImageUrl;
//            if (!String.IsNullOrEmpty(s)) {
//                Image img = new Image();

//                // NOTE: The Url resolution happens right here, because the image is not parented
//                //       and will not be able to resolve when it tries to do so.
//                img.ImageUrl = ResolveClientUrl(s);

//                s = ToolTip;
//                if (!String.IsNullOrEmpty(s)) {
//                    img.ToolTip = s;
//                }

//                s = Text;
//                if (!String.IsNullOrEmpty(s)) {
//                    img.AlternateText = s;
//                }
//                img.RenderControl(writer);
//            }
//            else {
//                if (HasRenderingData()) {
//                    base.RenderContents(writer);
//                }
//                else {
//                    writer.Write(Text);
//                }
//            }
//        }
//    }
//}

