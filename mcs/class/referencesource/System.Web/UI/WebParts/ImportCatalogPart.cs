//------------------------------------------------------------------------------
// <copyright file="ImportCatalogPart.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Security;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.Security;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Web.Util;
    using System.Xml;

    /// <devdoc>
    /// </devdoc>
    public sealed class ImportCatalogPart : CatalogPart {

        private WebPart _availableWebPart;
        private string _importedPartDescription;
        private WebPartDescriptionCollection _availableWebPartDescriptions;
        private FileUpload _upload;
        private Button _uploadButton;
        private string _importErrorMessage;

        private const int baseIndex = 0;
        private const int importedPartDescriptionIndex = 1;
        private const int controlStateArrayLength = 2;

        private const string TitlePropertyName = "Title";
        private const string DescriptionPropertyName = "Description";
        private const string IconPropertyName = "CatalogIconImageUrl";
        private const string ImportedWebPartID = "ImportedWebPart";

        private static readonly WebPartDescriptionCollection DesignModeAvailableWebPart =
            new WebPartDescriptionCollection(new WebPartDescription[] {
                new WebPartDescription("webpart1", String.Format(CultureInfo.CurrentCulture,
                    SR.GetString(SR.CatalogPart_SampleWebPartTitle), "1"), null, null)});

        [WebCategory("Appearance")]
        [WebSysDefaultValue(SR.ImportCatalogPart_Browse)]
        [WebSysDescription(SR.ImportCatalogPart_BrowseHelpText)]
        public string BrowseHelpText {
            get {
                object o = ViewState["BrowseHelpText"];
                return (o != null) ? (string)o : SR.GetString(SR.ImportCatalogPart_Browse);
            }
            set {
                ViewState["BrowseHelpText"] = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), Themeable(false)]
        public override string DefaultButton {
            get { return base.DefaultButton; }
            set { base.DefaultButton = value; }
        }

        [WebCategory("Appearance")]
        [WebSysDefaultValue(SR.ImportCatalogPart_ImportedPartLabel)]
        [WebSysDescription(SR.ImportCatalogPart_ImportedPartLabelText)]
        public string ImportedPartLabelText {
            get {
                object o = ViewState["ImportedPartLabelText"];
                return (o != null) ? (string)o : SR.GetString(SR.ImportCatalogPart_ImportedPartLabel);
            }
            set {
                ViewState["ImportedPartLabelText"] = value;
            }
        }

        [WebCategory("Appearance")]
        [WebSysDefaultValue(SR.ImportCatalogPart_ImportedPartErrorLabel)]
        [WebSysDescription(SR.ImportCatalogPart_PartImportErrorLabelText)]
        public string PartImportErrorLabelText {
            get {
                object o = ViewState["PartImportErrorLabelText"];
                return (o != null) ? (string)o : SR.GetString(SR.ImportCatalogPart_ImportedPartErrorLabel);
            }
            set {
                ViewState["PartImportErrorLabelText"] = value;
            }
        }

        [
        WebSysDefaultValue(SR.ImportCatalogPart_PartTitle),
        ]
        public override string Title {
            get {
                string s = (string)ViewState["Title"];
                return (s != null) ? s : SR.GetString(SR.ImportCatalogPart_PartTitle);
            }
            set {
                ViewState["Title"] = value;
            }
        }

        [WebCategory("Appearance")]
        [WebSysDefaultValue(SR.ImportCatalogPart_UploadButton)]
        [WebSysDescription(SR.ImportCatalogPart_UploadButtonText)]
        public string UploadButtonText {
            get {
                object o = ViewState["UploadButtonText"];
                return (o != null) ? (string)o : SR.GetString(SR.ImportCatalogPart_UploadButton);
            }
            set {
                ViewState["UploadButtonText"] = value;
            }
        }

        [WebCategory("Appearance")]
        [WebSysDefaultValue(SR.ImportCatalogPart_Upload)]
        [WebSysDescription(SR.ImportCatalogPart_UploadHelpText)]
        public string UploadHelpText {
            get {
                object o = ViewState["UploadHelpText"];
                return (o != null) ? (string)o : SR.GetString(SR.ImportCatalogPart_Upload);
            }
            set {
                ViewState["UploadHelpText"] = value;
            }
        }

        protected internal override void CreateChildControls() {
            Controls.Clear();

            _upload = new FileUpload();
            Controls.Add(_upload);

            _uploadButton = new Button();
            _uploadButton.ID = "Upload";
            _uploadButton.CommandName = "upload";
            _uploadButton.Click += new EventHandler(OnUpload);
            Controls.Add(_uploadButton);

            if (!DesignMode && Page != null) {
                IScriptManager scriptManager = Page.ScriptManager;
                if (scriptManager != null) {
                    scriptManager.RegisterPostBackControl(_uploadButton);
                }
            }
        }

        public override WebPartDescriptionCollection GetAvailableWebPartDescriptions() {
            if (DesignMode) {
                return DesignModeAvailableWebPart;
            }

            CreateAvailableWebPartDescriptions();
            return _availableWebPartDescriptions;
        }

        private void CreateAvailableWebPartDescriptions() {
            if (_availableWebPartDescriptions != null) {
                return;
            }

            if (WebPartManager == null || String.IsNullOrEmpty(_importedPartDescription)) {
                _availableWebPartDescriptions = new WebPartDescriptionCollection();
                return;
            }

            // Run in minimal trust
            PermissionSet pset = new PermissionSet(PermissionState.None);
            // add in whatever perms are appropriate
            pset.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
            pset.AddPermission(new AspNetHostingPermission(AspNetHostingPermissionLevel.Minimal));

            pset.PermitOnly();
            bool permitOnly = true;
            string title = null;
            string description = null;
            string icon = null;
            // Extra try-catch block to prevent elevation of privilege attack via exception filter
            try {
                try {
                    // Get the WebPart description from its saved XML description.
                    using (StringReader sr = new StringReader(_importedPartDescription)) {
                        using (XmlReader reader = XmlUtils.CreateXmlReader(sr)) {
                            if (reader != null) {
                                reader.MoveToContent();
                                // Check if imported part is authorized

                                // Get to the metadata
                                reader.MoveToContent();
                                reader.ReadStartElement(WebPartManager.ExportRootElement);
                                reader.ReadStartElement(WebPartManager.ExportPartElement);
                                reader.ReadStartElement(WebPartManager.ExportMetaDataElement);

                                // Get the type name
                                string partTypeName = null;
                                string userControlTypeName = null;
                                while (reader.Name != WebPartManager.ExportTypeElement) {
                                    reader.Skip();
                                    if (reader.EOF) {
                                        throw new EndOfStreamException();
                                    }
                                }
                                if (reader.Name == WebPartManager.ExportTypeElement) {
                                    partTypeName = reader.GetAttribute(WebPartManager.ExportTypeNameAttribute);
                                    userControlTypeName = reader.GetAttribute(WebPartManager.ExportUserControlSrcAttribute);
                                }

                                // If we are in shared scope, we are importing a shared WebPart
                                bool isShared = (WebPartManager.Personalization.Scope == PersonalizationScope.Shared);

                                if (!String.IsNullOrEmpty(partTypeName)) {
                                    // Need medium trust to call BuildManager.GetType()
                                    PermissionSet mediumPset = new PermissionSet(PermissionState.None);
                                    mediumPset.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
                                    mediumPset.AddPermission(new AspNetHostingPermission(AspNetHostingPermissionLevel.Medium));

                                    CodeAccessPermission.RevertPermitOnly();
                                    permitOnly = false;
                                    mediumPset.PermitOnly();
                                    permitOnly = true;

                                    Type partType = WebPartUtil.DeserializeType(partTypeName, true);

                                    CodeAccessPermission.RevertPermitOnly();
                                    permitOnly = false;
                                    pset.PermitOnly();
                                    permitOnly = true;

                                    // First check if the type is authorized
                                    if (!WebPartManager.IsAuthorized(partType, null, null, isShared)) {
                                        _importErrorMessage = SR.GetString(SR.WebPartManager_ForbiddenType);
                                        return;
                                    }
                                    // If the type is not a webpart, create a generic Web Part
                                    if (!partType.IsSubclassOf(typeof(WebPart)) && !partType.IsSubclassOf(typeof(Control))) {
                                        // We only allow for Controls (VSWhidbey 428511)
                                        _importErrorMessage = SR.GetString(SR.WebPartManager_TypeMustDeriveFromControl);
                                        return;
                                    }
                                }
                                else {
                                    // Check if the path is authorized
                                    if (!WebPartManager.IsAuthorized(typeof(UserControl), userControlTypeName, null, isShared)) {
                                        _importErrorMessage = SR.GetString(SR.WebPartManager_ForbiddenType);
                                        return;
                                    }
                                }
                                while (!reader.EOF) {
                                    while (!reader.EOF && !(reader.NodeType == XmlNodeType.Element &&
                                            reader.Name == WebPartManager.ExportPropertyElement)) {
                                        reader.Read();
                                    }
                                    if (reader.EOF) {
                                        break;
                                    }
                                    string name = reader.GetAttribute(WebPartManager.ExportPropertyNameAttribute);
                                    if (name == TitlePropertyName) {
                                        title = reader.ReadElementString();
                                    }
                                    else if (name == DescriptionPropertyName) {
                                        description = reader.ReadElementString();
                                    }
                                    else if (name == IconPropertyName) {
                                        string url = reader.ReadElementString().Trim();
                                        if (!CrossSiteScriptingValidation.IsDangerousUrl(url)) {
                                            icon = url;
                                        }
                                    }
                                    else {
                                        reader.Read();
                                        continue;
                                    }
                                    if (title != null && description != null && icon != null) {
                                        break;
                                    }
                                    reader.Read();
                                }
                            }
                        }
                        if (String.IsNullOrEmpty(title)) {
                            title = SR.GetString(SR.Part_Untitled);
                        }

                        _availableWebPartDescriptions = new WebPartDescriptionCollection(
                                new WebPartDescription[] {new WebPartDescription(ImportedWebPartID, title, description, icon)});
                    }
                }
                catch (XmlException) {
                    _importErrorMessage = SR.GetString(SR.WebPartManager_ImportInvalidFormat);
                    return;
                }
                catch {
                    _importErrorMessage = (!String.IsNullOrEmpty(_importErrorMessage)) ?
                        _importErrorMessage :
                        SR.GetString(SR.WebPart_DefaultImportErrorMessage);
                    return;
                }
                finally {
                    if (permitOnly) {
                        // revert if you're not just exiting the stack frame anyway
                        CodeAccessPermission.RevertPermitOnly();
                    }
                }
            }
            catch {
                throw;
            }
        }

        public override WebPart GetWebPart(WebPartDescription description) {
            if (description == null) {
                throw new ArgumentNullException("description");
            }

            WebPartDescriptionCollection webPartDescriptions = GetAvailableWebPartDescriptions();
            if (!webPartDescriptions.Contains(description)) {
                throw new ArgumentException(SR.GetString(SR.CatalogPart_UnknownDescription), "description");
            }

            if (_availableWebPart != null) {
                return _availableWebPart;
            }
            // Import the WebPart from its saved XML description.
            using (XmlReader reader = XmlUtils.CreateXmlReader(new StringReader(_importedPartDescription))) {
                if (reader != null && WebPartManager != null) {
                    _availableWebPart = WebPartManager.ImportWebPart(reader, out _importErrorMessage);
                }
            }

            // If import failed, clear the cached description
            if (_availableWebPart == null) {
                _importedPartDescription = null;
                _availableWebPartDescriptions = null;
            }
            return _availableWebPart;
        }

        /// <devdoc>
        /// <para>Loads the control state for those properties that should persist across postbacks
        ///   even when EnableViewState=false.</para>
        /// </devdoc>
        protected internal override void LoadControlState(object savedState) {
            if (savedState == null) {
                base.LoadControlState(null);
            }
            else {
                object[] myState = (object[])savedState;
                if (myState.Length != controlStateArrayLength) {
                    throw new ArgumentException(SR.GetString(SR.Invalid_ControlState));
                }

                base.LoadControlState(myState[baseIndex]);
                if (myState[importedPartDescriptionIndex] != null) {
                    _importedPartDescription = (string)myState[importedPartDescriptionIndex];
                    // Calling this method to be sure to emit any import error message in time:
                    // Otherwise, the descriptions will only be constructed (and the error messages
                    // generated) when the list of imported parts is rendered, which happens after
                    // the importcatalogpart itself is rendered. The error message being rendered
                    // by the ICP, we have to call this now
                    GetAvailableWebPartDescriptions();
                }
            }
        }

        /// <devdoc>
        /// Registers the ImportCatalogPart for control state.
        /// </devdoc>
        protected internal override void OnInit(EventArgs e) {
            base.OnInit(e);
            Page.RegisterRequiresControlState(this);
        }

        internal void OnUpload(object sender, EventArgs e) {
            string fileName = _upload.FileName;
            Stream contents = _upload.FileContent;
            if (!String.IsNullOrEmpty(fileName) && contents != null) {
                using (StreamReader sr = new StreamReader(contents, true)) {
                    _importedPartDescription = sr.ReadToEnd();

                    // Clear cache
                    _availableWebPart = null;
                    _availableWebPartDescriptions = null;
                    _importErrorMessage = null;

                    if (String.IsNullOrEmpty(_importedPartDescription)) {
                        _importErrorMessage = SR.GetString(SR.ImportCatalogPart_NoFileName);
                    }
                    else {
                        GetAvailableWebPartDescriptions();
                    }
                }
            }
            else {
                _importErrorMessage = SR.GetString(SR.ImportCatalogPart_NoFileName);
            }
        }

        /// <devdoc>
        /// <para>Saves the control state for those properties that should persist across postbacks
        ///   even when EnableViewState=false.</para>
        /// </devdoc>
        protected internal override object SaveControlState() {
            object[] myState = new object[controlStateArrayLength];

            myState[baseIndex] = base.SaveControlState();
            myState[importedPartDescriptionIndex] = _importedPartDescription;

            for (int i=0; i < controlStateArrayLength; i++) {
                if (myState[i] != null) {
                    return myState;
                }
            }

            // More performant to return null than an array of null values
            return null;
        }

        protected internal override void Render(HtmlTextWriter writer) {
            if (Page != null) {
                Page.VerifyRenderingInServerForm(this);
            }

            base.Render(writer);
        }

        protected internal override void RenderContents(HtmlTextWriter writer) {
            // HACK: Need this for child controls to be created at design-time when control is inside template
            EnsureChildControls();

            CatalogZoneBase zone = Zone;
            if (zone != null && !zone.LabelStyle.IsEmpty) {
                zone.LabelStyle.AddAttributesToRender(writer, this);
            }
            writer.AddAttribute(HtmlTextWriterAttribute.For, _upload.ClientID);
            writer.RenderBeginTag(HtmlTextWriterTag.Label);
            writer.Write(BrowseHelpText);
            writer.RenderEndTag();
            writer.WriteBreak();

            if (zone != null && !zone.EditUIStyle.IsEmpty) {
                _upload.ApplyStyle(zone.EditUIStyle);
            }
            _upload.RenderControl(writer);
            writer.WriteBreak();

            if (zone != null && !zone.LabelStyle.IsEmpty) {
                zone.LabelStyle.AddAttributesToRender(writer, this);
            }
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            writer.Write(UploadHelpText);
            writer.RenderEndTag();
            writer.WriteBreak();

            if (zone != null && !zone.EditUIStyle.IsEmpty) {
                _uploadButton.ApplyStyle(zone.EditUIStyle);
            }
            _uploadButton.Text = UploadButtonText;
            _uploadButton.RenderControl(writer);

            if (_importedPartDescription != null || _importErrorMessage != null || DesignMode) {
                writer.WriteBreak();
                if (_importErrorMessage != null) {
                    if (zone != null && !zone.ErrorStyle.IsEmpty) {
                        zone.ErrorStyle.AddAttributesToRender(writer, this);
                    }
                    writer.RenderBeginTag(HtmlTextWriterTag.Span);
                    writer.Write(PartImportErrorLabelText);
                    writer.RenderEndTag();

                    writer.RenderBeginTag(HtmlTextWriterTag.Hr);
                    writer.RenderEndTag();

                    if (zone != null && !zone.ErrorStyle.IsEmpty) {
                        zone.ErrorStyle.AddAttributesToRender(writer, this);
                    }
                    writer.RenderBeginTag(HtmlTextWriterTag.Span);
                    // We encode the error message because it is user-provided via the import file.
                    writer.WriteEncodedText(_importErrorMessage);
                    writer.RenderEndTag();
                }
                else {
                    if (zone != null && !zone.LabelStyle.IsEmpty) {
                        zone.LabelStyle.AddAttributesToRender(writer, this);
                    }
                    writer.RenderBeginTag(HtmlTextWriterTag.Span);
                    writer.Write(ImportedPartLabelText);
                    writer.RenderEndTag();

                    writer.RenderBeginTag(HtmlTextWriterTag.Hr);
                    writer.RenderEndTag();
                }
            }
        }
    }
}

