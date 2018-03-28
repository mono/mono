//------------------------------------------------------------------------------
// <copyright file="IgnoreFileBuildProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------



namespace System.Web.Compilation {

using System;
using System.Web.Util;

// This is a marker class that causes files to be ignored by the compilation system.
// Otherwise, if no build provider is registered, unknown extensions get treated as
// static files and copied during deployment precompilation.  This provider prevents
// files from being copied. (DevDiv 35450)
internal class IgnoreFileBuildProvider: BuildProvider {

    internal IgnoreFileBuildProvider() {
        // Since it's just a marker, it should never be instantiated
        Debug.Assert(false);
    }

}
}
