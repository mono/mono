//------------------------------------------------------------------------------
// <copyright file="IHttpHandlerFactory.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * Handler factory interface
 */
namespace System.Web {
    
    using System.Security.Permissions;
    /*
     * Handler factory -- gets Handler by requestType,path,file
     */

    /// <devdoc>
    ///    <para>
    ///       Defines the contract that factories must implement to dynamically
    ///       create IHttpHandler instances.
    ///    </para>
    /// </devdoc>
    public interface IHttpHandlerFactory {

        /// <devdoc>
        ///    <para>
        ///       Returns an instance of an IHttpHandler class.
        ///    </para>
        /// </devdoc>
        IHttpHandler GetHandler(HttpContext context, String requestType, String url, String pathTranslated);

        /// <devdoc>
        ///    <para>
        ///       Enables a factory to recycle or re-use an existing handler
        ///       instance.
        ///    </para>
        /// </devdoc>
        void ReleaseHandler(IHttpHandler handler);
    }

    internal interface IHttpHandlerFactory2 : IHttpHandlerFactory {

        /// <devdoc>
        ///    <para>
        ///       Returns an instance of an IHttpHandler class. Works directly with a VirtualPath object
        ///       to avoid unnecessary conversions and creations.
        ///    </para>
        /// </devdoc>
        IHttpHandler GetHandler(HttpContext context, String requestType, VirtualPath virtualPath, String physicalPath);
    }
}
