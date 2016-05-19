//------------------------------------------------------------------------------
// <copyright file="XmlSchemaComplexContentExtension.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>  
// <owner current="true" primary="true">[....]</owner>                                                              
//------------------------------------------------------------------------------

namespace System.Xml.Schema {

    using System.Collections;
    using System.Xml.Serialization;

    /// <include file='doc\XmlSchemaComplexContentExtension.uex' path='docs/doc[@for="XmlSchemaComplexContentExtension"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public class XmlSchemaComplexContentExtension : XmlSchemaContent {
        XmlSchemaParticle particle;
        XmlSchemaObjectCollection attributes = new XmlSchemaObjectCollection();
        XmlSchemaAnyAttribute anyAttribute;
        XmlQualifiedName baseTypeName = XmlQualifiedName.Empty; 

        /// <include file='doc\XmlSchemaComplexContentExtension.uex' path='docs/doc[@for="XmlSchemaComplexContentExtension.BaseTypeName"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlAttribute("base")]
        public XmlQualifiedName BaseTypeName { 
            get { return baseTypeName; }
            set { baseTypeName = (value == null ? XmlQualifiedName.Empty : value); }
        }

        /// <include file='doc\XmlSchemaComplexContentExtension.uex' path='docs/doc[@for="XmlSchemaComplexContentExtension.Particle"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlElement("group", typeof(XmlSchemaGroupRef)),
         XmlElement("choice", typeof(XmlSchemaChoice)),
         XmlElement("all", typeof(XmlSchemaAll)),
         XmlElement("sequence", typeof(XmlSchemaSequence))]
        public XmlSchemaParticle Particle {
            get { return particle; }
            set { particle = value; }
        }

        /// <include file='doc\XmlSchemaComplexContentExtension.uex' path='docs/doc[@for="XmlSchemaComplexContentExtension.Attributes"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlElement("attribute", typeof(XmlSchemaAttribute)),
         XmlElement("attributeGroup", typeof(XmlSchemaAttributeGroupRef))]
        public XmlSchemaObjectCollection Attributes {
            get { return attributes; }
        }


        /// <include file='doc\XmlSchemaComplexContentExtension.uex' path='docs/doc[@for="XmlSchemaComplexContentExtension.AnyAttribute"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlElement("anyAttribute")]
        public XmlSchemaAnyAttribute AnyAttribute {
            get { return anyAttribute; }
            set { anyAttribute = value; }
        }
        
        internal void SetAttributes(XmlSchemaObjectCollection newAttributes) {
            attributes = newAttributes;
        }
    }

}

