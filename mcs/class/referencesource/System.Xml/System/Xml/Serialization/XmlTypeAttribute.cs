//------------------------------------------------------------------------------
// <copyright file="XmlTypeAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>                                                                
//------------------------------------------------------------------------------

namespace System.Xml.Serialization {

    using System;

    /// <include file='doc\XmlTypeAttribute.uex' path='docs/doc[@for="XmlTypeAttribute"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Struct)]
    public class XmlTypeAttribute : System.Attribute {
        bool includeInSchema = true;
        bool anonymousType;
        string ns;
        string typeName;

        /// <include file='doc\XmlTypeAttribute.uex' path='docs/doc[@for="XmlTypeAttribute.XmlTypeAttribute"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlTypeAttribute() {
        }

        /// <include file='doc\XmlTypeAttribute.uex' path='docs/doc[@for="XmlTypeAttribute.XmlTypeAttribute1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlTypeAttribute(string typeName) {
            this.typeName = typeName;
        }

        /// <include file='doc\XmlTypeAttribute.uex' path='docs/doc[@for="XmlTypeAttribute.AnonymousType"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool AnonymousType {
            get { return anonymousType; }
            set { anonymousType = value; }        
        }

        /// <include file='doc\XmlTypeAttribute.uex' path='docs/doc[@for="XmlTypeAttribute.IncludeInSchema"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool IncludeInSchema {
            get { return includeInSchema; }
            set { includeInSchema = value; }
        }

        /// <include file='doc\XmlTypeAttribute.uex' path='docs/doc[@for="XmlTypeAttribute.TypeName"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string TypeName {
            get { return typeName == null ? string.Empty : typeName; }
            set { typeName = value; }
        }

        /// <include file='doc\XmlTypeAttribute.uex' path='docs/doc[@for="XmlTypeAttribute.Namespace"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string Namespace {
            get { return ns; }
            set { ns = value; }
        }
    }
}
