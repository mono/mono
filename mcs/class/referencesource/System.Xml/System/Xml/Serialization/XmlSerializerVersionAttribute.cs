
//------------------------------------------------------------------------------
// <copyright file="XmlSerializerVersionAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>                                                                
//------------------------------------------------------------------------------

namespace System.Xml.Serialization {
    using System;


    /// <include file='doc\XmlSerializerVersionAttribute.uex' path='docs/doc[@for="XmlSerializerVersionAttribute"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class XmlSerializerVersionAttribute : System.Attribute {
        string mvid;
        string serializerVersion;
        string ns;
        Type type;
        
        /// <include file='doc\XmlSerializerVersionAttribute.uex' path='docs/doc[@for="XmlSerializerVersionAttribute.XmlSerializerVersionAttribute"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlSerializerVersionAttribute() {
        }
        
        /// <include file='doc\XmlSerializerVersionAttribute.uex' path='docs/doc[@for="XmlSerializerVersionAttribute.XmlSerializerAssemblyAttribute1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlSerializerVersionAttribute(Type type) {
            this.type = type;
        }
        
        /// <include file='doc\XmlSerializerVersionAttribute.uex' path='docs/doc[@for="XmlSerializerVersionAttribute.ParentAssemblyId"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string ParentAssemblyId {
            get { return mvid; }
            set { mvid = value; }
        }

        /// <include file='doc\XmlSerializerVersionAttribute.uex' path='docs/doc[@for="XmlSerializerVersionAttribute.ParentAssemblyId"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string Version {
            get { return serializerVersion; }
            set { serializerVersion = value; }
        }


        /// <include file='doc\XmlSerializerVersionAttribute.uex' path='docs/doc[@for="XmlSerializerVersionAttribute.Namespace"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string Namespace {
            get { return ns; }
            set { ns = value; }
        }

        /// <include file='doc\XmlSerializerVersionAttribute.uex' path='docs/doc[@for="XmlSerializerVersionAttribute.TypeName"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Type Type {
            get { return type; }
            set { type = value; }
        }
    }
}
