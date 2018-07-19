//------------------------------------------------------------------------------
// <copyright file="IScriptManager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {
    using System;
    using System.Reflection;

    internal interface IScriptResourceMapping {
        IScriptResourceDefinition GetDefinition(string resourceName);
        IScriptResourceDefinition GetDefinition(string resourceName, Assembly resourceAssembly);
    }
}
