//------------------------------------------------------------------------------
// <copyright file="XmlSchemaType.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>  
// <owner current="true" primary="true">Microsoft</owner>                                                               
//------------------------------------------------------------------------------

using System.Collections;
using System.ComponentModel;
using System.Xml.Serialization;

namespace System.Xml.Schema {


    /// <include file='doc\XmlSchemaType.uex' path='docs/doc[@for="XmlSchemaType"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public class XmlSchemaType : XmlSchemaAnnotated {
        string name;
        XmlSchemaDerivationMethod final = XmlSchemaDerivationMethod.None;
        XmlSchemaDerivationMethod derivedBy;
        XmlSchemaType baseSchemaType;
        XmlSchemaDatatype datatype;
        XmlSchemaDerivationMethod finalResolved;
        volatile SchemaElementDecl elementDecl;
        volatile XmlQualifiedName qname = XmlQualifiedName.Empty; 
        XmlSchemaType redefined;

        //compiled information
        XmlSchemaContentType contentType;

        /// <include file='doc\XmlSchemaType.uex' path='docs/doc[@for="XmlSchemaType.GetXsdSimpleType"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static XmlSchemaSimpleType GetBuiltInSimpleType(XmlQualifiedName qualifiedName) {
            if (qualifiedName == null) {
                throw new ArgumentNullException("qualifiedName");
            }
            return DatatypeImplementation.GetSimpleTypeFromXsdType(qualifiedName);
        }
        
        /// <include file='doc\XmlSchemaType.uex' path='docs/doc[@for="XmlSchemaType.GetXsdSimpleType"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static XmlSchemaSimpleType GetBuiltInSimpleType(XmlTypeCode typeCode) {
            return DatatypeImplementation.GetSimpleTypeFromTypeCode(typeCode);
        }

        /// <include file='doc\XmlSchemaType.uex' path='docs/doc[@for="XmlSchemaType.GetXsdComplexType"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static XmlSchemaComplexType GetBuiltInComplexType(XmlTypeCode typeCode) {
            if (typeCode == XmlTypeCode.Item) {
                return XmlSchemaComplexType.AnyType;
            }
            return null;
        }

        /// <include file='doc\XmlSchemaType.uex' path='docs/doc[@for="XmlSchemaType.GetXsdComplexType"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static XmlSchemaComplexType GetBuiltInComplexType(XmlQualifiedName qualifiedName) {
            if (qualifiedName == null) {
                throw new ArgumentNullException("qualifiedName");
            }
            if (qualifiedName.Equals(XmlSchemaComplexType.AnyType.QualifiedName)) {
                return XmlSchemaComplexType.AnyType;
            }
            if (qualifiedName.Equals(XmlSchemaComplexType.UntypedAnyType.QualifiedName)) {
                return XmlSchemaComplexType.UntypedAnyType;
            }
            return null;
        }

        /// <include file='doc\XmlSchemaType.uex' path='docs/doc[@for="XmlSchemaType.Name"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlAttribute("name")]
        public string Name { 
            get { return name; }
            set { name = value; }
        }

        /// <include file='doc\XmlSchemaType.uex' path='docs/doc[@for="XmlSchemaType.Final"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlAttribute("final"), DefaultValue(XmlSchemaDerivationMethod.None)]
        public XmlSchemaDerivationMethod Final {
             get { return final; }
             set { final = value; }
        }

        /// <include file='doc\XmlSchemaType.uex' path='docs/doc[@for="XmlSchemaType.QualifiedName"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public XmlQualifiedName QualifiedName {
            get { return qname; }
        }

        /// <include file='doc\XmlSchemaType.uex' path='docs/doc[@for="XmlSchemaType.FinalResolved"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public XmlSchemaDerivationMethod FinalResolved {
             get { return finalResolved; }
        }

        /// <include file='doc\XmlSchemaType.uex' path='docs/doc[@for="XmlSchemaType.BaseSchemaType"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        [Obsolete("This property has been deprecated. Please use BaseXmlSchemaType property that returns a strongly typed base schema type. http://go.microsoft.com/fwlink/?linkid=14202")]
        public object BaseSchemaType {
            get {
                if (baseSchemaType == null)
                    return null;

                if (baseSchemaType.QualifiedName.Namespace == XmlReservedNs.NsXs) {
                    return baseSchemaType.Datatype;
                }
                return baseSchemaType;
            }
        }
        
        /// <include file='doc\XmlSchemaType.uex' path='docs/doc[@for="XmlSchemaType.BaseSchemaType"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public XmlSchemaType BaseXmlSchemaType {
            get { return baseSchemaType;}
        }

        /// <include file='doc\XmlSchemaType.uex' path='docs/doc[@for="XmlSchemaType.DerivedBy"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public XmlSchemaDerivationMethod DerivedBy {
            get { return derivedBy; }
        }

        /// <include file='doc\XmlSchemaType.uex' path='docs/doc[@for="XmlSchemaType.Datatype"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public XmlSchemaDatatype Datatype {
            get { return datatype;}
        }

        /// <include file='doc\XmlSchemaType.uex' path='docs/doc[@for="XmlSchemaType.IsMixed"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public virtual bool IsMixed {
            get { return false; }
            set {;}
        }
    
        [XmlIgnore]
        public XmlTypeCode TypeCode {
            get {
                if (this == XmlSchemaComplexType.AnyType) {
                    return XmlTypeCode.Item;
                }
                if (datatype == null) {
                    return XmlTypeCode.None;
                }
                return datatype.TypeCode;
            }
        }

        [XmlIgnore]
        internal XmlValueConverter ValueConverter {
            get {
                if (datatype == null) { //Default converter
                    return XmlUntypedConverter.Untyped;
                }
                return datatype.ValueConverter;
            }
        }

        internal XmlReader Validate(XmlReader reader, XmlResolver resolver, XmlSchemaSet schemaSet, ValidationEventHandler valEventHandler) {
            if (schemaSet != null) {
                XmlReaderSettings readerSettings = new XmlReaderSettings();
                readerSettings.ValidationType = ValidationType.Schema;
                readerSettings.Schemas = schemaSet;
                readerSettings.ValidationEventHandler += valEventHandler;
                return new XsdValidatingReader(reader, resolver, readerSettings, this);
            }
            return null;
        }

        internal XmlSchemaContentType SchemaContentType {
            get {
                return contentType;
            }
        }

        internal void SetQualifiedName(XmlQualifiedName value) {
            qname = value;
        }

        internal void SetFinalResolved(XmlSchemaDerivationMethod value) {
             finalResolved = value; 
        }

        internal void SetBaseSchemaType(XmlSchemaType value) { 
            baseSchemaType = value;
        }

        internal void SetDerivedBy(XmlSchemaDerivationMethod value) { 
            derivedBy = value;
        }

        internal void SetDatatype(XmlSchemaDatatype value) { 
            datatype = value;
        }

        internal SchemaElementDecl ElementDecl {
            get { return elementDecl; }
            set { elementDecl = value; }
        }

        [XmlIgnore]
        internal XmlSchemaType Redefined {
            get { return redefined; }
            set { redefined = value; }
        }

        internal virtual XmlQualifiedName DerivedFrom {
            get { return XmlQualifiedName.Empty; }
        }
        
        internal void SetContentType(XmlSchemaContentType value) { 
            contentType = value; 
        }
       
        public static bool IsDerivedFrom(XmlSchemaType derivedType, XmlSchemaType baseType, XmlSchemaDerivationMethod except) {
            if (derivedType == null || baseType == null) {
                return false;
            }

            if (derivedType == baseType) {
                return true;
            }
            
            if (baseType == XmlSchemaComplexType.AnyType) { //Not checking for restriction blocked since all types are implicitly derived by restriction from xs:anyType
                return true;
            }
            do {
                XmlSchemaSimpleType dt = derivedType as XmlSchemaSimpleType;
                XmlSchemaSimpleType bt = baseType as XmlSchemaSimpleType;
                if (bt != null && dt != null) { //SimpleTypes
                    if (bt == DatatypeImplementation.AnySimpleType) { //Not checking block=restriction
                        return true;
                    }
                    if ((except & derivedType.DerivedBy) != 0 || !dt.Datatype.IsDerivedFrom(bt.Datatype)) {
                        return false;
                    }
                    return true;
                }
                else { //Complex types
                    if ((except & derivedType.DerivedBy) != 0) {
                        return false;
                    }
                    derivedType = derivedType.BaseXmlSchemaType;
                    if (derivedType == baseType) {
                        return true;
                    }
                }

            } while(derivedType != null);

            return false;
        }


        internal static bool IsDerivedFromDatatype(XmlSchemaDatatype derivedDataType, XmlSchemaDatatype baseDataType, XmlSchemaDerivationMethod except) {
            if (DatatypeImplementation.AnySimpleType.Datatype == baseDataType) {
                return true;
            }
            return derivedDataType.IsDerivedFrom(baseDataType);
        }

        [XmlIgnore]
        internal override string NameAttribute {
            get { return Name; }
            set { Name = value; }
        }
    }

}

