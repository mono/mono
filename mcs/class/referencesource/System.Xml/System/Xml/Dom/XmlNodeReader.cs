//------------------------------------------------------------------------------
// <copyright file="XmlNodeReader.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Xml
{
    using System;
    using System.Text;
    using System.IO;
    using System.Diagnostics;
    using System.Collections;
    using System.Collections.Generic;
    using System.Xml.Schema;
    using System.Globalization;

    internal class XmlNodeReaderNavigator {
            XmlNode     curNode;
            XmlNode     elemNode;
            XmlNode     logNode;
            int         attrIndex;
            int         logAttrIndex;

            //presave these 2 variables since they shouldn't change.
            XmlNameTable    nameTable;
            XmlDocument     doc;

            int     nAttrInd; //used to identify virtual attributes of DocumentType node and XmlDeclaration node

            const String     strPublicID     = "PUBLIC";
            const String     strSystemID     = "SYSTEM";
            const String     strVersion      = "version";
            const String     strStandalone   = "standalone";
            const String     strEncoding     = "encoding";


            //caching variables for perf reasons
            int     nDeclarationAttrCount;
            int     nDocTypeAttrCount;

            //variables for roll back the moves
            int     nLogLevel;
            int     nLogAttrInd;
            bool    bLogOnAttrVal;
            bool    bCreatedOnAttribute;

            internal struct VirtualAttribute {
                internal String name;
                internal String value;

                internal VirtualAttribute(String name, String value) {
                    this.name = name;
                    this.value = value;
                }
            };

            internal VirtualAttribute [] decNodeAttributes = {
                new VirtualAttribute( null, null ),
                new VirtualAttribute( null, null ),
                new VirtualAttribute( null, null )
            };

            internal VirtualAttribute [] docTypeNodeAttributes = {
                new VirtualAttribute( null, null ),
                new VirtualAttribute( null, null )
            };

            bool bOnAttrVal;

            public XmlNodeReaderNavigator( XmlNode node ) {
                curNode = node;
                logNode = node;
                XmlNodeType nt = curNode.NodeType;
                if ( nt == XmlNodeType.Attribute ) {
                    elemNode = null;
                    attrIndex = -1;
                    bCreatedOnAttribute = true;
                }
                else {
                    elemNode = node;
                    attrIndex = -1;
                    bCreatedOnAttribute = false;
                }
                //presave this for pref reason since it shouldn't change.
                if ( nt == XmlNodeType.Document )
                    this.doc = (XmlDocument)curNode;
                else
                    this.doc = node.OwnerDocument;
                this.nameTable = doc.NameTable;
                this.nAttrInd = -1;
                //initialize the caching variables
                this.nDeclarationAttrCount = -1;
                this.nDocTypeAttrCount = -1;
                this.bOnAttrVal = false;
                this.bLogOnAttrVal = false;
            }

            public XmlNodeType NodeType {
                get {
                    XmlNodeType nt = curNode.NodeType;
                    if ( nAttrInd != -1 ) {
                        Debug.Assert( nt == XmlNodeType.XmlDeclaration || nt == XmlNodeType.DocumentType );
                        if ( this.bOnAttrVal )
                            return XmlNodeType.Text;
                        else
                            return XmlNodeType.Attribute;
                    }
                    return nt;
                }
            }

            public String NamespaceURI {
                get { return curNode.NamespaceURI; }
            }

            public String Name {
                get {
                    if ( nAttrInd != -1 ) {
                        Debug.Assert( curNode.NodeType == XmlNodeType.XmlDeclaration || curNode.NodeType == XmlNodeType.DocumentType );
                        if ( this.bOnAttrVal )
                            return String.Empty; //Text node's name is String.Empty
                        else {
                            Debug.Assert( nAttrInd >= 0 && nAttrInd < AttributeCount );
                            if ( curNode.NodeType == XmlNodeType.XmlDeclaration )
                                return decNodeAttributes[nAttrInd].name;
                            else
                                return docTypeNodeAttributes[nAttrInd].name;
                        }
                    }
                    if ( IsLocalNameEmpty ( curNode.NodeType ) )
                        return String.Empty;
                    return curNode.Name;
                }
            }

            public String LocalName {
                get {
                    if ( nAttrInd != -1 )
                        //for the nodes in this case, their LocalName should be the same as their name
                        return Name;
                    if ( IsLocalNameEmpty( curNode.NodeType ))
                        return String.Empty;
                    return curNode.LocalName;
                }
            }

            internal bool IsOnAttrVal {
                get {
                    return this.bOnAttrVal;
                }
            }

            internal XmlNode OwnerElementNode {
                get {
                    if( this.bCreatedOnAttribute )
                        return null;
                    return  this.elemNode;
                }
            }

            internal bool CreatedOnAttribute {
                get {
                    return  this.bCreatedOnAttribute;
                }
            }

            private bool IsLocalNameEmpty ( XmlNodeType nt) {
                switch ( nt ) {
                    case XmlNodeType.None :
                    case XmlNodeType.Text :
                    case XmlNodeType.CDATA :
                    case XmlNodeType.Comment :
                    case XmlNodeType.Document :
                    case XmlNodeType.DocumentFragment :
                    case XmlNodeType.Whitespace :
                    case XmlNodeType.SignificantWhitespace :
                    case XmlNodeType.EndElement :
                    case XmlNodeType.EndEntity :
                        return true;
                    case XmlNodeType.Element :
                    case XmlNodeType.Attribute :
                    case XmlNodeType.EntityReference :
                    case XmlNodeType.Entity :
                    case XmlNodeType.ProcessingInstruction :
                    case XmlNodeType.DocumentType :
                    case XmlNodeType.Notation :
                    case XmlNodeType.XmlDeclaration :
                        return false;
                    default :
                        return true;
                }
            }

            public String Prefix {
                get { return curNode.Prefix; }
            }

            public bool HasValue {
                //In DOM, DocumentType node and XmlDeclaration node doesn't value
                //In XPathNavigator, XmlDeclaration node's value is its InnerText; DocumentType doesn't have value
                //In XmlReader, DocumentType node's value is its InternalSubset which is never null ( at least String.Empty )
                get {
                    if ( nAttrInd != -1 ) {
                        //Pointing at the one of virtual attributes of Declaration or DocumentType nodes
                        Debug.Assert( curNode.NodeType == XmlNodeType.XmlDeclaration || curNode.NodeType == XmlNodeType.DocumentType );
                        Debug.Assert( nAttrInd >= 0 && nAttrInd < AttributeCount );
                        return true;
                    }
                    if ( curNode.Value != null || curNode.NodeType == XmlNodeType.DocumentType )
                        return true;
                    return false;
                }
            }

            public String Value {
                //See comments in HasValue
                get {
                    String retValue = null;
                    XmlNodeType nt = curNode.NodeType;
                    if ( nAttrInd != -1 ) {
                        //Pointing at the one of virtual attributes of Declaration or DocumentType nodes
                        Debug.Assert( nt == XmlNodeType.XmlDeclaration || nt == XmlNodeType.DocumentType );
                        Debug.Assert( nAttrInd >= 0 && nAttrInd < AttributeCount );
                        if ( curNode.NodeType == XmlNodeType.XmlDeclaration )
                            return decNodeAttributes[nAttrInd].value;
                        else
                            return docTypeNodeAttributes[nAttrInd].value;
                    }
                    if ( nt == XmlNodeType.DocumentType )
                        retValue = ((XmlDocumentType)curNode).InternalSubset; //in this case nav.Value will be null
                    else if ( nt == XmlNodeType.XmlDeclaration ) {
                        StringBuilder strb = new StringBuilder(String.Empty);
                        if ( nDeclarationAttrCount == -1 )
                            InitDecAttr();
                        for ( int i = 0; i < nDeclarationAttrCount; i++ ) {
                            strb.Append(decNodeAttributes[i].name + "=\"" +decNodeAttributes[i].value + "\"");
                            if( i != ( nDeclarationAttrCount-1 ) )
                                strb.Append( " " );
                        }
                        retValue = strb.ToString();
                    } else
                        retValue = curNode.Value;
                    return ( retValue == null )? String.Empty : retValue;
                }
            }

            public String BaseURI {
                get { return curNode.BaseURI; }
            }

            public XmlSpace XmlSpace {
                get { return curNode.XmlSpace; }
            }

            public String XmlLang {
                get { return curNode.XmlLang; }
            }

            public bool IsEmptyElement {
                get {
                    if (curNode.NodeType == XmlNodeType.Element) {
                        return((XmlElement)curNode).IsEmpty;
                    }
                    return false;
                }
            }

            public bool IsDefault {
                get {
                    if (curNode.NodeType == XmlNodeType.Attribute) {
                        return !((XmlAttribute)curNode).Specified;
                    }
                    return false;
                }
            }

            public IXmlSchemaInfo SchemaInfo {
                get {
                    return curNode.SchemaInfo;
                }
            }

            public XmlNameTable NameTable {
                get { return nameTable; }
            }

            public int AttributeCount {
                get {
                    if( this.bCreatedOnAttribute )
                        return 0;
                    XmlNodeType nt = curNode.NodeType;
                    if ( nt == XmlNodeType.Element )
                        return ((XmlElement)curNode).Attributes.Count;
                    else if ( nt == XmlNodeType.Attribute
                            || ( this.bOnAttrVal && nt != XmlNodeType.XmlDeclaration && nt != XmlNodeType.DocumentType ) )
                        return elemNode.Attributes.Count;
                    else if ( nt == XmlNodeType.XmlDeclaration ) {
                        if ( nDeclarationAttrCount != -1 )
                            return nDeclarationAttrCount;
                        InitDecAttr();
                        return nDeclarationAttrCount;
                    } else if ( nt == XmlNodeType.DocumentType ) {
                        if ( nDocTypeAttrCount != -1 )
                            return nDocTypeAttrCount;
                        InitDocTypeAttr();
                        return nDocTypeAttrCount;
                    }
                    return 0;
                }
            }

            private void CheckIndexCondition(int attributeIndex) {
                if (attributeIndex < 0 || attributeIndex >= AttributeCount) {
                    throw new ArgumentOutOfRangeException( "attributeIndex" );
                }
            }

            //8 functions below are the helper functions to deal with virtual attributes of XmlDeclaration nodes and DocumentType nodes.
            private void InitDecAttr() {
                int i = 0;
                String strTemp = doc.Version;
                if ( strTemp != null && strTemp.Length != 0 ) {
                    decNodeAttributes[i].name = strVersion;
                    decNodeAttributes[i].value = strTemp;
                    i++;
                }
                strTemp = doc.Encoding;
                if ( strTemp != null && strTemp.Length != 0 ) {
                    decNodeAttributes[i].name = strEncoding;
                    decNodeAttributes[i].value = strTemp;
                    i++;
                }
                strTemp = doc.Standalone;
                if ( strTemp != null && strTemp.Length != 0 ) {
                    decNodeAttributes[i].name = strStandalone;
                    decNodeAttributes[i].value = strTemp;
                    i++;
                }
                nDeclarationAttrCount = i;
            }

            public String GetDeclarationAttr( XmlDeclaration decl, String name ) {
                //PreCondition: curNode is pointing at Declaration node or one of its virtual attributes
                if ( name == strVersion )
                    return decl.Version;
                if ( name == strEncoding )
                    return decl.Encoding;
                if ( name == strStandalone )
                    return decl.Standalone;
                return null;
            }

            public String GetDeclarationAttr( int i ) {
                if ( nDeclarationAttrCount == -1 )
                    InitDecAttr();
                return decNodeAttributes[i].value;
            }

            public int GetDecAttrInd( String name ) {
                if ( nDeclarationAttrCount == -1 )
                    InitDecAttr();
                for ( int i = 0 ; i < nDeclarationAttrCount; i++ ) {
                    if ( decNodeAttributes[i].name == name )
                        return i;
                }
                return -1;
            }

            private void InitDocTypeAttr() {
                int i = 0;
                XmlDocumentType docType = doc.DocumentType;
                if ( docType == null ) {
                    nDocTypeAttrCount = 0;
                    return;
                }
                String strTemp = docType.PublicId;
                if ( strTemp != null ) {
                    docTypeNodeAttributes[i].name = strPublicID;
                    docTypeNodeAttributes[i].value = strTemp;
                    i++;
                }
                strTemp = docType.SystemId;
                if ( strTemp != null ) {
                    docTypeNodeAttributes[i].name = strSystemID;
                    docTypeNodeAttributes[i].value = strTemp;
                    i++;
                }
                nDocTypeAttrCount = i;
            }

            public String GetDocumentTypeAttr ( XmlDocumentType docType, String name ) {
                //PreCondition: nav is pointing at DocumentType node or one of its virtual attributes
                if ( name == strPublicID )
                    return docType.PublicId;
                if ( name == strSystemID )
                    return docType.SystemId;
                return null;
            }

            public String GetDocumentTypeAttr( int i ) {
                if ( nDocTypeAttrCount == -1 )
                    InitDocTypeAttr();
                return docTypeNodeAttributes[i].value;
            }

            public int GetDocTypeAttrInd( String name ) {
                if ( nDocTypeAttrCount == -1 )
                    InitDocTypeAttr();
                for ( int i = 0 ; i < nDocTypeAttrCount; i++ ) {
                    if ( docTypeNodeAttributes[i].name == name )
                        return i;
                }
                return -1;
            }

            private String GetAttributeFromElement( XmlElement elem, String name ) {
                XmlAttribute attr = elem.GetAttributeNode( name );
                if ( attr != null )
                    return attr.Value;
                return null;
            }

            public String GetAttribute( String name ) {
                if( this.bCreatedOnAttribute )
                    return null;
                switch ( curNode.NodeType ) {
                    case XmlNodeType.Element:
                        return GetAttributeFromElement((XmlElement)curNode, name);
                    case XmlNodeType.Attribute :
                        return GetAttributeFromElement((XmlElement)elemNode, name);
                    case XmlNodeType.XmlDeclaration:
                        return GetDeclarationAttr( (XmlDeclaration)curNode, name );
                    case XmlNodeType.DocumentType:
                        return GetDocumentTypeAttr( (XmlDocumentType)curNode, name );
                }
                return null;
            }

            private String GetAttributeFromElement( XmlElement elem, String name, String ns ) {
                XmlAttribute attr = elem.GetAttributeNode( name, ns );
                if ( attr != null )
                    return attr.Value;
                return null;
            }
            public String GetAttribute( String name, String ns ) {
                if( this.bCreatedOnAttribute )
                    return null;
                switch ( curNode.NodeType ) {
                    case XmlNodeType.Element:
                        return GetAttributeFromElement((XmlElement)curNode, name, ns);
                    case XmlNodeType.Attribute :
                        return GetAttributeFromElement((XmlElement)elemNode, name, ns);
                    case XmlNodeType.XmlDeclaration:
                        return (ns.Length == 0) ? GetDeclarationAttr( (XmlDeclaration)curNode, name ) : null;
                    case XmlNodeType.DocumentType:
                        return (ns.Length == 0) ? GetDocumentTypeAttr( (XmlDocumentType)curNode, name ) : null;
                }
                return null;
            }

            public String GetAttribute( int attributeIndex ) {
                if( this.bCreatedOnAttribute )
                    return null;
                switch ( curNode.NodeType ) {
                    case XmlNodeType.Element:
                        CheckIndexCondition( attributeIndex );
                        return ((XmlElement)curNode).Attributes[attributeIndex].Value;
                    case XmlNodeType.Attribute :
                        CheckIndexCondition( attributeIndex );
                        return ((XmlElement)elemNode).Attributes[attributeIndex].Value;
                    case XmlNodeType.XmlDeclaration: {
                        CheckIndexCondition( attributeIndex );
                        return GetDeclarationAttr( attributeIndex );
                    }
                    case XmlNodeType.DocumentType: {
                        CheckIndexCondition( attributeIndex );
                        return GetDocumentTypeAttr( attributeIndex );
                    }
                }
                throw new ArgumentOutOfRangeException( "attributeIndex" ); //for other senario, AttributeCount is 0, i has to be out of range
            }

            public void LogMove( int level ) {
                logNode = curNode;
                nLogLevel = level;
                nLogAttrInd = nAttrInd;
                logAttrIndex = attrIndex;
                this.bLogOnAttrVal = this.bOnAttrVal;
            }

            //The function has to be used in pair with ResetMove when the operation fails after LogMove() is
            //    called because it relies on the values of nOrigLevel, logNav and nOrigAttrInd to be acurate.
            public void RollBackMove( ref int level ) {
                curNode = logNode;
                level = nLogLevel;
                nAttrInd = nLogAttrInd;
                attrIndex = logAttrIndex;
                this.bOnAttrVal = this.bLogOnAttrVal;
             }

            private bool IsOnDeclOrDocType {
                get {
                    XmlNodeType nt = curNode.NodeType;
                    return ( nt == XmlNodeType.XmlDeclaration || nt == XmlNodeType.DocumentType );
                }
            }

            public void ResetToAttribute( ref int level ) {
                //the current cursor is pointing at one of the attribute children -- this could be caused by
                //  the calls to ReadAttributeValue(..)
                if( this.bCreatedOnAttribute )
                    return;
                if ( this.bOnAttrVal ) {
                    if ( IsOnDeclOrDocType ) {
                        level-=2;
                    } else {
                        while ( curNode.NodeType != XmlNodeType.Attribute && ( ( curNode = curNode.ParentNode ) != null ) )
                            level-- ;
                    }
                    this.bOnAttrVal = false;
                }
            }

            public void ResetMove( ref int level, ref XmlNodeType nt ) {
                LogMove( level );
                if( this.bCreatedOnAttribute )
                    return;
                if ( nAttrInd != -1 ) {
                    Debug.Assert( IsOnDeclOrDocType );
                    if ( this.bOnAttrVal ) {
                        level--;
                        this.bOnAttrVal = false;
                    }
                    nLogAttrInd = nAttrInd;
                    level--;
                    nAttrInd = -1;
                    nt = curNode.NodeType;
                    return;
                }
                if ( this.bOnAttrVal && curNode.NodeType != XmlNodeType.Attribute )
                    ResetToAttribute( ref level );
                if ( curNode.NodeType == XmlNodeType.Attribute ) {
                    curNode = ((XmlAttribute)curNode).OwnerElement;
                    attrIndex = -1;
                    level--;
                    nt = XmlNodeType.Element;
                }
                if ( curNode.NodeType == XmlNodeType.Element )
                    elemNode = curNode;
            }

            public bool MoveToAttribute( string name ) {
                return MoveToAttribute( name, string.Empty );
            }
            private bool MoveToAttributeFromElement( XmlElement elem, String name, String ns ) {
                XmlAttribute attr = null;
                if( ns.Length == 0 )
                    attr = elem.GetAttributeNode( name );
                else
                    attr = elem.GetAttributeNode( name, ns );
                if ( attr != null ) {
                    this.bOnAttrVal = false;
                    elemNode = elem;
                    curNode = attr;
                    attrIndex = elem.Attributes.FindNodeOffsetNS(attr);
                    if (attrIndex != -1) {
                        return true;
                    }
                }
                return false;
            }

            public bool MoveToAttribute( string name, string namespaceURI ) {
                if( this.bCreatedOnAttribute )
                    return false;
                XmlNodeType nt = curNode.NodeType;
                if ( nt == XmlNodeType.Element )
                    return MoveToAttributeFromElement((XmlElement)curNode, name, namespaceURI );
                else if ( nt == XmlNodeType.Attribute )
                    return MoveToAttributeFromElement((XmlElement)elemNode, name, namespaceURI );
                else if (  nt == XmlNodeType.XmlDeclaration && namespaceURI.Length == 0 ) {
                    if ( ( nAttrInd = GetDecAttrInd( name ) ) != -1 ) {
                        this.bOnAttrVal = false;
                        return true;
                    }
                } else if ( nt == XmlNodeType.DocumentType && namespaceURI.Length == 0 ) {
                    if ( ( nAttrInd = GetDocTypeAttrInd( name ) ) != -1 ) {
                        this.bOnAttrVal = false;
                        return true;
                    }
                }
                return false;
            }

            public void MoveToAttribute( int attributeIndex ) {
                if( this.bCreatedOnAttribute )
                    return;
                XmlAttribute attr = null;
                switch ( curNode.NodeType ) {
                    case XmlNodeType.Element:
                        CheckIndexCondition( attributeIndex );
                        attr = ((XmlElement)curNode).Attributes[attributeIndex];
                        if ( attr != null ) {
                            elemNode = curNode;
                            curNode = (XmlNode) attr;
                            attrIndex = attributeIndex;
                        }
                        break;
                    case XmlNodeType.Attribute:
                        CheckIndexCondition( attributeIndex );
                        attr = ((XmlElement)elemNode).Attributes[attributeIndex];
                        if ( attr != null ) {
                            curNode = (XmlNode) attr;
                            attrIndex = attributeIndex;
                        }
                        break;
                    case XmlNodeType.XmlDeclaration :
                    case XmlNodeType.DocumentType :
                        CheckIndexCondition( attributeIndex );
                        nAttrInd = attributeIndex;
                        break;
                }
            }

            public bool MoveToNextAttribute( ref int level ) {
                if( this.bCreatedOnAttribute )
                        return false;
                XmlNodeType nt = curNode.NodeType;
                if ( nt == XmlNodeType.Attribute ) {
                    if( attrIndex >= ( elemNode.Attributes.Count-1 ) )
                        return false;
                    else {
                        curNode = elemNode.Attributes[++attrIndex];
                        return true;
                    }
                } else if ( nt == XmlNodeType.Element ) {
                    if ( curNode.Attributes.Count > 0 ) {
                        level++;
                        elemNode = curNode;
                        curNode = curNode.Attributes[0];
                        attrIndex = 0;
                        return true;
                    }
                } else if ( nt == XmlNodeType.XmlDeclaration ) {
                    if ( nDeclarationAttrCount == -1 )
                        InitDecAttr();
                    nAttrInd++;
                    if ( nAttrInd < nDeclarationAttrCount ) {
                        if ( nAttrInd == 0 ) level++;
                        this.bOnAttrVal = false;
                        return true;
                    }
                    nAttrInd--;
                } else if ( nt == XmlNodeType.DocumentType ) {
                    if ( nDocTypeAttrCount == -1 )
                        InitDocTypeAttr();
                    nAttrInd++;
                    if ( nAttrInd < nDocTypeAttrCount ) {
                        if ( nAttrInd == 0 ) level++;
                        this.bOnAttrVal = false;
                        return true;
                    }
                    nAttrInd--;
                }
                return false;
            }

            public bool MoveToParent() {
                XmlNode parent = curNode.ParentNode;
                if ( parent != null ) {
                    curNode = parent;
                    if( !bOnAttrVal )
                        attrIndex = 0;
                    return true;
                }
                return false;
            }

            public bool MoveToFirstChild() {
                XmlNode firstChild = curNode.FirstChild;
                if ( firstChild != null ) {
                    curNode = firstChild;
                    if( !bOnAttrVal )
                        attrIndex = -1;
                    return true;
                }
                return false;
            }

            private bool MoveToNextSibling( XmlNode node ) {
                XmlNode nextSibling = node.NextSibling;
                if ( nextSibling != null ) {
                    curNode = nextSibling;
                    if( !bOnAttrVal )
                        attrIndex = -1;
                    return true;
                }
                return false;
            }

            public bool MoveToNext() {
                if ( curNode.NodeType != XmlNodeType.Attribute )
                    return MoveToNextSibling( curNode );
                else
                    return MoveToNextSibling( elemNode );
            }

            public bool MoveToElement() {
                if( this.bCreatedOnAttribute )
                    return false;
                switch ( curNode.NodeType ) {
                    case XmlNodeType.Attribute :
                        if ( elemNode != null ) {
                            curNode = elemNode;
                            attrIndex = -1;
                            return true;
                        }
                        break;
                    case XmlNodeType.XmlDeclaration :
                    case XmlNodeType.DocumentType : {
                        if ( nAttrInd != -1 ) {
                            nAttrInd = -1;
                            return true;
                        }
                        break;
                    }
                }
                return false;
            }

            public String LookupNamespace(string prefix) {
                if( this.bCreatedOnAttribute )
                    return null;
                if ( prefix == "xmlns" ) {
                    return nameTable.Add( XmlReservedNs.NsXmlNs );
                }
                if ( prefix == "xml" ) {
                    return nameTable.Add( XmlReservedNs.NsXml );
                }

                // construct the name of the xmlns attribute
                string attrName;
                if ( prefix == null )
                    prefix = string.Empty;
                if ( prefix.Length == 0 )
                    attrName = "xmlns";
                else
                    attrName = "xmlns:" + prefix;

                // walk up the XmlNode parent chain, looking for the xmlns attribute
                XmlNode node = curNode;
                while ( node != null ) {
                    if ( node.NodeType == XmlNodeType.Element ) {
                        XmlElement elem = (XmlElement)node;
                        if ( elem.HasAttributes ) {
                            XmlAttribute attr = elem.GetAttributeNode( attrName );
                            if ( attr != null ) {
                                return attr.Value;
                            }
                        }
                    }
                    else if ( node.NodeType == XmlNodeType.Attribute ) {
                        node = ((XmlAttribute)node).OwnerElement;
                        continue;
                    }
                    node = node.ParentNode;
                }
                if ( prefix.Length == 0 ) {
                    return string.Empty;
                }
                return null;
            }

            internal string DefaultLookupNamespace( string prefix ) {
                if( !this.bCreatedOnAttribute ) {
                    if ( prefix == "xmlns" ) {
                        return nameTable.Add( XmlReservedNs.NsXmlNs );
                    }
                    if ( prefix == "xml" ) {
                        return nameTable.Add( XmlReservedNs.NsXml );
                    }
                    if ( prefix == string.Empty ) {
                        return nameTable.Add( string.Empty );
                    }
                }
                return null;
            }

            internal String LookupPrefix(string namespaceName) {
                if( this.bCreatedOnAttribute || namespaceName == null ) {
                    return null;
                }
                if ( namespaceName == XmlReservedNs.NsXmlNs ) {
                    return nameTable.Add( "xmlns" );
                }
                if ( namespaceName == XmlReservedNs.NsXml ) {
                    return nameTable.Add( "xml" );
                }
                if ( namespaceName == string.Empty ) {
                    return string.Empty;
                }
                // walk up the XmlNode parent chain, looking for the xmlns attribute with namespaceName value
                XmlNode node = curNode;
                while ( node != null ) {
                    if ( node.NodeType == XmlNodeType.Element ) {
                        XmlElement elem = (XmlElement)node;
                        if ( elem.HasAttributes ) {
                            XmlAttributeCollection attrs = elem.Attributes;
                            for ( int i = 0; i < attrs.Count; i++ ) {
                                XmlAttribute a = attrs[i];
                                if ( a.Value == namespaceName ) {
                                   if ( a.Prefix.Length == 0 && a.LocalName == "xmlns" ) {
                                       if ( LookupNamespace( string.Empty ) == namespaceName ) {
                                           return string.Empty;
                                       }
                                   }
                                   else if ( a.Prefix == "xmlns" ) {
                                       string pref = a.LocalName;
                                       if ( LookupNamespace( pref ) == namespaceName ) {
                                           return nameTable.Add( pref );
                                       }
                                   }
                                }
                            }
                        }
                    }
                    else if ( node.NodeType == XmlNodeType.Attribute ) {
                        node = ((XmlAttribute)node).OwnerElement;
                        continue;
                    }
                    node = node.ParentNode;
                }
                return null;
            }

            internal IDictionary<string,string> GetNamespacesInScope( XmlNamespaceScope scope ) {
                Dictionary<string,string> dict = new Dictionary<string, string>();
                if( this.bCreatedOnAttribute )
                    return dict;

                // walk up the XmlNode parent chain and add all namespace declarations to the dictionary
                XmlNode node = curNode;
                while ( node != null ) {
                    if ( node.NodeType == XmlNodeType.Element ) {
                        XmlElement elem = (XmlElement)node;
                        if ( elem.HasAttributes ) {
                            XmlAttributeCollection attrs = elem.Attributes;
                            for ( int i = 0; i < attrs.Count; i++ ) {
                                XmlAttribute a = attrs[i];
                                if ( a.LocalName == "xmlns" && a.Prefix.Length == 0 ) {
                                    if  ( !dict.ContainsKey( string.Empty ) ) {
                                        dict.Add( nameTable.Add( string.Empty ), nameTable.Add( a.Value ) );
                                    }
                                }
                                else if ( a.Prefix == "xmlns" ) {
                                    string localName = a.LocalName;
                                    if ( !dict.ContainsKey( localName ) ) {
                                        dict.Add( nameTable.Add( localName ), nameTable.Add( a.Value ) );
                                    }
                                }
                            }
                        }
                        if ( scope == XmlNamespaceScope.Local ) {
                            break;
                        }
                    }
                    else if ( node.NodeType == XmlNodeType.Attribute ) {
                        node = ((XmlAttribute)node).OwnerElement;
                        continue;
                    }
                    node = node.ParentNode;
                };

                if ( scope != XmlNamespaceScope.Local ) {
                    if ( dict.ContainsKey( string.Empty ) && dict[string.Empty] == string.Empty ) {
                        dict.Remove( string.Empty );
                    }
                    if ( scope == XmlNamespaceScope.All ) {
                        dict.Add( nameTable.Add( "xml" ), nameTable.Add( XmlReservedNs.NsXml ) );
                    }
                }
                return dict;
            }

            public bool ReadAttributeValue( ref int level, ref bool bResolveEntity, ref XmlNodeType nt ) {
                if ( nAttrInd != -1 ) {
                    Debug.Assert( curNode.NodeType == XmlNodeType.XmlDeclaration || curNode.NodeType == XmlNodeType.DocumentType );
                    if ( !this.bOnAttrVal ) {
                        this.bOnAttrVal = true;
                        level++;
                        nt = XmlNodeType.Text;
                        return true;
                    }
                    return false;
                }
                if( curNode.NodeType == XmlNodeType.Attribute ) {
                    XmlNode firstChild = curNode.FirstChild;
                    if ( firstChild != null ) {
                        curNode = firstChild;
                        nt = curNode.NodeType;
                        level++;
                        this.bOnAttrVal = true;
                        return true;
                    }
                }
                else if ( this.bOnAttrVal ) {
                    XmlNode nextSibling = null;
                    if ( curNode.NodeType == XmlNodeType.EntityReference && bResolveEntity ) {
                        //going down to ent ref node
                        curNode = curNode.FirstChild;
                        nt = curNode.NodeType;
                        Debug.Assert( curNode != null );
                        level++;
                        bResolveEntity = false;
                        return true;
                    }
                    else
                        nextSibling = curNode.NextSibling;
                    if ( nextSibling == null ) {
                        XmlNode parentNode = curNode.ParentNode;
                        //Check if its parent is entity ref node is sufficient, because in this senario, ent ref node can't have more than 1 level of children that are not other ent ref nodes
                        if ( parentNode != null && parentNode.NodeType == XmlNodeType.EntityReference ) {
                            //come back from ent ref node
                            curNode = parentNode;
                            nt = XmlNodeType.EndEntity;
                            level--;
                            return true;
                        }
                    }
                    if ( nextSibling != null ) {
                        curNode = nextSibling;
                        nt = curNode.NodeType;
                        return true;
                    }
                    else
                        return false;
                }
                return false;
            }

            public XmlDocument Document {
                get {
                    return this.doc;
                }
            }
    }

    // Represents a reader that provides fast, non-cached forward only stream access
    // to XML data in an XmlDocument or a specific XmlNode within an XmlDocument.
    public class XmlNodeReader: XmlReader, IXmlNamespaceResolver
    {
        XmlNodeReaderNavigator  readerNav;

        XmlNodeType             nodeType;   // nodeType of the node that the reader is currently positioned on
        int                     curDepth;   // depth of attrNav ( also functions as reader's depth )
        ReadState               readState;  // current reader's state
        bool                    fEOF;       // flag to show if reaches the end of file
        //mark to the state that EntityReference node is supposed to be resolved
        bool                    bResolveEntity;
        bool                    bStartFromDocument;

        bool                        bInReadBinary;
        ReadContentAsBinaryHelper   readBinaryHelper;


        // Creates an instance of the XmlNodeReader class using the specified XmlNode.
        public XmlNodeReader ( XmlNode node ) {
            if (node == null) {
                throw new ArgumentNullException("node");
            }
            readerNav = new XmlNodeReaderNavigator( node );
            this.curDepth = 0;

            readState = ReadState.Initial;
            fEOF = false;
            nodeType = XmlNodeType.None;
            bResolveEntity = false;
            bStartFromDocument = false;
        }

        //function returns if the reader currently in valid reading states
        internal bool IsInReadingStates() {
            return ( readState == ReadState.Interactive ); // || readState == ReadState.EndOfFile
        }

        //
        // Node Properties
        //

        // Gets the type of the current node.
        public override XmlNodeType NodeType {
            get { return ( IsInReadingStates() )? nodeType : XmlNodeType.None; }
        }

        // Gets the name of
        // the current node, including the namespace prefix.
        public override string Name {
            get {
                if ( !IsInReadingStates() )
                    return String.Empty;
                return readerNav.Name;
            }
        }

        // Gets the name of the current node without the namespace prefix.
        public override string LocalName {
            get {
                if ( !IsInReadingStates() )
                    return String.Empty;
                return readerNav.LocalName;
            }
        }

        // Gets the namespace URN (as defined in the W3C Namespace Specification)
        // of the current namespace scope.
        public override string NamespaceURI {
            get {
                if ( !IsInReadingStates() )
                    return String.Empty;
                return readerNav.NamespaceURI;
            }
        }

        // Gets the namespace prefix associated with the current node.
        public override string Prefix {
            get {
                if ( !IsInReadingStates() )
                    return String.Empty;
                return readerNav.Prefix;
            }
        }

        // Gets a value indicating whether
        // XmlNodeReader.Value has a value to return.
        public override bool HasValue {
            get {
                if ( !IsInReadingStates() )
                    return false;
                return readerNav.HasValue;
            }
        }

        // Gets the text value of the current node.
        public override string Value {
            get {
                if ( !IsInReadingStates() )
                    return String.Empty;
                return readerNav.Value;
            }
        }

        // Gets the depth of the
        // current node in the XML element stack.
        public override int Depth {
            get { return curDepth; }
        }

        // Gets the base URI of the current node.
        public override String BaseURI {
            get { return readerNav.BaseURI; }
        }

        public override bool CanResolveEntity {
            get { return true; }
        }

        // Gets a value indicating whether the current
        // node is an empty element (for example, <MyElement/>.
        public override bool IsEmptyElement {
            get {
                if ( !IsInReadingStates() )
                    return false;
                return readerNav.IsEmptyElement;
            }
        }

        // Gets a value indicating whether the current node is an
        // attribute that was generated from the default value defined
        // in the DTD or schema.
        public override bool IsDefault {
            get {
                if ( !IsInReadingStates() )
                    return false;
                return readerNav.IsDefault;
            }
        }

        // Gets the current xml:space scope.
        public override XmlSpace XmlSpace {
            get {
                if ( !IsInReadingStates() )
                    return XmlSpace.None;
                return readerNav.XmlSpace;
            }
        }

        // Gets the current xml:lang scope.
        public override string XmlLang {
            // Assume everything is in Unicode
            get {
                if ( !IsInReadingStates() )
                    return String.Empty;
                return readerNav.XmlLang;
            }
        }

        public override IXmlSchemaInfo SchemaInfo {
            get {
                if (!IsInReadingStates()) {
                    return null;
                }
                return readerNav.SchemaInfo;
            }
        }

        //
        // Attribute Accessors
        //

        // Gets the number of attributes on the current node.
        public override int AttributeCount {
            get {
                if ( !IsInReadingStates() || nodeType == XmlNodeType.EndElement )
                    return 0;
                return readerNav.AttributeCount;
            }
        }

        // Gets the value of the attribute with the specified name.
        public override string GetAttribute(string name) {
            //if not on Attribute, only element node could have attributes
            if ( !IsInReadingStates() )
                return null;
            return readerNav.GetAttribute( name );
        }

        // Gets the value of the attribute with the specified name and namespace.
        public override string GetAttribute(string name, string namespaceURI) {
            //if not on Attribute, only element node could have attributes
            if ( !IsInReadingStates() )
                return null;
            String ns = ( namespaceURI == null ) ? String.Empty : namespaceURI;
            return readerNav.GetAttribute( name, ns );
        }

        // Gets the value of the attribute with the specified index.
        public override string GetAttribute(int attributeIndex) {
            if ( !IsInReadingStates() )
                throw new ArgumentOutOfRangeException( "attributeIndex" );
            //CheckIndexCondition( i );
            //Debug.Assert( nav.NodeType == XmlNodeType.Element );
            return readerNav.GetAttribute( attributeIndex );
        }

        // Moves to the attribute with the specified name.
        public override bool MoveToAttribute(string name) {
            if ( !IsInReadingStates() )
                return false;
            readerNav.ResetMove( ref curDepth, ref nodeType );
            if ( readerNav.MoveToAttribute( name ) ) { //, ref curDepth ) ) {
                curDepth++;
                nodeType = readerNav.NodeType;
                if ( bInReadBinary ) {
                    FinishReadBinary();
                }
                return true;
            }
            readerNav.RollBackMove(ref curDepth);
            return false;
        }

        // Moves to the attribute with the specified name and namespace.
        public override bool MoveToAttribute(string name, string namespaceURI) {
            if ( !IsInReadingStates() )
                return false;
            readerNav.ResetMove( ref curDepth, ref nodeType );
            String ns = ( namespaceURI == null ) ? String.Empty : namespaceURI;
            if ( readerNav.MoveToAttribute( name,  ns ) ) { //, ref curDepth ) ) {
                curDepth++;
                nodeType = readerNav.NodeType;
                if ( bInReadBinary ) {
                    FinishReadBinary();
                }
                return true;
            }
            readerNav.RollBackMove(ref curDepth);
            return false;
        }

        // Moves to the attribute with the specified index.
        public override void MoveToAttribute(int attributeIndex) {
            if ( !IsInReadingStates() )
                throw new ArgumentOutOfRangeException( "attributeIndex" );
            readerNav.ResetMove( ref curDepth, ref nodeType );
            try {
                if (AttributeCount > 0) {
                    readerNav.MoveToAttribute( attributeIndex );
                    if ( bInReadBinary ) {
                        FinishReadBinary();
                    }
                }
                else
                throw new ArgumentOutOfRangeException( "attributeIndex" );
            } catch {
                readerNav.RollBackMove(ref curDepth);
                throw;
            }
            curDepth++;
            nodeType = readerNav.NodeType;
        }

        // Moves to the first attribute.
        public override bool MoveToFirstAttribute() {
            if ( !IsInReadingStates() )
                return false;
            readerNav.ResetMove( ref curDepth, ref nodeType );
            if (AttributeCount > 0) {
                readerNav.MoveToAttribute( 0 );
                curDepth++;
                nodeType = readerNav.NodeType;
                if ( bInReadBinary ) {
                    FinishReadBinary();
                }
                return true;
            }
            readerNav.RollBackMove( ref curDepth );
            return false;
        }

        // Moves to the next attribute.
        public override bool MoveToNextAttribute() {
            if ( !IsInReadingStates() || nodeType == XmlNodeType.EndElement )
                return false;
            readerNav.LogMove( curDepth );
            readerNav.ResetToAttribute( ref curDepth );
            if ( readerNav.MoveToNextAttribute( ref curDepth ) ) {
                nodeType = readerNav.NodeType;
                if ( bInReadBinary ) {
                    FinishReadBinary();
                }
                return true;
            }
            readerNav.RollBackMove( ref curDepth );
            return false;
        }

        // Moves to the element that contains the current attribute node.
        public override bool MoveToElement() {
            if ( !IsInReadingStates() )
                return false;
            readerNav.LogMove( curDepth );
            readerNav.ResetToAttribute( ref curDepth );
            if ( readerNav.MoveToElement() ) {
                curDepth--;
                nodeType = readerNav.NodeType;
                if ( bInReadBinary ) {
                    FinishReadBinary();
                }
                return true;
            }
            readerNav.RollBackMove( ref curDepth );
            return false;
        }

        //
        // Moving through the Stream
        //

        // Reads the next node from the stream.
        public override bool Read() {
            return Read( false );
        }
        private bool Read( bool fSkipChildren ) {
            if( fEOF )
                return false;

            if ( readState == ReadState.Initial ) {
                // if nav is pointing at the document node, start with its children
                // otherwise,start with the node.
                if ( ( readerNav.NodeType == XmlNodeType.Document ) || ( readerNav.NodeType == XmlNodeType.DocumentFragment ) ) {
                    bStartFromDocument = true;
                    if ( !ReadNextNode(fSkipChildren) ) {
                        readState = ReadState.Error;
                        return false;
                    }
                }
                ReSetReadingMarks();
                readState = ReadState.Interactive;
                nodeType = readerNav.NodeType;
                //_depth = 0;
                curDepth = 0;
                return true;
            }

            if ( bInReadBinary ) {
                FinishReadBinary();
            }

            bool bRead = false;
            if( ( readerNav.CreatedOnAttribute ) )
                return false;
            ReSetReadingMarks();
            bRead = ReadNextNode(fSkipChildren);
            if ( bRead ) {
                return true;
            } else {
                if ( readState == ReadState.Initial || readState == ReadState.Interactive )
                    readState = ReadState.Error;
                if ( readState == ReadState.EndOfFile )
                    nodeType = XmlNodeType.None;
                return false;
            }
        }

        private bool ReadNextNode( bool fSkipChildren ) {
            if ( readState != ReadState.Interactive && readState != ReadState.Initial ) {
                nodeType = XmlNodeType.None;
                return false;
            }

            bool bDrillDown = !fSkipChildren;
            XmlNodeType nt = readerNav.NodeType;
            //only goes down when nav.NodeType is of element or of document at the initial state, other nav.NodeType will not be parsed down
            //if nav.NodeType is of EntityReference, ResolveEntity() could be called to get the content parsed;
            bDrillDown = bDrillDown
                        && ( nodeType != XmlNodeType.EndElement )
                        && ( nodeType != XmlNodeType.EndEntity )
                        && ( nt == XmlNodeType.Element || ( nt == XmlNodeType.EntityReference && bResolveEntity ) ||
                            ( ( ( readerNav.NodeType == XmlNodeType.Document ) || ( readerNav.NodeType == XmlNodeType.DocumentFragment ) ) && readState == ReadState.Initial) );
            //first see if there are children of current node, so to move down
            if ( bDrillDown ) {
                if ( readerNav.MoveToFirstChild() ) {
                    nodeType = readerNav.NodeType;
                    curDepth++;
                    if ( bResolveEntity )
                        bResolveEntity = false;
                    return true;
                } else if ( readerNav.NodeType == XmlNodeType.Element
                            && !readerNav.IsEmptyElement ) {
                    nodeType = XmlNodeType.EndElement;
                    return true;
                }
                else if (readerNav.NodeType == XmlNodeType.EntityReference && bResolveEntity) {
                    bResolveEntity = false;
                    nodeType = XmlNodeType.EndEntity;
                    return true;
                }
                // if fails to move to it 1st Child, try to move to next below
                return ReadForward( fSkipChildren );
            } else {
                if ( readerNav.NodeType == XmlNodeType.EntityReference && bResolveEntity ) {
                    //The only way to get to here is because Skip() is called directly after ResolveEntity()
                    // in this case, user wants to skip the first Child of EntityRef node and fSkipChildren is true
                    // We want to pointing to the first child node.
                    if (readerNav.MoveToFirstChild()) {
                        nodeType = readerNav.NodeType;
                        curDepth++;
                    }
                    else {
                        nodeType = XmlNodeType.EndEntity;
                    }
                    bResolveEntity = false;
                    return true;
                }
            }
            return ReadForward( fSkipChildren );  //has to get the next node by moving forward
        }

        private void SetEndOfFile() {
            fEOF = true;
            readState = ReadState.EndOfFile;
            nodeType = XmlNodeType.None;
        }

        private bool ReadAtZeroLevel(bool fSkipChildren) {
            Debug.Assert( curDepth == 0 );
            if ( !fSkipChildren
                && nodeType != XmlNodeType.EndElement
                && readerNav.NodeType == XmlNodeType.Element
                && !readerNav.IsEmptyElement ) {
                nodeType = XmlNodeType.EndElement;
                return true;
            } else {
                SetEndOfFile();
                return false;
            }
        }

        private bool ReadForward( bool fSkipChildren ) {
            if ( readState == ReadState.Error )
                return false;

            if ( !bStartFromDocument && curDepth == 0 ) {
                //already on top most node and we shouldn't move to next
                return ReadAtZeroLevel(fSkipChildren);
            }
            //else either we are not on top level or we are starting from the document at the very beginning in which case
            //  we will need to read all the "top" most nodes
            if ( readerNav.MoveToNext() ) {
                nodeType = readerNav.NodeType;
                return true;
            } else {
                //need to check its parent
                if ( curDepth == 0 )
                    return ReadAtZeroLevel(fSkipChildren);
                if ( readerNav.MoveToParent() ) {
                    if ( readerNav.NodeType == XmlNodeType.Element ) {
                        curDepth--;
                        nodeType = XmlNodeType.EndElement;
                        return true;
                    } else if ( readerNav.NodeType == XmlNodeType.EntityReference ) {
                        //coming back from entity reference node -- must be getting down through call ResolveEntity()
                        curDepth--;
                        nodeType = XmlNodeType.EndEntity;
                        return true;
                    }
                    return true;
                }
            }
            return false;
        }

        //the function reset the marks used for ReadChars() and MoveToAttribute(...), ReadAttributeValue(...)
        private void ReSetReadingMarks() {
            //_attrValInd = -1;
            readerNav.ResetMove( ref curDepth, ref nodeType );
            //attrNav.MoveTo( nav );
            //curDepth = _depth;
        }

        // Gets a value indicating whether the reader is positioned at the
        // end of the stream.
        public override bool EOF {
            get { return (readState != ReadState.Closed) && fEOF; }
        }

        // Closes the stream, changes the XmlNodeReader.ReadState
        // to Closed, and sets all the properties back to zero.
        public override void Close() {
            readState = ReadState.Closed;
        }

        // Gets the read state of the stream.
        public override ReadState ReadState {
            get { return readState; }
        }

        // Skips to the end tag of the current element.
        public override void Skip() {
            Read( true );
        }

        // Reads the contents of an element as a string.
	    public override string ReadString() {
		    if ((this.NodeType == XmlNodeType.EntityReference) && bResolveEntity) {
			    if (! this.Read()) {
			            throw new InvalidOperationException(Res.GetString(Res.Xml_InvalidOperation));
			    }
		    }
		    return base.ReadString();
	    }
	
        //
        // Partial Content Read Methods
        //

        // Gets a value indicating whether the current node
        // has any attributes.
        public override bool HasAttributes {
            get {
                return ( AttributeCount > 0 );
            }
        }

        //
        // Nametable and Namespace Helpers
        //

        // Gets the XmlNameTable associated with this implementation.
        public override XmlNameTable NameTable {
            get { return readerNav.NameTable; }
        }

        // Resolves a namespace prefix in the current element's scope.
        public override String LookupNamespace(string prefix) {
            if ( !IsInReadingStates() )
                return null;
            string ns = readerNav.LookupNamespace( prefix );
            if (ns != null && ns.Length == 0) {
                return null;
            }
            return ns;
        }

        // Resolves the entity reference for nodes of NodeType EntityReference.
        public override void ResolveEntity() {
            if ( !IsInReadingStates() || ( nodeType != XmlNodeType.EntityReference ) )
                throw new InvalidOperationException(Res.GetString(Res.Xnr_ResolveEntity));
            bResolveEntity = true;;
        }

        // Parses the attribute value into one or more Text and/or
        // EntityReference node types.
        public override bool ReadAttributeValue() {
            if ( !IsInReadingStates() )
                return false;
            if ( readerNav.ReadAttributeValue( ref curDepth, ref bResolveEntity, ref nodeType ) ) {
                bInReadBinary = false;
                return true;
            }
            return false;
        }

        public override bool CanReadBinaryContent {
            get {
                return true;
            }
        }

        public override int ReadContentAsBase64( byte[] buffer, int index, int count ) {
            if ( readState != ReadState.Interactive ) {
                return 0;
            }

            // init ReadContentAsBinaryHelper when called first time
            if ( !bInReadBinary ) {
                readBinaryHelper = ReadContentAsBinaryHelper.CreateOrReset( readBinaryHelper, this );
            }

            // turn off bInReadBinary in order to have a normal Read() behavior when called from readBinaryHelper
            bInReadBinary = false;

            // call to the helper
            int readCount = readBinaryHelper.ReadContentAsBase64( buffer, index, count );

            // turn on bInReadBinary in again and return
            bInReadBinary = true;
            return readCount;
        }

        public override int ReadContentAsBinHex( byte[] buffer, int index, int count ) {
            if ( readState != ReadState.Interactive ) {
                return 0;
            }

            // init ReadContentAsBinaryHelper when called first time
            if ( !bInReadBinary ) {
                readBinaryHelper = ReadContentAsBinaryHelper.CreateOrReset( readBinaryHelper, this );
            }

            // turn off bInReadBinary in order to have a normal Read() behavior when called from readBinaryHelper
            bInReadBinary = false;

            // call to the helper
            int readCount = readBinaryHelper.ReadContentAsBinHex( buffer, index, count );

            // turn on bInReadBinary in again and return
            bInReadBinary = true;
            return readCount;
        }

        public override int ReadElementContentAsBase64( byte[] buffer, int index, int count ) {
            if ( readState != ReadState.Interactive ) {
                return 0;
            }

            // init ReadContentAsBinaryHelper when called first time
            if ( !bInReadBinary ) {
                readBinaryHelper = ReadContentAsBinaryHelper.CreateOrReset( readBinaryHelper, this );
            }

            // turn off bInReadBinary in order to have a normal Read() behavior when called from readBinaryHelper
            bInReadBinary = false;

            // call to the helper
            int readCount = readBinaryHelper.ReadElementContentAsBase64( buffer, index, count );

            // turn on bInReadBinary in again and return
            bInReadBinary = true;
            return readCount;
        }

        public override int ReadElementContentAsBinHex( byte[] buffer, int index, int count ) {
            if ( readState != ReadState.Interactive ) {
                return 0;
            }

            // init ReadContentAsBinaryHelper when called first time
            if ( !bInReadBinary ) {
                readBinaryHelper = ReadContentAsBinaryHelper.CreateOrReset( readBinaryHelper, this );
            }

            // turn off bInReadBinary in order to have a normal Read() behavior when called from readBinaryHelper
            bInReadBinary = false;

            // call to the helper
            int readCount = readBinaryHelper.ReadElementContentAsBinHex( buffer, index, count );

            // turn on bInReadBinary in again and return
            bInReadBinary = true;
            return readCount;
        }

        void FinishReadBinary() {
            bInReadBinary = false;
            readBinaryHelper.Finish();
        }

        //
        // IXmlNamespaceResolver
        //

        IDictionary<string,string> IXmlNamespaceResolver.GetNamespacesInScope( XmlNamespaceScope scope ) {
            return readerNav.GetNamespacesInScope( scope );
        }

        string IXmlNamespaceResolver.LookupPrefix( string namespaceName ) {
            return readerNav.LookupPrefix( namespaceName );
        }

        String IXmlNamespaceResolver.LookupNamespace( string prefix ) {
            if ( !IsInReadingStates() ) {
                return readerNav.DefaultLookupNamespace( prefix );
            }
            string ns = readerNav.LookupNamespace( prefix );
            if ( ns != null ) {
                ns = readerNav.NameTable.Add( ns );
            }
            return ns;
        }

        // DTD/Schema info used by XmlReader.GetDtdSchemaInfo()
        internal override IDtdInfo DtdInfo {
            get {
                return readerNav.Document.DtdSchemaInfo;
            }
        }
    }
}
