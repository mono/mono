//------------------------------------------------------------------------------
// <copyright file="AuthenticationConfig.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * AuthenticationConfigHandler class
 *
 * Copyright (c) 1999 Microsoft Corporation
 */

namespace System.Web.Configuration {
    using System.Runtime.Serialization;
    using System.Web.Util;
    using System.Collections;
    using System.IO;
    using System.Security.Principal;
    using System.Xml;
    using System.Security.Cryptography;
    using System.Configuration;
    using System.Globalization;
    using System.Web.Hosting;
    using System.Web.Compilation;

    static internal class AuthenticationConfig {
        private static AuthenticationMode? s_explicitMode;

        internal static AuthenticationMode Mode {
            get {
                if (s_explicitMode.HasValue) {
                    return s_explicitMode.Value;
                }
                else {
                    AuthenticationSection settings = RuntimeConfig.GetAppConfig().Authentication;
                    settings.ValidateAuthenticationMode();
                    return settings.Mode;
                }
            }
            set {
                Debug.Assert(BuildManager.PreStartInitStage == PreStartInitStage.DuringPreStartInit);
                Debug.Assert(value == AuthenticationMode.Forms, "Only Forms mode can be set to override config");
                s_explicitMode = value;
            }
        }

        internal static String GetCompleteLoginUrl(HttpContext context, String loginUrl) {
            if (String.IsNullOrEmpty(loginUrl)) {
                return String.Empty;
            }

            if (UrlPath.IsRelativeUrl(loginUrl)) {
                loginUrl = UrlPath.Combine(HttpRuntime.AppDomainAppVirtualPathString, loginUrl);
            }

            return loginUrl;
        }

        internal static bool AccessingLoginPage(HttpContext context, String loginUrl) {
            if (String.IsNullOrEmpty(loginUrl)) {
                return false;
            }

            loginUrl = GetCompleteLoginUrl(context, loginUrl);
            if (String.IsNullOrEmpty(loginUrl)) {
                return false;
            }

            // Ignore query string
            int iqs = loginUrl.IndexOf('?');
            if (iqs >= 0) {
                loginUrl = loginUrl.Substring(0, iqs);
            }

            String requestPath = context.Request.Path;

            if (StringUtil.EqualsIgnoreCase(requestPath, loginUrl)) {
                return true;
            }

            // It could be that loginUrl in config was UrlEncoded (ASURT 98932)
            if (loginUrl.IndexOf('%') >= 0) {
                String decodedLoginUrl;
                // encoding is unknown try UTF-8 first, then request encoding

                decodedLoginUrl = HttpUtility.UrlDecode(loginUrl);
                if (StringUtil.EqualsIgnoreCase(requestPath, decodedLoginUrl)) {
                    return true;
                }

                decodedLoginUrl = HttpUtility.UrlDecode(loginUrl, context.Request.ContentEncoding);
                if (StringUtil.EqualsIgnoreCase(requestPath, decodedLoginUrl)) {
                    return true;
                }
            }

            return false;
        }
    }
}
