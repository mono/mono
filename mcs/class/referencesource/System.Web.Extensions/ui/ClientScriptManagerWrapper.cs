//------------------------------------------------------------------------------
// <copyright file="ClientScriptManagerWrapper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Reflection;
    using System.Web.UI;

    internal sealed class ClientScriptManagerWrapper : IClientScriptManager {
        private readonly ClientScriptManager _clientScriptManager;

        internal ClientScriptManagerWrapper(ClientScriptManager clientScriptManager) {
            Debug.Assert(clientScriptManager != null);
            _clientScriptManager = clientScriptManager;
        }

        #region IClientScriptManager Members
        Dictionary<Assembly, Dictionary<String, Object>> IClientScriptManager.RegisteredResourcesToSuppress {
            get {
                return _clientScriptManager.RegisteredResourcesToSuppress;
            }
        }

        string IClientScriptManager.GetPostBackEventReference(PostBackOptions options) {
            return _clientScriptManager.GetPostBackEventReference(options);
        }

        string IClientScriptManager.GetWebResourceUrl(Type type, string resourceName) {
            return _clientScriptManager.GetWebResourceUrl(type, resourceName);
        }

        void IClientScriptManager.RegisterClientScriptBlock(Type type, string key, string script) {
            _clientScriptManager.RegisterClientScriptBlock(type, key, script);
        }

        void IClientScriptManager.RegisterClientScriptInclude(Type type, string key, string url) {
            _clientScriptManager.RegisterClientScriptInclude(type, key, url);
        }

        void IClientScriptManager.RegisterClientScriptBlock(Type type, string key, string script, bool addScriptTags) {
            _clientScriptManager.RegisterClientScriptBlock(type, key, script, addScriptTags);
        }

        void IClientScriptManager.RegisterStartupScript(Type type, string key, string script, bool addScriptTags) {
            _clientScriptManager.RegisterStartupScript(type, key, script, addScriptTags);
        }
        #endregion
    }
}
