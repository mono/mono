//------------------------------------------------------------------------------
// <copyright file="HTTPNotFoundHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * Synchronous Http request handler interface
 * 
 * Copyright (c) 1998 Microsoft Corporation
 */

namespace System.Web {


    /// <devdoc>
    ///    <para>Provides a synchronous Http request handler interface.</para>
    /// </devdoc>
    internal class HttpNotFoundHandler : IHttpHandler {
        
        internal HttpNotFoundHandler() {
        }


        /// <devdoc>
        ///    <para>Drives web processing execution.</para>
        /// </devdoc>
        public void ProcessRequest(HttpContext context) {
            PerfCounters.IncrementCounter(AppPerfCounter.REQUESTS_NOT_FOUND);

            throw new HttpException(404, 
                                    SR.GetString(SR.Path_not_found, context.Request.Path));
        }


        /// <devdoc>
        ///    <para>Indicates whether an HttpNotFoundHandler instance can be recycled and used 
        ///       for another request.</para>
        /// </devdoc>
        public bool IsReusable {
            get { return true; }
        }
    }


    internal class HttpForbiddenHandler : IHttpHandler {
        
        internal HttpForbiddenHandler() {
        }


        /// <devdoc>
        ///    <para>Drives web processing execution.</para>
        /// </devdoc>
        public void ProcessRequest(HttpContext context) {
            PerfCounters.IncrementCounter(AppPerfCounter.REQUESTS_NOT_FOUND);

            throw new HttpException(403, 
                                    SR.GetString(SR.Path_forbidden, context.Request.Path));
        }


        /// <devdoc>
        ///    <para>Indicates whether an HttpForbiddenHandler instance can be recycled and used 
        ///       for another request.</para>
        /// </devdoc>
        public bool IsReusable {
            get { return true; }
        }
    }


    /// <devdoc>
    ///    <para>Provides a synchronous Http request handler interface.</para>
    /// </devdoc>
    internal class HttpMethodNotAllowedHandler : IHttpHandler {
        
        internal HttpMethodNotAllowedHandler() {
        }


        /// <devdoc>
        ///    <para> Drives 
        ///       web processing execution.</para>
        /// </devdoc>
        public void ProcessRequest(HttpContext context) {
            throw new HttpException(405,
                                    SR.GetString(SR.Path_forbidden, context.Request.HttpMethod));
        }


        /// <devdoc>
        ///    <para>Indicates whether an HttpForbiddenHandler instance can be recycled and used 
        ///       for another request.</para>
        /// </devdoc>
        public bool IsReusable {
            get { return true; }
        }
    }


    /// <devdoc>
    ///    <para>Provides a synchronous Http request handler interface.</para>
    /// </devdoc>
    internal class HttpNotImplementedHandler : IHttpHandler {
        
        internal HttpNotImplementedHandler() {
        }


        /// <devdoc>
        ///    <para>Drives web processing execution.</para>
        /// </devdoc>
        public void ProcessRequest(HttpContext context) {
            throw new HttpException(501, 
                                    SR.GetString(SR.Method_for_path_not_implemented, context.Request.HttpMethod, context.Request.Path));
        }


        /// <devdoc>
        ///    <para>Indicates whether an HttpNotImplementedHandler instance can be recycled and 
        ///       used for another request.</para>
        /// </devdoc>
        public bool IsReusable {
            get { return true; }
        }
    }
}

