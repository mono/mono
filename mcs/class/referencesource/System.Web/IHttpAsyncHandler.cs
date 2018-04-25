//------------------------------------------------------------------------------
// <copyright file="IHttpAsyncHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * Asynchronous Http request handler interface
 * 
 * Copyright (c) 2000 Microsoft Corporation
 */

namespace System.Web {

    using System.Security.Permissions;

    /// <devdoc>
    ///    <para>When implemented by a class, defines the contract that Http Async Handler objects must
    ///       implement.</para>
    /// </devdoc>
    public interface IHttpAsyncHandler : IHttpHandler {

        /// <devdoc>
        ///    <para>Registers handler for async notification.</para>
        /// </devdoc>
        IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, Object extraData);

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>

        void EndProcessRequest(IAsyncResult result);
    }

}
