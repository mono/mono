//------------------------------------------------------------------------------
// <copyright file="Util.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * Implements various utility functions used by the template code
 *
 * Copyright (c) 1998 Microsoft Corporation
 */

namespace System.Web.UI {
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Serialization.Formatters;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web.Compilation;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using System.Web.Management;
    using System.Web.Security;
    //using System.Web.Security.Cryptography;
    using System.Web.UI.WebControls;
    using System.Web.Util;
    using Microsoft.Win32;
    //using Debug = System.Web.Util.Debug;

internal static class Util {

    internal static string GetUrlWithApplicationPath(HttpContextBase context, string url) {
        string appPath = context.Request.ApplicationPath ?? String.Empty;
        if (!appPath.EndsWith("/", StringComparison.OrdinalIgnoreCase)) {
            appPath += "/";
        }

        return context.Response.ApplyAppPathModifier(appPath + url);
    }
}

}
