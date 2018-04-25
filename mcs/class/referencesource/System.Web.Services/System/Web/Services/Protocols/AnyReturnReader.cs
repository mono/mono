//------------------------------------------------------------------------------
// <copyright file="AnyReturnReader.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Protocols {
    using System.IO;
    using System;
    using System.Web.Services;
    using System.Net;

    /// <include file='doc\AnyReturnReader.uex' path='docs/doc[@for="AnyReturnReader"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public class AnyReturnReader : MimeReturnReader {
        /// <include file='doc\AnyReturnReader.uex' path='docs/doc[@for="AnyReturnReader.Initialize"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override void Initialize(object o) {
        }

        /// <include file='doc\AnyReturnReader.uex' path='docs/doc[@for="AnyReturnReader.GetInitializer"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override object GetInitializer(LogicalMethodInfo methodInfo) {
            if (methodInfo.IsVoid) return null;
            return this;
        }

        /// <include file='doc\AnyReturnReader.uex' path='docs/doc[@for="AnyReturnReader.Read"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override object Read(WebResponse response, Stream responseStream) {
            // caller needs to call close on the stream
            return responseStream;
        }
    }
}
