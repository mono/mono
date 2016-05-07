//------------------------------------------------------------------------------
// <copyright file="SoapTypeAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>                                                                
//------------------------------------------------------------------------------

namespace System.Xml.Serialization {
    using System;

    /// <include file='doc\SoapTypeAttribute.uex' path='docs/doc[@for="SoapTypeAttribute"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Struct)]
    public class SoapTypeAttribute : System.Attribute {
        string ns;
        string typeName;
        bool includeInSchema = true;

        /// <include file='doc\SoapTypeAttribute.uex' path='docs/doc[@for="SoapTypeAttribute.SoapTypeAttribute"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public SoapTypeAttribute() {
        }

        /// <include file='doc\SoapTypeAttribute.uex' path='docs/doc[@for="SoapTypeAttribute.SoapTypeAttribute1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public SoapTypeAttribute(string typeName) {
            this.typeName = typeName;
        }

        /// <include file='doc\SoapTypeAttribute.uex' path='docs/doc[@for="SoapTypeAttribute.SoapTypeAttribute2"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public SoapTypeAttribute(string typeName, string ns) {
            this.typeName = typeName;
            this.ns = ns;
        }

        /// <include file='doc\SoapTypeAttribute.uex' path='docs/doc[@for="SoapTypeAttribute.IncludeInSchema"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool IncludeInSchema {
            get { return includeInSchema; }
            set { includeInSchema = value; }
        }

        /// <include file='doc\SoapTypeAttribute.uex' path='docs/doc[@for="SoapTypeAttribute.TypeName"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string TypeName {
            get { return typeName == null ? string.Empty : typeName; }
            set { typeName = value; }
        }

        /// <include file='doc\SoapTypeAttribute.uex' path='docs/doc[@for="SoapTypeAttribute.Namespace"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string Namespace {
            get { return ns; }
            set { ns = value; }
        }
    }
}
