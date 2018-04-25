//------------------------------------------------------------------------------
// <copyright file="PersonalizationProvider.cs" company="Microsoft">
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
    using System.Web.Hosting;
    using System.Web.Util;

    /// <devdoc>
    /// The provider used to access the personalization store for WebPart pages.
    /// </devdoc>
    public abstract class PersonalizationProvider : ProviderBase {

        private const string scopeFieldName = "__WPPS";
        private const string sharedScopeFieldValue = "s";
        private const string userScopeFieldValue = "u";

        private ICollection _supportedUserCapabilities;

        /// <devdoc>
        /// Initializes an instance of PersonalizationProvider.
        /// </devdoc>
        protected PersonalizationProvider() {
        }

        /// <devdoc>
        /// The name of the application that this provider should use to store
        /// and retrieve personalization data from.
        /// </devdoc>
        public abstract string ApplicationName { get; set; }

        /// <devdoc>
        /// </devdoc>
        protected virtual IList CreateSupportedUserCapabilities() {
            ArrayList list = new ArrayList();

            list.Add(WebPartPersonalization.EnterSharedScopeUserCapability);
            list.Add(WebPartPersonalization.ModifyStateUserCapability);

            return list;
        }

        /// <devdoc>
        /// </devdoc>
        public virtual PersonalizationScope DetermineInitialScope(WebPartManager webPartManager, PersonalizationState loadedState) {
            if (webPartManager == null) {
                throw new ArgumentNullException("webPartManager");
            }

            Page page = webPartManager.Page;
            if (page == null) {
                throw new ArgumentException(SR.GetString(SR.PropertyCannotBeNull, "Page"),
                                            "webPartManager");
            }

            HttpRequest request = page.RequestInternal;
            if (request == null) {
                throw new ArgumentException(SR.GetString(SR.PropertyCannotBeNull, "Page.Request"),
                                            "webPartManager");
            }

            PersonalizationScope scope = webPartManager.Personalization.InitialScope;

            IPrincipal user = null;
            if (request.IsAuthenticated) {
                user = page.User;
            }

            if (user == null) {
                // if no user has been authenticated, then just load all user data
                scope = PersonalizationScope.Shared;
            }
            else {
                if (page.IsPostBack) {
                    string postedMode = page.Request[scopeFieldName];
                    if (postedMode == sharedScopeFieldValue) {
                        scope = PersonalizationScope.Shared;
                    }
                    else if (postedMode == userScopeFieldValue) {
                        scope = PersonalizationScope.User;
                    }
                }
                else if ((page.PreviousPage != null) &&
                         (page.PreviousPage.IsCrossPagePostBack == false)) {
                    WebPartManager previousWebPartManager = WebPartManager.GetCurrentWebPartManager(page.PreviousPage);

                    if (previousWebPartManager != null) {
                        // Note that we check the types of the page, so we don't
                        // look the at the PreviousPage in a cross-page posting scenario
                        scope = previousWebPartManager.Personalization.Scope;
                    }
                }
                // Special-case Web Part Export so it executes in the same security context as the page itself (VSWhidbey 426574)
                // Setting the initial scope from what's been asked for in the export parameters
                else if (page.IsExportingWebPart) {
                    scope = (page.IsExportingWebPartShared ? PersonalizationScope.Shared : PersonalizationScope.User);
                }

                if ((scope == PersonalizationScope.Shared) &&
                    (webPartManager.Personalization.CanEnterSharedScope == false)) {
                    scope = PersonalizationScope.User;
                }
            }

            string fieldValue = (scope == PersonalizationScope.Shared) ? sharedScopeFieldValue : userScopeFieldValue;
            page.ClientScript.RegisterHiddenField(scopeFieldName, fieldValue);

            return scope;
        }

        /// <devdoc>
        /// </devdoc>
        public virtual IDictionary DetermineUserCapabilities(WebPartManager webPartManager) {
            if (webPartManager == null) {
                throw new ArgumentNullException("webPartManager");
            }

            Page page = webPartManager.Page;
            if (page == null) {
                throw new ArgumentException(SR.GetString(SR.PropertyCannotBeNull, "Page"),
                                            "webPartManager");
            }

            HttpRequest request = page.RequestInternal;
            if (request == null) {
                throw new ArgumentException(SR.GetString(SR.PropertyCannotBeNull, "Page.Request"),
                                            "webPartManager");
            }

            IPrincipal user = null;
            if (request.IsAuthenticated) {
                user = page.User;
            }

            if (user != null) {
                if (_supportedUserCapabilities == null) {
                    _supportedUserCapabilities = CreateSupportedUserCapabilities();
                }

                if ((_supportedUserCapabilities != null) && (_supportedUserCapabilities.Count != 0)) {
                    WebPartsSection configSection = RuntimeConfig.GetConfig().WebParts;
                    if (configSection != null) {
                        WebPartsPersonalizationAuthorization authConfig = configSection.Personalization.Authorization;
                        if (authConfig != null) {
                            IDictionary capabilities = new HybridDictionary();

                            foreach (WebPartUserCapability capability in _supportedUserCapabilities) {
                                if (authConfig.IsUserAllowed(user, capability.Name)) {
                                    capabilities[capability] = capability;
                                }
                            }
                            return capabilities;
                        }
                    }
                }
            }

            return new HybridDictionary();
        }

        public abstract PersonalizationStateInfoCollection FindState(PersonalizationScope scope,
                                                                     PersonalizationStateQuery query,
                                                                     int pageIndex, int pageSize,
                                                                     out int totalRecords);

        public abstract int GetCountOfState(PersonalizationScope scope, PersonalizationStateQuery query);

        /// <devdoc>
        /// </devdoc>
        private void GetParameters(WebPartManager webPartManager, out string path, out string userName) {
            if (webPartManager == null) {
                throw new ArgumentNullException("webPartManager");
            }

            Page page = webPartManager.Page;
            if (page == null) {
                throw new ArgumentException(SR.GetString(SR.PropertyCannotBeNull, "Page"),
                                            "webPartManager");
            }

            HttpRequest request = page.RequestInternal;
            if (request == null) {
                throw new ArgumentException(SR.GetString(SR.PropertyCannotBeNull, "Page.Request"),
                                            "webPartManager");
            }

            path = request.AppRelativeCurrentExecutionFilePath;
            userName = null;

            if ((webPartManager.Personalization.Scope == PersonalizationScope.User) && page.Request.IsAuthenticated) {
                userName = page.User.Identity.Name;
            }
        }

        /// <devdoc>
        /// Loads the data from the data store for the specified path and user.
        /// If only shared data is to be loaded, userName is null or empty.
        /// </devdoc>
        protected abstract void LoadPersonalizationBlobs(WebPartManager webPartManager, string path, string userName, ref byte[] sharedDataBlob, ref byte[] userDataBlob);

        /// <devdoc>
        /// Allows the provider to load personalization data. The specified
        /// WebPartManager is used to access the current page, which can be used
        /// to retrieve the path and user information.
        /// </devdoc>
        public virtual PersonalizationState LoadPersonalizationState(WebPartManager webPartManager, bool ignoreCurrentUser) {
            if (webPartManager == null) {
                throw new ArgumentNullException("webPartManager");
            }

            string path;
            string userName;
            GetParameters(webPartManager, out path, out userName);

            if (ignoreCurrentUser) {
                userName = null;
            }

            byte[] sharedDataBlob = null;
            byte[] userDataBlob = null;
            LoadPersonalizationBlobs(webPartManager, path, userName, ref sharedDataBlob, ref userDataBlob);

            BlobPersonalizationState blobState = new BlobPersonalizationState(webPartManager);
            blobState.LoadDataBlobs(sharedDataBlob, userDataBlob);

            return blobState;
        }

        /// <devdoc>
        /// Removes the data from the data store for the specified path and user.
        /// If userName is null or empty, the shared data is to be reset.
        /// </devdoc>
        protected abstract void ResetPersonalizationBlob(WebPartManager webPartManager, string path, string userName);

        /// <devdoc>
        /// Allows the provider to reset personalization data. The specified
        /// WebPartManager is used to access the current page, which can be used
        /// to retrieve the path and user information.
        /// </devdoc>
        public virtual void ResetPersonalizationState(WebPartManager webPartManager) {
            if (webPartManager == null) {
                throw new ArgumentNullException("webPartManager");
            }

            string path;
            string userName;
            GetParameters(webPartManager, out path, out userName);

            ResetPersonalizationBlob(webPartManager, path, userName);
        }

        public abstract int ResetState(PersonalizationScope scope, string[] paths, string[] usernames);

        public abstract int ResetUserState(string path, DateTime userInactiveSinceDate);

        /// <devdoc>
        /// Saves the data into the data store for the specified path and user.
        /// If only shared data is to be saved, userName is null or empty.
        /// </devdoc>
        protected abstract void SavePersonalizationBlob(WebPartManager webPartManager, string path, string userName, byte[] dataBlob);

        /// <devdoc>
        /// Allows the provider to save personalization data. The specified information
        /// contains a reference to the WebPartManager, which is used to access the
        /// current Page, and its path and user information.
        /// </devdoc>
        public virtual void SavePersonalizationState(PersonalizationState state) {
            if (state == null) {
                throw new ArgumentNullException("state");
            }

            BlobPersonalizationState blobState = state as BlobPersonalizationState;
            if (blobState == null) {
                throw new ArgumentException(SR.GetString(SR.PersonalizationProvider_WrongType), "state");
            }

            WebPartManager webPartManager = blobState.WebPartManager;

            string path;
            string userName;
            GetParameters(webPartManager, out path, out userName);

            byte[] dataBlob = null;
            bool reset = blobState.IsEmpty;

            if (reset == false) {
                dataBlob = blobState.SaveDataBlob();
                reset = (dataBlob == null) || (dataBlob.Length == 0);
            }

            if (reset) {
                ResetPersonalizationBlob(webPartManager, path, userName);
            }
            else {
                SavePersonalizationBlob(webPartManager, path, userName, dataBlob);
            }
        }
    }
}
