//------------------------------------------------------------------------------
// <copyright file="BehaviorEditorPart.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    public sealed class BehaviorEditorPart : EditorPart {

        private CheckBox _allowClose;
        private CheckBox _allowConnect;
        private CheckBox _allowHide;
        private CheckBox _allowMinimize;
        private CheckBox _allowZoneChange;
        private DropDownList _exportMode;
        private DropDownList _helpMode;
        private TextBox _description;
        private TextBox _titleUrl;
        private TextBox _titleIconImageUrl;
        private TextBox _catalogIconImageUrl;
        private TextBox _helpUrl;
        private TextBox _importErrorMessage;
        private TextBox _authorizationFilter;
        private CheckBox _allowEdit;

        private string _allowCloseErrorMessage;
        private string _allowConnectErrorMessage;
        private string _allowHideErrorMessage;
        private string _allowMinimizeErrorMessage;
        private string _allowZoneChangeErrorMessage;
        private string _exportModeErrorMessage;
        private string _helpModeErrorMessage;
        private string _descriptionErrorMessage;
        private string _titleUrlErrorMessage;
        private string _titleIconImageUrlErrorMessage;
        private string _catalogIconImageUrlErrorMessage;
        private string _helpUrlErrorMessage;
        private string _importErrorMessageErrorMessage;
        private string _authorizationFilterErrorMessage;
        private string _allowEditErrorMessage;

        private const int TextBoxColumns = 30;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), Themeable(false)]
        public override string DefaultButton {
            get { return base.DefaultButton; }
            set { base.DefaultButton = value; }
        }

        public override bool Display {
            get {
                if (WebPartToEdit != null &&
                    WebPartToEdit.IsShared &&
                    WebPartManager != null &&
                    (WebPartManager.Personalization.Scope == PersonalizationScope.User)) {
                    return false;
                }

                return base.Display;
            }
        }

        private bool HasError {
            get {
                return (_allowCloseErrorMessage != null || _allowConnectErrorMessage != null ||
                        _allowHideErrorMessage != null || _allowMinimizeErrorMessage != null ||
                        _allowZoneChangeErrorMessage != null || _exportModeErrorMessage != null ||
                        _helpModeErrorMessage != null || _descriptionErrorMessage != null ||
                        _titleUrlErrorMessage != null || _titleIconImageUrlErrorMessage != null ||
                        _catalogIconImageUrlErrorMessage != null || _helpUrlErrorMessage != null ||
                        _importErrorMessageErrorMessage != null || _authorizationFilterErrorMessage != null ||
                        _allowEditErrorMessage != null);
            }
        }

        [
        WebSysDefaultValue(SR.BehaviorEditorPart_PartTitle),
        ]
        public override string Title {
            get {
                string s = (string)ViewState["Title"];
                return (s != null) ? s : SR.GetString(SR.BehaviorEditorPart_PartTitle);
            }
            set {
                ViewState["Title"] = value;
            }
        }

        public override bool ApplyChanges() {
            WebPart webPart = WebPartToEdit;

            Debug.Assert(webPart != null);
            if (webPart != null) {
                EnsureChildControls();

                bool allowLayoutChange = webPart.Zone.AllowLayoutChange;

                if (allowLayoutChange) {
                    try {
                        webPart.AllowClose = _allowClose.Checked;
                    }
                    catch (Exception e) {
                        _allowCloseErrorMessage = CreateErrorMessage(e.Message);
                    }

                }

                try {
                    webPart.AllowConnect = _allowConnect.Checked;
                }
                catch (Exception e) {
                    _allowConnectErrorMessage = CreateErrorMessage(e.Message);
                }

                if (allowLayoutChange) {
                    try {
                        webPart.AllowHide = _allowHide.Checked;
                    }
                    catch (Exception e) {
                        _allowHideErrorMessage = CreateErrorMessage(e.Message);
                    }
                }

                if (allowLayoutChange) {
                    try {
                        webPart.AllowMinimize = _allowMinimize.Checked;
                    }
                    catch (Exception e) {
                        _allowMinimizeErrorMessage = CreateErrorMessage(e.Message);
                    }
                }

                if (allowLayoutChange) {
                    try {
                        webPart.AllowZoneChange = _allowZoneChange.Checked;
                    }
                    catch (Exception e) {
                        _allowZoneChangeErrorMessage = CreateErrorMessage(e.Message);
                    }
                }

                try {
                    TypeConverter exportModeConverter = TypeDescriptor.GetConverter(typeof(WebPartExportMode));
                    webPart.ExportMode = (WebPartExportMode)exportModeConverter.ConvertFromString(_exportMode.SelectedValue);
                }
                catch (Exception e) {
                    _exportModeErrorMessage = CreateErrorMessage(e.Message);
                }

                try {
                    TypeConverter helpModeConverter = TypeDescriptor.GetConverter(typeof(WebPartHelpMode));
                    webPart.HelpMode = (WebPartHelpMode)helpModeConverter.ConvertFromString(_helpMode.SelectedValue);
                }
                catch (Exception e) {
                    _helpModeErrorMessage = CreateErrorMessage(e.Message);
                }

                try {
                    webPart.Description = _description.Text;
                }
                catch (Exception e) {
                    _descriptionErrorMessage = CreateErrorMessage(e.Message);
                }

                string value = _titleUrl.Text;
                if (CrossSiteScriptingValidation.IsDangerousUrl(value)) {
                    _titleUrlErrorMessage = SR.GetString(SR.EditorPart_ErrorBadUrl);
                }
                else {
                    try {
                        webPart.TitleUrl = value;
                    }
                    catch (Exception e) {
                        _titleUrlErrorMessage = CreateErrorMessage(e.Message);
                    }
                }

                value = _titleIconImageUrl.Text;
                if (CrossSiteScriptingValidation.IsDangerousUrl(value)) {
                    _titleIconImageUrlErrorMessage = SR.GetString(SR.EditorPart_ErrorBadUrl);
                }
                else {
                    try {
                        webPart.TitleIconImageUrl = value;
                    }
                    catch (Exception e) {
                        _titleIconImageUrlErrorMessage = CreateErrorMessage(e.Message);
                    }
                }

                value = _catalogIconImageUrl.Text;
                if (CrossSiteScriptingValidation.IsDangerousUrl(value)) {
                    _catalogIconImageUrlErrorMessage = SR.GetString(SR.EditorPart_ErrorBadUrl);
                }
                else {
                    try {
                        webPart.CatalogIconImageUrl = value;
                    }
                    catch (Exception e) {
                        _catalogIconImageUrlErrorMessage = CreateErrorMessage(e.Message);
                    }
                }

                value = _helpUrl.Text;
                if (CrossSiteScriptingValidation.IsDangerousUrl(value)) {
                    _helpUrlErrorMessage = SR.GetString(SR.EditorPart_ErrorBadUrl);
                }
                else {
                    try {
                        webPart.HelpUrl = value;
                    }
                    catch (Exception e) {
                        _helpUrlErrorMessage = CreateErrorMessage(e.Message);
                    }
                }

                try {
                    webPart.ImportErrorMessage = _importErrorMessage.Text;
                }
                catch (Exception e) {
                    _importErrorMessageErrorMessage = CreateErrorMessage(e.Message);
                }

                try {
                    webPart.AuthorizationFilter = _authorizationFilter.Text;
                }
                catch (Exception e) {
                    _authorizationFilterErrorMessage = CreateErrorMessage(e.Message);
                }

                try {
                    webPart.AllowEdit = _allowEdit.Checked;
                }
                catch (Exception e) {
                    _allowEditErrorMessage = CreateErrorMessage(e.Message);
                }
            }

            return !HasError;
        }

        protected internal override void CreateChildControls() {
            ControlCollection controls = Controls;
            controls.Clear();

            _allowClose = new CheckBox();
            controls.Add(_allowClose);

            _allowConnect = new CheckBox();
            controls.Add(_allowConnect);

            _allowHide = new CheckBox();
            controls.Add(_allowHide);

            _allowMinimize = new CheckBox();
            controls.Add(_allowMinimize);

            _allowZoneChange = new CheckBox();
            controls.Add(_allowZoneChange);

            TypeConverter exportModeConverter = TypeDescriptor.GetConverter(typeof(WebPartExportMode));
            _exportMode = new DropDownList();
            _exportMode.Items.AddRange(new ListItem[] {
                new ListItem(SR.GetString(SR.BehaviorEditorPart_ExportModeNone),
                             exportModeConverter.ConvertToString(WebPartExportMode.None)),
                new ListItem(SR.GetString(SR.BehaviorEditorPart_ExportModeAll),
                             exportModeConverter.ConvertToString(WebPartExportMode.All)),
                new ListItem(SR.GetString(SR.BehaviorEditorPart_ExportModeNonSensitiveData),
                             exportModeConverter.ConvertToString(WebPartExportMode.NonSensitiveData)),
            });
            controls.Add(_exportMode);

            TypeConverter helpModeConverter = TypeDescriptor.GetConverter(typeof(WebPartHelpMode));
            _helpMode = new DropDownList();
            _helpMode.Items.AddRange(new ListItem[] {
                new ListItem(SR.GetString(SR.BehaviorEditorPart_HelpModeModal),
                             helpModeConverter.ConvertToString(WebPartHelpMode.Modal)),
                new ListItem(SR.GetString(SR.BehaviorEditorPart_HelpModeModeless),
                             helpModeConverter.ConvertToString(WebPartHelpMode.Modeless)),
                new ListItem(SR.GetString(SR.BehaviorEditorPart_HelpModeNavigate),
                             helpModeConverter.ConvertToString(WebPartHelpMode.Navigate)),
            });
            controls.Add(_helpMode);

            _description = new TextBox();
            _description.Columns = TextBoxColumns;
            controls.Add(_description);

            _titleUrl = new TextBox();
            _titleUrl.Columns = TextBoxColumns;
            controls.Add(_titleUrl);

            _titleIconImageUrl = new TextBox();
            _titleIconImageUrl.Columns = TextBoxColumns;
            controls.Add(_titleIconImageUrl);

            _catalogIconImageUrl = new TextBox();
            _catalogIconImageUrl.Columns = TextBoxColumns;
            controls.Add(_catalogIconImageUrl);

            _helpUrl = new TextBox();
            _helpUrl.Columns = TextBoxColumns;
            controls.Add(_helpUrl);

            _importErrorMessage = new TextBox();
            _importErrorMessage.Columns = TextBoxColumns;
            controls.Add(_importErrorMessage);

            _authorizationFilter = new TextBox();
            _authorizationFilter.Columns = TextBoxColumns;
            controls.Add(_authorizationFilter);

            _allowEdit = new CheckBox();
            controls.Add(_allowEdit);

            // We don't need viewstate enabled on our child controls.  Disable for perf.
            foreach (Control c in controls) {
                c.EnableViewState = false;
            }
        }

        protected internal override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);

            // We want to synchronize the EditorPart to the state of the WebPart on every page load,
            // so we stay current if the WebPart changes in the background.
            if (Display && Visible && !HasError) {
                SyncChanges();
            }
        }

        protected internal override void RenderContents(HtmlTextWriter writer) {
            if (Page != null) {
                Page.VerifyRenderingInServerForm(this);
            }

            // HACK: Need this for child controls to be created at design-time when control is inside template
            EnsureChildControls();

            string[] propertyDisplayNames = new string[] {
                SR.GetString(SR.BehaviorEditorPart_Description),
                SR.GetString(SR.BehaviorEditorPart_TitleLink),
                SR.GetString(SR.BehaviorEditorPart_TitleIconImageLink),
                SR.GetString(SR.BehaviorEditorPart_CatalogIconImageLink),
                SR.GetString(SR.BehaviorEditorPart_HelpLink),
                SR.GetString(SR.BehaviorEditorPart_HelpMode),
                SR.GetString(SR.BehaviorEditorPart_ImportErrorMessage),
                SR.GetString(SR.BehaviorEditorPart_ExportMode),
                SR.GetString(SR.BehaviorEditorPart_AuthorizationFilter),
                SR.GetString(SR.BehaviorEditorPart_AllowClose),
                SR.GetString(SR.BehaviorEditorPart_AllowConnect),
                SR.GetString(SR.BehaviorEditorPart_AllowEdit),
                SR.GetString(SR.BehaviorEditorPart_AllowHide),
                SR.GetString(SR.BehaviorEditorPart_AllowMinimize),
                SR.GetString(SR.BehaviorEditorPart_AllowZoneChange),
            };

            WebControl[] propertyEditors = new WebControl[] {
                _description,
                _titleUrl,
                _titleIconImageUrl,
                _catalogIconImageUrl,
                _helpUrl,
                _helpMode,
                _importErrorMessage,
                _exportMode,
                _authorizationFilter,
                _allowClose,
                _allowConnect,
                _allowEdit,
                _allowHide,
                _allowMinimize,
                _allowZoneChange,
            };

            string[] errorMessages = new string[] {
                _descriptionErrorMessage,
                _titleUrlErrorMessage,
                _titleIconImageUrlErrorMessage,
                _catalogIconImageUrlErrorMessage,
                _helpUrlErrorMessage,
                _helpModeErrorMessage,
                _importErrorMessageErrorMessage,
                _exportModeErrorMessage,
                _authorizationFilterErrorMessage,
                _allowCloseErrorMessage,
                _allowConnectErrorMessage,
                _allowEditErrorMessage,
                _allowHideErrorMessage,
                _allowMinimizeErrorMessage,
                _allowZoneChangeErrorMessage,
            };

            RenderPropertyEditors(writer, propertyDisplayNames, null /* propertyDescriptions */,
                                  propertyEditors, errorMessages);
        }

        public override void SyncChanges() {
            WebPart webPart = WebPartToEdit;

            Debug.Assert(webPart != null);
            if (webPart != null) {
                bool allowLayoutChange = webPart.Zone.AllowLayoutChange;

                EnsureChildControls();
                _allowClose.Checked = webPart.AllowClose;
                _allowClose.Enabled = allowLayoutChange;

                _allowConnect.Checked = webPart.AllowConnect;

                _allowHide.Checked = webPart.AllowHide;
                _allowHide.Enabled = allowLayoutChange;

                _allowMinimize.Checked = webPart.AllowMinimize;
                _allowMinimize.Enabled = allowLayoutChange;

                _allowZoneChange.Checked = webPart.AllowZoneChange;
                _allowZoneChange.Enabled = allowLayoutChange;

                TypeConverter exportModeConverter = TypeDescriptor.GetConverter(typeof(WebPartExportMode));
                _exportMode.SelectedValue = exportModeConverter.ConvertToString(webPart.ExportMode);

                TypeConverter helpModeConverter = TypeDescriptor.GetConverter(typeof(WebPartHelpMode));
                _helpMode.SelectedValue = helpModeConverter.ConvertToString(webPart.HelpMode);

                _description.Text = webPart.Description;
                _titleUrl.Text = webPart.TitleUrl;
                _titleIconImageUrl.Text = webPart.TitleIconImageUrl;
                _catalogIconImageUrl.Text = webPart.CatalogIconImageUrl;
                _helpUrl.Text = webPart.HelpUrl;
                _importErrorMessage.Text = webPart.ImportErrorMessage;
                _authorizationFilter.Text = webPart.AuthorizationFilter;
                _allowEdit.Checked = webPart.AllowEdit;
            }
        }
    }
}
