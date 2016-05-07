//------------------------------------------------------------------------------
// <copyright file="HttpGetClientProtocol.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Protocols {
    using System.Net;
    using System.IO;

    /// <include file='doc\HttpGetClientProtocol.uex' path='docs/doc[@for="HttpGetClientProtocol"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public class HttpGetClientProtocol : HttpSimpleClientProtocol {
        /// <include file='doc\HttpGetClientProtocol.uex' path='docs/doc[@for="HttpGetClientProtocol.HttpGetClientProtocol"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public HttpGetClientProtocol()
            : base() {
        }

        /// <include file='doc\HttpGetClientProtocol.uex' path='docs/doc[@for="HttpGetClientProtocol.GetWebRequest"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override WebRequest GetWebRequest(Uri uri) {
            WebRequest request = base.GetWebRequest(uri);            
            request.Method = "GET";
            return request;
        }
    }
}
