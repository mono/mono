//------------------------------------------------------------------------------
// <copyright file="XmlDocument.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Xml
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Xml.Schema;
    using System.Xml.XPath;
    using System.Security;
    using System.Security.Permissions;
    using System.Globalization;
    using System.Runtime.Versioning;

    // Represents an entire document. An XmlDocument contains XML data.
    public class XmlDocument: XmlNode {
        private XmlImplementation implementation;
        private DomNameTable domNameTable; // hash table of XmlName
        private XmlLinkedNode lastChild;
        private XmlNamedNodeMap entities;
        private Hashtable htElementIdMap;
        private Hashtable htElementIDAttrDecl; //key: id; object: the ArrayList of the elements that have the same id (connected or disconnected)
        private SchemaInfo schemaInfo;
        private XmlSchemaSet schemas; // schemas associated with the cache
        private bool reportValidity;
        //This variable represents the actual loading status. Since, IsLoading will
        //be manipulated soemtimes for adding content to EntityReference this variable
        //has been added which would always represent the loading status of document.
        private bool actualLoadingStatus;

        private XmlNodeChangedEventHandler onNodeInsertingDelegate;
        private XmlNodeChangedEventHandler onNodeInsertedDelegate;
        private XmlNodeChangedEventHandler onNodeRemovingDelegate;
        private XmlNodeChangedEventHandler onNodeRemovedDelegate;
        private XmlNodeChangedEventHandler onNodeChangingDelegate;
        private XmlNodeChangedEventHandler onNodeChangedDelegate;

        // false if there are no ent-ref present, true if ent-ref nodes are or were present (i.e. if all ent-ref were removed, the doc will not clear this flag)
        internal bool fEntRefNodesPresent;
        internal bool fCDataNodesPresent;

        private bool preserveWhitespace;
        private bool isLoading;

        // special name strings for
        internal string strDocumentName;
        internal string strDocumentFragmentName;
        internal string strCommentName;
        internal string strTextName;
        internal string strCDataSectionName;
        internal string strEntityName;
        internal string strID;
        internal string strXmlns;
        internal string strXml;
        internal string strSpace;
        internal string strLang;
        internal string strEmpty;

        internal string strNonSignificantWhitespaceName;
        internal string strSignificantWhitespaceName;
        internal string strReservedXmlns;
        internal string strReservedXml;

        internal String baseURI;

        private XmlResolver resolver;
        internal bool       bSetResolver;
        internal object     objLock;

        private XmlAttribute namespaceXml;

        static internal EmptyEnumerator EmptyEnumerator = new EmptyEnumerator();
        static internal IXmlSchemaInfo NotKnownSchemaInfo = new XmlSchemaInfo(XmlSchemaValidity.NotKnown);
        static internal IXmlSchemaInfo ValidSchemaInfo = new XmlSchemaInfo(XmlSchemaValidity.Valid);
        static internal IXmlSchemaInfo InvalidSchemaInfo = new XmlSchemaInfo(XmlSchemaValidity.Invalid);

        // Initializes a new instance of the XmlDocument class.
        public XmlDocument(): this( new XmlImplementation() ) {
        }

        // Initializes a new instance
        // of the XmlDocument class with the specified XmlNameTable.
        public XmlDocument( XmlNameTable nt ) : this( new XmlImplementation( nt ) ) {
        }

        protected internal XmlDocument( XmlImplementation imp ): base() {

            implementation = imp;
            domNameTable = new DomNameTable( this );

            // force the following string instances to be default in the nametable
            XmlNameTable nt = this.NameTable;
            nt.Add( string.Empty );            
            strDocumentName = nt.Add( "#document" );
            strDocumentFragmentName  = nt.Add( "#document-fragment" );
            strCommentName = nt.Add( "#comment" );
            strTextName = nt.Add( "#text" );
            strCDataSectionName = nt.Add( "#cdata-section" );
            strEntityName = nt.Add( "#entity" );
            strID = nt.Add( "id" );
            strNonSignificantWhitespaceName = nt.Add( "#whitespace" );
            strSignificantWhitespaceName = nt.Add( "#significant-whitespace" );
            strXmlns = nt.Add( "xmlns" );
            strXml = nt.Add(  "xml" );
            strSpace = nt.Add(  "space" );
            strLang = nt.Add(  "lang" );
            strReservedXmlns = nt.Add( XmlReservedNs.NsXmlNs );
            strReservedXml = nt.Add( XmlReservedNs.NsXml );
            strEmpty = nt.Add( String.Empty );
            baseURI = String.Empty; 

            objLock = new object();
        }

        internal SchemaInfo DtdSchemaInfo {
            get { return schemaInfo; }
            set { schemaInfo = value; }
        }

        // NOTE: This does not correctly check start name char, but we cannot change it since it would be a breaking change.
        internal static void CheckName( String name ) {
            int endPos = ValidateNames.ParseNmtoken( name, 0 );
            if (endPos < name.Length) {
                throw new XmlException(Res.Xml_BadNameChar, XmlException.BuildCharExceptionArgs(name, endPos));
            }
        }

        internal XmlName AddXmlName(string prefix, string localName, string namespaceURI, IXmlSchemaInfo schemaInfo) { 
            XmlName n = domNameTable.AddName(prefix, localName, namespaceURI, schemaInfo); 
            Debug.Assert( (prefix == null) ? (n.Prefix.Length == 0) : (prefix == n.Prefix) );
            Debug.Assert( n.LocalName == localName );
            Debug.Assert( (namespaceURI == null) ? (n.NamespaceURI.Length == 0) : (n.NamespaceURI == namespaceURI) );
            return n;
        }

        internal XmlName GetXmlName(string prefix, string localName, string namespaceURI, IXmlSchemaInfo schemaInfo) { 
            XmlName n = domNameTable.GetName(prefix, localName, namespaceURI, schemaInfo); 
            Debug.Assert(n == null || ((prefix == null) ? (n.Prefix.Length == 0) : (prefix == n.Prefix)));
            Debug.Assert(n == null || n.LocalName == localName);
            Debug.Assert(n == null || ((namespaceURI == null) ? (n.NamespaceURI.Length == 0) : (n.NamespaceURI == namespaceURI)));
            return n;
        }

        internal XmlName AddAttrXmlName(string prefix, string localName, string namespaceURI, IXmlSchemaInfo schemaInfo) { 
            XmlName xmlName = AddXmlName(prefix, localName, namespaceURI, schemaInfo); 
            Debug.Assert( (prefix == null) ? (xmlName.Prefix.Length == 0) : (prefix == xmlName.Prefix) );
            Debug.Assert( xmlName.LocalName == localName );
            Debug.Assert( (namespaceURI == null) ? (xmlName.NamespaceURI.Length == 0) : (xmlName.NamespaceURI == namespaceURI) );

            if ( !this.IsLoading ) {
                // Use atomized versions instead of prefix, localName and nsURI
                object oPrefix       = xmlName.Prefix;
                object oNamespaceURI = xmlName.NamespaceURI;
                object oLocalName    = xmlName.LocalName;
                if ( ( oPrefix == (object)strXmlns || ( oPrefix == (object)strEmpty && oLocalName == (object)strXmlns ) ) ^ ( oNamespaceURI == (object)strReservedXmlns ) )
                    throw new ArgumentException( Res.GetString( Res.Xdom_Attr_Reserved_XmlNS, namespaceURI ) );
            }
            return xmlName;
        }

        internal bool AddIdInfo( XmlName eleName, XmlName attrName ) {
            //when XmlLoader call XmlDocument.AddInfo, the element.XmlName and attr.XmlName 
            //have already been replaced with the ones that don't have namespace values (or just
            //string.Empty) because in DTD, the namespace is not supported
            if ( htElementIDAttrDecl == null || htElementIDAttrDecl[eleName] == null ) {
                if ( htElementIDAttrDecl == null )
                    htElementIDAttrDecl = new Hashtable();
                htElementIDAttrDecl.Add(eleName, attrName);
                return true;
            }
            return false;
        }

        private XmlName GetIDInfoByElement_(XmlName eleName)
        {
            //When XmlDocument is getting the IDAttribute for a given element, 
            //we need only compare the prefix and localname of element.XmlName with
            //the registered htElementIDAttrDecl.
            XmlName newName = GetXmlName(eleName.Prefix, eleName.LocalName, string.Empty, null);
            if (newName != null) {
                return (XmlName)(htElementIDAttrDecl[newName]);
            }
            return null;
        }

        internal XmlName GetIDInfoByElement( XmlName eleName ) {
            if (htElementIDAttrDecl == null)
                return null;
            else
                return GetIDInfoByElement_(eleName);
        }

        private WeakReference GetElement(ArrayList elementList, XmlElement elem) {
            ArrayList gcElemRefs = new ArrayList();
            foreach( WeakReference elemRef in elementList) {
                if ( !elemRef.IsAlive)
                    //take notes on the garbage collected nodes
                    gcElemRefs.Add(elemRef);
                else {
                    if ((XmlElement)(elemRef.Target) == elem)
                        return elemRef;
                }
            }                 
            //Clear out the gced elements
            foreach( WeakReference elemRef in gcElemRefs)
                elementList.Remove(elemRef);
            return null;
        }

        internal void AddElementWithId( string id, XmlElement elem ) {
            if (htElementIdMap == null || !htElementIdMap.Contains(id)) {
                if ( htElementIdMap == null )
                    htElementIdMap = new Hashtable();
                ArrayList elementList = new ArrayList();
                elementList.Add(new WeakReference(elem));
                htElementIdMap.Add(id, elementList);
            }
            else {
                // there are other element(s) that has the same id
                ArrayList elementList = (ArrayList)(htElementIdMap[id]);
                if (GetElement(elementList, elem) == null)
                    elementList.Add(new WeakReference(elem));
            }
        }

        internal void RemoveElementWithId( string id, XmlElement elem ) {
            if (htElementIdMap != null && htElementIdMap.Contains(id)) {
                ArrayList elementList = (ArrayList)(htElementIdMap[id]);
                WeakReference elemRef = GetElement(elementList, elem);
                if (elemRef != null) {
                    elementList.Remove(elemRef);
                    if (elementList.Count == 0)
                        htElementIdMap.Remove(id);
                }
            }
        }


        // Creates a duplicate of this node.
        public override XmlNode CloneNode( bool deep ) {
            XmlDocument clone = Implementation.CreateDocument();
            clone.SetBaseURI(this.baseURI);
            if (deep)
                clone.ImportChildren( this, clone, deep );

            return clone;
        }

        // Gets the type of the current node.
        public override XmlNodeType NodeType {
            get { return XmlNodeType.Document; }
        }

        public override XmlNode ParentNode {
            get { return null; }
        }

        // Gets the node for the DOCTYPE declaration.
        public virtual XmlDocumentType DocumentType {
            get { return(XmlDocumentType) FindChild( XmlNodeType.DocumentType ); }
        }

        internal virtual XmlDeclaration Declaration {
            get {
                if ( HasChildNodes ) {
                    XmlDeclaration dec = FirstChild as XmlDeclaration;
                    return dec;
                }
                return null;
            }
        }

        // Gets the XmlImplementation object for this document.
        public XmlImplementation Implementation {
            get { return this.implementation; }
        }

        // Gets the name of the node.
        public override String Name  {
            get { return strDocumentName; }
        }

        // Gets the name of the current node without the namespace prefix.
        public override String LocalName {
            get { return strDocumentName; }
        }

        // Gets the root XmlElement for the document.
        public XmlElement DocumentElement {
            get { return(XmlElement)FindChild(XmlNodeType.Element); }
        }

        internal override bool IsContainer {
            get { return true; }
        }

        internal override XmlLinkedNode LastNode
        {
            get { return lastChild; }
            set { lastChild = value; }
        }

        // Gets the XmlDocument that contains this node.
        public override XmlDocument OwnerDocument
        {
            get { return null; }
        }

        public XmlSchemaSet Schemas {
            get { 
                if (schemas == null) {
                    schemas = new XmlSchemaSet(NameTable);
                }
                return schemas; 
            }

            set { 
                schemas = value; 
            }
        }

        internal bool CanReportValidity {
            get { return reportValidity; }
        }

        internal bool HasSetResolver {
            get { return bSetResolver; }
        }

        internal XmlResolver GetResolver() {
           return resolver;
        }

        public virtual XmlResolver XmlResolver {
            set {
                if ( value != null ) {
                    try {
                        new NamedPermissionSet( "FullTrust" ).Demand();
                    }
                    catch ( SecurityException e ) {
                        throw new SecurityException( Res.GetString( Res.Xml_UntrustedCodeSettingResolver ), e );
                    }
                }   

                resolver = value;
                if ( !bSetResolver )
                    bSetResolver = true;

                XmlDocumentType dtd = this.DocumentType;
                if ( dtd != null ) {
                    dtd.DtdSchemaInfo = null;
                }
            }
        }
        internal override bool IsValidChildType( XmlNodeType type ) {
            switch ( type ) {
                case XmlNodeType.ProcessingInstruction:
                case XmlNodeType.Comment:
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                    return true;

                case XmlNodeType.DocumentType:
                    if ( DocumentType != null )
                        throw new InvalidOperationException( Res.GetString(Res.Xdom_DualDocumentTypeNode) );
                    return true;

                case XmlNodeType.Element:
                    if ( DocumentElement != null )
                        throw new InvalidOperationException( Res.GetString(Res.Xdom_DualDocumentElementNode) );
                    return true;

                case XmlNodeType.XmlDeclaration:
                    if ( Declaration != null )
                        throw new InvalidOperationException( Res.GetString(Res.Xdom_DualDeclarationNode) );
                    return true;

                default:
                    return false;
            }
        }
        // the function examines all the siblings before the refNode
        //  if any of the nodes has type equals to "nt", return true; otherwise, return false;
        private bool HasNodeTypeInPrevSiblings( XmlNodeType nt, XmlNode refNode ) {
            if ( refNode == null )
                return false;

            XmlNode node = null;
            if ( refNode.ParentNode != null )
                node = refNode.ParentNode.FirstChild;
            while ( node != null ) {
                if ( node.NodeType == nt )
                    return true;
                if ( node == refNode )
                    break;
                node = node.NextSibling;
            }
            return false;
        }

        // the function examines all the siblings after the refNode
        //  if any of the nodes has the type equals to "nt", return true; otherwise, return false;
        private bool HasNodeTypeInNextSiblings( XmlNodeType nt, XmlNode refNode ) {
            XmlNode node = refNode;
            while ( node != null ) {
                if ( node.NodeType == nt )
                    return true;
                node = node.NextSibling;
            }
            return false;
        }

        internal override bool CanInsertBefore( XmlNode newChild, XmlNode refChild ) {
            if ( refChild == null )
                refChild = FirstChild;

            if ( refChild == null )
                return true;

            switch ( newChild.NodeType ) {
                case XmlNodeType.XmlDeclaration:
                    return ( refChild == FirstChild ); 

                case XmlNodeType.ProcessingInstruction:
                case XmlNodeType.Comment:
                    return refChild.NodeType != XmlNodeType.XmlDeclaration;

                case XmlNodeType.DocumentType: {
                    if ( refChild.NodeType != XmlNodeType.XmlDeclaration ) {
                        //if refChild is not the XmlDeclaration node, only need to go through the sibling before and including refChild to
                        //  make sure no Element ( rootElem node ) before the current position
                        return !HasNodeTypeInPrevSiblings( XmlNodeType.Element, refChild.PreviousSibling );
                    }
                }
                break;

                case XmlNodeType.Element: {
                    if ( refChild.NodeType != XmlNodeType.XmlDeclaration ) {
                        //if refChild is not the XmlDeclaration node, only need to go through the siblings after and including the refChild to
                        //  make sure no DocType node and XmlDeclaration node after the current posistion.
                        return !HasNodeTypeInNextSiblings( XmlNodeType.DocumentType, refChild );
                    }
                }
                break;
            }

            return false;
        }

        internal override bool CanInsertAfter( XmlNode newChild, XmlNode refChild ) {
            if ( refChild == null )
                refChild = LastChild;

            if ( refChild == null )
                return true;

            switch ( newChild.NodeType ) {
                case XmlNodeType.ProcessingInstruction:
                case XmlNodeType.Comment:
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                    return true;

                case XmlNodeType.DocumentType: {
                    //we will have to go through all the siblings before the refChild just to make sure no Element node ( rootElem )
                    //  before the current position
                    return !HasNodeTypeInPrevSiblings( XmlNodeType.Element, refChild );
                }

                case XmlNodeType.Element: {
                    return !HasNodeTypeInNextSiblings( XmlNodeType.DocumentType, refChild.NextSibling );
                }

            }

            return false;
        }

        // Creates an XmlAttribute with the specified name.
        public XmlAttribute CreateAttribute( String name ) {
            String prefix = String.Empty;
            String localName = String.Empty;
            String namespaceURI = String.Empty;

            SplitName( name, out prefix, out localName );

            SetDefaultNamespace( prefix, localName, ref namespaceURI );

            return CreateAttribute( prefix, localName, namespaceURI );
        }

        internal void SetDefaultNamespace( String prefix, String localName, ref String namespaceURI ) {
            if ( prefix == strXmlns || ( prefix.Length == 0 && localName == strXmlns ) ) {
                namespaceURI = strReservedXmlns;
            } else if ( prefix == strXml ) {
                namespaceURI = strReservedXml;
            }
        }

        // Creates a XmlCDataSection containing the specified data.
        public virtual XmlCDataSection CreateCDataSection( String data ) {
            fCDataNodesPresent = true;
            return new XmlCDataSection( data, this );
        }

        // Creates an XmlComment containing the specified data.
        public virtual XmlComment CreateComment( String data ) {
            return new XmlComment( data, this );
        }

        // Returns a new XmlDocumentType object.
        [PermissionSetAttribute( SecurityAction.InheritanceDemand, Name = "FullTrust" )]
        public virtual XmlDocumentType CreateDocumentType( string name, string publicId, string systemId, string internalSubset ) {
            return new XmlDocumentType( name, publicId, systemId, internalSubset, this );
        }

        // Creates an XmlDocumentFragment.
        public virtual XmlDocumentFragment CreateDocumentFragment() {
            return new XmlDocumentFragment( this );
        }

        // Creates an element with the specified name.
        public XmlElement CreateElement( String name ) {
            string prefix = String.Empty;
            string localName = String.Empty;
            SplitName( name, out prefix, out localName );
            return CreateElement( prefix, localName, string.Empty );
        }


        internal void AddDefaultAttributes( XmlElement elem ) {
            SchemaInfo schInfo = DtdSchemaInfo;
            SchemaElementDecl ed = GetSchemaElementDecl( elem );
            if ( ed != null && ed.AttDefs != null ) {
                IDictionaryEnumerator attrDefs = ed.AttDefs.GetEnumerator();
                while ( attrDefs.MoveNext() ) {
                    SchemaAttDef attdef = (SchemaAttDef)attrDefs.Value;
                    if ( attdef.Presence == SchemaDeclBase.Use.Default ||
                         attdef.Presence == SchemaDeclBase.Use.Fixed ) {
                         //build a default attribute and return
                         string attrPrefix = string.Empty;
                         string attrLocalname = attdef.Name.Name;
                         string attrNamespaceURI = string.Empty;
                         if ( schInfo.SchemaType == SchemaType.DTD )
                            attrPrefix = attdef.Name.Namespace;
                         else {
                            attrPrefix = attdef.Prefix;
                            attrNamespaceURI = attdef.Name.Namespace;
                         }
                         XmlAttribute defattr = PrepareDefaultAttribute( attdef, attrPrefix, attrLocalname, attrNamespaceURI );
                         elem.SetAttributeNode( defattr );
                    }
                }
            }
        }

        private SchemaElementDecl GetSchemaElementDecl( XmlElement elem ) {
            SchemaInfo schInfo = DtdSchemaInfo;
            if ( schInfo != null ) {
                //build XmlQualifiedName used to identify the element schema declaration
                XmlQualifiedName   qname = new XmlQualifiedName( elem.LocalName, schInfo.SchemaType == SchemaType.DTD ? elem.Prefix : elem.NamespaceURI );
                //get the schema info for the element
                SchemaElementDecl elemDecl;
                if ( schInfo.ElementDecls.TryGetValue(qname, out elemDecl) ) {
                    return elemDecl;
                }
            }
            return null;
        }

        //Will be used by AddDeafulatAttributes() and GetDefaultAttribute() methods
        private XmlAttribute PrepareDefaultAttribute( SchemaAttDef attdef, string attrPrefix, string attrLocalname, string attrNamespaceURI ) {
            SetDefaultNamespace( attrPrefix, attrLocalname, ref attrNamespaceURI );
            XmlAttribute defattr = CreateDefaultAttribute( attrPrefix, attrLocalname, attrNamespaceURI );
            //parsing the default value for the default attribute
            defattr.InnerXml = attdef.DefaultValueRaw;
            //during the expansion of the tree, the flag could be set to true, we need to set it back.
            XmlUnspecifiedAttribute unspAttr = defattr as XmlUnspecifiedAttribute;
            if ( unspAttr != null ) {
                unspAttr.SetSpecified( false );
            }
            return defattr;
        }

        // Creates an XmlEntityReference with the specified name.
        public virtual XmlEntityReference CreateEntityReference( String name ) {
            return new XmlEntityReference( name, this );
        }

        // Creates a XmlProcessingInstruction with the specified name
        // and data strings.
        public virtual XmlProcessingInstruction CreateProcessingInstruction( String target, String data ) {
            return new XmlProcessingInstruction( target, data, this );
        }

        // Creates a XmlDeclaration node with the specified values.
        public virtual XmlDeclaration CreateXmlDeclaration( String version, string encoding, string standalone ) {
            return new XmlDeclaration( version, encoding, standalone, this );
        }

        // Creates an XmlText with the specified text.
        public virtual XmlText CreateTextNode( String text ) {
            return new XmlText( text, this );
        }

        // Creates a XmlSignificantWhitespace node.
        public virtual XmlSignificantWhitespace CreateSignificantWhitespace( string text ) {
            return new XmlSignificantWhitespace( text, this );
        }

        public override XPathNavigator CreateNavigator() {
            return CreateNavigator(this);
        }

        internal protected virtual XPathNavigator CreateNavigator(XmlNode node) {
            XmlNodeType nodeType = node.NodeType;
            XmlNode parent;
            XmlNodeType parentType;

            switch (nodeType) {
                case XmlNodeType.EntityReference:
                case XmlNodeType.Entity:
                case XmlNodeType.DocumentType:
                case XmlNodeType.Notation:
                case XmlNodeType.XmlDeclaration:
                    return null;
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                case XmlNodeType.SignificantWhitespace:
                    parent = node.ParentNode;
                    if (parent != null) {
                        do {
                            parentType = parent.NodeType;
                            if (parentType == XmlNodeType.Attribute) {
                                return null;
                            }
                            else if (parentType == XmlNodeType.EntityReference) {
                                parent = parent.ParentNode;
                            }
                            else {
                                break;
                            }
                        }
                        while (parent != null);
                    }
                    node = NormalizeText(node);
                    break;
                case XmlNodeType.Whitespace:
                    parent = node.ParentNode;
                    if (parent != null) {
                        do {
                            parentType = parent.NodeType;
                            if (parentType == XmlNodeType.Document
                                || parentType == XmlNodeType.Attribute) {
                                return null;
                            }
                            else if (parentType == XmlNodeType.EntityReference) {
                                parent = parent.ParentNode;
                            }
                            else {
                                break;
                            }
                        }
                        while (parent != null);
                    }
                    node = NormalizeText(node);
                    break;
                default:
                    break;
            }
            return new DocumentXPathNavigator(this, node);
        }

        internal static bool IsTextNode( XmlNodeType nt ) {
            switch( nt ) {
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                    return true;
                default:
                    return false;
            }
        }

        private XmlNode NormalizeText( XmlNode n ) {
            XmlNode retnode = null;
            while( IsTextNode( n.NodeType ) ) {
                retnode = n;
                n = n.PreviousSibling;

                if( n == null ) {
                    XmlNode intnode = retnode;
                    while ( true ) {
                        if  ( intnode.ParentNode != null && intnode.ParentNode.NodeType == XmlNodeType.EntityReference ) {
                            if (intnode.ParentNode.PreviousSibling != null ) {
                                n = intnode.ParentNode.PreviousSibling;
                                break;
                            }
                            else {
                                intnode = intnode.ParentNode;
                                if( intnode == null )
                                    break;
                            }
                        }
                        else
                            break;
                    }
                }

                if( n == null )
                    break;                    
                while( n.NodeType == XmlNodeType.EntityReference ) {                    
                    n = n.LastChild;
                }
            }
            return retnode;
        }

        // Creates a XmlWhitespace node.
        public virtual XmlWhitespace CreateWhitespace( string text ) {
            return new XmlWhitespace( text, this );
        }

        // Returns an XmlNodeList containing
        // a list of all descendant elements that match the specified name.
        public virtual XmlNodeList GetElementsByTagName( String name ) {
            return new XmlElementList( this, name );
        }

        // DOM Level 2

        // Creates an XmlAttribute with the specified LocalName
        // and NamespaceURI.
        public XmlAttribute CreateAttribute( String qualifiedName, String namespaceURI ) {
            string prefix = String.Empty;
            string localName = String.Empty;

            SplitName( qualifiedName, out prefix, out localName );
            return CreateAttribute( prefix, localName, namespaceURI );
        }

        // Creates an XmlElement with the specified LocalName and
        // NamespaceURI.
        public XmlElement CreateElement( String qualifiedName, String namespaceURI ) {
            string prefix = String.Empty;
            string localName = String.Empty;
            SplitName( qualifiedName, out prefix, out localName );
            return CreateElement( prefix, localName, namespaceURI );
        }

        // Returns a XmlNodeList containing
        // a list of all descendant elements that match the specified name.
        public virtual XmlNodeList GetElementsByTagName( String localName, String namespaceURI ) {
            return new XmlElementList( this, localName, namespaceURI );
        }

        // Returns the XmlElement with the specified ID.
        public virtual XmlElement GetElementById( string elementId ) {
            if (htElementIdMap != null) {
                ArrayList elementList = (ArrayList)(htElementIdMap[elementId]);
                if (elementList != null) {
                    foreach (WeakReference elemRef in elementList) {
                        XmlElement elem = (XmlElement)elemRef.Target;
                        if (elem != null
                            && elem.IsConnected())
                            return elem;
                    }
                }
            }
            return null;
        }

        // Imports a node from another document to this document.
        public virtual XmlNode ImportNode( XmlNode node, bool deep ) {
            return ImportNodeInternal( node, deep );
        }

        private XmlNode ImportNodeInternal( XmlNode node, bool deep ) {
            XmlNode newNode = null;

            if ( node == null ) {
                throw new InvalidOperationException(  Res.GetString(Res.Xdom_Import_NullNode) );
            }
            else {
                switch ( node.NodeType ) {
                    case XmlNodeType.Element:
                        newNode = CreateElement( node.Prefix, node.LocalName, node.NamespaceURI );
                        ImportAttributes( node, newNode );
                        if ( deep )
                            ImportChildren( node, newNode, deep );
                        break;

                    case XmlNodeType.Attribute:
                        Debug.Assert( ((XmlAttribute)node).Specified );
                        newNode = CreateAttribute( node.Prefix, node.LocalName, node.NamespaceURI );
                        ImportChildren( node, newNode, true );
                        break;

                    case XmlNodeType.Text:
                        newNode = CreateTextNode( node.Value );
                        break;
                    case XmlNodeType.Comment:
                        newNode = CreateComment( node.Value);
                        break;
                    case XmlNodeType.ProcessingInstruction:
                        newNode = CreateProcessingInstruction( node.Name, node.Value );
                        break;
                    case XmlNodeType.XmlDeclaration:
                        XmlDeclaration decl = (XmlDeclaration) node;
                        newNode = CreateXmlDeclaration( decl.Version, decl.Encoding, decl.Standalone );
                        break;
                    case XmlNodeType.CDATA:
                        newNode = CreateCDataSection( node.Value );
                        break;
                    case XmlNodeType.DocumentType:
                        XmlDocumentType docType = (XmlDocumentType)node;
                        newNode = CreateDocumentType( docType.Name, docType.PublicId, docType.SystemId, docType.InternalSubset );
                        break;
                    case XmlNodeType.DocumentFragment:
                        newNode = CreateDocumentFragment();
                        if (deep)
                            ImportChildren( node, newNode, deep );
                        break;

                    case XmlNodeType.EntityReference:
                        newNode = CreateEntityReference( node.Name );
                        // we don't import the children of entity reference because they might result in different
                        // children nodes given different namesapce context in the new document.
                        break;

                    case XmlNodeType.Whitespace:
                        newNode = CreateWhitespace( node.Value );
                        break;

                    case XmlNodeType.SignificantWhitespace:
                        newNode = CreateSignificantWhitespace( node.Value );
                        break;

                    default:
                        throw new InvalidOperationException( String.Format( CultureInfo.InvariantCulture, Res.GetString(Res.Xdom_Import), node.NodeType.ToString() ) );
                }
            }

            return newNode;
        }

        private void ImportAttributes( XmlNode fromElem, XmlNode toElem ) {
            int cAttr = fromElem.Attributes.Count;
            for ( int iAttr = 0; iAttr < cAttr; iAttr++ ) {
                if ( fromElem.Attributes[iAttr].Specified )
                    toElem.Attributes.SetNamedItem( ImportNodeInternal( fromElem.Attributes[iAttr], true ) );
            }
        }

        private void ImportChildren( XmlNode fromNode, XmlNode toNode, bool deep ) {
            Debug.Assert( toNode.NodeType != XmlNodeType.EntityReference );
            for ( XmlNode n = fromNode.FirstChild; n != null; n = n.NextSibling ) {
                toNode.AppendChild( ImportNodeInternal( n, deep ) );
            }
        }

        // Microsoft extensions
         
        // Gets the XmlNameTable associated with this
        // implementation.
        public XmlNameTable NameTable
        {
            get { return implementation.NameTable; }
        }

        // Creates a XmlAttribute with the specified Prefix, LocalName,
        // and NamespaceURI.
        public virtual XmlAttribute CreateAttribute( string prefix, string localName, string namespaceURI ) {
            return new XmlAttribute( AddAttrXmlName( prefix, localName, namespaceURI, null ), this );
        }

        protected internal virtual XmlAttribute CreateDefaultAttribute( string prefix, string localName, string namespaceURI ) {
            return new XmlUnspecifiedAttribute( prefix, localName, namespaceURI, this );
        }

        public virtual XmlElement CreateElement( string prefix, string localName, string namespaceURI) {
            XmlElement elem = new XmlElement( AddXmlName( prefix, localName, namespaceURI, null ), true, this );
            if ( !IsLoading )
                AddDefaultAttributes( elem );
            return elem;
        }

        // Gets or sets a value indicating whether to preserve whitespace.
        public bool PreserveWhitespace {
            get { return preserveWhitespace;}
            set { preserveWhitespace = value;}
        }

        // Gets a value indicating whether the node is read-only.
        public override bool IsReadOnly {
            get { return false;}
        }

        internal XmlNamedNodeMap Entities {
            get {
                if ( entities == null )
                    entities = new XmlNamedNodeMap( this );
                return entities;
            }
            set { entities = value; }
        }

        internal bool IsLoading {
            get { return isLoading;}
            set { isLoading = value; }
        }

        internal bool ActualLoadingStatus{
            get { return actualLoadingStatus;}
            set { actualLoadingStatus = value; }
        }


        // Creates a XmlNode with the specified XmlNodeType, Prefix, Name, and NamespaceURI.
        public virtual XmlNode CreateNode( XmlNodeType type, string prefix, string name, string namespaceURI ) {
            switch (type) {
                case XmlNodeType.Element:
                    if (prefix != null)
                        return CreateElement( prefix, name, namespaceURI );
                    else
                        return CreateElement( name, namespaceURI );

                case XmlNodeType.Attribute:
                    if (prefix != null)
                        return CreateAttribute( prefix, name, namespaceURI );
                    else
                        return CreateAttribute( name, namespaceURI );

                case XmlNodeType.Text:
                    return CreateTextNode( string.Empty );

                case XmlNodeType.CDATA:
                    return CreateCDataSection( string.Empty );

                case XmlNodeType.EntityReference:
                    return CreateEntityReference( name );

                case XmlNodeType.ProcessingInstruction:
                    return CreateProcessingInstruction( name, string.Empty );

                case XmlNodeType.XmlDeclaration:
                    return CreateXmlDeclaration( "1.0", null, null );

                case XmlNodeType.Comment:
                    return CreateComment( string.Empty );

                case XmlNodeType.DocumentFragment:
                    return CreateDocumentFragment();

                case XmlNodeType.DocumentType:
                    return CreateDocumentType( name, string.Empty, string.Empty, string.Empty );

                case XmlNodeType.Document:
                    return new XmlDocument();

                case XmlNodeType.SignificantWhitespace:
                    return CreateSignificantWhitespace( string.Empty );

                case XmlNodeType.Whitespace:
                    return CreateWhitespace( string.Empty );

                default:
                    throw new ArgumentException( Res.GetString( Res.Arg_CannotCreateNode, type ) );
            }
        }

        // Creates an XmlNode with the specified node type, Name, and
        // NamespaceURI.
        public virtual XmlNode CreateNode( string nodeTypeString, string name, string namespaceURI ) {
            return CreateNode( ConvertToNodeType( nodeTypeString ), name, namespaceURI );
        }

        // Creates an XmlNode with the specified XmlNodeType, Name, and
        // NamespaceURI.
        public virtual XmlNode CreateNode( XmlNodeType type, string name, string namespaceURI ) {
            return CreateNode( type, null, name, namespaceURI );
        }

        // Creates an XmlNode object based on the information in the XmlReader.
        // The reader must be positioned on a node or attribute.
        [PermissionSetAttribute( SecurityAction.InheritanceDemand, Name = "FullTrust" )]
        public virtual XmlNode ReadNode( XmlReader reader ) {
            XmlNode node = null;
            try {
                IsLoading = true;
                XmlLoader loader = new XmlLoader();
                node = loader.ReadCurrentNode( this, reader );
            }
            finally {
                IsLoading = false;
            }
            return node;
        }

        internal XmlNodeType ConvertToNodeType( string nodeTypeString ) {
            if ( nodeTypeString == "element" ) {
                return XmlNodeType.Element;
            }
            else if ( nodeTypeString == "attribute" ) {
                return XmlNodeType.Attribute;
            }
            else if ( nodeTypeString == "text" ) {
                return XmlNodeType.Text;
            }
            else if ( nodeTypeString == "cdatasection" ) {
                return XmlNodeType.CDATA;
            }
            else if ( nodeTypeString == "entityreference" ) {
                return XmlNodeType.EntityReference;
            }
            else if ( nodeTypeString == "entity" ) {
                return XmlNodeType.Entity;
            }
            else if ( nodeTypeString == "processinginstruction" ) {
                return XmlNodeType.ProcessingInstruction;
            }
            else if ( nodeTypeString == "comment" ) {
                return XmlNodeType.Comment;
            }
            else if ( nodeTypeString == "document" ) {
                return XmlNodeType.Document;
            }
            else if ( nodeTypeString == "documenttype" ) {
                return XmlNodeType.DocumentType;
            }
            else if ( nodeTypeString == "documentfragment" ) {
                return XmlNodeType.DocumentFragment;
            }
            else if ( nodeTypeString == "notation" ) {
                return XmlNodeType.Notation;
            }
            else if ( nodeTypeString == "significantwhitespace" ) {
                return XmlNodeType.SignificantWhitespace;
            }
            else if ( nodeTypeString == "whitespace" ) {
                return XmlNodeType.Whitespace;
            }
            throw new ArgumentException( Res.GetString( Res.Xdom_Invalid_NT_String, nodeTypeString ) );
        }


        private XmlTextReader SetupReader( XmlTextReader tr ) {
            tr.XmlValidatingReaderCompatibilityMode = true;
            tr.EntityHandling = EntityHandling.ExpandCharEntities;
            if ( this.HasSetResolver )
                tr.XmlResolver = GetResolver();
            return tr;
        }

        // Loads the XML document from the specified URL.
        [ResourceConsumption(ResourceScope.Machine)]
        [ResourceExposure(ResourceScope.Machine)]
        public virtual void Load( string filename ) {
            XmlTextReader reader = SetupReader( new XmlTextReader( filename, NameTable ) );
            try {
                Load( reader );
            }
            finally {
                reader.Close();
            }
        }

        public virtual void Load( Stream inStream ) {
            XmlTextReader reader = SetupReader( new XmlTextReader( inStream, NameTable ) );
            try {
                Load( reader );
            }
            finally {
                reader.Impl.Close( false );
            }
        }

        // Loads the XML document from the specified TextReader.
        public virtual void Load( TextReader txtReader ) {
            XmlTextReader reader = SetupReader( new XmlTextReader( txtReader, NameTable ) );
            try {
                Load( reader );
            }
            finally {
                reader.Impl.Close( false );
            }
        }

        // Loads the XML document from the specified XmlReader.
        public virtual void Load( XmlReader reader ) {
            try {
                IsLoading = true;
                actualLoadingStatus = true;
                RemoveAll();
                fEntRefNodesPresent = false;
                fCDataNodesPresent  = false;
                reportValidity = true;

                XmlLoader loader = new XmlLoader();
                loader.Load( this, reader, preserveWhitespace );
            }
            finally {
                IsLoading = false;
                actualLoadingStatus = false;

                // Ensure the bit is still on after loading a dtd 
                reportValidity = true;
            }
        }

        // Loads the XML document from the specified string.
        public virtual void LoadXml( string xml ) {
            XmlTextReader reader = SetupReader( new XmlTextReader( new StringReader( xml ), NameTable ));
            try {
                Load( reader );
            }
            finally {
                reader.Close();
            }
        }

        //TextEncoding is the one from XmlDeclaration if there is any
        internal Encoding TextEncoding {
            get {
                if ( Declaration != null )
                {
                    string value = Declaration.Encoding;
                    if ( value.Length > 0 ) {
                        return System.Text.Encoding.GetEncoding( value );
                    }
                }
                return null;
            }
        }

        public override string InnerText {
            set {
                throw new InvalidOperationException(Res.GetString(Res.Xdom_Document_Innertext));
            }
        }

        public override string InnerXml {
            get {
                return base.InnerXml;
            }
            set {
                LoadXml( value );
            }
        }

        // Saves the XML document to the specified file.
        //Saves out the to the file with exact content in the XmlDocument.
        [ResourceConsumption(ResourceScope.Machine)]
        [ResourceExposure(ResourceScope.Machine)]
        public virtual void Save( string filename ) {
            if ( DocumentElement == null )
                throw new XmlException( Res.Xml_InvalidXmlDocument, Res.GetString( Res.Xdom_NoRootEle ) );
            XmlDOMTextWriter xw = new XmlDOMTextWriter( filename, TextEncoding );
            try {
                if ( preserveWhitespace == false )
                    xw.Formatting = Formatting.Indented;
                 WriteTo( xw );
                 xw.Flush();
            }
            finally {
                xw.Close();
            }
        }

        //Saves out the to the file with exact content in the XmlDocument.
        public virtual void Save( Stream outStream ) {
            XmlDOMTextWriter xw = new XmlDOMTextWriter( outStream, TextEncoding );
            if ( preserveWhitespace == false )
                xw.Formatting = Formatting.Indented;
            WriteTo( xw );
            xw.Flush();
        }

        // Saves the XML document to the specified TextWriter.
        //
        //Saves out the file with xmldeclaration which has encoding value equal to
        //that of textwriter's encoding
        public virtual void Save( TextWriter writer ) {
            XmlDOMTextWriter xw = new XmlDOMTextWriter( writer );
            if ( preserveWhitespace == false )
                    xw.Formatting = Formatting.Indented;
            Save( xw );
        }

        // Saves the XML document to the specified XmlWriter.
        // 
        //Saves out the file with xmldeclaration which has encoding value equal to
        //that of textwriter's encoding
        public virtual void Save( XmlWriter w ) {
            XmlNode n = this.FirstChild;
            if( n == null )
                return;
            if( w.WriteState == WriteState.Start ) {
                if( n is XmlDeclaration ) {
                    if( Standalone.Length == 0 )
                        w.WriteStartDocument();
                    else if( Standalone == "yes" )
                        w.WriteStartDocument( true );
                    else if( Standalone == "no" )
                        w.WriteStartDocument( false );
                    n = n.NextSibling;
                }
                else {
                    w.WriteStartDocument();
                }
            }
            while( n != null ) {
                //Debug.Assert( n.NodeType != XmlNodeType.XmlDeclaration );
                n.WriteTo( w );
                n = n.NextSibling;
            }
            w.Flush();
        }

        // Saves the node to the specified XmlWriter.
        // 
        //Writes out the to the file with exact content in the XmlDocument.
        public override void WriteTo( XmlWriter w ) {
            WriteContentTo( w );
        }

        // Saves all the children of the node to the specified XmlWriter.
        // 
        //Writes out the to the file with exact content in the XmlDocument.
        public override void WriteContentTo( XmlWriter xw ) {
            foreach( XmlNode n in this ) {
                n.WriteTo( xw );
            }
        }

        public void Validate(ValidationEventHandler validationEventHandler) {
            Validate(validationEventHandler, this);
        }

        public void Validate(ValidationEventHandler validationEventHandler, XmlNode nodeToValidate) {
            if (this.schemas == null || this.schemas.Count == 0) { //Should we error
                throw new InvalidOperationException(Res.GetString(Res.XmlDocument_NoSchemaInfo));
            }
            XmlDocument parentDocument = nodeToValidate.Document;
            if (parentDocument != this) {
                throw new ArgumentException(Res.GetString(Res.XmlDocument_NodeNotFromDocument, "nodeToValidate"));
            }
            if (nodeToValidate == this) {
                reportValidity = false;
            }
            DocumentSchemaValidator validator = new DocumentSchemaValidator(this, schemas, validationEventHandler);
            validator.Validate(nodeToValidate);
            if (nodeToValidate == this) {
                reportValidity = true;
            }
        }

        public event XmlNodeChangedEventHandler NodeInserting {
            add {
                onNodeInsertingDelegate += value;
            }
            remove {
                onNodeInsertingDelegate -= value;
            }
        }

        public event XmlNodeChangedEventHandler NodeInserted {
            add {
                onNodeInsertedDelegate += value;
            }
            remove {
                onNodeInsertedDelegate -= value;
            }
        }

        public event XmlNodeChangedEventHandler NodeRemoving {
            add {
                onNodeRemovingDelegate += value;
            }
            remove {
                onNodeRemovingDelegate -= value;
            }
        }

        public event XmlNodeChangedEventHandler NodeRemoved {
            add {
                onNodeRemovedDelegate += value;
            }
            remove {
                onNodeRemovedDelegate -= value;
            }
        }

        public event XmlNodeChangedEventHandler NodeChanging {
            add {
                onNodeChangingDelegate += value;
            }
            remove {
                onNodeChangingDelegate -= value;
            }
        }

        public event XmlNodeChangedEventHandler NodeChanged {
            add {
                onNodeChangedDelegate += value;
            }
            remove {
                onNodeChangedDelegate -= value;
            }
        }

        internal override XmlNodeChangedEventArgs GetEventArgs(XmlNode node, XmlNode oldParent, XmlNode newParent, string oldValue, string newValue, XmlNodeChangedAction action) {
            reportValidity = false;

            switch (action) {
                case XmlNodeChangedAction.Insert:
                    if (onNodeInsertingDelegate == null && onNodeInsertedDelegate == null) {
                        return null;
                    }
                    break;
                case XmlNodeChangedAction.Remove:
                    if (onNodeRemovingDelegate == null && onNodeRemovedDelegate == null) {
                        return null;
                    }
                    break;
                case XmlNodeChangedAction.Change:
                    if (onNodeChangingDelegate == null && onNodeChangedDelegate == null) {
                        return null;
                    }
                    break;
            }
            return new XmlNodeChangedEventArgs( node, oldParent, newParent, oldValue, newValue, action );
        }

        internal XmlNodeChangedEventArgs GetInsertEventArgsForLoad( XmlNode node, XmlNode newParent ) {
            if (onNodeInsertingDelegate == null && onNodeInsertedDelegate == null) {
                return null;
            }
            string nodeValue = node.Value;
            return new XmlNodeChangedEventArgs(node, null, newParent, nodeValue, nodeValue, XmlNodeChangedAction.Insert);
        }

        internal override void BeforeEvent( XmlNodeChangedEventArgs args ) {
            if ( args != null ) {
                switch ( args.Action ) {
                    case XmlNodeChangedAction.Insert:
                        if ( onNodeInsertingDelegate != null )
                            onNodeInsertingDelegate( this, args );
                        break;

                    case XmlNodeChangedAction.Remove:
                        if ( onNodeRemovingDelegate != null )
                            onNodeRemovingDelegate( this, args );
                        break;

                    case XmlNodeChangedAction.Change:
                        if ( onNodeChangingDelegate != null )
                            onNodeChangingDelegate( this, args );
                        break;
                }
            }
        }

        internal override void AfterEvent( XmlNodeChangedEventArgs args ) {
            if ( args != null ) {
                switch ( args.Action ) {
                    case XmlNodeChangedAction.Insert:
                        if ( onNodeInsertedDelegate != null )
                            onNodeInsertedDelegate( this, args );
                        break;

                    case XmlNodeChangedAction.Remove:
                        if ( onNodeRemovedDelegate != null )
                            onNodeRemovedDelegate( this, args );
                        break;

                    case XmlNodeChangedAction.Change:
                        if ( onNodeChangedDelegate != null )
                            onNodeChangedDelegate( this, args );
                        break;
                }
            }
        }

        // The function such through schema info to find out if there exists a default attribute with passed in names in the passed in element
        // If so, return the newly created default attribute (with children tree);
        // Otherwise, return null.

        internal XmlAttribute GetDefaultAttribute( XmlElement elem, string attrPrefix, string attrLocalname, string attrNamespaceURI ) {
            SchemaInfo schInfo = DtdSchemaInfo;
            SchemaElementDecl ed = GetSchemaElementDecl( elem );
            if ( ed != null && ed.AttDefs != null ) {
                IDictionaryEnumerator attrDefs = ed.AttDefs.GetEnumerator();
                while ( attrDefs.MoveNext() ) {
                    SchemaAttDef attdef = (SchemaAttDef)attrDefs.Value;
                    if ( attdef.Presence == SchemaDeclBase.Use.Default ||
                        attdef.Presence == SchemaDeclBase.Use.Fixed ) {
                        if ( attdef.Name.Name == attrLocalname ) {
                            if ( ( schInfo.SchemaType == SchemaType.DTD && attdef.Name.Namespace == attrPrefix ) ||
                                 ( schInfo.SchemaType != SchemaType.DTD && attdef.Name.Namespace == attrNamespaceURI ) ) {
                                 //find a def attribute with the same name, build a default attribute and return
                                 XmlAttribute defattr = PrepareDefaultAttribute( attdef, attrPrefix, attrLocalname, attrNamespaceURI );
                                 return defattr;
                            }
                        }
                    }
                }
            }
            return null;
        }

        internal String Version {
            get {
                XmlDeclaration decl = Declaration;
                if ( decl != null )
                    return decl.Version;
                return null;
            }
        }

        internal String Encoding {
            get {
                XmlDeclaration decl = Declaration;
                if ( decl != null )
                    return decl.Encoding;
                return null;
            }
        }

        internal String Standalone {
            get {
                XmlDeclaration decl = Declaration;
                if ( decl != null )
                    return decl.Standalone;
                return null;
            }
        }

        internal XmlEntity GetEntityNode( String name ) {
            if ( DocumentType != null ) {
                XmlNamedNodeMap entites = DocumentType.Entities;
                if ( entites != null )
                    return (XmlEntity)(entites.GetNamedItem( name ));
            }
            return null;
        }

        public override IXmlSchemaInfo SchemaInfo {
            get {
                if (reportValidity) {
                    XmlElement documentElement = DocumentElement;
                    if (documentElement != null) {
                        switch (documentElement.SchemaInfo.Validity) {
                            case XmlSchemaValidity.Valid:
                                return ValidSchemaInfo;
                            case XmlSchemaValidity.Invalid:
                                return InvalidSchemaInfo;
                        }
                    }
                }
                return NotKnownSchemaInfo;
            }
        }

        public override String BaseURI {
            get { return baseURI; }
        }

        internal void SetBaseURI( String inBaseURI ) {
            baseURI = inBaseURI;
        }

        internal override XmlNode AppendChildForLoad( XmlNode newChild, XmlDocument doc ) {
            Debug.Assert( doc == this );

            if ( !IsValidChildType( newChild.NodeType ))
                throw new InvalidOperationException( Res.GetString(Res.Xdom_Node_Insert_TypeConflict) );

            if ( !CanInsertAfter( newChild, LastChild ) )
                throw new InvalidOperationException( Res.GetString(Res.Xdom_Node_Insert_Location) );

            XmlNodeChangedEventArgs args = GetInsertEventArgsForLoad( newChild, this );

            if ( args != null )
                BeforeEvent( args );

            XmlLinkedNode newNode = (XmlLinkedNode) newChild;

            if ( lastChild == null ) {
                newNode.next = newNode;
            }
            else {
                newNode.next = lastChild.next;
                lastChild.next = newNode;
            }

            lastChild = newNode;
            newNode.SetParentForLoad( this );

            if ( args != null )
                AfterEvent( args );

            return newNode;
        }

        internal override XPathNodeType XPNodeType { get { return XPathNodeType.Root; } }

        internal bool HasEntityReferences {
            get {
                return fEntRefNodesPresent;
            }
        }

        internal XmlAttribute NamespaceXml {  
            get {
                if (namespaceXml == null) {
                    namespaceXml = new XmlAttribute(AddAttrXmlName(strXmlns, strXml, strReservedXmlns, null), this);
                    namespaceXml.Value = strReservedXml;
                }
                return namespaceXml;
            }
        }
    }
}
