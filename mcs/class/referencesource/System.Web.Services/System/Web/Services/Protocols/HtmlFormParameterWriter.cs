//------------------------------------------------------------------------------
// <copyright file="HtmlFormParameterWriter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Protocols {
    using System.IO;
    using System;
    using System.Text;
    using System.Xml.Serialization;
    using System.Reflection;
    using System.Collections;
    using System.Web.Services;
    using System.Net;

    /// <include file='doc\HtmlFormParameterWriter.uex' path='docs/doc[@for="HtmlFormParameterWriter"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public class HtmlFormParameterWriter : UrlEncodedParameterWriter {
        /// <include file='doc\HtmlFormParameterWriter.uex' path='docs/doc[@for="HtmlFormParameterWriter.UsesWriteRequest"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override bool UsesWriteRequest { get { return true; } }

        /// <include file='doc\HtmlFormParameterWriter.uex' path='docs/doc[@for="HtmlFormParameterWriter.InitializeRequest"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override void InitializeRequest(WebRequest request, object[] values) {
            request.ContentType = ContentType.Compose(HtmlFormParameterReader.MimeType, RequestEncoding);
        }

        /// <include file='doc\HtmlFormParameterWriter.uex' path='docs/doc[@for="HtmlFormParameterWriter.WriteRequest"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override void WriteRequest(Stream requestStream, object[] values) {            
            if (values.Length == 0) return;

            // just use ASCII encoding since we're url-escaping everything...
            TextWriter writer = new StreamWriter(requestStream, new ASCIIEncoding());
            Encode(writer, values);                        
            writer.Flush();            
        }
    }
}
