//------------------------------------------------------------------------------
// <copyright file="ResXBuildProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Compilation {

using System;
using System.Resources;
using System.IO;
using System.Web.Hosting;

/// BuildProvider for .resx files
internal sealed class ResXBuildProvider : BaseResourcesBuildProvider {

    protected override IResourceReader GetResourceReader(Stream inputStream) {
#if !FEATURE_PAL // FEATURE_PAL 
        ResXResourceReader reader = new ResXResourceReader(inputStream);

        // Give the BasePath to the reader so it can resolve relative references (VSWhidbey 208154)
        // NOTE: this will not work with a non-file based VirtualPathProvider
        string physicalPath = HostingEnvironment.MapPath(VirtualPath);
        reader.BasePath = Path.GetDirectoryName(physicalPath);

        return reader;
#else // !FEATURE_PAL 
        throw new NotImplementedException("ROTORTODO - ResXResourceReader");
#endif // !FEATURE_PAL 

    }
}

}
