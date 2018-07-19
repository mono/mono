//------------------------------------------------------------------------------
// <copyright file="ICustomLoaderHelperFunctions.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Hosting {
    using System;

    internal interface ICustomLoaderHelperFunctions {
        string AppPhysicalPath { get; }
        bool? CustomLoaderIsEnabled { get; } // true = always enabled, false = always disabled, null = check trust level

        string GetTrustLevel(string appConfigMetabasePath);
        string MapPath(string relativePath);
    }
}
