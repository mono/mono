//------------------------------------------------------------------------------
// <copyright file="XmlSchemaSimpleTypeRestriction.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner> 
//------------------------------------------------------------------------------

namespace System.Xml.Schema {

    using System.Collections;
    using System.Xml.Serialization;

    /// <include file='doc\XmlSchemaSimpleTypeRestriction.uex' path='docs/doc[@for="XmlSchemaSimpleTypeRestriction"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public class XmlSchemaSimpleTypeRestriction : XmlSchemaSimpleTypeContent {
        XmlQualifiedName baseTypeName = XmlQualifiedName.Empty;
        XmlSchemaSimpleType baseType;
        XmlSchemaObjectCollection facets = new XmlSchemaObjectCollection();

        /// <include file='doc\XmlSchemaSimpleTypeRestriction.uex' path='docs/doc[@for="XmlSchemaSimpleTypeRestriction.BaseTypeName"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlAttribute("base")]
        public XmlQualifiedName BaseTypeName {
            get { return baseTypeName; }
            set { baseTypeName = (value == null ? XmlQualifiedName.Empty : value); }
        }

        /// <include file='doc\XmlSchemaSimpleTypeRestriction.uex' path='docs/doc[@for="XmlSchemaSimpleTypeRestriction.BaseType"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlElement("simpleType", typeof(XmlSchemaSimpleType))]
        public XmlSchemaSimpleType BaseType {
            get { return baseType; }
            set { baseType = value; }
        }

        /// <include file='doc\XmlSchemaSimpleTypeRestriction.uex' path='docs/doc[@for="XmlSchemaSimpleTypeRestriction.Facets"]/*' />
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

        internal override XmlSchemaObject Clone() {
            XmlSchemaSimpleTypeRestriction newRestriction = (XmlSchemaSimpleTypeRestriction)MemberwiseClone();
            newRestriction.BaseTypeName = baseTypeName.Clone();
            return newRestriction;
        }
    }
}

