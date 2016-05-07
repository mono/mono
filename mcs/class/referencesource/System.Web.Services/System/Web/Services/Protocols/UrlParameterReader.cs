//------------------------------------------------------------------------------
// <copyright file="UrlParameterReader.cs" company="Microsoft">
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

    /// <include file='doc\UrlParameterReader.uex' path='docs/doc[@for="UrlParameterReader"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public class UrlParameterReader : ValueCollectionParameterReader {
        /// <include file='doc\UrlParameterReader.uex' path='docs/doc[@for="UrlParameterReader.Read"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override object[] Read(HttpRequest request) {
            return Read(request.QueryString);
        }
    }

}
