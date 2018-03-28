//------------------------------------------------------------------------------
// <copyright file="SimpleHandlerFactory.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * Handler Factory implementation for ASP.NET files
 *
 * Copyright (c) 2000 Microsoft Corporation
 */

namespace System.Web.UI {

using System;
using System.Web.Compilation;
using Debug = System.Web.Util.Debug;

/*
 * Handler Factory implementation for ASP.NET files
 */
internal class SimpleHandlerFactory : IHttpHandlerFactory2 {
    internal SimpleHandlerFactory() {
    }

    public virtual IHttpHandler GetHandler(HttpContext context, string requestType,
        string virtualPath, string path) {

        // This should never get called
        //Debug.Assert(false);

        return ((IHttpHandlerFactory2)this).GetHandler(context, requestType,
            VirtualPath.CreateNonRelative(virtualPath), path);
    }

    IHttpHandler IHttpHandlerFactory2.GetHandler(HttpContext context, String requestType,
        VirtualPath virtualPath, String physicalPath) {

        BuildResultCompiledType result = (BuildResultCompiledType)BuildManager.GetVPathBuildResult(
            context, virtualPath);

        // Make sure the type has the correct base class (ASURT 123677)
        Util.CheckAssignableType(typeof(IHttpHandler), result.ResultType);

        return (IHttpHandler) result.CreateInstance();
    }

    public virtual void ReleaseHandler(IHttpHandler handler) {
    }
}

}
