//------------------------------------------------------------------------------
// <copyright file="XmlSchemaComplexContentRestriction.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>                                                                
//------------------------------------------------------------------------------

namespace System.Xml.Schema {

    using System.Collections;
    using System.Xml.Serialization;

    /// <include file='doc\XmlSchemaComplexContentRestriction.uex' path='docs/doc[@for="XmlSchemaComplexContentRestriction"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public class XmlSchemaComplexContentRestriction : XmlSchemaContent {
        XmlSchemaParticle particle;
        XmlSchemaObjectCollection attributes = new XmlSchemaObjectCollection();
        XmlSchemaAnyAttribute anyAttribute;
        XmlQualifiedName baseTypeName = XmlQualifiedName.Empty; 

        /// <include file='doc\XmlSchemaComplexContentRestriction.uex' path='docs/doc[@for="XmlSchemaComplexContentRestriction.BaseTypeName"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlAttribute("base")]
        public XmlQualifiedName BaseTypeName { 
            get { return baseTypeName; }
            set { baseTypeName = (value == null ? XmlQualifiedName.Empty : value); }
        }

        /// <include file='doc\XmlSchemaComplexContentRestriction.uex' path='docs/doc[@for="XmlSchemaComplexContentRestriction.Particle"]/*' />
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

        /// <include file='doc\XmlSchemaComplexContentRestriction.uex' path='docs/doc[@for="XmlSchemaComplexContentRestriction.Attributes"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlElement("attribute", typeof(XmlSchemaAttribute)),
         XmlElement("attributeGroup", typeof(XmlSchemaAttributeGroupRef))]
        public XmlSchemaObjectCollection Attributes {
            get { return attributes; }
        }

        /// <include file='doc\XmlSchemaComplexContentRestriction.uex' path='docs/doc[@for="XmlSchemaComplexContentRestriction.AnyAttribute"]/*' />
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

