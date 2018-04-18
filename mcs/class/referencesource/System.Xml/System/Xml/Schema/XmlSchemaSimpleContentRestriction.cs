//------------------------------------------------------------------------------
// <copyright file="XmlSchemaSimpleContentRestriction.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright> 
// <owner current="true" primary="true">Microsoft</owner>                                                                
//------------------------------------------------------------------------------

namespace System.Xml.Schema {

    using System.Xml.Serialization;

    /// <include file='doc\XmlSchemaSimpleContentRestriction.uex' path='docs/doc[@for="XmlSchemaSimpleContentRestriction"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public class XmlSchemaSimpleContentRestriction : XmlSchemaContent {
        XmlQualifiedName baseTypeName = XmlQualifiedName.Empty; 
        XmlSchemaSimpleType baseType;
        XmlSchemaObjectCollection facets = new XmlSchemaObjectCollection();
        XmlSchemaObjectCollection attributes = new XmlSchemaObjectCollection();
        XmlSchemaAnyAttribute anyAttribute;

        /// <include file='doc\XmlSchemaSimpleContentRestriction.uex' path='docs/doc[@for="XmlSchemaSimpleContentRestriction.BaseTypeName"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlAttribute("base")]
        public XmlQualifiedName BaseTypeName { 
            get { return baseTypeName; }
            set { baseTypeName = (value == null ? XmlQualifiedName.Empty : value); }
        }
        
        /// <include file='doc\XmlSchemaSimpleContentRestriction.uex' path='docs/doc[@for="XmlSchemaSimpleContentRestriction.BaseType"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlElement("simpleType", typeof(XmlSchemaSimpleType))]
        public XmlSchemaSimpleType BaseType { 
            get { return baseType; }
            set { baseType = value; }
        }
        
        /// <include file='doc\XmlSchemaSimpleContentRestriction.uex' path='docs/doc[@for="XmlSchemaSimpleContentRestriction.Facets"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlElement("length", typeof(XmlSchemaLengthFacet)),
         XmlElement("minLength", typeof(XmlSchemaMinLengthFacet)),
         XmlElement("maxLength", typeof(XmlSchemaMaxLengthFacet)),
         XmlElement("pattern", typeof(XmlSchemaPatternFacet)),
         XmlElement("enumeration", typeof(XmlSchemaEnumerationFacet)),
         XmlElement("maxInclusive", typeof(XmlSchemaMaxInclusiveFacet)),
         XmlElement("maxExclusive", typeof(XmlSchemaMaxExclusiveFacet)),
         XmlElement("minInclusive", typeof(XmlSchemaMinInclusiveFacet)),
         XmlElement("minExclusive", typeof(XmlSchemaMinExclusiveFacet)),
         XmlElement("totalDigits", typeof(XmlSchemaTotalDigitsFacet)),
         XmlElement("fractionDigits", typeof(XmlSchemaFractionDigitsFacet)),
         XmlElement("whiteSpace", typeof(XmlSchemaWhiteSpaceFacet))]
        public XmlSchemaObjectCollection Facets {
            get { return facets; }
        }

        /// <include file='doc\XmlSchemaSimpleContentRestriction.uex' path='docs/doc[@for="XmlSchemaSimpleContentRestriction.Attributes"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlElement("attribute", typeof(XmlSchemaAttribute)),
         XmlElement("attributeGroup", typeof(XmlSchemaAttributeGroupRef))]
        public XmlSchemaObjectCollection Attributes {
            get { return attributes; }
        }

        /// <include file='doc\XmlSchemaSimpleContentRestriction.uex' path='docs/doc[@for="XmlSchemaSimpleContentRestriction.AnyAttribute"]/*' />
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

