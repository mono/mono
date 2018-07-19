//------------------------------------------------------------------------------
// <copyright file="UrlParameterWriter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Protocols {
    using System.IO;
    using System;
    using System.Xml.Serialization;
    using System.Reflection;
    using System.Collections;
    using System.Text;
    using System.Web.Services;
    using System.Net;
    using System.Globalization;

    /// <include file='doc\UrlParameterWriter.uex' path='docs/doc[@for="UrlParameterWriter"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public class UrlParameterWriter : UrlEncodedParameterWriter {
        /// <include file='doc\UrlParameterWriter.uex' path='docs/doc[@for="UrlParameterWriter.GetRequestUrl"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override string GetRequestUrl(string url, object[] parameters) {
            if (parameters.Length == 0) return url;
            StringBuilder builder = new StringBuilder(url);
            builder.Append('?');
            TextWriter writer = new StringWriter(builder, CultureInfo.InvariantCulture);
            Encode(writer, parameters);
            writer.Flush();
            return builder.ToString();
        }
    }
}
