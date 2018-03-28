//------------------------------------------------------------------------------
// <copyright file="WebPartPersonalization.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Configuration;
    using System.Configuration.Provider;
    using System.Security.Principal;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.UI;
    using System.Web.Util;
    using System.Web.Hosting;

    [TypeConverterAttribute(typeof(EmptyStringExpandableObjectConverter))]
    public class WebPartPersonalization {

        public static readonly WebPartUserCapability ModifyStateUserCapability = new WebPartUserCapability("modifyState");
        public static readonly WebPartUserCapability EnterSharedScopeUserCapability = new WebPartUserCapability("enterSharedScope");

        private WebPartManager _owner;

        // Properties
        private bool _enabled;
        private string _providerName;
        private PersonalizationScope _initialScope;

        // Computed state
        private bool _initialized;
        private bool _initializedSet;
        private PersonalizationProvider _provider;
        private PersonalizationScope _currentScope;
        private IDictionary _userCapabilities;
        private PersonalizationState _personalizationState;
        private bool _scopeToggled;
        private bool _shouldResetPersonalizationState;

        /// <devdoc>
        /// </devdoc>
        public WebPartPersonalization(WebPartManager owner) {
            if (owner == null) {
                throw new ArgumentNullException("owner");
            }

            _owner = owner;

            _enabled = true;
        }

        /// <devdoc>
        /// Indicates whether the current user has the permissions to switch
        /// into shared personalization scope.
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public bool CanEnterSharedScope {
            get {
                // We cannot cache this value, since UserCapabilities is protected virtual,
                // and could return a different value at any time
                IDictionary userCapabilities = UserCapabilities;
                bool canEnterSharedScope = (userCapabilities != null) &&
                    (userCapabilities.Contains(WebPartPersonalization.EnterSharedScopeUserCapability));
                return canEnterSharedScope;
            }
        }

        /// <devdoc>
        /// </devdoc>
        [
        DefaultValue(true),
        NotifyParentProperty(true),
        WebSysDescription(SR.WebPartPersonalization_Enabled)
        ]
        public virtual bool Enabled {
            get {
                return _enabled;
            }
            set {
                if (!WebPartManager.DesignMode && _initializedSet && (value != Enabled)) {
                    throw new InvalidOperationException(
                        SR.GetString(SR.WebPartPersonalization_MustSetBeforeInit, "Enabled", "WebPartPersonalization"));
                }

                _enabled = value;
            }
        }

        // Returns true if the current user in the current scope on the current page has personalization data
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public virtual bool HasPersonalizationState {
            get {
                if (_provider == null) {
                    throw new InvalidOperationException(SR.GetString(SR.WebPartPersonalization_CantUsePropertyBeforeInit,
                                                        "HasPersonalizationState", "WebPartPersonalization"));
                }

                Page page = WebPartManager.Page;
                if (page == null) {
                    throw new InvalidOperationException(SR.GetString(SR.PropertyCannotBeNull, "WebPartManager.Page"));
                }

                HttpRequest request = page.RequestInternal;
                if (request == null) {
                    throw new InvalidOperationException(SR.GetString(SR.PropertyCannotBeNull, "WebPartManager.Page.Request"));
                }

                PersonalizationStateQuery query = new PersonalizationStateQuery();
                query.PathToMatch = request.AppRelativeCurrentExecutionFilePath;
                if (Scope == PersonalizationScope.User && request.IsAuthenticated) {
                    query.UsernameToMatch = page.User.Identity.Name;
                }

                return (_provider.GetCountOfState(Scope, query) > 0);
            }
        }

        /// <devdoc>
        /// Allows changing the initial personalization scope that is given
        /// preference when requesting the page on its first request.
        /// This must be set before personalization data is loaded into the
        /// WebPartManager.
        /// </devdoc>
        [
        DefaultValue(PersonalizationScope.User),
        NotifyParentProperty(true),
        WebSysDescription(SR.WebPartPersonalization_InitialScope)
        ]
        public virtual PersonalizationScope InitialScope {
            get {
                return _initialScope;
            }
            set {
                if ((value < PersonalizationScope.User) || (value > PersonalizationScope.Shared)) {
                    throw new ArgumentOutOfRangeException("value");
                }

                if (!WebPartManager.DesignMode && _initializedSet && (value != InitialScope)) {
                    throw new InvalidOperationException(
                        SR.GetString(SR.WebPartPersonalization_MustSetBeforeInit, "InitialScope", "WebPartPersonalization"));
                }

                _initialScope = value;
            }
        }

        /// <devdoc>
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public bool IsEnabled {
            get {
                return IsInitialized;
            }
        }

        /// <devdoc>
        /// Indicates whether personalization state has been loaded. Properties of this
        /// object cannot be saved once this object is initialized.
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        protected bool IsInitialized {
            get {
                return _initialized;
            }
        }

        /// <devdoc>
        /// Determines if personalization and the ability to modify personalization state is enabled
        /// in the current request. This depends on whether a user is authenticated for this request,
        /// and if that user has the rights to modify personalization state.
        /// To check if just personalization is enabled at all, the IsEnabled property
        /// should be used.
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public bool IsModifiable {
            get {
                // We cannot cache this value, since UserCapabilities is protected virtual,
                // and could return a different value at any time
                IDictionary userCapabilities = UserCapabilities;
                bool isModifiable = (userCapabilities != null) &&
                    (userCapabilities.Contains(WebPartPersonalization.ModifyStateUserCapability));
                return isModifiable;
            }
        }

        /// <devdoc>
        /// </devdoc>
        [
        DefaultValue(""),
        NotifyParentProperty(true),
        WebSysDescription(SR.WebPartPersonalization_ProviderName)
        ]
        public virtual string ProviderName {
            get {
                return (_providerName != null) ? _providerName : String.Empty;
            }
            set {
                if (!WebPartManager.DesignMode && _initializedSet &&
                    !String.Equals(value, ProviderName, StringComparison.Ordinal)) {
                    throw new InvalidOperationException(
                        SR.GetString(SR.WebPartPersonalization_MustSetBeforeInit, "ProviderName", "WebPartPersonalization"));
                }

                _providerName = value;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public PersonalizationScope Scope {
            get {
                return _currentScope;
            }
        }

        internal bool ScopeToggled {
            get {
                return _scopeToggled;
            }
        }

        // True if the personalization data was reset this request.  If so, we will not save data
        // at the end of the request.
        protected bool ShouldResetPersonalizationState {
            get {
                return _shouldResetPersonalizationState;
            }
            set {
                _shouldResetPersonalizationState = value;
            }
        }

        protected virtual IDictionary UserCapabilities {
            get {
                if (_userCapabilities == null) {
                    _userCapabilities = new HybridDictionary();
                }
                return _userCapabilities;
            }
        }

        protected WebPartManager WebPartManager {
            get {
                return _owner;
            }
        }

        /// <devdoc>
        /// </devdoc>
        protected internal virtual void ApplyPersonalizationState() {
            if (IsEnabled) {
                EnsurePersonalizationState();
                _personalizationState.ApplyWebPartManagerPersonalization();
            }
        }

        /// <devdoc>
        /// </devdoc>
        protected internal virtual void ApplyPersonalizationState(WebPart webPart) {
            if (webPart == null) {
                throw new ArgumentNullException("webPart");
            }

            if (IsEnabled) {
                EnsurePersonalizationState();
                _personalizationState.ApplyWebPartPersonalization(webPart);
            }
        }

        // Helper method used by CopyPersonalizationState()
        private void ApplyPersonalizationState(Control control, PersonalizationInfo info) {
            ITrackingPersonalizable trackingPersonalizable = control as ITrackingPersonalizable;
            IPersonalizable customPersonalizable = control as IPersonalizable;

            if (trackingPersonalizable != null) {
                trackingPersonalizable.BeginLoad();
            }

            // If customPersonalizable is null, then info.CustomProperties should also be null
            Debug.Assert(!(customPersonalizable == null && info.CustomProperties != null));

            if (customPersonalizable != null && info.CustomProperties != null && info.CustomProperties.Count > 0) {
                customPersonalizable.Load(info.CustomProperties);
            }

            if (info.Properties != null && info.Properties.Count > 0) {
                BlobPersonalizationState.SetPersonalizedProperties(control, info.Properties);
            }

            if (trackingPersonalizable != null) {
                trackingPersonalizable.EndLoad();
            }
        }

        protected virtual void ChangeScope(PersonalizationScope scope) {
            PersonalizationProviderHelper.CheckPersonalizationScope(scope);

            if (scope == _currentScope) {
                return;
            }

            if ((scope == PersonalizationScope.Shared) && (!CanEnterSharedScope)) {
                throw new InvalidOperationException(SR.GetString(SR.WebPartPersonalization_CannotEnterSharedScope));
            }

            _currentScope = scope;
            _scopeToggled = true;
        }

        // Extracts the personalization state from webPartA, and applies it to webPartB.
        // Assumes that webPartA and webPartB are the same type.  If the WebParts are GenericWebParts,
        // then copies the personalization state from the ChildControl of webPartA to the
        // ChildControl of webPartB.
        protected internal virtual void CopyPersonalizationState(WebPart webPartA, WebPart webPartB) {
            if (webPartA == null) {
                throw new ArgumentNullException("webPartA");
            }
            if (webPartB == null) {
                throw new ArgumentNullException("webPartB");
            }
            if (webPartA.GetType() != webPartB.GetType()) {
                throw new ArgumentException(SR.GetString(SR.WebPartPersonalization_SameType, "webPartA", "webPartB"));
            }

            CopyPersonalizationState((Control)webPartA, (Control)webPartB);

            GenericWebPart genericWebPartA = webPartA as GenericWebPart;
            GenericWebPart genericWebPartB = webPartB as GenericWebPart;
            // Assert that the GenericWebParts are either both null or both non-null
            Debug.Assert((genericWebPartA == null) == (genericWebPartB == null));
            if (genericWebPartA != null && genericWebPartB != null) {
                Control childControlA = genericWebPartA.ChildControl;
                Control childControlB = genericWebPartB.ChildControl;

                if (childControlA == null) {
                    throw new ArgumentException(SR.GetString(SR.PropertyCannotBeNull, "ChildControl"), "webPartA");
                }
                if (childControlB == null) {
                    throw new ArgumentException(SR.GetString(SR.PropertyCannotBeNull, "ChildControl"), "webPartB");
                }
                if (childControlA.GetType() != childControlB.GetType()) {
                    throw new ArgumentException(SR.GetString(SR.WebPartPersonalization_SameType, "webPartA.ChildControl", "webPartB.ChildControl"));
                }

                CopyPersonalizationState(childControlA, childControlB);
            }

            // IPersonalizable.IsDirty should always be false on the new WebPart, since the only data
            // on the new WebPart was loaded via personalization, which should not cause the control
            // to be dirty.  However, we want to save the IPersonalizable data on this request, so
            // we call SetDirty() to force the IPersonalizable data to be saved.
            SetDirty(webPartB);
        }

        private void CopyPersonalizationState(Control controlA, Control controlB) {
            PersonalizationInfo info = ExtractPersonalizationState(controlA);
            ApplyPersonalizationState(controlB, info);
        }

        /// <devdoc>
        /// </devdoc>
        private void DeterminePersonalizationProvider() {
            string providerName = ProviderName;
            if (String.IsNullOrEmpty(providerName)) {
                // Use the default provider
                _provider = PersonalizationAdministration.Provider;
                // The default provider can never be null
                Debug.Assert(_provider != null);
            }
            else {
                // Look for a provider with the specified name
                PersonalizationProvider provider = PersonalizationAdministration.Providers[providerName];
                if (provider != null) {
                    _provider = provider;
                }
                else {
                    throw new ProviderException(
                        SR.GetString(SR.WebPartPersonalization_ProviderNotFound, providerName));
                }
            }
        }

        /// <devdoc>
        /// </devdoc>
        public void EnsureEnabled(bool ensureModifiable) {
            bool value = (ensureModifiable ? IsModifiable : IsEnabled);

            if (!value) {
                string message;
                if (ensureModifiable) {
                    message = SR.GetString(SR.WebPartPersonalization_PersonalizationNotModifiable);
                }
                else {
                    message = SR.GetString(SR.WebPartPersonalization_PersonalizationNotEnabled);
                }
                throw new InvalidOperationException(message);
            }
        }

        private void EnsurePersonalizationState() {
            if (_personalizationState == null) {
                throw new InvalidOperationException(SR.GetString(SR.WebPartPersonalization_PersonalizationStateNotLoaded));
            }
        }

        /// <devdoc>
        /// </devdoc>
        protected internal virtual void ExtractPersonalizationState() {
            // If we reset the personalization data on this request, we should not extract data since
            // it will not be saved.
            if (IsEnabled && !ShouldResetPersonalizationState) {
                EnsurePersonalizationState();
                _personalizationState.ExtractWebPartManagerPersonalization();
            }
        }

        /// <devdoc>
        /// </devdoc>
        protected internal virtual void ExtractPersonalizationState(WebPart webPart) {
            // If we reset the personalization data on this request, we should not extract data since
            // it will not be saved.
            if (IsEnabled && !ShouldResetPersonalizationState) {
                EnsurePersonalizationState();
                _personalizationState.ExtractWebPartPersonalization(webPart);
            }
        }

        // Helper method used by CopyPersonalizationState()
        private PersonalizationInfo ExtractPersonalizationState(Control control) {
            ITrackingPersonalizable trackingPersonalizable = control as ITrackingPersonalizable;
            IPersonalizable customPersonalizable = control as IPersonalizable;

            if (trackingPersonalizable != null) {
                trackingPersonalizable.BeginSave();
            }

            PersonalizationInfo info = new PersonalizationInfo();
            if (customPersonalizable != null) {
                info.CustomProperties = new PersonalizationDictionary();
                customPersonalizable.Save(info.CustomProperties);
            }
            info.Properties = BlobPersonalizationState.GetPersonalizedProperties(control, PersonalizationScope.Shared);

            if (trackingPersonalizable != null) {
                trackingPersonalizable.EndSave();
            }

            return info;
        }

        // Returns the AuthorizationFilter string for a WebPart before it is instantiated
        // Returns null if there is no personalized value for AuthorizationFilter
        protected internal virtual string GetAuthorizationFilter(string webPartID) {
            if (String.IsNullOrEmpty(webPartID)) {
                throw ExceptionUtil.ParameterNullOrEmpty("webPartID");
            }

            EnsureEnabled(false);
            EnsurePersonalizationState();
            return _personalizationState.GetAuthorizationFilter(webPartID);
        }

        /// <devdoc>
        /// </devdoc>
        internal void LoadInternal() {
            if (Enabled) {
                _currentScope = Load();
                _initialized = true;
            }
            _initializedSet = true;
        }

        /// <devdoc>
        /// Loads personalization information from its storage, and computes the initial personalization
        /// scope. This method determines the provider type to be used, and the current user's capabilities.
        /// </devdoc>
        protected virtual PersonalizationScope Load() {
            if (!Enabled) {
                throw new InvalidOperationException(SR.GetString(SR.WebPartPersonalization_PersonalizationNotEnabled));
            }

            // Determine the provider early, as it is needed to continue execution.
            // Provider is used to detect user's capabilities, load personalization state
            // and determine initial scope.
            DeterminePersonalizationProvider();
            Debug.Assert(_provider != null);

            Page page = WebPartManager.Page;
            if (page == null) {
                throw new InvalidOperationException(SR.GetString(SR.PropertyCannotBeNull, "WebPartManager.Page"));
            }

            HttpRequest request = page.RequestInternal;
            if (request == null) {
                throw new InvalidOperationException(SR.GetString(SR.PropertyCannotBeNull, "WebPartManager.Page.Request"));
            }

            // Ask the provider to load information about what are the capabilities of
            // the current user
            if (request.IsAuthenticated) {
                _userCapabilities = _provider.DetermineUserCapabilities(WebPartManager);
            }

            // A derived WebPartPersonalization can have ignoreCurrentUser to be true.
            _personalizationState = _provider.LoadPersonalizationState(WebPartManager, /* ignoreCurrentUser */ false);
            if (_personalizationState == null) {
                // We can't assume that _personalizationState will be non-null, because
                // it depends on the provider implementation.
                throw new ProviderException(SR.GetString(SR.WebPartPersonalization_CannotLoadPersonalization));
            }

            return _provider.DetermineInitialScope(WebPartManager, _personalizationState);
        }

        // Resets the personalization data for the current user in the current scope on the current page
        public virtual void ResetPersonalizationState() {
            EnsureEnabled(/* ensureModifiable */ true);

            if (_provider == null) {
                throw new InvalidOperationException(SR.GetString(SR.WebPartPersonalization_CantCallMethodBeforeInit,
                                                    "ResetPersonalizationState", "WebPartPersonalization"));
            }

            _provider.ResetPersonalizationState(WebPartManager);
            ShouldResetPersonalizationState = true;

            Page page = WebPartManager.Page;
            if (page == null) {
                throw new InvalidOperationException(SR.GetString(SR.PropertyCannotBeNull, "WebPartManager.Page"));
            }

            // Transfer execution to a new instance of the same page.  The new page will execute
            // after personalization data has been reset.
            TransferToCurrentPage(page);
        }

        /// <devdoc>
        /// </devdoc>
        internal void SaveInternal() {
            if (IsModifiable) {
                Save();
            }
        }

        /// <devdoc>
        /// Saves personalization information back to its storage if necessary.
        /// </devdoc>
        protected virtual void Save() {
            EnsureEnabled(/* ensureModifiable */ true);

            EnsurePersonalizationState();

            if (_provider == null) {
                throw new InvalidOperationException(SR.GetString(SR.WebPartPersonalization_CantCallMethodBeforeInit,
                                                    "Save", "WebPartPersonalization"));
            }

            // If we reset the personalization data on this request, we should not save data to the
            // DB on this request.  It is likely we would not save data anyway, since the data probably
            // did not change since the last request, but there are some scenarios where the data could
            // have changed (like a WebPart using personalization as a cache).
            if (_personalizationState.IsDirty && !ShouldResetPersonalizationState) {
                _provider.SavePersonalizationState(_personalizationState);
            }
        }

        /// <devdoc>
        /// </devdoc>
        protected internal virtual void SetDirty() {
            if (IsEnabled) {
                EnsurePersonalizationState();
                _personalizationState.SetWebPartManagerDirty();
            }
        }

        /// <devdoc>
        /// </devdoc>
        protected internal virtual void SetDirty(WebPart webPart) {
            if (IsEnabled) {
                EnsurePersonalizationState();
                _personalizationState.SetWebPartDirty(webPart);
            }
        }

        /// <devdoc>
        /// </devdoc>
        public virtual void ToggleScope() {
            EnsureEnabled(/* ensureModifiable */ false);

            Page page = WebPartManager.Page;
            if (page == null) {
                throw new InvalidOperationException(SR.GetString(SR.PropertyCannotBeNull, "WebPartManager.Page"));
            }
            if (page.IsExportingWebPart) {
                // If the page is exporting, the page determines the desired scope, and it is not meaningful
                // to toggle the scope. Note that we no-op the call, rather than throwing because
                // that would require exposing a CanToggleScope property, require page developers
                // to check for it. Furthermore, we don't guarantee ToggleScope does toggle
                // (eg. in the case of absense of user capability to enter shared scope), so
                // simply no-op'ing isn't terribly bad...
                return;
            }

            Page previousPage = page.PreviousPage;
            if ((previousPage != null) &&
                (previousPage.IsCrossPagePostBack == false)) {
                WebPartManager previousWebPartManager = WebPartManager.GetCurrentWebPartManager(previousPage);

                if ((previousWebPartManager != null) && (previousWebPartManager.Personalization.ScopeToggled)) {
                    // Looks like some sort of infinite recursion is going on
                    // and we're toggling again. Like the previous case, we're
                    // going to no-op and protect ourselves from "----" code...
                    return;
                }
            }

            if (_currentScope == PersonalizationScope.Shared) {
                ChangeScope(PersonalizationScope.User);
            }
            else {
                ChangeScope(PersonalizationScope.Shared);
            }

            // Transfer execution to a new instance of the same page.  The new page will detect the scope
            // it should run in, when it begins to load personalization data.
            TransferToCurrentPage(page);
        }

        // Transfers execution to a new instance of the same page. We need to clear the form collection,
        // since it should not carry over to the page in the new scope (i.e. ViewState). If the form
        // method is GET, then we must not include the query string, since the entire form collection
        // is in the query string.  If the form method is POST (or there is no form), then we must
        // include the query string, since the developer could be using the query string to drive the
        // logic of their page (VSWhidbey 444385 and 527117).
        private void TransferToCurrentPage(Page page) {
            HttpRequest request = page.RequestInternal;
            if (request == null) {
                throw new InvalidOperationException(SR.GetString(SR.PropertyCannotBeNull, "WebPartManager.Page.Request"));
            }

            string path = request.CurrentExecutionFilePath;
            if (page.Form == null || String.Equals(page.Form.Method, "post", StringComparison.OrdinalIgnoreCase)) {
                string queryString = page.ClientQueryString;
                if (!String.IsNullOrEmpty(queryString)) {
                    path += "?" + queryString;
                }
            }

            IScriptManager scriptManager = page.ScriptManager;
            if ((scriptManager != null) && scriptManager.IsInAsyncPostBack) {
                request.Response.Redirect(path);
            }
            else {
                page.Server.Transfer(path, /* preserveForm */ false);
            }
        }

        private sealed class PersonalizationInfo {
            public IDictionary Properties;
            public PersonalizationDictionary CustomProperties;
        }
    }
}
