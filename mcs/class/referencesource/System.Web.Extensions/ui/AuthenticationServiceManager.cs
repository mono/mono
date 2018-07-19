//------------------------------------------------------------------------------
// <copyright file="AuthenticationServiceManager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.UI {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Text;
    using System.Web;
    using System.Web.ApplicationServices;
    using System.Web.Resources;
    using System.Web.Script.Serialization;
    using System.Web.Security;

    [
    DefaultProperty("Path"),
    TypeConverter(typeof(EmptyStringExpandableObjectConverter))
    ]
    public class AuthenticationServiceManager {

        private string _path;

        internal static void ConfigureAuthenticationService(ref StringBuilder sb, HttpContext context, ScriptManager scriptManager, List<ScriptManagerProxy> proxies) {
            string authServiceUrl = null;
            AuthenticationServiceManager authManager;

            if(scriptManager.HasAuthenticationServiceManager) {
                authManager = scriptManager.AuthenticationService;

                // get ScriptManager.ServiceUrl
                authServiceUrl = authManager.Path.Trim();
                if(authServiceUrl.Length > 0) {
                    authServiceUrl = scriptManager.ResolveUrl(authServiceUrl);
                }
            }

            // combine proxy ServiceUrls (find the first one that has specified one)
            if(proxies != null) {
                foreach(ScriptManagerProxy proxy in proxies) {
                    if(proxy.HasAuthenticationServiceManager) {
                        authManager = proxy.AuthenticationService;

                        // combine urls
                        authServiceUrl = ApplicationServiceManager.MergeServiceUrls(authManager.Path, authServiceUrl, proxy);
                    }
                }
            }
            AuthenticationServiceManager.GenerateInitializationScript(ref sb, context, scriptManager, authServiceUrl);
        }

        private static void GenerateInitializationScript(ref StringBuilder sb, HttpContext context, ScriptManager scriptManager, string serviceUrl) {
            bool authEnabled = ApplicationServiceHelper.AuthenticationServiceEnabled;

            if (authEnabled) {
                if (sb == null) {
                    sb = new StringBuilder(ApplicationServiceManager.StringBuilderCapacity);
                }

                // The default path points to the built-in service (if it is enabled)
                // Note that the client can't default to this path because it doesn't know what the app root is, we must tell it.
                // We must specify the default path to the proxy even if a custom path is provided, because on the client they could
                // reset the path back to the default if they want.
                string defaultServicePath = scriptManager.ResolveClientUrl("~/" + System.Web.Script.Services.WebServiceData._authenticationServiceFileName);
                sb.Append("Sys.Services._AuthenticationService.DefaultWebServicePath = '");
                sb.Append(HttpUtility.JavaScriptStringEncode(defaultServicePath));
                sb.Append("';\n");
            }

            bool pathSpecified = !String.IsNullOrEmpty(serviceUrl);
            if(pathSpecified) {
                if (sb == null) {
                    sb = new StringBuilder(ApplicationServiceManager.StringBuilderCapacity);
                }
                sb.Append("Sys.Services.AuthenticationService.set_path('");
                sb.Append(HttpUtility.JavaScriptStringEncode(serviceUrl));
                sb.Append("');\n");
            }

            // only emit this script if (1) the auth webservice is enabled or (2) a custom webservice url is specified
            if ((authEnabled || pathSpecified) &&
                (context != null && context.Request.IsAuthenticated)) {
                Debug.Assert(sb != null);
                sb.Append("Sys.Services.AuthenticationService._setAuthenticated(true);\n");
            }
       } 

        [
        DefaultValue(""),
        Category("Behavior"),
        NotifyParentProperty(true),
        ResourceDescription("ApplicationServiceManager_Path"),
        UrlProperty()
        ]
        public string Path {
            get {
                return _path ?? String.Empty;
            }
            set {
                _path = value;
            }
        }
    }
}
