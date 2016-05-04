//------------------------------------------------------------------------------
// <copyright file="XmlSchemaInfo.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner> 
//------------------------------------------------------------------------------

using System.Xml;
using System.Collections;

namespace System.Xml.Schema {

    /// <include file='doc\IXmlSchemaInfo.uex' path='docs/doc[@for="IXmlSchemaInfo"]/*' />
    public class XmlSchemaInfo : IXmlSchemaInfo {
        bool isDefault;
        bool isNil;
        XmlSchemaElement schemaElement;
        XmlSchemaAttribute schemaAttribute;
        XmlSchemaType schemaType;
        XmlSchemaSimpleType memberType;
        XmlSchemaValidity validity;
        XmlSchemaContentType contentType;
    
        public XmlSchemaInfo() {
            Clear();
        }

        internal XmlSchemaInfo(XmlSchemaValidity validity) : this() {
            this.validity = validity;
        }

        public XmlSchemaValidity Validity {
            get {
                return validity;                
            }
            set {
                validity = value;
            }
        }

        public bool IsDefault { 
            get {
                return isDefault;
            }
            set {
                isDefault = value;
            }
        }
        
        public bool IsNil { 
            get {
                return isNil;
            }
            set {
                isNil = value;
            }
        }

        public XmlSchemaSimpleType MemberType { 
            get {
                return memberType;
            }
            set {
                memberType = value;
            }
        }

        public XmlSchemaType SchemaType {
            get {
                return schemaType;
            }
            set {
                schemaType = value;
                if (schemaType != null) { //Member type will not change its content type
                    contentType = schemaType.SchemaContentType;
                }
                else {
                    contentType = XmlSchemaContentType.Empty;
                }
            }
        }

        public XmlSchemaElement SchemaElement {
            get {
                return schemaElement;
            }
            set {
                schemaElement = value;
                if (value != null) { //Setting non-null SchemaElement means SchemaAttribute should be null
                    schemaAttribute = null;
                }
            }       
        }

        public XmlSchemaAttribute SchemaAttribute {
            get {
                return schemaAttribute;
            }
            set {
                schemaAttribute = value;
                if (value != null) { //Setting non-null SchemaAttribute means SchemaElement should be null
                    schemaElement = null;
                }
            }   
        }

        public XmlSchemaContentType ContentType {
            get {
                return contentType;
            }
            set {
                contentType = value;
            }
        }

        internal XmlSchemaType XmlType {
            get {
                if (memberType != null) {
                    return memberType;
                }
                return schemaType;
            }
        }

        internal bool HasDefaultValue {
            get {
                return schemaElement != null && schemaElement.ElementDecl.DefaultValueTyped != null;
            }
        }

        internal bool IsUnionType {
            get {
                if (schemaType == null || schemaType.Datatype == null) {
                    return false;
                }
                return schemaType.Datatype.Variety == XmlSchemaDatatypeVariety.Union;
            }
        }

        internal void Clear() {
            isNil = false;
            isDefault = false;
            schemaType = null;
            schemaElement = null;
            schemaAttribute = null;
            memberType = null;
            validity = XmlSchemaValidity.NotKnown;
            contentType = XmlSchemaContentType.Empty;
        }
    }
}
