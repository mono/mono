//------------------------------------------------------------------------------
// <copyright file="PageRequestManager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Security;
    using System.Text;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.Resources;
    using System.Web.Script.Serialization;
    using System.Reflection;
    using System.Security.Permissions;

    internal sealed class PageRequestManager {

        // Type tokens for partial rendering format
        internal const string UpdatePanelVersionToken = "#";
        internal const string UpdatePanelVersionNumber = "4";
        internal const string PageRedirectToken = "pageRedirect";
        internal const string HiddenFieldToken = "hiddenField";
        private const string AsyncPostBackControlIDsToken = "asyncPostBackControlIDs";
        private const string PostBackControlIDsToken = "postBackControlIDs";
        private const string UpdatePanelIDsToken = "updatePanelIDs";
        private const string AsyncPostBackTimeoutToken = "asyncPostBackTimeout";
        private const string ChildUpdatePanelIDsToken = "childUpdatePanelIDs";
        private const string UpdatePanelsToRefreshToken = "panelsToRefreshIDs";
        private const string FormActionToken = "formAction";
        private const string DataItemToken = "dataItem";
        private const string DataItemJsonToken = "dataItemJson";
        internal const string ArrayDeclarationToken = "arrayDeclaration";
        internal const string ExpandoToken = "expando";
        internal const string OnSubmitToken = "onSubmit";
        internal const string ScriptBlockToken = "scriptBlock";
        internal const string ScriptStartupBlockToken = "scriptStartupBlock";
        internal const string ScriptDisposeToken = "scriptDispose";
        internal const string ErrorToken = "error";
        internal const string AsyncPostBackErrorKey = "System.Web.UI.PageRequestManager:AsyncPostBackError";
        internal const string AsyncPostBackErrorMessageKey = "System.Web.UI.PageRequestManager:AsyncPostBackErrorMessage";
        internal const string AsyncPostBackErrorHttpCodeKey = "System.Web.UI.PageRequestManager:AsyncPostBackErrorHttpCode";
        private const string PageTitleToken = "pageTitle";
        private const string FocusToken = "focus";
        private const string AsyncPostFormField = "__ASYNCPOST";

        private const char LengthEncodeDelimiter = '|';
        private static readonly Version MinimumW3CDomVersion = new Version(1, 0);
        private static readonly Version MinimumEcmaScriptVersion = new Version(1, 0);

        private ScriptManager _owner;

        private List<UpdatePanel> _allUpdatePanels;
        private List<UpdatePanel> _updatePanelsToRefresh;
        private List<UpdatePanel> _childUpdatePanelsToRefresh;
        private List<Control> _asyncPostBackControls;
        private List<Control> _postBackControls;
        private ScriptDataItemCollection _scriptDataItems;
        private string _updatePanelRequiresUpdate;
        private string[] _updatePanelsRequireUpdate;
        private HtmlTextWriter _updatePanelWriter;
        private bool _panelsInitialized;
        private string _asyncPostBackSourceElementID;

        // Stolen from Whidbey Page.cs for focus support
        private static readonly Version FocusMinimumEcmaVersion = new Version("1.4");
        private static readonly Version FocusMinimumJScriptVersion = new Version("3.0");
        private string _focusedControlID;
        private Control _focusedControl;
        private bool _requireFocusScript;

        public PageRequestManager(ScriptManager owner) {
            Debug.Assert(owner != null);
            _owner = owner;
        }


        public string AsyncPostBackSourceElementID {
            get {
                if (_asyncPostBackSourceElementID == null) {
                    return String.Empty;
                }
                return _asyncPostBackSourceElementID;
            }
        }

        // Stolen from Whidbey Page.cs
        private bool ClientSupportsFocus {
            get {
                HttpBrowserCapabilitiesBase browser = _owner.IPage.Request.Browser;
                return
                    (browser.EcmaScriptVersion >= FocusMinimumEcmaVersion) ||
                    (browser.JScriptVersion >= FocusMinimumJScriptVersion);
            }
        }

        private bool EnableLegacyRendering {
            get {
                return _owner.EnableLegacyRendering;
            }
        }

        [SecuritySafeCritical()]
        private bool CustomErrorsSectionHasRedirect(int httpCode) {
            bool hasRedirect = (_owner.CustomErrorsSection.DefaultRedirect != null);
            if (!hasRedirect) {
                if (_owner.CustomErrorsSection.Errors != null) {
                    foreach (CustomError error in _owner.CustomErrorsSection.Errors) {
                        if (error.StatusCode == httpCode) {
                            hasRedirect = true;
                            break;
                        }
                    }
                }
            }
            return hasRedirect;
        }

        // Optimized version of EncodeString that writes directly to a writer. This
        // eliminates the need to create several copies of the same string as well
        // as a StringBuilder.
        internal static void EncodeString(TextWriter writer, string type, string id, string content) {
            Debug.Assert(!String.IsNullOrEmpty(type), "Type should always be specified");
            if (id == null) {
                id = String.Empty;
            }
            if (content == null) {
                content = String.Empty;
            }
            Debug.Assert(type.IndexOf(LengthEncodeDelimiter) == -1, "Should not be a " + LengthEncodeDelimiter + " in type");
            Debug.Assert(id.IndexOf(LengthEncodeDelimiter) == -1, "Should not be a " + LengthEncodeDelimiter + " in id");

            // len|type|id|content|
            //             -------   len

            writer.Write(content.Length.ToString(CultureInfo.InvariantCulture));
            writer.Write(LengthEncodeDelimiter);
            writer.Write(type);
            writer.Write(LengthEncodeDelimiter);
            writer.Write(id);
            writer.Write(LengthEncodeDelimiter);
            // DevDiv 75383: We used to escape null characters from the content, but this had a non trivial hit on perf
            // They were escaped because XMLHttpRequest in IE truncates content after a null character.
            // However, when HTML contains a null character, subsequent content is truncated anyway, so the value of escaping nulls
            // in the first place is not clear and it was decided it is not worth the perf hit.
            writer.Write(content);
            writer.Write(LengthEncodeDelimiter);
        }

        private string GetAllUpdatePanelIDs() {
            return GetUpdatePanelIDsFromList(_allUpdatePanels, IDType.Both, true);
        }

        private string GetAsyncPostBackControlIDs(bool includeQuotes) {
            return GetControlIDsFromList(_asyncPostBackControls, includeQuotes);
        }

        private string GetChildUpdatePanelIDs() {
            return GetUpdatePanelIDsFromList(_childUpdatePanelsToRefresh, IDType.UniqueID, false);
        }

        private static string GetControlIDsFromList(List<Control> list, bool includeQuotes) {
            if (list != null && list.Count > 0) {
                StringBuilder idList = new StringBuilder();
                bool first = true;
                for (int i = 0; i < list.Count; i++) {
                    var control = list[i];
                    if (!control.Visible) {
                        // If the panel isn't visible, the client doesn't need to know about it
                        continue;
                    }
                    if (!first) {
                        idList.Append(',');
                    }
                    first = false;
                    if (includeQuotes) {
                        idList.Append('\'');
                    }
                    idList.Append(control.UniqueID);
                    if (includeQuotes) {
                        idList.Append('\'');
                    }
                    if (control.EffectiveClientIDMode == ClientIDMode.AutoID) {
                        if (includeQuotes) {
                            idList.Append(",''");
                        }
                        else {
                            idList.Append(',');
                        }
                    }
                    else {
                        if (includeQuotes) {
                            idList.Append(",'");
                            idList.Append(control.ClientID);
                            idList.Append('\'');
                        }
                        else {
                            idList.Append(',');
                            idList.Append(control.ClientID);
                        }
                    }
                }
                return idList.ToString();
            }
            return String.Empty;
        }

        private static Exception GetControlRegistrationException(Control control) {
            // DevDiv Bugs 145573: It is ok to register the Page as an async/postback control
            if (control == null) {
                return new ArgumentNullException("control");
            }
            if (!(control is INamingContainer) &&
                !(control is IPostBackDataHandler) &&
                !(control is IPostBackEventHandler)) {
                return new ArgumentException(String.Format(CultureInfo.InvariantCulture, AtlasWeb.ScriptManager_InvalidControlRegistration, control.ID));
            }
            return null;
        }

        // This code is roughly stolen from HttpException.GetHttpCodeForException()
        private static int GetHttpCodeForException(Exception e) {
            HttpException he = e as HttpException;
            if (he != null) {
                return he.GetHttpCode();
            }
            else if (e is UnauthorizedAccessException) {
                return 401;
            }
            else if (e is PathTooLongException) {
                return 414;
            }

            // If there is an inner exception, try to get the code from it
            if (e.InnerException != null)
                return GetHttpCodeForException(e.InnerException);

            // If all else fails, use 500
            return 500;
        }

        private static string GetMasterPageUniqueID(Page page) {
            // return the UniqueID of the root master page, if any.
            // The root master page has the full UniqueID prefix that 
            // all controls will have at the start of their 'UniqueID',
            // counter intuitively it is not the last Master Page with this
            // full uniqueID.
            MasterPage m = page.Master;
            if (m != null) {
                while (m.Master != null) {
                    m = m.Master;
                }
                return m.UniqueID;
            }
            return String.Empty;
        }

        private string GetPostBackControlIDs(bool includeQuotes) {
            return GetControlIDsFromList(_postBackControls,  includeQuotes);
        }

        private string GetRefreshingUpdatePanelIDs() {
            return GetUpdatePanelIDsFromList(_updatePanelsToRefresh, IDType.Both, false);
        }

        private static string GetUpdatePanelIDsFromList(List<UpdatePanel> list, IDType idType, bool includeChildrenAsTriggersPrefix) {
            if (list != null && list.Count > 0) {
                StringBuilder updatePanelIDs = new StringBuilder();
                bool first = true;
                for (int i = 0; i < list.Count; i++) {
                    var up = list[i];
                    if (!up.Visible) {
                        // If the panel isn't visible, the client doesn't need to know about it
                        continue;
                    }
                    if (!first) {
                        updatePanelIDs.Append(',');
                    }
                    first = false;
                    // We send down the UniqueID instead of the ClientID because
                    // we need both versions on the client. You can convert from
                    // UniqueID to ClientID, but not back.

                    // If the UpdatePanel has its ClientID set, we cannot convert
                    // it to UniqueID, so we send both.

                    // We also send down a bool indicating whether the children of
                    // the panel count as triggers or not.
                    if (includeChildrenAsTriggersPrefix) {
                        updatePanelIDs.Append(up.ChildrenAsTriggers ? 't' : 'f');
                    }
                    updatePanelIDs.Append(up.UniqueID);
                    if (idType == IDType.Both) {
                        updatePanelIDs.Append(',');
                        if (up.EffectiveClientIDMode != ClientIDMode.AutoID) {
                            updatePanelIDs.Append(up.ClientID);
                        }
                    }
                }
                return updatePanelIDs.ToString();
            }
            return String.Empty;
        }

        internal static bool IsAsyncPostBackRequest(HttpRequestBase request) {
            // Detect the header for async postbacks. A header can appear
            // multiple times, and each header entry can contain a comma-separated
            // list of values. ASP.NET doesn't split the comma-separated values for
            // us so we have to do it.

            // We used to use the Pragma header but some browsers, such as Opera,
            // do not support sending it through XMLHttpRequest. Instead we use a
            // custom header, X-MicrosoftAjax.
            string[] headerValues = request.Headers.GetValues("X-MicrosoftAjax");
            if (headerValues != null) {
                for (int i = 0; i < headerValues.Length; i++) {
                    string[] headerContents = headerValues[i].Split(',');
                    for (int j = 0; j < headerContents.Length; j++) {
                        if (headerContents[j].Trim() == "Delta=true") {
                            return true;
                        }
                    }
                }
            }
            // DevDiv Bugs 188713: X-MicrosoftAjax header is stripped by some firewalls
            string asyncPost = request.Form[AsyncPostFormField];
            return !String.IsNullOrEmpty(asyncPost) &&
                (asyncPost.Trim() == "true");
        }

        internal void LoadPostData(string postDataKey, NameValueCollection postCollection) {
            // Check if the async postback was caused by a specific panel, and if so, force
            // that panel to update, regardless of whether it had any explicit triggers, etc.
            // If the post back data starts with the ScriptManager's UniqueID that means the
            // async postback was caused by a control outside of an UpdatePanel, and the rest
            // of the string is the UniqueID of that control.
            string postBackSourceInfo = postCollection[postDataKey];
            if (postBackSourceInfo != null) {
                string postBackTarget; // The target of the postback - either the ScriptManager or an UpdatePanel

                int indexOfPipe = postBackSourceInfo.IndexOf('|');
                if (indexOfPipe != -1) {
                    // We have a target and source element
                    postBackTarget = postBackSourceInfo.Substring(0, indexOfPipe);
                    _asyncPostBackSourceElementID = postBackSourceInfo.Substring(indexOfPipe + 1);
                }
                else {
                    // We only have a target
                    postBackTarget = postBackSourceInfo;
                    _asyncPostBackSourceElementID = String.Empty;
                }

                if (postBackTarget != _owner.UniqueID) {
                    if (postBackTarget.IndexOf(',') != -1) {
                        _updatePanelRequiresUpdate = null;
                        _updatePanelsRequireUpdate = postBackTarget.Split(',');
                    }
                    else {
                        _updatePanelRequiresUpdate = postBackTarget;
                        _updatePanelsRequireUpdate = null;
                    }
                }
            }

            // Initialize all UpdatePanels (and their triggers, specifically) so that
            // they can hook events, etc. before other controls can process their
            // own post data.
            // LoadPostData on ScriptManager only gets called during async posts, and
            // is guaranteed to be called before any other controls have a chance to
            // process their post data.
            // During regular posts the UpdatePanel initializes itself in OnLoad.
            if ((_allUpdatePanels != null) && (_allUpdatePanels.Count != 0)) {
                foreach (UpdatePanel panel in _allUpdatePanels) {
                    panel.Initialize();
                }
            }

            _panelsInitialized = true;
        }

        internal void OnInit() {
            // Check if the browser supports partial rendering. We only do the check
            // if the user hasn't already forced the feature to be on or off explicitly.
            if (_owner.EnablePartialRendering && !_owner._supportsPartialRenderingSetByUser) {
                HttpBrowserCapabilitiesBase browser = _owner.IPage.Request.Browser;
                // There is no browser cap that directly tells us whether the browser
                // supports XmlHttpRequest so we use the next best capability, which is
                // the SupportsCallback property.
                // Checking the other properties helps exclude agents such as crawlers.
                bool supportsPartialRendering =
                    (browser.W3CDomVersion >= MinimumW3CDomVersion) &&
                    (browser.EcmaScriptVersion >= MinimumEcmaScriptVersion) &&
                    browser.SupportsCallback;
                if (supportsPartialRendering) {
                    // If we still think we want to support it, now do a more expensive
                    // check for XHTML legacy rendering support.
                    supportsPartialRendering = !EnableLegacyRendering;
                }
                _owner.SupportsPartialRendering = supportsPartialRendering;
            }

            if (_owner.IsInAsyncPostBack) {
                _owner.IPage.Error += OnPageError;
            }
        }

        private void OnPageError(object sender, EventArgs e) {
            Exception ex = _owner.IPage.Server.GetLastError();
            _owner.OnAsyncPostBackError(new AsyncPostBackErrorEventArgs(ex));

            string errorMessage = _owner.AsyncPostBackErrorMessage;
            if (String.IsNullOrEmpty(errorMessage) && !_owner.Control.Context.IsCustomErrorEnabled) {
                // Only use the exception's message if we're not doing custom errors
                errorMessage = ex.Message;
            }

            int httpCode = GetHttpCodeForException(ex);

            bool showAsyncErrorMessage = false;

            if (_owner.AllowCustomErrorsRedirect && _owner.Control.Context.IsCustomErrorEnabled) {
                // Figure out if there's going to be a redirect for this error
                bool hasRedirect = CustomErrorsSectionHasRedirect(httpCode);
                if (!hasRedirect) {
                    // If there's no redirect, we need to send back the error message
                    showAsyncErrorMessage = true;
                }
                // If there was a redirect we do nothing since ASP.NET will automatically
                // redirect the user to the error page anyway. This way we don't have to
                // worry about how to resolve the paths from config.
            }
            else {
                // If we're not going to use custom errors, just send back the error message
                showAsyncErrorMessage = true;
            }

            if (showAsyncErrorMessage) {
                IDictionary items = _owner.Control.Context.Items;
                items[AsyncPostBackErrorKey] = true;
                items[AsyncPostBackErrorMessageKey] = errorMessage;
                items[AsyncPostBackErrorHttpCodeKey] = httpCode;
            }
        }

        internal void OnPreRender() {
            _owner.IPage.SetRenderMethodDelegate(RenderPageCallback);
        }

        private void ProcessFocus(HtmlTextWriter writer) {
            // Roughly stolen from Whidbey Page.cs
            if (_requireFocusScript) {
                Debug.Assert(ClientSupportsFocus, "If ClientSupportsFocus is false then we never should have set _requireFocusScript to true.");
                string focusedControlId = String.Empty;

                // Someone calling SetFocus(controlId) has the most precedent
                if (!String.IsNullOrEmpty(_focusedControlID)) {
                    focusedControlId = _focusedControlID;
                }
                else {
                    if (_focusedControl != null && _focusedControl.Visible) {
                        focusedControlId = _focusedControl.ClientID;
                    }
                }
                if (focusedControlId.Length > 0) {
                    // Register focus script library
                    string focusResourceUrl = _owner.GetScriptResourceUrl("Focus.js", typeof(HtmlForm).Assembly);
                    EncodeString(writer, ScriptBlockToken, "ScriptPath", focusResourceUrl);

                    // Send the target control ID to the client
                    EncodeString(writer, FocusToken, String.Empty, focusedControlId);
                }
            }
        }

        private void ProcessScriptRegistration(HtmlTextWriter writer) {
            _owner.ScriptRegistration.RenderActiveArrayDeclarations(_updatePanelsToRefresh, writer);
            _owner.ScriptRegistration.RenderActiveScripts(_updatePanelsToRefresh, writer);
            _owner.ScriptRegistration.RenderActiveSubmitStatements(_updatePanelsToRefresh, writer);
            _owner.ScriptRegistration.RenderActiveExpandos(_updatePanelsToRefresh, writer);
            _owner.ScriptRegistration.RenderActiveHiddenFields(_updatePanelsToRefresh, writer);
            _owner.ScriptRegistration.RenderActiveScriptDisposes(_updatePanelsToRefresh, writer);
        }

        private void ProcessUpdatePanels() {
            Debug.Assert(_owner.IsInAsyncPostBack);
            Debug.Assert(_updatePanelsToRefresh == null);

            if (_allUpdatePanels != null) {
                _updatePanelsToRefresh = new List<UpdatePanel>(_allUpdatePanels.Count);
                _childUpdatePanelsToRefresh = new List<UpdatePanel>(_allUpdatePanels.Count);

                // Process the UpdatePanels to determine which are to be set in
                // partial rendering mode.

                // We need to process the list such that parent UpdatePanels are
                // evaluated first. A child UpdatePanel inside a parent that is being
                // updated should not be considered in partial rendering mode.
                // Ordinarily child controls get initialized first before their parents
                // so you'd expect the list to be in reverse order, but this isn't the case.
                // UpdatePanels instantiate their templates in their OnInit, so a child
                // UpdatePanel only exists in the control tree after the parent has been
                // initialized.

                HtmlForm form = _owner.Page.Form;

                for (int i = 0; i < _allUpdatePanels.Count; i++) {
                    UpdatePanel panel = _allUpdatePanels[i];

                    // Check whether the panel thinks it wants to update. Possible reasons
                    // a panel might be updating:
                    // - Postback data indicates the postback came from within the panel
                    // - Postback data indicates the postbacks was caused by PageRequestManager.beginAsyncPost
                    //   and the update panel was explicitly requested to update
                    // - Explicit call to panel.Update()
                    // - Panel UpdateMode set to Always
                    // - Trigger fired (not yet implemented)

                    bool requiresUpdate = panel.RequiresUpdate ||
                        (_updatePanelRequiresUpdate != null && String.Equals(panel.UniqueID, _updatePanelRequiresUpdate, StringComparison.Ordinal)) ||
                        (_updatePanelsRequireUpdate != null && Array.IndexOf(_updatePanelsRequireUpdate, panel.UniqueID) != -1);

                    // Check and see if a parent panel will take update this panel, whether
                    // this panel wants to update or not. If so, then this panel doesn't need
                    // to be in update mode since it will get included in the rendering
                    // by its parent anyway.
                    // If this parent doesn't want to update then we don't need to do any
                    // additional checks because whether it renders depends entirely on
                    // whether the parent wants to render.
                    Control parent = panel.Parent;
                    while (parent != form) {
                        UpdatePanel parentUpdatePanel = parent as UpdatePanel;
                        if ((parentUpdatePanel != null) &&
                            (_updatePanelsToRefresh.Contains(parentUpdatePanel) || _childUpdatePanelsToRefresh.Contains(parentUpdatePanel))) {
                            // This panel is inside another UpdatePanel that is being
                            // rendered, so it should render in normal mode.
                            requiresUpdate = false;
                            _childUpdatePanelsToRefresh.Add(panel);
                            break;
                        }

                        parent = parent.Parent;

                        if (parent == null) {
                            // This UpdatePanel was not inside an HtmlForm
                            // This really shouldn't happen, because the UpdatePanel would have thrown
                            // an exception on the initial GET request that it should be inside a form,
                            // so we'll just ignore it now...
                            requiresUpdate = false;
                            break;
                        }
                    }

                    if (requiresUpdate) {
                        panel.SetAsyncPostBackMode(true);
                        _updatePanelsToRefresh.Add(panel);
                    }
                    else {
                        panel.SetAsyncPostBackMode(false);
                    }
                }
            }
        }

        public void RegisterAsyncPostBackControl(Control control) {
            Exception ex = GetControlRegistrationException(control);
            if (ex != null) {
                throw ex;
            }
            if (_postBackControls != null && _postBackControls.Contains(control)) {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, AtlasWeb.ScriptManager_CannotRegisterBothPostBacks, control.ID));
            }
            if (_asyncPostBackControls == null) {
                _asyncPostBackControls = new List<Control>();
            }
            // It is acceptable to register the same control twice since the same
            // control might be referred to by more than one trigger.
            if (!_asyncPostBackControls.Contains(control)) {
                _asyncPostBackControls.Add(control);
            }
        }

        public void RegisterDataItem(Control control, string dataItem, bool isJsonSerialized) {
            if (control == null) {
                throw new ArgumentNullException("control");
            }
            if (!_owner.IsInAsyncPostBack) {
                throw new InvalidOperationException(AtlasWeb.PageRequestManager_RegisterDataItemInNonAsyncRequest);
            }
            if (_scriptDataItems == null) {
                _scriptDataItems = new ScriptDataItemCollection();
            }
            else {
                if (_scriptDataItems.ContainsControl(control)) {
                    throw new ArgumentException(
                        String.Format(
                            CultureInfo.InvariantCulture,
                            AtlasWeb.PageRequestManager_RegisterDataItemTwice,
                            control.ID),
                        "control");
                }
            }
            _scriptDataItems.Add(new ScriptDataItem(control, dataItem, isJsonSerialized));
        }

        private void RegisterFocusScript() {
            if (ClientSupportsFocus && !_requireFocusScript) {
                _requireFocusScript = true;
            }
        }

        public void RegisterPostBackControl(Control control) {
            Exception ex = GetControlRegistrationException(control);
            if (ex != null) {
                throw ex;
            }
            if (_asyncPostBackControls != null && _asyncPostBackControls.Contains(control)) {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, AtlasWeb.ScriptManager_CannotRegisterBothPostBacks, control.ID));
            }
            if (_postBackControls == null) {
                _postBackControls = new List<Control>();
            }
            // It is acceptable to register the same control twice since the same
            // control might be referred to by more than one trigger.
            if (!_postBackControls.Contains(control)) {
                _postBackControls.Add(control);
            }
        }

        internal void RegisterUpdatePanel(UpdatePanel updatePanel) {
            Debug.Assert(updatePanel != null);
            if (_allUpdatePanels == null) {
                _allUpdatePanels = new List<UpdatePanel>();
            }
            Debug.Assert(!_allUpdatePanels.Contains(updatePanel),
                String.Format(CultureInfo.InvariantCulture, "The UpdatePanel with ID '{0}' has already been registered with the ScriptManager.", updatePanel.ID));
            _allUpdatePanels.Add(updatePanel);

            if (_panelsInitialized) {
                // Do catch-up for panels that may have been added later in
                // the lifecycle during an async post.
                Debug.Assert(_owner.IsInAsyncPostBack, "Catch-up initialization should only be done in async posts.");
                updatePanel.Initialize();
            }
        }

        // Only call this method when these condictions are met:
        // if (!((IControl)_owner).DesignMode && !_owner.IsInAsyncPostBack && _owner.SupportsPartialRendering 
        //     && (_owner.MicrosoftAjaxMode != MicrosoftAjaxMode.Disabled))
        internal void Render(HtmlTextWriter writer) {
            _owner.IPage.VerifyRenderingInServerForm(_owner);
            RenderPageRequestManagerScript(writer);
        }

        private void RenderFormCallback(HtmlTextWriter writer, Control containerControl) {

            Debug.Assert(_updatePanelWriter != null, "_updatePanelWriter should be set by RenderPageCallback before RenderFormCallback is called.");

            // Suppress rendering of default content; instead just render out
            // update panels
            if (_updatePanelsToRefresh != null) {
                foreach (UpdatePanel panel in _updatePanelsToRefresh) {
                    if (panel.Visible) {
                        // Write UpdatePanels to the response's writer; the writer passed in is a
                        // dummy parserWriter.  It will contain hidden fields that are written to
                        // the response's writer later in RenderPageCallback.
                        panel.RenderControl(_updatePanelWriter);
                    }
                }
            }

            IPage page = _owner.IPage;
            if (page.EnableEventValidation) {
                // If the page has event validation turned on, we need to run through
                // the render logic for the rest of the page as well. However, the
                // rendering is essentially ignored.
                // UpdatePanels that were already rendered above do not re-render by checking
                // a flag whether they already rendered.
                
                // 





                // DevDiv 55447: Do not use Response.Flush and a NullStream to prevent Response.Writes
                // from being written to the output stream, as calling Response.Flush causes headers to
                // be written. This prevents cookies from being issued after this, for example.
                // Instead, use a NullWriter that ignores Writes. We can change the writer used by HttpResponse
                // using the internal SwitchWriter method.
                // We must do this since data written via Response.Write will make the partial update
                // response invalid.

                TextWriter oldWriter = null;
                bool writerSwitched = false;
                try {
                    // beginning of possible direct Response.Writes
                    oldWriter = page.Response.SwitchWriter(TextWriter.Null);
                    // if we cant switch the writer for some reason we need to know not to switch it back again in the finally block
                    // writerSwitched will be false
                    writerSwitched = true;

                    // nullHtmlWriter captures any writes by controls to the textwriter they are passed.
                    // Note that we could possibly just let null TextWriter we switched catch this data, but this
                    // is more efficient.
                    HtmlTextWriter nullHtmlWriter = new HtmlTextWriter(TextWriter.Null);
                    foreach (Control control in containerControl.Controls) {
                        control.RenderControl(nullHtmlWriter);
                    }
                }
                finally {
                    // end of possible direct Response.Writes
                    if (writerSwitched) {
                        page.Response.SwitchWriter(oldWriter);
                    }
                }
            }
        }

        private void RenderPageCallback(HtmlTextWriter writer, Control pageControl) {
            ProcessUpdatePanels();

            // Although we could use the pageControl parameter it's hard to write
            // unit tests for it. Instead we just use our own page, which is the
            // same instance anyway (but easier to test with).

            HttpResponseBase response = _owner.IPage.Response;

            response.ContentType = "text/plain";
            response.Cache.SetNoServerCaching();

            // Write out the version identifier, which helps the client-side deal with the response
            // in a back-compatible way when there are changes made server-side.
            EncodeString(writer, UpdatePanelVersionToken, String.Empty, UpdatePanelVersionNumber);

            // Render the form. It will render its tag, hidden fields, etc.
            // and then call our render method delegate, which will in turn render
            // all the UpdatePanels
            IHtmlForm formControl = _owner.IPage.Form;
            formControl.SetRenderMethodDelegate(RenderFormCallback);

            // Let updatePanels write directly to Response
            _updatePanelWriter = writer;

            // Let form header/footer write to a parser
            ParserHtmlTextWriter formWriter = new ParserHtmlTextWriter();
            formControl.RenderControl(formWriter);

            // Write out built-in ASP.NET hidden fields that were rendered directly by the page
            // or registered through RegisterHiddenField
            var hiddenFields = _owner.IPage.HiddenFieldsToRender;
            if (hiddenFields != null) {
                foreach (KeyValuePair<String, String> entry in hiddenFields) {
                    if (ControlUtil.IsBuiltInHiddenField(entry.Key)) {
                        EncodeString(writer, HiddenFieldToken, entry.Key, entry.Value);
                    }
                }
            }

            // Write out PageRequestManager settings that can change during an async postback.
            // This is required for dynamic UpdatePanels since the list of panels could
            // change.
            EncodeString(writer, AsyncPostBackControlIDsToken, String.Empty, GetAsyncPostBackControlIDs(false));
            EncodeString(writer, PostBackControlIDsToken, String.Empty, GetPostBackControlIDs(false));
            EncodeString(writer, UpdatePanelIDsToken, String.Empty, GetAllUpdatePanelIDs());
            EncodeString(writer, ChildUpdatePanelIDsToken, String.Empty, GetChildUpdatePanelIDs());
            EncodeString(writer, UpdatePanelsToRefreshToken, String.Empty, GetRefreshingUpdatePanelIDs());
            EncodeString(writer, AsyncPostBackTimeoutToken, String.Empty, _owner.AsyncPostBackTimeout.ToString(CultureInfo.InvariantCulture));
            if (formWriter.FormAction != null) {
                EncodeString(writer, FormActionToken, String.Empty, formWriter.FormAction);
            }
            if (_owner.IPage.Header != null) {
                string pageTitle = _owner.IPage.Title;
                if (!String.IsNullOrEmpty(pageTitle)) {
                    EncodeString(writer, PageTitleToken, String.Empty, pageTitle);
                }
            }
            RenderDataItems(writer);

            ProcessScriptRegistration(writer);

            // We process the focus after regular script registrations to
            // make sure that if it ends up including some script that it
            // executes last.
            ProcessFocus(writer);
        }

        private void RenderDataItems(HtmlTextWriter writer) {
            if (_scriptDataItems != null) {
                foreach (ScriptDataItem dataItem in _scriptDataItems) {
                    EncodeString(
                        writer,
                        dataItem.IsJsonSerialized ? DataItemJsonToken : DataItemToken,
                        dataItem.Control.ClientID,
                        dataItem.DataItem);
                }
            }
        }

        internal void RenderPageRequestManagerScript(HtmlTextWriter writer) {
            // 






            // Script format:
            // <script type=""text/javascript"">
            // //<![CDATA[
            // Sys.WebForms.PageRequestManager._initialize('{0}', '{1}', [{2}], [{3}], [{4}], {5}, {6});
            // //]]>
            // </script>

            // Writing directly to the writer is more performant than building
            // up a big string with formatting and then writing it out later.
            
            writer.Write(@"<script type=""text/javascript"">
//<![CDATA[
Sys.WebForms.PageRequestManager._initialize('");
            writer.Write(_owner.UniqueID);
            writer.Write(@"', '");
            writer.Write(_owner.IPage.Form.ClientID);
            writer.Write(@"', [");
            RenderUpdatePanelIDsFromList(writer, _allUpdatePanels);
            writer.Write("], [");
            writer.Write(GetAsyncPostBackControlIDs(true));
            writer.Write("], [");
            writer.Write(GetPostBackControlIDs(true));
            writer.Write("], ");
            writer.Write(_owner.AsyncPostBackTimeout.ToString(CultureInfo.InvariantCulture));
            writer.Write(", '");
            writer.Write(GetMasterPageUniqueID(_owner.Page));
            writer.WriteLine("');");
            writer.Write(@"//]]>
</script>
");
        }

        private static void RenderUpdatePanelIDsFromList(HtmlTextWriter writer, List<UpdatePanel> list) {
            // Writing directly to the writer is more performant than building
            // up a big string with formatting and then writing it out later.
            if (list != null && list.Count > 0) {
                bool first = true;
                for (int i = 0; i < list.Count; i++) {
                    UpdatePanel up = list[i];
                    if (!up.Visible) {
                        // If the panel isn't visible, the client doesn't need to know about it
                        continue;
                    }
                    if (!first) {
                        writer.Write(',');
                    }
                    first = false;

                    // Due to the settable ClientID feature, UpdatePanel
                    // needs both the clientID and uniqueID
                    // We also send down a bool indicating whether the children of
                    // the panel count as triggers or not.
                    // ['[t|f]uniqueid1','clientid1','[t|f]uniqueid2','clientid2',...]
                    writer.Write("'");
                    writer.Write(up.ChildrenAsTriggers ? 't' : 'f');
                    writer.Write(up.UniqueID);
                    writer.Write("',");
                    if (up.EffectiveClientIDMode == ClientIDMode.AutoID) {
                        writer.Write("''");
                    }
                    else {
                        writer.Write("'");
                        writer.Write(up.ClientID);
                        writer.Write("'");
                    }
                }
            }
        }

        public void SetFocus(Control control) {
            // We always call the real Page's method at least to do parameter validation
            _owner.IPage.SetFocus(control);

            // If it's not async, just let the page do whatever it wants. If we are
            // in an async post, we need to keep track of what to focus later on.
            if (_owner.IsInAsyncPostBack) {
                _focusedControl = control;
                _focusedControlID = null;
                RegisterFocusScript();
            }
        }

        public void SetFocus(string clientID) {
            // We always call the real Page's method at least to do parameter validation
            _owner.IPage.SetFocus(clientID);
            SetFocusInternal(clientID);
        }

        internal void SetFocusInternal(string clientID) {
            // If it's not async, just let the page do whatever it wants. If we are
            // in an async post, we need to keep track of what to focus later on.
            if (_owner.IsInAsyncPostBack) {
                _focusedControlID = clientID.Trim();
                _focusedControl = null;
                RegisterFocusScript();
            }
        }

        internal void UnregisterUpdatePanel(UpdatePanel updatePanel) {
            Debug.Assert(updatePanel != null);
            if ((_allUpdatePanels == null) || !_allUpdatePanels.Contains(updatePanel)) {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, AtlasWeb.ScriptManager_UpdatePanelNotRegistered, updatePanel.ID), "updatePanel");
            }
            _allUpdatePanels.Remove(updatePanel);
        }

        private sealed class ParserHtmlTextWriter : HtmlTextWriter {
            private bool _writingForm;
            private string _formAction;

            public ParserHtmlTextWriter() : base(TextWriter.Null) {
            }

            public string FormAction {
                get {
                    return _formAction;
                }
            }

            public override void WriteBeginTag(string tagName) {
                base.WriteBeginTag(tagName);

                _writingForm = (tagName == "form");
            }

            public override void WriteAttribute(string name, string value, bool fEncode) {
                base.WriteAttribute(name, value, fEncode);

                if (_writingForm) {
                    if (name == "action") {
                        _formAction = value;
                    }
                }
            }
        }

        private sealed class ScriptDataItem {
            private Control _control;
            private string _dataItem;
            private bool _isJsonSerialized;

            public ScriptDataItem(Control control, string dataItem, bool isJsonSerialized) {
                _control = control;
                _dataItem = (dataItem == null) ? String.Empty : dataItem;
                _isJsonSerialized = isJsonSerialized;
            }

            public Control Control {
                get {
                    return _control;
                }
            }

            public string DataItem {
                get {
                    return _dataItem;
                }
            }

            public bool IsJsonSerialized {
                get {
                    return _isJsonSerialized;
                }
            }
        }

        private sealed class ScriptDataItemCollection : List<ScriptDataItem> {
            public bool ContainsControl(Control control) {
                foreach (ScriptDataItem dataItem in this) {
                    if (dataItem.Control == control) {
                        return true;
                    }
                }
                return false;
            }
        }

        private enum IDType {
            UniqueID,
            Both
        }
    }
}
