//------------------------------------------------------------------------------
// <copyright file="UrlAuthFailedErrorFormatter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * AuthorizationConfigHandler class
 *
 * Copyright (c) 1999 Microsoft Corporation
 */


namespace System.Web.Configuration {
    using System.Runtime.Serialization;
    using System.Web.Util;
    using System.Collections;
    using System.Collections.Specialized;
    using System.IO;
    using System.Security.Principal;
    using System.Xml;
    using System.Security.Cryptography;
    using System.Configuration;
    using System.Globalization;
    using System.Web.Management;

    /////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////
    internal class UrlAuthFailedErrorFormatter : ErrorFormatter {
        private StringCollection _adaptiveMiscContent = new StringCollection();

        internal UrlAuthFailedErrorFormatter() {
        }

        internal /*public*/ static string GetErrorText() {
            return GetErrorText(HttpContext.Current);
        }

        internal static string GetErrorText(HttpContext context) {
            bool dontShowSensitiveInfo = context.IsCustomErrorEnabled;
            return (new UrlAuthFailedErrorFormatter()).GetErrorMessage(context, dontShowSensitiveInfo);
        }

        protected override string ErrorTitle {
            get { return SR.GetString(SR.Assess_Denied_Title);}
            // "Access Denied
        }

        protected override string Description {
            get {
                return SR.GetString(SR.Assess_Denied_Description2);
                //"An error occurred while accessing the resources required to serve this request. &nbsp; This typically happens when the web server is not configured to give you access to the requested URL.";
            }
        }

        protected override string MiscSectionTitle {
            get {
                return SR.GetString(SR.Assess_Denied_Section_Title2);
                //return "Error message 401.2";
            }
        }

        protected override string MiscSectionContent {
            get {
                // VSWhidbey 493720: Do Html encode to preserve space characters
                string miscContent = HttpUtility.FormatPlainTextAsHtml(SR.GetString(SR.Assess_Denied_Misc_Content2));
                AdaptiveMiscContent.Add(miscContent);
                return miscContent;
                //return "Access denied due to the web server's configuration. Ask the web server's administrator for help.";
            }
        }

        protected override string ColoredSquareTitle {
            get { return null;}
        }

        protected override string ColoredSquareContent {
            get { return null;}
        }

        protected override StringCollection AdaptiveMiscContent {
            get { return _adaptiveMiscContent;}
        }

        protected override bool ShowSourceFileInfo {
            get { return false;}
        }
    }
}

