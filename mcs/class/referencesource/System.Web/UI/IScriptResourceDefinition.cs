//------------------------------------------------------------------------------
// <copyright file="IScriptManager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {
    using System;
    using System.Reflection;

    internal interface IScriptResourceDefinition {
        string Path { get; }
        string DebugPath { get; }
        string CdnPath { get; }
        string CdnDebugPath { get; }
        string CdnPathSecureConnection { get; }
        string CdnDebugPathSecureConnection { get; }
        string ResourceName { get; }
        Assembly ResourceAssembly { get; }
    }
}
