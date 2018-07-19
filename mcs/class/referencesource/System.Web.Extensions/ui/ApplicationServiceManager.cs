//------------------------------------------------------------------------------
// <copyright file="ApplicationServiceManager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.UI {
    using System;
    using System.Web.UI;
    using System.Web.Resources;
    using System.Globalization;

    internal static class ApplicationServiceManager {
        public const int StringBuilderCapacity = 128;

        public static string MergeServiceUrls(string serviceUrl, string existingUrl, Control urlBase) {
            serviceUrl = serviceUrl.Trim();

            if(serviceUrl.Length > 0) {
                serviceUrl = urlBase.ResolveClientUrl(serviceUrl);

                if(String.IsNullOrEmpty(existingUrl)) {
                    // proxy has specified a url and we don't have one yet, so use it
                    existingUrl = serviceUrl;
                }
                else {
                    // proxy has specified a url but we arleady have a url either from ScriptManager itself or a previous proxy.
                    // The urls must agree or an exception is thrown.
                    if(!string.Equals(serviceUrl, existingUrl, StringComparison.OrdinalIgnoreCase)) {
                        throw new ArgumentException(AtlasWeb.AppService_MultiplePaths);
                    }
                }
            }
            return existingUrl;
        }
    }
}
