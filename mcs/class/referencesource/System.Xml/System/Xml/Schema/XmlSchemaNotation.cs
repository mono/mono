//------------------------------------------------------------------------------
// <copyright file="XmlSchemaNotation.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright> 
// <owner current="true" primary="true">[....]</owner>                                                                
//------------------------------------------------------------------------------

namespace System.Xml.Schema {

    using System.Xml.Serialization;

    /// <include file='doc\XmlSchemaNotation.uex' path='docs/doc[@for="XmlSchemaNotation"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public class XmlSchemaNotation : XmlSchemaAnnotated {
        string name;        
        string publicId;
        string systemId;
        XmlQualifiedName qname = XmlQualifiedName.Empty; 
        
        /// <include file='doc\XmlSchemaNotation.uex' path='docs/doc[@for="XmlSchemaNotation.Name"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlAttribute("name")]
        public string Name { 
            get { return name; }
            set { name = value; }
        }

        /// <include file='doc\XmlSchemaNotation.uex' path='docs/doc[@for="XmlSchemaNotation.Public"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlAttribute("public")]
        public string Public {
            get { return publicId; }
            set { publicId = value; }
        }

        /// <include file='doc\XmlSchemaNotation.uex' path='docs/doc[@for="XmlSchemaNotation.System"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlAttribute("system")]
        public string System {
            get { return systemId; }
            set { systemId = value; }
        }

        [XmlIgnore]
        internal XmlQualifiedName QualifiedName {
            get { return qname; }
            set { qname = value; }
        }

        [XmlIgnore]
        internal override string NameAttribute {
            get { return Name; }
            set { Name = value; }
        }
    }
}
