//------------------------------------------------------------------------------
// <copyright file="MailDefinition.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System.Net.Mail;
    using System.Net.Mime;
    using System.Collections;
    using System.ComponentModel;
    using System.IO;
    using System.Drawing.Design;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Util;
    using System.Text;
    using System.Web.Configuration;
    using System.Configuration;

    /// <devdoc>
    /// Defines an email message.  Smaller object model than System.Net.Mail.MailMessage.  Creates a MailMessage
    /// from a string or a file containing the message body.  Can perform textual substitutions in the message body
    /// when given a dictionary mapping strings to their replacements.
    /// </devdoc>
    [
    Bindable(false),
    TypeConverterAttribute(typeof(EmptyStringExpandableObjectConverter)),
    ParseChildren(true, "")
    ]
    public sealed class MailDefinition : IStateManager {
        private bool _isTrackingViewState;
        private StateBag _viewState;
        private EmbeddedMailObjectsCollection _embeddedObjects;
        private string _bodyFileName;

        /// <devdoc>
        /// The file that contains the body of the e-mail message.
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(""),
        WebSysDescription(SR.MailDefinition_BodyFileName),
        Editor("System.Web.UI.Design.WebControls.MailDefinitionBodyFileNameEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        UrlProperty("*.*"),
        NotifyParentProperty(true)
        ]
        public string BodyFileName {
            get {
                return (_bodyFileName == null) ? String.Empty : _bodyFileName;
            }
            set {
                _bodyFileName = value;
            }
        }


        /// <devdoc>
        /// A semicolon-delimited list of e-mail addresses that receive a carbon copy (CC) of the e-mail message.
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(""),
        WebSysDescription(SR.MailDefinition_CC),
        NotifyParentProperty(true)
        ]
        public string CC {
            get {
                object obj = ViewState["CC"];
                return (obj == null) ? String.Empty : (string)obj;
            }
            set {
                ViewState["CC"] = value;
            }
        }

        // <include file='doc\MailDefinition.uex' path='docs/doc[@for="MailDefinition.From"]/*' />
        /// <devdoc>
        /// The sender's e-mail address.
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(""),
        WebSysDescription(SR.MailDefinition_From),
        NotifyParentProperty(true)
        ]
        public string From {
            get {
                object obj = ViewState["From"];
                return (obj == null) ? String.Empty : (string)obj;
            }
            set {
                ViewState["From"] = value;
            }
        }


        /// <devdoc>
        /// Embedded mail objects
        /// </devdoc>
        [
        DefaultValue(null),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Behavior"),
        WebSysDescription(SR.MailDefinition_EmbeddedObjects),
        ]
        public EmbeddedMailObjectsCollection EmbeddedObjects {
            get {
                if (_embeddedObjects == null) {
                    _embeddedObjects = new EmbeddedMailObjectsCollection();
                }
                return _embeddedObjects;
            }
        }

        [
        WebCategory("Behavior"),
        DefaultValue(false),
        WebSysDescription(SR.MailDefinition_IsBodyHtml),
        NotifyParentProperty(true)
        ]
        public bool IsBodyHtml {
            get {
                object obj = ViewState["IsBodyHtml"];
                return (obj == null) ? false : (bool)obj;
            }
            set {
                ViewState["IsBodyHtml"] = value;
            }
        }

        [
        WebCategory("Behavior"),
        DefaultValue(MailPriority.Normal),
        WebSysDescription(SR.MailDefinition_Priority),
        NotifyParentProperty(true)
        ]
        public MailPriority Priority {
            get {
                object obj = ViewState["Priority"];
                return (obj == null) ? MailPriority.Normal : (MailPriority) obj;
            }
            set {
                if (value < MailPriority.Normal || value > MailPriority.High) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["Priority"] = value;
            }
        }


        /// <devdoc>
        /// The subject line of the e-mail message.
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(""),
        WebSysDescription(SR.MailDefinition_Subject),
        NotifyParentProperty(true)
        ]
        public string Subject {
            get {
                object obj = ViewState["Subject"];
                return (obj == null) ? String.Empty : (string)obj;
            }
            set {
                ViewState["Subject"] = value;
            }
        }

        // 
        internal string SubjectInternal {
            get {
                return (string)ViewState["Subject"];
            }
        }

        /// <devdoc>
        /// Manages the viewstate for this class, since we don't extend Control.
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        private StateBag ViewState {
            get {
                if (_viewState == null) {
                    _viewState = new StateBag(false);
                    if (_isTrackingViewState) {
                        ((IStateManager)_viewState).TrackViewState();
                    }
                }
                return _viewState;
            }
        }


        /// <devdoc>
        /// Creates a MailMessage using the BodyFileName property.
        /// </devdoc>
        public MailMessage CreateMailMessage(string recipients, IDictionary replacements, Control owner) {
            if (owner == null) {
                throw new ArgumentNullException("owner");
            }

            string body = String.Empty;
            string bodyFileName = BodyFileName;
            if (!String.IsNullOrEmpty(bodyFileName)) {
                string path = bodyFileName;
                if (!UrlPath.IsAbsolutePhysicalPath(path)) {
                    // Relative so we need to add the template source directory to the path
                    path = UrlPath.Combine(owner.AppRelativeTemplateSourceDirectory, path);
                }

                TextReader reader = new StreamReader(owner.OpenFile(path));
                try {
                    body = reader.ReadToEnd();
                }
                finally {
                    reader.Close();
                }
            }
            return CreateMailMessage(recipients, replacements, body, owner);
        }


        /// <devdoc>
        /// Creates a MailMessage using the body parameter.
        /// </devdoc>
        public MailMessage CreateMailMessage(string recipients, IDictionary replacements, string body, Control owner) {
            if (owner == null) {
                throw new ArgumentNullException("owner");
            }

            string from = From;
            if (String.IsNullOrEmpty(from)) {
                System.Net.Configuration.SmtpSection smtpSection = RuntimeConfig.GetConfig().Smtp;
                if (smtpSection == null || smtpSection.Network == null || String.IsNullOrEmpty(smtpSection.From)) {
                    throw new HttpException(SR.GetString(SR.MailDefinition_NoFromAddressSpecified));
                }
                else {
                    from = smtpSection.From;
                }
            }

            MailMessage message = null;
            try {
                message = new MailMessage(from, recipients);
                if (!String.IsNullOrEmpty(CC)) {
                    message.CC.Add(CC);
                }
                if (!String.IsNullOrEmpty(Subject)) {
                    message.Subject = Subject;
                }

                message.Priority = Priority;

                if (replacements != null && !String.IsNullOrEmpty(body)) {
                    foreach (object key in replacements.Keys) {
                        string fromString = key as string;
                        string toString = replacements[key] as string;

                        if ((fromString == null) || (toString == null)) {
                            throw new ArgumentException(SR.GetString(SR.MailDefinition_InvalidReplacements));
                        }
                        // DevDiv 151177
                        // According to http://msdn2.microsoft.com/en-us/library/ewy2t5e0.aspx, some special 
                        // constructs (starting with "$") are recognized in the replacement patterns. References of
                        // these constructs will be replaced with predefined strings in the final output. To use the 
                        // character "$" as is in the replacement patterns, we need to replace all references of single "$"
                        // with "$$", because "$$" in replacement patterns are replaced with a single "$" in the 
                        // final output. 
                        toString = toString.Replace("$", "$$");
                        body = Regex.Replace(body, fromString, toString, RegexOptions.IgnoreCase);
                    }
                }
                // If there are any embedded objects, we need to construct an alternate view with text/html
                // And add all of the embedded objects as linked resouces
                if (EmbeddedObjects.Count > 0) {
                    string viewContentType = (IsBodyHtml ? MediaTypeNames.Text.Html : MediaTypeNames.Text.Plain);
                    AlternateView view = AlternateView.CreateAlternateViewFromString(body, null, viewContentType);
                    foreach (EmbeddedMailObject part in EmbeddedObjects) {
                        string path = part.Path;
                        if (String.IsNullOrEmpty(path)) {
                            throw ExceptionUtil.PropertyNullOrEmpty("EmbeddedMailObject.Path");
                        }
                        if (!UrlPath.IsAbsolutePhysicalPath(path)) {
                            VirtualPath virtualPath = VirtualPath.Combine(owner.TemplateControlVirtualDirectory,
                                VirtualPath.Create(path));
                            path = virtualPath.AppRelativeVirtualPathString;
                        }

                        // The FileStream will be closed by MailMessage.Dispose()
                        LinkedResource lr = null;
                        try {
                            Stream stream = null;
                            try {
                                stream = owner.OpenFile(path);
                                lr = new LinkedResource(stream);
                            }
                            catch {
                                if (stream != null) {
                                    ((IDisposable)stream).Dispose();
                                }
                                throw;
                            }
                            lr.ContentId = part.Name;
                            lr.ContentType.Name = UrlPath.GetFileName(path);
                            view.LinkedResources.Add(lr);
                        }
                        catch {
                            if (lr != null) {
                                lr.Dispose();
                            }
                            throw;
                        }
                    }
                    message.AlternateViews.Add(view);
                }
                else if (!String.IsNullOrEmpty(body)) {
                    message.Body = body;
                }

                message.IsBodyHtml = IsBodyHtml;
                return message;
            }
            catch {
                if (message != null) {
                    message.Dispose();
                }
                throw;
            }
        }

        #region IStateManager implementation
        /// <internalonly/>
        bool IStateManager.IsTrackingViewState {
            get {
                return _isTrackingViewState;
            }
        }

        /// <internalonly/>
        void IStateManager.LoadViewState(object savedState) {
            if (savedState != null) {
                ((IStateManager)ViewState).LoadViewState(savedState);
            }
        }

        /// <internalonly/>
        object IStateManager.SaveViewState() {
            if (_viewState != null) {
                return ((IStateManager)_viewState).SaveViewState();
            }
            return null;
        }

        /// <internalonly/>
        void IStateManager.TrackViewState() {
            _isTrackingViewState = true;
            if (_viewState != null) {
                ((IStateManager)_viewState).TrackViewState();
            }
        }
        #endregion
    }
}
