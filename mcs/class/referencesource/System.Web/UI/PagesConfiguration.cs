//------------------------------------------------------------------------------
// <copyright file="PagesConfiguration.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * Code related to the <assemblies> config section
 *
 * Copyright (c) 1999 Microsoft Corporation
 */

namespace System.Web.UI {
    public enum CompilationMode {
        Auto,        // Use no-compile mode when possible
        Never,          // Never compile pages, and fail if no-compile is not possible
        Always          // Always compile pages
    }
}
