//------------------------------------------------------------------------------
// <copyright file="XmlSchemaAttributeGroupRef.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright> 
// <owner current="true" primary="true">Microsoft</owner>                                                               
//------------------------------------------------------------------------------

namespace System.Xml.Schema {

    using System.Collections;
    using System.Xml.Serialization;

    /// <include file='doc\XmlSchemaAttributeGroupRef.uex' path='docs/doc[@for="XmlSchemaAttributeGroupRef"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public class XmlSchemaAttributeGroupRef : XmlSchemaAnnotated {
        XmlQualifiedName refName = XmlQualifiedName.Empty; 

        /// <include file='doc\XmlSchemaAttributeGroupRef.uex' path='docs/doc[@for="XmlSchemaAttributeGroupRef.RefName"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlAttribute("ref")]
        public XmlQualifiedName RefName { 
            get { return refName; }
            set { refName = (value == null ? XmlQualifiedName.Empty : value); }
        }
    }

}
