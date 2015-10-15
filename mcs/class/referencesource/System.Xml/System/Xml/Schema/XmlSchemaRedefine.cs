//------------------------------------------------------------------------------
// <copyright file="XmlSchemaRedefine.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright> 
// <owner current="true" primary="true">Microsoft</owner>                                                                
//------------------------------------------------------------------------------

namespace System.Xml.Schema {

    using System.Xml.Serialization;

    /// <include file='doc\XmlSchemaRedefine.uex' path='docs/doc[@for="XmlSchemaRedefine"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public class XmlSchemaRedefine : XmlSchemaExternal {
        XmlSchemaObjectCollection items = new XmlSchemaObjectCollection();
        XmlSchemaObjectTable attributeGroups = new XmlSchemaObjectTable();
        XmlSchemaObjectTable types = new XmlSchemaObjectTable();
        XmlSchemaObjectTable groups = new XmlSchemaObjectTable();

        
		/// <include file='doc\XmlSchemaRedefine.uex' path='docs/doc[@for="XmlSchemaRedefine.XmlSchemaRedefine"]/*' />
		/// <devdoc>
		///    <para>[To be supplied.]</para>
		/// </devdoc>
        public XmlSchemaRedefine() {
            Compositor = Compositor.Redefine;
        }

        /// <include file='doc\XmlSchemaRedefine.uex' path='docs/doc[@for="XmlSchemaRedefine.Items"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlElement("annotation", typeof(XmlSchemaAnnotation)),
         XmlElement("attributeGroup", typeof(XmlSchemaAttributeGroup)),
         XmlElement("complexType", typeof(XmlSchemaComplexType)),
         XmlElement("group", typeof(XmlSchemaGroup)),
         XmlElement("simpleType", typeof(XmlSchemaSimpleType))]
        public XmlSchemaObjectCollection Items {
            get { return items; }
        }

        /// <include file='doc\XmlSchemaRedefine.uex' path='docs/doc[@for="XmlSchemaRedefine.AttributeGroups"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public XmlSchemaObjectTable AttributeGroups {
            get { return attributeGroups; }
        }

        /// <include file='doc\XmlSchemaRedefine.uex' path='docs/doc[@for="XmlSchemaRedefine.SchemaTypes"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public XmlSchemaObjectTable SchemaTypes {
            get { return types; }
        }

        /// <include file='doc\XmlSchemaRedefine.uex' path='docs/doc[@for="XmlSchemaRedefine.Groups"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public XmlSchemaObjectTable Groups {
            get { return groups; }
        }

        internal override void AddAnnotation(XmlSchemaAnnotation annotation) {
            items.Add(annotation);
        }
    }
}
