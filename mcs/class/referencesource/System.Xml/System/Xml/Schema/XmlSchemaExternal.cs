//------------------------------------------------------------------------------
// <copyright file="XmlSchemaExternal.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>                                                                 
//------------------------------------------------------------------------------

namespace System.Xml.Schema {

    using System.Collections;
    using System.ComponentModel;
    using System.Xml.Serialization;

    /// <include file='doc\XmlSchemaExternal.uex' path='docs/doc[@for="XmlSchemaExternal"]/*' />
    public abstract class XmlSchemaExternal : XmlSchemaObject {
        string location;
        Uri    baseUri;
        XmlSchema schema; 
        string id;
        XmlAttribute[] moreAttributes;
        Compositor compositor;

        /// <include file='doc\XmlSchemaExternal.uex' path='docs/doc[@for="XmlSchemaExternal.SchemaLocation"]/*' />
        [XmlAttribute("schemaLocation", DataType="anyURI")]
        public string SchemaLocation {
            get { return location; }
            set { location = value; }
        }

        /// <include file='doc\XmlSchemaExternal.uex' path='docs/doc[@for="XmlSchemaExternal.Schema"]/*' />
        [XmlIgnore]
        public XmlSchema Schema {
            get { return schema; }
            set { schema = value; }
        }

        /// <include file='doc\XmlSchemaExternal.uex' path='docs/doc[@for="XmlSchemaExternal.Id"]/*' />
        [XmlAttribute("id", DataType="ID")]
        public string Id {
            get { return id; }
            set { id = value; }
        }

        /// <include file='doc\XmlSchemaExternal.uex' path='docs/doc[@for="XmlSchemaExternal.UnhandledAttributes"]/*' />
        [XmlAnyAttribute]
        public XmlAttribute[] UnhandledAttributes {
            get { return moreAttributes; }
            set { moreAttributes = value; }
        }

        [XmlIgnore]
        internal Uri BaseUri {
            get { return baseUri; }
            set { baseUri = value; }
        }

        [XmlIgnore]
        internal override string IdAttribute {
            get { return Id; }
            set { Id = value; }
        }

        internal override void SetUnhandledAttributes(XmlAttribute[] moreAttributes) {
            this.moreAttributes = moreAttributes;
        }

        internal Compositor Compositor {
            get {
                return compositor;
            }
            set {
                compositor = value;
            }
        }
    }
}
