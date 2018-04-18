//------------------------------------------------------------------------------
// <copyright file="XmlName.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Xml {
    using System.Text;
    using System.Diagnostics;
    using System.Xml.Schema;

    internal class XmlName : IXmlSchemaInfo {
        string prefix;
        string localName;
        string ns;
        string name;
        int hashCode;
        internal XmlDocument ownerDoc;
        internal XmlName next;

        public static XmlName Create(string prefix, string localName, string ns, int hashCode, XmlDocument ownerDoc, XmlName next, IXmlSchemaInfo schemaInfo) {
            if (schemaInfo == null) {
                return new XmlName(prefix, localName, ns, hashCode, ownerDoc, next);
            }
            else {
                return new XmlNameEx(prefix, localName, ns, hashCode, ownerDoc, next, schemaInfo);
            }
        }

        internal XmlName(string prefix, string localName, string ns, int hashCode, XmlDocument ownerDoc, XmlName next) {
            this.prefix = prefix;
            this.localName = localName;
            this.ns = ns;
            this.name = null;
            this.hashCode = hashCode;
            this.ownerDoc = ownerDoc;
            this.next = next;
        }

        public string LocalName {
            get { 
                return localName;
            }
        }

        public string NamespaceURI {
            get { 
                return ns;
            }
        }

        public string Prefix {
            get { 
                return prefix;
            }
        }

        public int HashCode {
            get { 
                return hashCode;
            }
        }

        public XmlDocument OwnerDocument {
            get { 
                return ownerDoc;
            }
        }

        public string Name {
            get {
                if ( name == null ) {
                    Debug.Assert( prefix != null );
                    if ( prefix.Length > 0 ) {
                        if ( localName.Length > 0 ) {
                            string n = string.Concat( prefix, ":", localName );
                            lock ( ownerDoc.NameTable ) {
                                if ( name == null ) {
                                    name = ownerDoc.NameTable.Add( n );
                                }
                            }
                        }
                        else {
                            name = prefix;
                        }
                    }
                    else {
                        name = localName;
                    }
                    Debug.Assert( Ref.Equal( name, ownerDoc.NameTable.Get( name ) ) );
                }
                return name;
            }
        }

        public virtual XmlSchemaValidity Validity { 
            get { 
                return XmlSchemaValidity.NotKnown;
            } 
        }

        public virtual bool IsDefault { 
            get { 
                return false; 
            } 
        }

        public virtual bool IsNil { 
            get { 
                return false;
            } 
        }

        public virtual XmlSchemaSimpleType MemberType {
            get { 
                return null; 
            }
        }

        public virtual XmlSchemaType SchemaType {
            get { 
                return null; 
            }
        }

        public virtual XmlSchemaElement SchemaElement {
            get { 
                return null; 
            }
        }

        public virtual XmlSchemaAttribute SchemaAttribute {
            get { 
                return null; 
            }
        }

        public virtual bool Equals(IXmlSchemaInfo schemaInfo) {
            return schemaInfo == null;
        }

        public static int GetHashCode(string name) {
            int hashCode = 0;
            if (name != null) {
                for (int i = name.Length - 1; i >= 0; i--) {
                    char ch = name[i];
                    if (ch == ':') break;
                    hashCode += (hashCode << 7) ^ ch;
                }
                hashCode -= hashCode >> 17;
                hashCode -= hashCode >> 11;
                hashCode -= hashCode >> 5;
            }
            return hashCode;
        }
    }

    internal sealed class XmlNameEx : XmlName {
        byte flags;
        XmlSchemaSimpleType memberType;
        XmlSchemaType schemaType;
        object decl;

        // flags
        // 0,1  : Validity
        // 2    : IsDefault
        // 3    : IsNil
        const byte ValidityMask = 0x03;
        const byte IsDefaultBit = 0x04;
        const byte IsNilBit     = 0x08;

        internal XmlNameEx(string prefix, string localName, string ns, int hashCode, XmlDocument ownerDoc, XmlName next, IXmlSchemaInfo schemaInfo) : base(prefix, localName, ns, hashCode, ownerDoc, next) {
            SetValidity(schemaInfo.Validity);
            SetIsDefault(schemaInfo.IsDefault);
            SetIsNil(schemaInfo.IsNil);
            memberType = schemaInfo.MemberType;
            schemaType = schemaInfo.SchemaType;
            decl = schemaInfo.SchemaElement != null 
                   ? (object)schemaInfo.SchemaElement 
                   : (object)schemaInfo.SchemaAttribute; 
        }

        public override XmlSchemaValidity Validity { 
            get { 
                return ownerDoc.CanReportValidity ? (XmlSchemaValidity)(flags & ValidityMask) : XmlSchemaValidity.NotKnown;
            } 
        }

        public override bool IsDefault { 
            get { 
                return (flags & IsDefaultBit) != 0;
            } 
        }

        public override bool IsNil { 
            get { 
                return (flags & IsNilBit) != 0;
            } 
        }

        public override XmlSchemaSimpleType MemberType {
            get { 
                return memberType; 
            }
        }

        public override XmlSchemaType SchemaType {
            get { 
                return schemaType; 
            }
        }

        public override XmlSchemaElement SchemaElement {
            get { 
                return decl as XmlSchemaElement; 
            }
        }

        public override XmlSchemaAttribute SchemaAttribute {
            get { 
                return decl as XmlSchemaAttribute; 
            }
        }

        public void SetValidity(XmlSchemaValidity value) {
            flags = (byte)((flags & ~ValidityMask) | (byte)(value));
        }

        public void SetIsDefault(bool value) {
            if (value) flags = (byte)(flags | IsDefaultBit);
            else flags = (byte)(flags & ~IsDefaultBit);
        }

        public void SetIsNil(bool value) {
            if (value) flags = (byte)(flags | IsNilBit);
            else flags = (byte)(flags & ~IsNilBit);
        }

        public override bool Equals(IXmlSchemaInfo schemaInfo) {
            if (schemaInfo != null
                && schemaInfo.Validity == (XmlSchemaValidity)(flags & ValidityMask)
                && schemaInfo.IsDefault == ((flags & IsDefaultBit) != 0) 
                && schemaInfo.IsNil == ((flags & IsNilBit) != 0) 
                && (object)schemaInfo.MemberType == (object)memberType 
                && (object)schemaInfo.SchemaType == (object)schemaType
                && (object)schemaInfo.SchemaElement == (object)(decl as XmlSchemaElement) 
                && (object)schemaInfo.SchemaAttribute == (object)(decl as XmlSchemaAttribute)) {
                return true;
            }
            return false;
        }
    }
}
