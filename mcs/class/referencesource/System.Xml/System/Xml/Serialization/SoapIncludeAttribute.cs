//------------------------------------------------------------------------------
// <copyright file="SoapIncludeAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>                                                                
//------------------------------------------------------------------------------

namespace System.Xml.Serialization {

    using System;

    /// <include file='doc\SoapIncludeAttribute.uex' path='docs/doc[@for="SoapIncludeAttribute"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple=true)]
    public class SoapIncludeAttribute : System.Attribute {
        Type type;

        /// <include file='doc\SoapIncludeAttribute.uex' path='docs/doc[@for="SoapIncludeAttribute.SoapIncludeAttribute"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public SoapIncludeAttribute(Type type) {
            this.type = type;
        }

        /// <include file='doc\SoapIncludeAttribute.uex' path='docs/doc[@for="SoapIncludeAttribute.Type"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Type Type {
            get { return type; }
            set { type = value; }
        }
    }
}
