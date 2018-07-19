//------------------------------------------------------------------------------
// <copyright file="ForceCopyBuildProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------



namespace System.Web.Compilation {

using System;
using System.Web.Util;

// This is a marker class that causes files to be copied as static files during
// precompilation, even though their extension may be associated with a source file
// type.  Concretely, this is used to make sure .js files get copied, as they could be 
// client side (VSWhidbey 337513)
internal class ForceCopyBuildProvider: BuildProvider {

    internal ForceCopyBuildProvider() {
        // Since it's just a marker, it should never be instantiated
        Debug.Assert(false);
    }

}

}
