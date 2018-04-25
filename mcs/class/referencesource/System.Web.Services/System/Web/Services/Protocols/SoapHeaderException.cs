//------------------------------------------------------------------------------
// <copyright file="SoapHeaderException.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Protocols {
    using System.Web.Services;
    using System.Xml;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;
    using System;
    using System.Reflection;
    using System.Collections;
    using System.IO;
    using System.ComponentModel;

    /// <include file='doc\SoapHeaderException.uex' path='docs/doc[@for="SoapHeaderException"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [Serializable]
    public class SoapHeaderException : SoapException {

        /// <include file='doc\SoapHeaderException.uex' path='docs/doc[@for="SoapHeaderException.SoapHeaderException6"]/*' />
        public SoapHeaderException() {
        }

        /// <include file='doc\SoapHeaderException.uex' path='docs/doc[@for="SoapHeaderException.SoapHeaderException"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public SoapHeaderException(string message, XmlQualifiedName code, string actor) 
            : base(message, code, actor) { 
        }

        /// <include file='doc\SoapHeaderException.uex' path='docs/doc[@for="SoapHeaderException.SoapHeaderException1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public SoapHeaderException(string message, XmlQualifiedName code, string actor, Exception innerException) 
            : base(message, code, actor, innerException) { 
        }

        /// <include file='doc\SoapHeaderException.uex' path='docs/doc[@for="SoapHeaderException.SoapHeaderException2"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public SoapHeaderException(string message, XmlQualifiedName code)
            : base(message, code) { 
        }

        /// <include file='doc\SoapHeaderException.uex' path='docs/doc[@for="SoapHeaderException.SoapHeaderException3"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public SoapHeaderException(string message, XmlQualifiedName code, Exception innerException) 
            : base(message, code, innerException) { 
        }

        /// <include file='doc\SoapHeaderException.uex' path='docs/doc[@for="SoapHeaderException.SoapHeaderException4"]/*' />
        public SoapHeaderException(string message, XmlQualifiedName code, string actor, string role, SoapFaultSubCode subCode, Exception innerException) 
            : base(message, code, actor, role, null, null, subCode, innerException) {
        }

        /// <include file='doc\SoapHeaderException.uex' path='docs/doc[@for="SoapHeaderException.SoapHeaderException5"]/*' />
        public SoapHeaderException(string message, XmlQualifiedName code, string actor, string role, string lang, SoapFaultSubCode subCode, Exception innerException) 
            : base(message, code, actor, role, lang, null, subCode, innerException) {
        }
        
        protected SoapHeaderException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}
