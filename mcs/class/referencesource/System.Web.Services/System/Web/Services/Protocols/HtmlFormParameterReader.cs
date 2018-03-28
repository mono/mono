//------------------------------------------------------------------------------
// <copyright file="HtmlFormParameterReader.cs" company="Microsoft">
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

    /// <include file='doc\HtmlFormParameterReader.uex' path='docs/doc[@for="HtmlFormParameterReader"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public class HtmlFormParameterReader : ValueCollectionParameterReader {
        internal const string MimeType = "application/x-www-form-urlencoded";

        /// <include file='doc\HtmlFormParameterReader.uex' path='docs/doc[@for="HtmlFormParameterReader.Read"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override object[] Read(HttpRequest request) {
            if (!ContentType.MatchesBase(request.ContentType, MimeType)) return null;
            return Read(request.Form);
        }
    }

}
