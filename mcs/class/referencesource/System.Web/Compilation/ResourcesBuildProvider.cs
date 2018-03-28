//------------------------------------------------------------------------------
// <copyright file="ResourcesBuildProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Compilation {

using System;
using System.Resources;
using System.IO;

/// BuildProvider for .resources files
internal class ResourcesBuildProvider : BaseResourcesBuildProvider {

    protected override IResourceReader GetResourceReader(Stream inputStream) {
        return new ResourceReader(inputStream);
    }
}

}
