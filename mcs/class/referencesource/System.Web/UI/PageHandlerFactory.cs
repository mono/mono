//------------------------------------------------------------------------------
// <copyright file="PageHandlerFactory.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * Handler Factory implementation for Page files
 *
 * Copyright (c) 1999 Microsoft Corporation
 */

namespace System.Web.UI {
using System.Runtime.Serialization.Formatters;

using System.IO;
using System.Security.Permissions;
using System.Web.Compilation;
using System.Web.Util;
using Debug=System.Web.Util.Debug;

/*
 * Handler Factory implementation for ASP.NET files
 */
[PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
[PermissionSet(SecurityAction.InheritanceDemand, Unrestricted=true)]
public class PageHandlerFactory : IHttpHandlerFactory2 {

    private bool _isInheritedInstance;

    protected internal PageHandlerFactory() {
        // Check whether this is the exact PageHandlerFactory, or a derived class
        _isInheritedInstance = (GetType() != typeof(PageHandlerFactory));
    }

    public virtual IHttpHandler GetHandler(HttpContext context, string requestType, string virtualPath, string path) {
        Debug.Trace("PageHandlerFactory", "PageHandlerFactory: " + virtualPath);

        // This should never get called in ISAPI mode but currently is in integrated mode
        // Debug.Assert(false);

        return GetHandlerHelper(context, requestType, VirtualPath.CreateNonRelative(virtualPath), path);
    }

    IHttpHandler IHttpHandlerFactory2.GetHandler(HttpContext context, String requestType,
        VirtualPath virtualPath, String physicalPath) {

        // If it's a derived class, we must call the old (less efficient) GetHandler, in
        // case it was overriden
        if (_isInheritedInstance) {
            return GetHandler(context, requestType, virtualPath.VirtualPathString, physicalPath);
        }

        return GetHandlerHelper(context, requestType, virtualPath, physicalPath);
    }

    public virtual void ReleaseHandler(IHttpHandler handler) { }

    private IHttpHandler GetHandlerHelper(HttpContext context, string requestType,
        VirtualPath virtualPath, string physicalPath) {

        Page page = BuildManager.CreateInstanceFromVirtualPath(
            virtualPath, typeof(Page), context, true /*allowCrossApp*/) as Page;
        if (page == null)
            return null;

        page.TemplateControlVirtualPath = virtualPath;

        return page;
    }

}
}
