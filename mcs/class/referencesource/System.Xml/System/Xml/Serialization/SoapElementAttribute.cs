//------------------------------------------------------------------------------
// <copyright file="SoapElementAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>                                                                
//------------------------------------------------------------------------------

namespace System.Xml.Serialization {
    using System;

    /// <include file='doc\SoapElementAttribute.uex' path='docs/doc[@for="SoapElementAttribute"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public class SoapElementAttribute : System.Attribute {
        string elementName;
        string dataType;
        bool nullable;
        
        /// <include file='doc\SoapElementAttribute.uex' path='docs/doc[@for="SoapElementAttribute.SoapElementAttribute"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public SoapElementAttribute() {
        }
        
        /// <include file='doc\SoapElementAttribute.uex' path='docs/doc[@for="SoapElementAttribute.SoapElementAttribute1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public SoapElementAttribute(string elementName) {
            this.elementName = elementName;
        }

        /// <include file='doc\SoapElementAttribute.uex' path='docs/doc[@for="SoapElementAttribute.ElementName"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string ElementName {
            get { return elementName == null ? string.Empty : elementName; }
            set { elementName = value; }
        }

        /// <include file='doc\SoapElementAttribute.uex' path='docs/doc[@for="SoapElementAttribute.DataType"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string DataType {
            get { return dataType == null ? string.Empty : dataType; }
            set { dataType = value; }
        }

        /// <include file='doc\SoapElementAttribute.uex' path='docs/doc[@for="SoapElementAttribute.IsNullable"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool IsNullable {
            get { return nullable; }
            set { nullable = value; }
        }

    }
}
