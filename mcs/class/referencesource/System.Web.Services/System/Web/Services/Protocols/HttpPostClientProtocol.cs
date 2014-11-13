//------------------------------------------------------------------------------
// <copyright file="HttpPostClientProtocol.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Protocols {
    using System.Net;
    using System.IO;

    /// <include file='doc\HttpPostClientProtocol.uex' path='docs/doc[@for="HttpPostClientProtocol"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public class HttpPostClientProtocol : HttpSimpleClientProtocol {
        /// <include file='doc\HttpPostClientProtocol.uex' path='docs/doc[@for="HttpPostClientProtocol.HttpPostClientProtocol"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public HttpPostClientProtocol() 
            : base() {
        }

        /// <include file='doc\HttpPostClientProtocol.uex' path='docs/doc[@for="HttpPostClientProtocol.GetWebRequest"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override WebRequest GetWebRequest(Uri uri) {
            WebRequest request = base.GetWebRequest(uri);
            request.Method = "POST";
            return request;
        }
    }
}
