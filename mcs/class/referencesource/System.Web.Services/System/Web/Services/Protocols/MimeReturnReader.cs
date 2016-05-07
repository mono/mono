//------------------------------------------------------------------------------
// <copyright file="MimeReturnReader.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Protocols {
    using System.IO;
    using System;
    using System.Xml.Serialization;
    using System.Reflection;
    using System.Collections;
    using System.Web.Services;
    using System.Net;

    /// <include file='doc\MimeReturnReader.uex' path='docs/doc[@for="MimeReturnReader"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public abstract class MimeReturnReader : MimeFormatter {
        /// <include file='doc\MimeReturnReader.uex' path='docs/doc[@for="MimeReturnReader.Read"]/*' />
        // It is the responsibility of the MimeReturnReader to call close on the responseStream.
        public abstract object Read(WebResponse response, Stream responseStream);
    }
}
