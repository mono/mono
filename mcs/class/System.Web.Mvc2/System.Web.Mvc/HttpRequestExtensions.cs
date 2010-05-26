/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This software is subject to the Microsoft Public License (Ms-PL). 
 * A copy of the license can be found in the license.htm file included 
 * in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

namespace System.Web.Mvc {
    using System;

    public static class HttpRequestExtensions {
        internal const string XHttpMethodOverrideKey = "X-HTTP-Method-Override";

        public static string GetHttpMethodOverride(this HttpRequestBase request) {
            if (request == null) {
                throw new ArgumentNullException("request");
            }

            string incomingVerb = request.HttpMethod;

            if (!String.Equals(incomingVerb, "POST", StringComparison.OrdinalIgnoreCase)) {
                return incomingVerb;
            }

            string verbOverride = null;
            string headerOverrideValue = request.Headers[XHttpMethodOverrideKey];
            if (!String.IsNullOrEmpty(headerOverrideValue)) {
                verbOverride = headerOverrideValue;
            }
            else {
                string formOverrideValue = request.Form[XHttpMethodOverrideKey];
                if (!String.IsNullOrEmpty(formOverrideValue)) {
                    verbOverride = formOverrideValue;
                }
                else {
                    string queryStringOverrideValue = request.QueryString[XHttpMethodOverrideKey];
                    if (!String.IsNullOrEmpty(queryStringOverrideValue)) {
                        verbOverride = queryStringOverrideValue;
                    }
                }
            }
            if (verbOverride != null) {
                if (!String.Equals(verbOverride, "GET", StringComparison.OrdinalIgnoreCase) &&
                    !String.Equals(verbOverride, "POST", StringComparison.OrdinalIgnoreCase)) {
                    incomingVerb = verbOverride;
                }
            }
            return incomingVerb;
        }
    }
}
