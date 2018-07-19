//------------------------------------------------------------------------------
// <copyright file="MimeParameterWriter.cs" company="Microsoft">
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
    using System.Text;

    /// <include file='doc\MimeParameterWriter.uex' path='docs/doc[@for="MimeParameterWriter"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public abstract class MimeParameterWriter : MimeFormatter {
        /// <include file='doc\MimeParameterWriter.uex' path='docs/doc[@for="MimeParameterWriter.UsesWriteRequest"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public virtual bool UsesWriteRequest { get { return false; } }

        /// <include file='doc\MimeParameterWriter.uex' path='docs/doc[@for="MimeParameterWriter.RequestEncoding"]/*' />
        public virtual Encoding RequestEncoding { 
            get { return null; }
            set { }
        }

        /// <include file='doc\MimeParameterWriter.uex' path='docs/doc[@for="MimeParameterWriter.GetRequestUrl"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public virtual string GetRequestUrl(string url, object[] parameters) {
            return url;
        }

        /// <include file='doc\MimeParameterWriter.uex' path='docs/doc[@for="MimeParameterWriter.InitializeRequest"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public virtual void InitializeRequest(WebRequest request, object[] values) {
            return;
        }
    
        /// <include file='doc\MimeParameterWriter.uex' path='docs/doc[@for="MimeParameterWriter.WriteRequest"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public virtual void WriteRequest(Stream requestStream, object[] values) {
            return;            
        }
    }

}
