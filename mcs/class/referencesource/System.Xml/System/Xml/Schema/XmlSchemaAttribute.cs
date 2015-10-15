//------------------------------------------------------------------------------
// <copyright file="XmlSchemaAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright> 
// <owner current="true" primary="true">Microsoft</owner>                                                               
//------------------------------------------------------------------------------

using System.Collections;
using System.ComponentModel;
using System.Xml.Serialization;

namespace System.Xml.Schema {

    /// <include file='doc\XmlSchemaAttribute.uex' path='docs/doc[@for="XmlSchemaAttribute"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public class XmlSchemaAttribute : XmlSchemaAnnotated {
        string defaultValue;
        string fixedValue;
        string name;

        XmlSchemaForm form = XmlSchemaForm.None;
        XmlSchemaUse use = XmlSchemaUse.None;

        XmlQualifiedName refName = XmlQualifiedName.Empty; 
        XmlQualifiedName typeName = XmlQualifiedName.Empty;
        XmlQualifiedName qualifiedName = XmlQualifiedName.Empty;

        XmlSchemaSimpleType type;
        XmlSchemaSimpleType attributeType;

        SchemaAttDef attDef;
        
        /// <include file='doc\XmlSchemaAttribute.uex' path='docs/doc[@for="XmlSchemaAttribute.DefaultValue"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlAttribute("default")]
        [DefaultValue(null)]
        public string DefaultValue { 
            get { return defaultValue; }
            set { defaultValue = value; }
        }

        /// <include file='doc\XmlSchemaAttribute.uex' path='docs/doc[@for="XmlSchemaAttribute.FixedValue"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlAttribute("fixed")]
        [DefaultValue(null)]
        public string FixedValue { 
            get { return fixedValue; }
            set { fixedValue = value; }
        }

        /// <include file='doc\XmlSchemaAttribute.uex' path='docs/doc[@for="XmlSchemaAttribute.Form"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlAttribute("form"), DefaultValue(XmlSchemaForm.None)]
        public XmlSchemaForm Form { 
            get { return form; }
            set { form = value; }
        }

        /// <include file='doc\XmlSchemaAttribute.uex' path='docs/doc[@for="XmlSchemaAttribute.Name"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlAttribute("name")]
        public string Name { 
            get { return name; }
            set { name = value; }
        }
        
        /// <include file='doc\XmlSchemaAttribute.uex' path='docs/doc[@for="XmlSchemaAttribute.RefName"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlAttribute("ref")]
        public XmlQualifiedName RefName { 
            get { return refName; }
            set { refName = (value == null ? XmlQualifiedName.Empty : value); }
        }
        
        /// <include file='doc\XmlSchemaAttribute.uex' path='docs/doc[@for="XmlSchemaAttribute.SchemaTypeName"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlAttribute("type")]
        public XmlQualifiedName SchemaTypeName { 
            get { return typeName; }
            set { typeName = (value == null ? XmlQualifiedName.Empty : value); }
        }
        
        /// <include file='doc\XmlSchemaAttribute.uex' path='docs/doc[@for="XmlSchemaAttribute.SchemaType"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlElement("simpleType")]
        public XmlSchemaSimpleType SchemaType {
            get { return type; }
            set { type = value; }
        }

        /// <include file='doc\XmlSchemaAttribute.uex' path='docs/doc[@for="XmlSchemaAttribute.Use"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlAttribute("use"), DefaultValue(XmlSchemaUse.None)]
        public XmlSchemaUse Use {
            get { return use; }
            set { use = value; }
        }

        /// <include file='doc\XmlSchemaAttribute.uex' path='docs/doc[@for="XmlSchemaAttribute.QualifiedName"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public XmlQualifiedName QualifiedName { 
            get { return qualifiedName; }
        }

        /// <include file='doc\XmlSchemaAttribute.uex' path='docs/doc[@for="XmlSchemaAttribute.AttributeType"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        [Obsolete("This property has been deprecated. Please use AttributeSchemaType property that returns a strongly typed attribute type. http://go.microsoft.com/fwlink/?linkid=14202")]
        public object AttributeType {
            get {
                if (attributeType == null)
                    return null;

                if (attributeType.QualifiedName.Namespace == XmlReservedNs.NsXs) {
                    return attributeType.Datatype;
                } 
                return attributeType;
            }
        }
        
        /// <include file='doc\XmlSchemaAttribute.uex' path='docs/doc[@for="XmlSchemaAttribute.AttributeSchemaType"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public XmlSchemaSimpleType AttributeSchemaType {
            get { return attributeType; }
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

        [XmlIgnore]
        internal XmlSchemaDatatype Datatype {
            get { 
                if (attributeType != null) {
                    return attributeType.Datatype; 
                }
                return null;
            }
        }

        internal void  SetQualifiedName(XmlQualifiedName value) { 
            qualifiedName = value;
        }

        internal void SetAttributeType(XmlSchemaSimpleType value) { 
            attributeType = value;
        }

        internal SchemaAttDef AttDef {
            get { return attDef; }
            set { attDef = value; }
        }

        internal bool HasDefault {
            get { return defaultValue != null; }
        }

        [XmlIgnore]
        internal override string NameAttribute {
            get { return Name; }
            set { Name = value; }
        }

         internal override XmlSchemaObject Clone() {
            XmlSchemaAttribute newAtt = (XmlSchemaAttribute)MemberwiseClone();

            //Deep clone the QNames as these will be updated on chameleon includes
            newAtt.refName = this.refName.Clone();
            newAtt.typeName = this.typeName.Clone();
            newAtt.qualifiedName = this.qualifiedName.Clone();
            return newAtt;
        }
    }
}
