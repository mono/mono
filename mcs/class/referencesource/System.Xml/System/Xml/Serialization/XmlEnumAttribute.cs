//------------------------------------------------------------------------------
// <copyright file="XmlEnumAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>                                                                
//------------------------------------------------------------------------------

namespace System.Xml.Serialization {

    using System;
    
    /// <include file='doc\XmlEnumAttribute.uex' path='docs/doc[@for="XmlEnumAttribute"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Field)]
    public class XmlEnumAttribute : System.Attribute {
        string name; 

        /// <include file='doc\XmlEnumAttribute.uex' path='docs/doc[@for="XmlEnumAttribute.XmlEnumAttribute"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlEnumAttribute() {
        }

        /// <include file='doc\XmlEnumAttribute.uex' path='docs/doc[@for="XmlEnumAttribute.XmlEnumAttribute1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlEnumAttribute(string name) {
            this.name = name;
        }

        /// <include file='doc\XmlEnumAttribute.uex' path='docs/doc[@for="XmlEnumAttribute.Name"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string Name {
            get { return name; }
            set { name = value; }
        }
    }
 }
