//------------------------------------------------------------------------------
// <copyright file="IScriptResourceHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.Handlers {
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Text;
    using System.Web.Util;

    internal interface IScriptResourceHandler {
        string GetScriptResourceUrl(
            Assembly assembly, string resourceName, CultureInfo culture, bool zip);

        string GetScriptResourceUrl(
            List<Tuple<Assembly, List<Tuple<string, CultureInfo>>>> assemblyResourceLists, bool zip);

        string GetEmptyPageUrl(string title);
    }
}
