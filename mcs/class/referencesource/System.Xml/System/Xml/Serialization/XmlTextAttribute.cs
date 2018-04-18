//------------------------------------------------------------------------------
// <copyright file="XmlTextAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>                                                                
//------------------------------------------------------------------------------

namespace System.Xml.Serialization {

    using System;
    
    /// <include file='doc\XmlTextAttribute.uex' path='docs/doc[@for="XmlTextAttribute"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public class XmlTextAttribute : System.Attribute {
        Type type;
        string dataType;

        /// <include file='doc\XmlTextAttribute.uex' path='docs/doc[@for="XmlTextAttribute.XmlTextAttribute"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlTextAttribute() {
        }
        
        /// <include file='doc\XmlTextAttribute.uex' path='docs/doc[@for="XmlTextAttribute.XmlTextAttribute1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlTextAttribute(Type type) {
            this.type = type;
        }
        
        /// <include file='doc\XmlTextAttribute.uex' path='docs/doc[@for="XmlTextAttribute.Type"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Type Type {
            get { return type; }
            set { type = value; }
        }

        /// <include file='doc\XmlTextAttribute.uex' path='docs/doc[@for="XmlTextAttribute.DataType"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string DataType {
            get { return dataType == null ? string.Empty : dataType; }
            set { dataType = value; }
        }
    }
}
