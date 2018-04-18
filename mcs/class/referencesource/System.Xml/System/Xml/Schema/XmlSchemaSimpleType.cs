//------------------------------------------------------------------------------
// <copyright file="XmlSchemaSimpleType.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>                                                                 
//------------------------------------------------------------------------------

namespace System.Xml.Schema {
    
    using System.Xml.Serialization;
    using System.Diagnostics;

    /// <include file='doc\XmlSchemaSimpleType.uex' path='docs/doc[@for="XmlSchemaSimpleType"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public class XmlSchemaSimpleType : XmlSchemaType {
        XmlSchemaSimpleTypeContent content;
       
        /// <include file='doc\XmlSchemaSimpleType.uex' path='docs/doc[@for="XmlSchemaSimpleType.Content"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        
        public XmlSchemaSimpleType() {
            Debug.Assert(SchemaContentType == XmlSchemaContentType.TextOnly);
        }
        
        /// <include file='doc\XmlSchemaSimpleType.uex' path='docs/doc[@for="XmlSchemaSimpleType.Content1"]/*' />
        [XmlElement("restriction", typeof(XmlSchemaSimpleTypeRestriction)), 
        XmlElement("list", typeof(XmlSchemaSimpleTypeList)),
        XmlElement("union", typeof(XmlSchemaSimpleTypeUnion))]
        public XmlSchemaSimpleTypeContent Content { 
            get { return content; }
            set { content = value; }
        }
        
        internal override XmlQualifiedName DerivedFrom {
            get {
                if (content == null) {
                    // type derived from anyType
                    return XmlQualifiedName.Empty;
                }
                if (content is XmlSchemaSimpleTypeRestriction) {
                    return ((XmlSchemaSimpleTypeRestriction)content).BaseTypeName;
                }
                return XmlQualifiedName.Empty;
            }
        }

        internal override XmlSchemaObject Clone() {
            XmlSchemaSimpleType newSimpleType = (XmlSchemaSimpleType)MemberwiseClone();
            if (content != null) {
                newSimpleType.Content = (XmlSchemaSimpleTypeContent)content.Clone();
            }
            return newSimpleType;
        }
    }
}

