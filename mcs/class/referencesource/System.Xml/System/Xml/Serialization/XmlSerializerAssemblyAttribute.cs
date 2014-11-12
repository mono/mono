
//------------------------------------------------------------------------------
// <copyright file="XmlSerializerAssemblyAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>                                                                
//------------------------------------------------------------------------------

namespace System.Xml.Serialization {
    using System;

    /// <include file='doc\XmlSerializerAssemblyAttribute.uex' path='docs/doc[@for="XmlSerializerAssemblyAttribute"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Struct, AllowMultiple=false)]
    public sealed class XmlSerializerAssemblyAttribute : System.Attribute {
        string assemblyName;
        string codeBase;

        /// <include file='doc\XmlSerializerAssemblyAttribute.uex' path='docs/doc[@for="XmlSerializerAssemblyAttribute.XmlSerializerAssemblyAttribute"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlSerializerAssemblyAttribute() : this(null, null) {}

        /// <include file='doc\XmlSerializerAssemblyAttribute.uex' path='docs/doc[@for="XmlSerializerAssemblyAttribute.XmlSerializerAssemblyAttribute1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlSerializerAssemblyAttribute(string assemblyName) : this(assemblyName, null) {}
        
        /// <include file='doc\XmlSerializerAssemblyAttribute.uex' path='docs/doc[@for="XmlSerializerAssemblyAttribute.XmlSerializerAssemblyAttribute2"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlSerializerAssemblyAttribute(string assemblyName, string codeBase) {
            this.assemblyName = assemblyName;
            this.codeBase = codeBase;
        }

        /// <include file='doc\XmlSerializerAssemblyAttribute.uex' path='docs/doc[@for="XmlSerializerAssemblyAttribute.Location"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string CodeBase {
            get { return codeBase; }
            set { codeBase = value; }
        }

        /// <include file='doc\XmlSerializerAssemblyAttribute.uex' path='docs/doc[@for="XmlSerializerAssemblyAttribute.AssemblyName"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string AssemblyName {
            get { return assemblyName; }
            set { assemblyName = value; }
        }
    }
}
