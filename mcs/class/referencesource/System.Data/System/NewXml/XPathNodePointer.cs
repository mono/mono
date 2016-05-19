//------------------------------------------------------------------------------
// <copyright file="XPathNodePointer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------
#pragma warning disable 618 // ignore obsolete warning about XmlDataDocument
namespace System.Xml {
    using System;
    using System.Data;
    using System.Diagnostics;
    using System.Xml.XPath;
  
    internal sealed class XPathNodePointer : IXmlDataVirtualNode {
        private WeakReference _owner;  // Owner of this pointer (an DataDocumentXPathNavigator). When the associated DataDocumentXPathNavigator (the owner) goes away, this XPathNodePointer can go away as well.
        private XmlDataDocument _doc;
        private XmlNode _node;
        private DataColumn _column;
        private bool _fOnValue;
        internal XmlBoundElement _parentOfNS;
        internal static int[] xmlNodeType_To_XpathNodeType_Map;
        internal static string s_strReservedXmlns = "http://www.w3.org/2000/xmlns/";
        internal static string s_strReservedXml = "http://www.w3.org/XML/1998/namespace";
        internal static string s_strXmlNS = "xmlns";
        private bool _bNeedFoliate;

        static XPathNodePointer(){
#if DEBUG
            int max=0, tempVal=0;            
            Array enumValues = Enum.GetValues(typeof(XmlNodeType));
            for ( int i = 0; i < enumValues.Length; i++) {
                tempVal = (int)enumValues.GetValue(i);
                if ( tempVal > max )
                    max = tempVal;
            }
            Debug.Assert( max == (int) XmlNodeType.XmlDeclaration );
#endif        
            xmlNodeType_To_XpathNodeType_Map = new int[20];
            xmlNodeType_To_XpathNodeType_Map[(int)(XmlNodeType.None)] = -1;      
            xmlNodeType_To_XpathNodeType_Map[(int)(XmlNodeType.Element)] = (int)XPathNodeType.Element;      
            xmlNodeType_To_XpathNodeType_Map[(int)(XmlNodeType.Attribute)] = (int)XPathNodeType.Attribute;      
            xmlNodeType_To_XpathNodeType_Map[(int)(XmlNodeType.Text)] = (int)XPathNodeType.Text;      
            xmlNodeType_To_XpathNodeType_Map[(int)(XmlNodeType.CDATA)] = (int)XPathNodeType.Text;      
            xmlNodeType_To_XpathNodeType_Map[(int)(XmlNodeType.EntityReference)] = -1;      
            xmlNodeType_To_XpathNodeType_Map[(int)(XmlNodeType.Entity)] = -1;      
            xmlNodeType_To_XpathNodeType_Map[(int)(XmlNodeType.ProcessingInstruction)] = (int)XPathNodeType.ProcessingInstruction;      
            xmlNodeType_To_XpathNodeType_Map[(int)(XmlNodeType.Comment)] = (int)XPathNodeType.Comment;      
            xmlNodeType_To_XpathNodeType_Map[(int)(XmlNodeType.Document)] = (int)XPathNodeType.Root;      
            xmlNodeType_To_XpathNodeType_Map[(int)(XmlNodeType.DocumentType)] = -1;      
            xmlNodeType_To_XpathNodeType_Map[(int)(XmlNodeType.DocumentFragment)] = (int)XPathNodeType.Root;      
            xmlNodeType_To_XpathNodeType_Map[(int)(XmlNodeType.Notation)] = -1;      
            xmlNodeType_To_XpathNodeType_Map[(int)(XmlNodeType.Whitespace)] = (int)XPathNodeType.Whitespace;      
            xmlNodeType_To_XpathNodeType_Map[(int)(XmlNodeType.SignificantWhitespace)] = (int)XPathNodeType.SignificantWhitespace;      
            xmlNodeType_To_XpathNodeType_Map[(int)(XmlNodeType.EndElement)] = -1;      
            xmlNodeType_To_XpathNodeType_Map[(int)(XmlNodeType.EndEntity)] = -1;      
            xmlNodeType_To_XpathNodeType_Map[(int)(XmlNodeType.XmlDeclaration)] = -1;      
           // xmlNodeType_To_XpathNodeType_Map[(int)(XmlNodeType.All)] = -1;      
        }

        private XPathNodeType DecideXPNodeTypeForTextNodes( XmlNode node ) {
            //the function can only be called on text like nodes.
            Debug.Assert( XmlDataDocument.IsTextNode( node.NodeType ) );
            XPathNodeType xnt = XPathNodeType.Whitespace;
            while( node != null ) {
                switch( node.NodeType ) {
                    case XmlNodeType.Whitespace :
                        break;
                    case XmlNodeType.SignificantWhitespace :
                        xnt = XPathNodeType.SignificantWhitespace; 
                        break;
                    case XmlNodeType.Text :
                    case XmlNodeType.CDATA :
                        return XPathNodeType.Text;
                    default :                        
                        return xnt;
                }                 
                node = this._doc.SafeNextSibling(node);
            }
            return xnt;
        }
        
        private XPathNodeType ConvertNodeType( XmlNode node ) {
            int xnt = -1;
            if ( XmlDataDocument.IsTextNode( node.NodeType ) )
                return DecideXPNodeTypeForTextNodes( node );
            xnt = xmlNodeType_To_XpathNodeType_Map[(int)(node.NodeType)];
            if ( xnt == (int) XPathNodeType.Attribute ) {
                if ( node.NamespaceURI == s_strReservedXmlns )
                    return XPathNodeType.Namespace;
                else 
                    return XPathNodeType.Attribute;
            }
            Debug.Assert( xnt != -1 );
            return (XPathNodeType)xnt;
        }

        private bool IsNamespaceNode( XmlNodeType nt, string ns ) {
            if ( nt == XmlNodeType.Attribute && ns == s_strReservedXmlns )
                return true;
            return false;
        }

        //when the constructor is called, the node has to be a valid XPath node at the valid location ( for example, the first
        //text/WS/SWS/CData nodes of a series continuous text-like nodes.
        internal XPathNodePointer( DataDocumentXPathNavigator owner,  XmlDataDocument doc, XmlNode node ) 
        : this ( owner, doc, node, null, false, null ){             
        }

        internal XPathNodePointer( DataDocumentXPathNavigator owner, XPathNodePointer pointer ) 
        : this ( owner, pointer._doc, pointer._node, pointer._column, pointer._fOnValue, pointer._parentOfNS ) { 
        }

        private XPathNodePointer( DataDocumentXPathNavigator owner, XmlDataDocument doc, XmlNode node, DataColumn c, bool bOnValue, XmlBoundElement parentOfNS ) {
            Debug.Assert( owner != null );
            this._owner = new WeakReference( owner );
            this._doc = doc;
            this._node = node;
            this._column = c;
            this._fOnValue = bOnValue;
            this._parentOfNS = parentOfNS;
            // Add this pointer to the document so it will be updated each time region changes it's foliation state.
            this._doc.AddPointer( (IXmlDataVirtualNode)this );
            _bNeedFoliate = false;
            AssertValid();
        }

        internal XPathNodePointer Clone( DataDocumentXPathNavigator owner ){
            //Debug.WriteLineIf( XmlTrace.traceXPathNodePointerFunctions.Enabled, "XPathNodePointer:Clone");
            RealFoliate();
            return new XPathNodePointer( owner, this ) ;
        }

        internal bool IsEmptyElement { 
            get {
                //Debug.WriteLineIf( XmlTrace.traceXPathNodePointerFunctions.Enabled, "XPathNodePointer:IsEmptyElement");
                AssertValid();
                if (_node != null && _column == null) {
                    if (_node.NodeType == XmlNodeType.Element) {
                        return((XmlElement)_node).IsEmpty;
                    }
                }
                return false;
            }
        }

        internal XPathNodeType NodeType { 
            get {
                //Debug.WriteLineIf( XmlTrace.traceXPathNodePointerFunctions.Enabled, "XPathNodePointer:NodeType");
                RealFoliate();
                AssertValid();
                if (this._node == null) {
                    return XPathNodeType.All; 
                }
                else if (this._column == null) {
                    return ConvertNodeType(this._node); 
                } 
                else if (this._fOnValue) {
                    return XPathNodeType.Text;
                } 
                else if (this._column.ColumnMapping == MappingType.Attribute) {
                    if ( this._column.Namespace == s_strReservedXmlns )
                        return XPathNodeType.Namespace;
                    else 
                        return XPathNodeType.Attribute;
                } 
                else //
                    return XPathNodeType.Element;
            }
        }

        //[....]: From CodeReview: Perf: We should have another array similar w/ 
        //  xmlNodeType_To_XpathNodeType_Map that will return String.Empty for everything but the element and
        //  attribute case.
        internal string LocalName { 
            get {
                //Debug.WriteLineIf( XmlTrace.traceXPathNodePointerFunctions.Enabled, "XPathNodePointer:LocalName");
                RealFoliate();
                AssertValid();
                if (this._node == null) {
                    return string.Empty;
                } 
                else if (this._column == null) {
                    XmlNodeType nt = this._node.NodeType;
                    if ( IsNamespaceNode( nt, this._node.NamespaceURI ) && this._node.LocalName == s_strXmlNS )
                        return string.Empty;
                    if ( nt == XmlNodeType.Element || nt == XmlNodeType.Attribute || nt == XmlNodeType.ProcessingInstruction )
                        return _node.LocalName;
                    return string.Empty;
                } 
                else if (this._fOnValue) {
                    return String.Empty;
                } 
                else //when column is not null
                    return _doc.NameTable.Add(_column.EncodedColumnName);
            }
        }

        //note that, we've have lost the prefix in this senario ( defoliation will toss prefix away. )
        internal string Name {
            get {
                //Debug.WriteLineIf( XmlTrace.traceXPathNodePointerFunctions.Enabled, "XPathNodePointer:Name");
                RealFoliate();
                AssertValid();
                if (this._node == null) {
                    return string.Empty;
                } 
                else if (this._column == null) {
                    XmlNodeType nt = this._node.NodeType;
                    if ( IsNamespaceNode( nt, this._node.NamespaceURI ) ) {
                        if ( this._node.LocalName == s_strXmlNS )
                            return string.Empty;
                        else
                            return this._node.LocalName;
                    }
                    if ( nt == XmlNodeType.Element || nt == XmlNodeType.Attribute || nt == XmlNodeType.ProcessingInstruction )
                        return _node.Name;
                    return string.Empty;
                } 
                else if (this._fOnValue) {
                    return String.Empty;
                } 
                else { //when column is not null
                    //we've lost prefix in this senario.
                    return _doc.NameTable.Add(_column.EncodedColumnName);
                }
            }
        }
        
        internal string NamespaceURI { 
            get {
                //Debug.WriteLineIf( XmlTrace.traceXPathNodePointerFunctions.Enabled, "XPathNodePointer:NamespaceURI");
                RealFoliate();
                AssertValid();
                if (this._node == null) {
                    return string.Empty;
                } 
                else if (this._column == null) {
                    XPathNodeType xnt = ConvertNodeType( this._node );
                    if ( xnt == XPathNodeType.Element || xnt == XPathNodeType.Root || xnt == XPathNodeType.Attribute ) 
                        return _node.NamespaceURI; 
                    return string.Empty;
                } 
                else if (this._fOnValue) {
                    return string.Empty;
                } 
                else { //When column is not null
                    if ( _column.Namespace == s_strReservedXmlns )
                        //namespace nodes has empty string as namespaceURI
                        return string.Empty;
                    return _doc.NameTable.Add(_column.Namespace);
                }
            }
        }

        //
        internal string Prefix { 
            get {
                //Debug.WriteLineIf( XmlTrace.traceXPathNodePointerFunctions.Enabled, "XPathNodePointer:Prefix");
                RealFoliate();
                AssertValid();
                if (this._node == null) {
                    return string.Empty;
                } 
                else if (this._column == null) {
                    if ( IsNamespaceNode( this._node.NodeType, this._node.NamespaceURI ) )
                        return string.Empty;
                    return _node.Prefix; 
                }
                return string.Empty;
            }
        }

        internal string Value { 
            get { 
                //Debug.WriteLineIf( XmlTrace.traceXPathNodePointerFunctions.Enabled, "XPathNodePointer:Value");
                RealFoliate();
                AssertValid();
                if (this._node == null) 
                    return null;
                else if (this._column == null) {                    
                    string strRet = this._node.Value;
                    if ( XmlDataDocument.IsTextNode( this._node.NodeType ) ) {
                        //concatenate adjacent textlike nodes
                        XmlNode parent = this._node.ParentNode;
                        if ( parent == null )
                            return strRet; 
                        XmlNode n = _doc.SafeNextSibling(this._node);
                        while ( n != null && XmlDataDocument.IsTextNode( n.NodeType ) ) {                            
                            strRet += n.Value;
                            n = _doc.SafeNextSibling(n);
                        }
                    }
                    return strRet;
                }
                else if (this._column.ColumnMapping == MappingType.Attribute || this._fOnValue) {
                    DataRow row = this.Row;
                    DataRowVersion rowVersion = ( row.RowState == DataRowState.Detached ) ? DataRowVersion.Proposed : DataRowVersion.Current;
                    object value = row[ this._column, rowVersion ];
                    if ( ! Convert.IsDBNull( value ) )
                        return this._column.ConvertObjectToXml( value );
                    return null;
                }
                else 
                    //
                    return null;
            }
        }

        internal string InnerText { 
            get {
                //Debug.WriteLineIf( XmlTrace.traceXPathNodePointerFunctions.Enabled, "XPathNodePointer:InnerText");
                RealFoliate();
                AssertValid();
                if (this._node == null) {
                    return string.Empty;
                }
                else if (this._column == null) {
                    //
                    if ( this._node.NodeType == XmlNodeType.Document ) {
                        //document node's region should always be uncompressed
                        XmlElement rootElem = ((XmlDocument)this._node).DocumentElement;
                        if ( rootElem != null )
                            return rootElem.InnerText; 
                        return string.Empty;
                    }
                    else
                        return this._node.InnerText;
                }
                else {
                    DataRow row = this.Row;
                    DataRowVersion rowVersion = ( row.RowState == DataRowState.Detached ) ? DataRowVersion.Proposed : DataRowVersion.Current;
                    object value = row[ this._column, rowVersion ];
                    if ( ! Convert.IsDBNull( value ) )
                        return this._column.ConvertObjectToXml( value );
                    return string.Empty;
                }
            }
        }

        internal String BaseURI {
            get { 
                //Debug.WriteLineIf( XmlTrace.traceXPathNodePointerFunctions.Enabled, "XPathNodePointer:BaseURI");
                RealFoliate();
                if ( this._node != null )
                    return this._node.BaseURI;
                return String.Empty; 
            }
        }

        internal String XmlLang { 
            get { 
                //Debug.WriteLineIf( XmlTrace.traceXPathNodePointerFunctions.Enabled, "XPathNodePointer:XmlLang");
                RealFoliate();
                XmlNode curNode = this._node;
                XmlBoundElement curBoundElem = null;
                object colVal = null;
                while ( curNode != null ) {
                    //

                    curBoundElem = curNode as XmlBoundElement;
                    if ( curBoundElem != null ) {
                        //this._doc.Foliate( curBoundElem, ElementState.WeakFoliation );
                        if ( curBoundElem.ElementState == ElementState.Defoliated ) {
                            //if not foliated, going through the columns to get the xml:lang
                            DataRow row = curBoundElem.Row;
                            foreach( DataColumn col in row.Table.Columns ) {
                                if ( col.Prefix == "xml" && col.EncodedColumnName == "lang" ) {
                                    colVal = row[col];
                                    if ( colVal == DBNull.Value )
                                        break; //goto its ancesstors
                                    return (String) colVal;
                                }
                            }
                        } 
                        else {
                            //if folicated, get the attribute directly
                            if ( curBoundElem.HasAttribute( "xml:lang" ) ) 
                                return curBoundElem.GetAttribute( "xml:lang" );                            
                        }
                    }
                    if ( curNode.NodeType == XmlNodeType.Attribute )
                        curNode = ((XmlAttribute)curNode).OwnerElement;
                    else
                        curNode = curNode.ParentNode;                    
                }
                return String.Empty;
            } 
        }
        
        private XmlBoundElement GetRowElement() {
            //Debug.WriteLineIf( XmlTrace.traceXPathNodePointerFunctions.Enabled, "XPathNodePointer:GetRowElement()");
            //AssertValid();

            XmlBoundElement rowElem;
            if ( this._column != null ) {
                rowElem = this._node as XmlBoundElement;
                Debug.Assert( rowElem != null );
                Debug.Assert( rowElem.Row != null );
                return rowElem;
            }

            _doc.Mapper.GetRegion( this._node, out rowElem );
            return rowElem;
        }

        //
        private DataRow Row {
            get { 
                //Debug.WriteLineIf( XmlTrace.traceXPathNodePointerFunctions.Enabled, "XPathNodePointer:Row");
                //AssertValid();
                XmlBoundElement rowElem = GetRowElement();
                if ( rowElem == null )
                    return null;

                Debug.Assert( rowElem.Row != null );
                return rowElem.Row;
            }
        }

        internal bool MoveTo( XPathNodePointer pointer ) {
            //Debug.WriteLineIf( XmlTrace.traceXPathNodePointerFunctions.Enabled, "XPathNodePointer:MoveTo(pointer)");
            AssertValid();
            if ( this._doc != pointer._doc )
                return false;
            /*
            XmlDataDocument docOld = this._doc;
            XmlDataDocument docNew = pointer._doc;
            if ( docNew != docOld ) {
                this._doc.RemovePointer( this );
                this._doc = pointer._doc;
                this._doc.AddPointer( this );
            } 
            */
            this._node = pointer._node;
            this._column = pointer._column;
            this._fOnValue = pointer._fOnValue;
            this._bNeedFoliate = pointer._bNeedFoliate;
            AssertValid();
            return true;
        }

        private void MoveTo( XmlNode node ) {
            //Debug.WriteLineIf( XmlTrace.traceXPathNodePointerFunctions.Enabled, "XPathNodePointer:MoveTo(node)");
            //AssertValid();
            // Should not move outside of this document
            Debug.Assert( node == this._doc || node.OwnerDocument == this._doc );
            this._node = node;
            this._column = null;
            this._fOnValue = false;
            //AssertValid();
        }

        private void MoveTo( XmlNode node, DataColumn column, bool _fOnValue ) {
            //Debug.WriteLineIf( XmlTrace.traceXPathNodePointerFunctions.Enabled, "XPathNodePointer:MoveTo(node, column, fOnValue)");
            //AssertValid();
            // Should not move outside of this document
            Debug.Assert( node == this._doc || node.OwnerDocument == this._doc );
            this._node = node;
            this._column = column;
            this._fOnValue = _fOnValue;
            //AssertValid();
        }

        private bool IsFoliated( XmlNode node ) {
            //Debug.WriteLineIf( XmlTrace.traceXPathNodePointerFunctions.Enabled, "XPathNodePointer:IsFoliated(node)");
            //AssertValid();
            if (node != null && node is XmlBoundElement)
                return((XmlBoundElement)node).IsFoliated;
            return true;
        }

        private int ColumnCount( DataRow row, bool fAttribute ) {
            //Debug.WriteLineIf( XmlTrace.traceXPathNodePointerFunctions.Enabled, "XPathNodePointer:ColumnCount(row,fAttribute)");
            DataColumn c = null;
            int count = 0;
            while ((c = NextColumn( row, c, fAttribute )) != null) {
                if ( c.Namespace != s_strReservedXmlns )
                    count++;
            }
            return count;
        }

        internal int AttributeCount {
            get {
                //Debug.WriteLineIf( XmlTrace.traceXPathNodePointerFunctions.Enabled, "XPathNodePointer:AttributeCount");
                RealFoliate();
                AssertValid();
                if (_node != null) {
                    if (_column == null && _node.NodeType == XmlNodeType.Element) {
                        if (!IsFoliated( _node )) 
                            return ColumnCount( Row, true );
                        else {
                            int nc = 0;
                            foreach ( XmlAttribute attr in _node.Attributes ) {
                                if ( attr.NamespaceURI != s_strReservedXmlns )
                                    nc++;
                            }
                            return nc;
                        }
                    }
                }
                return 0;
            }            
        }

        internal DataColumn NextColumn( DataRow row, DataColumn col, bool fAttribute ) {
            //Debug.WriteLineIf( XmlTrace.traceXPathNodePointerFunctions.Enabled, "XPathNodePointer:NextColumn(row,col,fAttribute)");
            if (row.RowState == DataRowState.Deleted)
                return null;

            DataTable table = row.Table;
            DataColumnCollection columns = table.Columns;
            int iColumn = (col != null) ? col.Ordinal + 1 : 0;
            int cColumns = columns.Count;
            DataRowVersion rowVersion = ( row.RowState == DataRowState.Detached ) ? DataRowVersion.Proposed : DataRowVersion.Current;

            for (; iColumn < cColumns; iColumn++) {
                DataColumn c = columns[iColumn];
                if (!_doc.IsNotMapped( c ) && (c.ColumnMapping == MappingType.Attribute) == fAttribute && ! Convert.IsDBNull( row[c, rowVersion] ) )
                    return c;
            }

            return null;
        }

        internal DataColumn PreviousColumn( DataRow row, DataColumn col, bool fAttribute ) {
            //Debug.WriteLineIf( XmlTrace.traceXPathNodePointerFunctions.Enabled, "XPathNodePointer:PreviousColumn(row,col,fAttribute)");
            if (row.RowState == DataRowState.Deleted)
                return null;

            DataTable table = row.Table;
            DataColumnCollection columns = table.Columns;
            int iColumn = (col != null) ? col.Ordinal - 1 : columns.Count - 1;
            int cColumns = columns.Count;
            DataRowVersion rowVersion = ( row.RowState == DataRowState.Detached ) ? DataRowVersion.Proposed : DataRowVersion.Current;

            for (; iColumn >= 0; iColumn--) {
                DataColumn c = columns[iColumn];
                if (!_doc.IsNotMapped( c ) && (c.ColumnMapping == MappingType.Attribute) == fAttribute && !Convert.IsDBNull( row[c, rowVersion] ) )
                    return c;
            }

            return null;
        }

        internal bool MoveToAttribute( string localName, string namespaceURI ) {
            //Debug.WriteLineIf( XmlTrace.traceXPathNodePointerFunctions.Enabled, "XPathNodePointer:MoveToAttribute(localName, namespaceURI)");
            RealFoliate();
            AssertValid();
            if ( namespaceURI == s_strReservedXmlns )
                return false;
            if (_node != null) {
                //_column.ColumnMapping checkin below is not really needed since the pointer should be pointing at the node before this
                // function should even be called ( there is always a call MoveToOwnerElement() before MoveToAttribute(..)
                if ((_column == null || _column.ColumnMapping == MappingType.Attribute) && _node.NodeType == XmlNodeType.Element) {
                    if (!IsFoliated( _node )) {
                        DataColumn c = null;
                        while ((c = NextColumn( Row, c, true )) != null) {
                            if (c.EncodedColumnName == localName && c.Namespace == namespaceURI) {
                                MoveTo( _node, c, false );
                                return true;
                            }
                        }
                    } 
                    else {
                        Debug.Assert( _node.Attributes != null );
                        XmlNode n = _node.Attributes.GetNamedItem(localName, namespaceURI);
                        if (n != null) {
                            MoveTo( n, null, false );
                            return true;
                        }
                    }
                }
            }
            return false;
        }
      
        internal bool MoveToNextAttribute( bool bFirst ) {
            //Debug.WriteLineIf( XmlTrace.traceXPathNodePointerFunctions.Enabled, "XPathNodePointer:MoveToNextAttribute(bFirst)");
            //
            RealFoliate();
            AssertValid();
            if (_node != null) {
                if ( bFirst && ( _column != null || _node.NodeType != XmlNodeType.Element ) )
                    return false;
                if ( !bFirst ) {
                    if ( _column != null && _column.ColumnMapping != MappingType.Attribute ) 
                        return false;
                    if ( _column == null && _node.NodeType != XmlNodeType.Attribute )
                        return false;
                }
                if ( !IsFoliated( _node ) ) {
                    DataColumn c = _column;
                    while ( ( c = NextColumn( Row, c, true ) ) != null ) {
                        if ( c.Namespace != s_strReservedXmlns ) {
                            MoveTo( _node, c, false );
                            return true;
                        }
                    }
                    return false;
                }
                else {
                    if ( bFirst ) {
                        XmlAttributeCollection attrs = _node.Attributes;
                        foreach ( XmlAttribute attr in attrs ) {
                            if ( attr.NamespaceURI != s_strReservedXmlns ) {
                                MoveTo( attr, null, false );
                                return true;
                            }
                        }
                    } 
                    else {
                        XmlAttributeCollection attrs = ((XmlAttribute)_node).OwnerElement.Attributes;
                        bool bFound = false;
                        foreach ( XmlAttribute attr in attrs ) {
                            if ( bFound && attr.NamespaceURI != s_strReservedXmlns ) {
                                MoveTo( attr, null, false );
                                return true;
                            }
                            if ( attr == _node )
                                bFound = true;
                        }
                    }
                }
            }
            return false;
        }
        
        private bool IsValidChild( XmlNode parent, XmlNode child ) {
            //Debug.WriteLineIf( XmlTrace.traceXPathNodePointerFunctions.Enabled, "XPathNodePointer:IsValidChild(parent,child)");
            int xntChildInt = xmlNodeType_To_XpathNodeType_Map[(int)(child.NodeType)];
            if ( xntChildInt == -1 )
                return false;
            int xntInt = xmlNodeType_To_XpathNodeType_Map[(int)(parent.NodeType)];
            Debug.Assert( xntInt != -1 );
            switch ( xntInt ) {
                case (int)XPathNodeType.Root:
                    return ( xntChildInt == (int)XPathNodeType.Element ||
                             xntChildInt == (int)XPathNodeType.Comment ||
                             xntChildInt == (int)XPathNodeType.ProcessingInstruction );
                case (int)XPathNodeType.Element:
                    return ( xntChildInt == (int)XPathNodeType.Element ||
                             xntChildInt == (int)XPathNodeType.Text ||
                             xntChildInt == (int)XPathNodeType.Comment ||
                             xntChildInt == (int)XPathNodeType.Whitespace ||
                             xntChildInt == (int)XPathNodeType.SignificantWhitespace ||
                             xntChildInt == (int)XPathNodeType.ProcessingInstruction );
                default :
                    return false;                    
            }
        }

        private bool IsValidChild( XmlNode parent, DataColumn c ) {
            //Debug.WriteLineIf( XmlTrace.traceXPathNodePointerFunctions.Enabled, "XPathNodePointer:IsValidChild(parent,c)");
            int xntInt = xmlNodeType_To_XpathNodeType_Map[(int)(parent.NodeType)];
            Debug.Assert( xntInt != -1 );
            switch ( xntInt ) {
                case (int)XPathNodeType.Root:
                    return c.ColumnMapping == MappingType.Element;
                case (int)XPathNodeType.Element:
                    return ( c.ColumnMapping == MappingType.Element || c.ColumnMapping == MappingType.SimpleContent );
                default :
                    return false;                    
            }            
        }
        
        internal bool MoveToNextSibling() {
            //Debug.WriteLineIf( XmlTrace.traceXPathNodePointerFunctions.Enabled, "XPathNodePointer:MoveToNextSibling()");
            RealFoliate();
            AssertValid();
            if (_node != null) {
                if ( _column != null ) {
                    if ( _fOnValue ) { 
                        // _fOnValue could be true only when the column is mapped as simplecontent or element
                        Debug.Assert( _column.ColumnMapping != MappingType.Attribute && _column.ColumnMapping != MappingType.Hidden );
                        return false;
                    }
                    DataRow curRow = Row;
                    DataColumn c = NextColumn( curRow, _column, false ); 
                    while ( c != null ) {
                        if ( IsValidChild( _node, c ) ) {
                            MoveTo( this._node, c, _doc.IsTextOnly(c));
                            return true;
                        }
                        c = NextColumn( curRow, c, false );
                    } 
                    XmlNode n = _doc.SafeFirstChild( _node );
                    if (n != null) {
                        MoveTo( n );
                        return true;
                    }
                } 
                else {
                    XmlNode n = _node;
                    XmlNode parent = _node.ParentNode;
                    if ( parent == null )
                        return false;
                    bool bTextLike = XmlDataDocument.IsTextNode( _node.NodeType );
                    do {
                        do {
                            n = _doc.SafeNextSibling(n);
                        } while ( n != null && bTextLike && XmlDataDocument.IsTextNode( n.NodeType ));
                    } while ( n != null && !IsValidChild(parent, n) );
                    if (n != null) {
                        MoveTo(n);
                        return true;
                    }
                }
            }
            return false;
        }
        
        internal bool MoveToPreviousSibling() {
            //Debug.WriteLineIf( XmlTrace.traceXPathNodePointerFunctions.Enabled, "XPathNodePointer:MoveToPreviousSibling()");
            RealFoliate();
            AssertValid();
            if (_node != null) {
                if (_column != null) {
                    if (_fOnValue)
                        return false;
                    DataRow curRow = Row;
                    DataColumn c = PreviousColumn( curRow, _column, false );
                    while ( c != null ) {
                        if ( IsValidChild(_node,c)) {
                            MoveTo( _node, c, _doc.IsTextOnly(c) );
                            return true;
                        }
                        c = PreviousColumn( curRow, c , false );
                    } 
                }
                else {
                    XmlNode n = _node;
                    XmlNode parent = _node.ParentNode;
                    if ( parent == null )
                        return false;
                    bool bTextLike = XmlDataDocument.IsTextNode( _node.NodeType );
                    do {
                        do {
                            n = _doc.SafePreviousSibling( n );
                        } while ( n != null && bTextLike && XmlDataDocument.IsTextNode( n.NodeType ) );
                    } while ( n != null && !IsValidChild(parent, n) );
                    if (n != null) {
                        MoveTo(n);
                        return true;
                    }
                    if (!IsFoliated( parent ) && (parent is XmlBoundElement)) {
                        DataRow row = ((XmlBoundElement)parent).Row;
                        if (row != null) {
                            DataColumn c = PreviousColumn( row, null, false );
                            if (c != null) {
                                MoveTo( parent, c, _doc.IsTextOnly(c) );
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        internal bool MoveToFirst() {
            //Debug.WriteLineIf( XmlTrace.traceXPathNodePointerFunctions.Enabled, "XPathNodePointer:MoveToFirst()");
            RealFoliate();
            AssertValid();
            if (_node != null) {
                DataRow curRow = null;
                XmlNode parent = null;
                if (_column != null) {
                    curRow = Row;
                    parent = _node;
                } 
                else {
                    parent = _node.ParentNode;
                    if ( parent == null )
                        return false;
                    if ( !IsFoliated( parent ) && (parent is XmlBoundElement) ) 
                        curRow = ((XmlBoundElement)parent).Row;
                } 
                //first check with the columns in the row
                if ( curRow != null ) {
                    DataColumn c = NextColumn( curRow, null, false );
                    while ( c != null ) {
                        if ( IsValidChild( _node, c ) ) {
                            MoveTo( _node, c, _doc.IsTextOnly( c ) );
                            return true;
                        }
                        c = NextColumn( curRow, c, false );
                    } 
                }
                //didn't find a valid column or maybe already Foliated, go through its children nodes
                XmlNode n = _doc.SafeFirstChild( parent );
                while ( n != null ) {
                    if ( IsValidChild( parent, n ) ) {
                        MoveTo( n );
                        return true;
                    }
                    n = _doc.SafeNextSibling( n );
                } 
            }
            return false;
        }

        internal bool HasChildren {
            get {
                //Debug.WriteLineIf( XmlTrace.traceXPathNodePointerFunctions.Enabled, "XPathNodePointer:HasChildren");
                RealFoliate();
                AssertValid();
                if (_node == null)
                    return false;

                if (_column != null) {
                    if ( _column.ColumnMapping == MappingType.Attribute || _column.ColumnMapping == MappingType.Hidden )
                        return false;
                    return !_fOnValue;
                } 
                if (!IsFoliated( _node )) {
                    // find virtual column elements first
                    DataRow curRow = Row;
                    DataColumn c = NextColumn( curRow, null, false );
                    while ( c != null ) {
                        if ( IsValidChild( _node, c) ) 
                            return true;
                        c = NextColumn( curRow, c, false );
                    } 
                }
                // look for anything
                XmlNode n = _doc.SafeFirstChild( _node );
                while ( n != null ) {
                    if ( IsValidChild( _node, n ) ) 
                        return true;
                    n = _doc.SafeNextSibling( n );
                } 

                return false;
            }
        }
        
        internal bool MoveToFirstChild() {
            //Debug.WriteLineIf( XmlTrace.traceXPathNodePointerFunctions.Enabled, "XPathNodePointer:MoveToFirstChild()");
            RealFoliate();
            AssertValid();
            if (_node == null)
                return false;

            if (_column != null) {
                if ( _column.ColumnMapping == MappingType.Attribute || _column.ColumnMapping == MappingType.Hidden )
                    return false;
                if (_fOnValue) //text node has no children to move to
                    return false;
                _fOnValue = true;
                return true;
            } 
            if (!IsFoliated( _node )) {
                // find virtual column elements first
                DataRow curRow = Row;
                DataColumn c = NextColumn( curRow, null, false );
                while ( c != null ) {
                    if ( IsValidChild( _node, c) ) {
                        MoveTo( _node, c, _doc.IsTextOnly(c) );
                        return true;
                    }
                    c = NextColumn( curRow, c, false );
                } 
            }
            // look for anything
            XmlNode n = _doc.SafeFirstChild( _node );
            while ( n != null ) {
                if ( IsValidChild( _node, n ) ) {
                    MoveTo(n);
                    return true;
                }
                n = _doc.SafeNextSibling( n );
            } 

            return false;
        }
        
        //this version of MoveToParent will consider Attribute type position and move to its owner element
        //
        internal bool MoveToParent() {
            //Debug.WriteLineIf( XmlTrace.traceXPathNodePointerFunctions.Enabled, "XPathNodePointer:MoveToParent()");        
            RealFoliate();
            AssertValid();
            if ( NodeType == XPathNodeType.Namespace ) {
                Debug.Assert( _parentOfNS != null );
                MoveTo( _parentOfNS );
                return true;
            }
            if (_node != null) {
                if (_column != null) {
                    if (_fOnValue && !_doc.IsTextOnly(_column)) {
                        MoveTo( _node, _column, false );
                        return true;
                    }
                    MoveTo( _node, null, false );
                    return true;
                }
                else {
                    XmlNode n = null;
                    if ( _node.NodeType == XmlNodeType.Attribute )
                        n = ((XmlAttribute)_node).OwnerElement;
                    else 
                        n = _node.ParentNode;
                    if (n != null) {
                        MoveTo(n);
                        return true;
                    }
                }
            }
            return false;
        }

        private XmlNode GetParent( XmlNode node ) {
            //Debug.WriteLineIf( XmlTrace.traceXPathNodePointerFunctions.Enabled, "XPathNodePointer:GetParent(node)");
            XPathNodeType xnt = ConvertNodeType( node );
            if ( xnt == XPathNodeType.Namespace ) {
                Debug.Assert( _parentOfNS != null );
                return _parentOfNS;
            }
            if ( xnt == XPathNodeType.Attribute )
                return ((XmlAttribute)node).OwnerElement;
            return node.ParentNode;
        }
        
        internal void MoveToRoot() {
            //Debug.WriteLineIf( XmlTrace.traceXPathNodePointerFunctions.Enabled, "XPathNodePointer:MoveToRoot()");
            XmlNode node = this._node;
            XmlNode parent = this._node;
            while ( parent != null ) {
                node = parent;
                parent = GetParent(parent);
            }
            this._node = node;
            this._column = null;
            this._fOnValue = false;
            AssertValid();
        }
        
        internal bool IsSamePosition( XPathNodePointer pointer ) {
            //Debug.WriteLineIf( XmlTrace.traceXPathNodePointerFunctions.Enabled, "XPathNodePointer:IsSamePosition(pointer)");
            RealFoliate();
            pointer.RealFoliate();
            AssertValid();
            pointer.AssertValid();
            if (_column == null && pointer._column == null)
                return ( pointer._node == this._node && pointer._parentOfNS == this._parentOfNS );

            return ( pointer._doc == this._doc 
                    && pointer._node == this._node 
                    && pointer._column == this._column 
                    && pointer._fOnValue == this._fOnValue 
                    && pointer._parentOfNS == this._parentOfNS );
        }

        private XmlNodeOrder CompareNamespacePosition( XPathNodePointer other ) {
            //Debug.WriteLineIf( XmlTrace.traceXPathNodePointerFunctions.Enabled, "XPathNodePointer:CompareNamespacePostion(other)");
            XPathNodePointer xp1 = this.Clone((DataDocumentXPathNavigator)(this._owner.Target));
            XPathNodePointer xp2 = other.Clone((DataDocumentXPathNavigator)(other._owner.Target));
            while ( xp1.MoveToNextNamespace(XPathNamespaceScope.All) ) {
                if ( xp1.IsSamePosition( other ) )
                    return XmlNodeOrder.Before;
            }
            return XmlNodeOrder.After;
        }
        
        private static XmlNode GetRoot( XmlNode node, ref int depth ) {
            //Debug.WriteLineIf( XmlTrace.traceXPathNodePointerFunctions.Enabled, "XPathNodePointer:GetRoot(node, depth)");
            depth = 0;
            XmlNode curNode = node;
            XmlNode parent = ( ( curNode.NodeType == XmlNodeType.Attribute ) ? ( ((XmlAttribute)curNode).OwnerElement ) : ( curNode.ParentNode ) );
            for ( ; parent != null; depth++ ) {
                curNode = parent; 
                parent = curNode.ParentNode; // no need to check for attribute since navigator can't be built on its children or navigate to its children 
            }
            return curNode;  
        }

        internal XmlNodeOrder ComparePosition( XPathNodePointer other ) {
            //Debug.WriteLineIf( XmlTrace.traceXPathNodePointerFunctions.Enabled, "XPathNodePointer:ComparePosition(other)");
            RealFoliate();
            other.RealFoliate();
            Debug.Assert( other != null );
            if ( IsSamePosition( other ) )
                return XmlNodeOrder.Same;
            XmlNode curNode1 = null, curNode2 = null;
            //deal with namespace node first
            if ( this.NodeType == XPathNodeType.Namespace && other.NodeType == XPathNodeType.Namespace ) {
                if ( this._parentOfNS == other._parentOfNS )
                    return this.CompareNamespacePosition( other );
                //if not from the same parent
                curNode1 = this._parentOfNS;
                curNode2 = other._parentOfNS;
            }
            else if ( this.NodeType == XPathNodeType.Namespace ) {
                Debug.Assert( other.NodeType != XPathNodeType.Namespace );
                if ( this._parentOfNS == other._node ) {
                    //from the same region, NS nodes come before all other nodes
                    if ( other._column == null )
                        return XmlNodeOrder.After;
                    else
                        return XmlNodeOrder.Before;
                }
                //if not from the same region
                curNode1 = this._parentOfNS;
                curNode2 = other._node;
            } 
            else if ( other.NodeType == XPathNodeType.Namespace ) {
                Debug.Assert( this.NodeType != XPathNodeType.Namespace );
                if ( this._node == other._parentOfNS ) {
                    //from the same region
                    if ( this._column == null )
                        return XmlNodeOrder.Before;
                    else
                        return XmlNodeOrder.After;
                }
                //if not from the same region
                curNode1 = this._node;
                curNode2 = other._parentOfNS;
            }
            else {
                if ( this._node == other._node ) {
                    //compare within the same region
                    if ( this._column == other._column ) {
                        //one is the children of the other
                        Debug.Assert( this._fOnValue != other._fOnValue );
                        if ( this._fOnValue )
                            return XmlNodeOrder.After;
                        else
                            return XmlNodeOrder.Before;
                    }
                    else {
                        Debug.Assert( this.Row == other.Row ); //in the same row
                        if ( this._column == null ) 
                            return XmlNodeOrder.Before;
                        else if ( other._column == null ) 
                            return XmlNodeOrder.After;
                        else if ( this._column.Ordinal < other._column.Ordinal )
                            return XmlNodeOrder.Before;
                        else
                            return XmlNodeOrder.After;
                    }
                }
                curNode1 = this._node;
                curNode2 = other._node;
                
            }

            Debug.Assert( curNode1 != null );
            Debug.Assert( curNode2 != null );

            if (curNode1 == null || curNode2 == null) {
                return XmlNodeOrder.Unknown;
            }


            int depth1 = -1, depth2 = -1;
            XmlNode root1 = XPathNodePointer.GetRoot( curNode1, ref depth1 );
            XmlNode root2 = XPathNodePointer.GetRoot( curNode2, ref depth2 );
            if ( root1 != root2 ) 
                return XmlNodeOrder.Unknown;

            if ( depth1 > depth2 ) {
                while ( curNode1 != null && depth1 > depth2 ) {
                    curNode1 = ( ( curNode1.NodeType == XmlNodeType.Attribute ) ? ( ((XmlAttribute)curNode1).OwnerElement ) : ( curNode1.ParentNode ) );
                    depth1--;
                }
                if ( curNode1 == curNode2 )
                    return XmlNodeOrder.After;
            }
            else if ( depth2 > depth1 ) {
                while ( curNode2 != null && depth2 > depth1 ) {
                    curNode2 = ( ( curNode2.NodeType == XmlNodeType.Attribute ) ? ( ((XmlAttribute)curNode2).OwnerElement ) : ( curNode2.ParentNode ) );
                    depth2--;
                }
                if ( curNode1 == curNode2 )
                    return XmlNodeOrder.Before;
            }

            XmlNode parent1 = GetParent(curNode1);
            XmlNode parent2 = GetParent(curNode2);
            XmlNode nextNode = null;
            while ( parent1 != null && parent2 != null ) {
                if ( parent1 == parent2 ) {
                    while (curNode1 != null ) {
                        nextNode = curNode1.NextSibling;
                        if ( nextNode == curNode2 )
                            return XmlNodeOrder.Before;
                        curNode1 = nextNode;
                    }
                    return XmlNodeOrder.After;
                }
                curNode1 = parent1;
                curNode2 = parent2;
                parent1 = curNode1.ParentNode;
                parent2 = curNode2.ParentNode;
            }
            
            //logically, we shouldn't reach here
            Debug.Assert( false );
            return XmlNodeOrder.Unknown;
        }
        
        internal XmlNode Node {
            get {
                //Debug.WriteLineIf( XmlTrace.traceXPathNodePointerFunctions.Enabled, "XPathNodePointer:Node");
                RealFoliate();
                AssertValid();

                if ( this._node == null )
                    return null;

                XmlBoundElement rowElem = GetRowElement();
                if ( rowElem != null ) {
                    //lock ( this._doc.pointers ) {
                        bool wasFoliationEnabled = this._doc.IsFoliationEnabled;
                        this._doc.IsFoliationEnabled = true;
                        this._doc.Foliate( rowElem, ElementState.StrongFoliation );
                        this._doc.IsFoliationEnabled = wasFoliationEnabled;
                    //}
                }
                RealFoliate();
                AssertValid();
                return this._node;
            }
        }

        bool IXmlDataVirtualNode.IsOnNode( XmlNode nodeToCheck ) {
            //Debug.WriteLineIf( XmlTrace.traceXPathNodePointerFunctions.Enabled, "XPathNodePointer:IsOnNode(nodeToCheck)");
            RealFoliate();
            return nodeToCheck == this._node;
        }

        bool IXmlDataVirtualNode.IsOnColumn( DataColumn col ) {
            //Debug.WriteLineIf( XmlTrace.traceXPathNodePointerFunctions.Enabled, "XPathNodePointer:IsOnColumn(col)");
            RealFoliate();
            return col == this._column;
        }
        
        void IXmlDataVirtualNode.OnFoliated( XmlNode foliatedNode ) {
            //Debug.WriteLineIf( XmlTrace.traceXPathNodePointerFunctions.Enabled, "XPathNodePointer:OnFoliated(foliatedNode)");
            // update the pointer if the element node has been foliated
            if (_node == foliatedNode) {
                // if already on this node, nothing to do!
                if (_column == null)
                    return;
                _bNeedFoliate = true;
            }
        }

        private void RealFoliate() {
            if ( !_bNeedFoliate )
                return;
            //Debug.WriteLineIf( XmlTrace.traceXPathNodePointerFunctions.Enabled, "XPathNodePointer:RealFoliate()");
            _bNeedFoliate = false;

            Debug.Assert( this._column != null );
        
            XmlNode n = null;

            if (_doc.IsTextOnly( _column ))
                n = _node.FirstChild;
            else  {
                if (_column.ColumnMapping == MappingType.Attribute) {
                    n = _node.Attributes.GetNamedItem( _column.EncodedColumnName, _column.Namespace );
                }
                else {
                    for (n = _node.FirstChild; n != null; n = n.NextSibling) {
                        if (n.LocalName == _column.EncodedColumnName && n.NamespaceURI == _column.Namespace)
                            break;
                    }
                }

                if (n != null && _fOnValue)
                    n = n.FirstChild;
            }

            if (n == null)
                throw new InvalidOperationException(Res.GetString(Res.DataDom_Foliation));

            // Cannot use MoveTo( n ); b/c the initial state for MoveTo is invalid (region is foliated but this is not)            
            this._node = n;
            this._column = null;
            this._fOnValue = false;
            AssertValid();
            _bNeedFoliate = false;
        }


        //The function only helps to find out if there is a namespace declaration of given name is defined on the given node
        //It will not check the accestors of the given node.
        private string GetNamespace( XmlBoundElement be, string name ) {
            //Debug.WriteLineIf( XmlTrace.traceXPathNodePointerFunctions.Enabled, "XPathNodePointer:GetNamespace(be,name)");

            if ( be == null )
                return null;
            XmlAttribute attr = null;
            if ( be.IsFoliated ) {
                attr = be.GetAttributeNode ( name, s_strReservedXmlns );
                if ( attr != null )
                    return attr.Value;
                else
                    return null;
            } 
            else { //defoliated so that we need to search through its column 
                DataRow curRow = be.Row;
                if ( curRow == null )
                    return null;
                //going through its attribute columns
                DataColumn curCol = PreviousColumn( curRow, null, true );
                while ( curCol != null ) {
                    if ( curCol.Namespace == s_strReservedXmlns ) {
                        //
                        DataRowVersion rowVersion = ( curRow.RowState == DataRowState.Detached ) ? DataRowVersion.Proposed : DataRowVersion.Current;
                        return curCol.ConvertObjectToXml( curRow[curCol,rowVersion] );
                    }
                    curCol = PreviousColumn( curRow, curCol, true );
                }
                return null;
            }
        }
        
        internal string GetNamespace(string name) {
           //Debug.WriteLineIf( XmlTrace.traceXPathNodePointerFunctions.Enabled, "XPathNodePointer:GetNamespace(name)");
            //we are checking the namespace nodes backwards comparing its normal order in DOM tree
            if ( name == "xml" )
                return s_strReservedXml;
            if ( name == "xmlns" )
                return s_strReservedXmlns;
            if ( name != null && name.Length == 0 )
                name = "xmlns";
            RealFoliate();
            XmlNode node = _node;
            XmlNodeType nt = node.NodeType;
            String retVal = null;
            while ( node != null ) {
                //first identify an element node in the ancestor + itself
                while ( node != null && ( ( nt = node.NodeType ) != XmlNodeType.Element ) ) {
                    if ( nt == XmlNodeType.Attribute )
                        node = ((XmlAttribute)node).OwnerElement;
                    else
                        node = node.ParentNode;
                }
                //found one -- inside if
                if ( node != null ) {
                    //must be element node
                    retVal = GetNamespace((XmlBoundElement)node, name);
                    if ( retVal != null )
                        return retVal;
                    //didn't find it, try the next parentnode
                    node = node.ParentNode;    
                }                
            }
            //nothing happens, then return string.empty.
            return string.Empty;
        }

        internal bool MoveToNamespace(string name) {
            //Debug.WriteLineIf( XmlTrace.traceXPathNodePointerFunctions.Enabled, "XPathNodePointer:MoveToNamespace(name)");
            _parentOfNS = this._node as XmlBoundElement;
            //only need to check with _node, even if _column is not null and its mapping type is element, it can't have attributes
            if ( _parentOfNS == null )
                return false; 
            string attrName = name;
            if ( attrName == "xmlns" )
                attrName = "xmlns:xmlns";
            if ( attrName != null && attrName.Length == 0 )
                attrName = "xmlns";
            RealFoliate();
            XmlNode node = this._node;
            XmlNodeType nt = node.NodeType;
            XmlAttribute attr = null;
            XmlBoundElement be = null;
            while ( node != null ) {
                //check current element node
                be = node as XmlBoundElement;
                if ( be != null ) {
                    if ( be.IsFoliated ) {
                        attr = be.GetAttributeNode ( name, s_strReservedXmlns );
                        if ( attr != null ) {
                            MoveTo( attr );
                            return true;
                        }
                    } 
                    else {//defoliated so that we need to search through its column 
                        DataRow curRow = be.Row;
                        if ( curRow == null )
                            return false;
                        //going through its attribute columns
                        DataColumn curCol = PreviousColumn( curRow, null, true );
                        while ( curCol != null ) {
                            if ( curCol.Namespace == s_strReservedXmlns && curCol.ColumnName == name ) {
                                MoveTo( be, curCol, false );
                                return true;
                            }
                            curCol = PreviousColumn( curRow, curCol, true );
                        }
                    }
                } 
                //didn't find it, try the next element anccester.
                do {
                    node = node.ParentNode;
                } while ( node != null && node.NodeType != XmlNodeType.Element );
            }
            //nothing happens, the name doesn't exist as a namespace node.
            _parentOfNS = null;
            return false;
        }

        //the function will find the next namespace node on the given bound element starting with the given column or attribte
        // wether to use column or attribute depends on if the bound element is folicated or not.
        private bool MoveToNextNamespace( XmlBoundElement be, DataColumn col, XmlAttribute curAttr ) {
            //Debug.WriteLineIf( XmlTrace.traceXPathNodePointerFunctions.Enabled, "XPathNodePointer:MoveToNextNamespace(be,col,curAttr)");
            if ( be != null ) {
                if ( be.IsFoliated ) {
                    XmlAttributeCollection attrs = be.Attributes;
                    XmlAttribute attr = null;
                    bool bFound = false;
                    if ( curAttr == null )
                        bFound = true; //the first namespace will be the one
#if DEBUG
                    if ( curAttr != null )
                        Debug.Assert( curAttr.NamespaceURI == s_strReservedXmlns );
#endif
                    Debug.Assert( attrs!=null );
                    int attrInd = attrs.Count;                
                    while ( attrInd > 0 ) {
                        attrInd--;
                        attr = attrs[attrInd];
                        if ( bFound && attr.NamespaceURI == s_strReservedXmlns && !DuplicateNS( be, attr.LocalName ) ) {
                            MoveTo(attr);
                            return true;
                        }
                        if ( attr == curAttr )
                            bFound = true;
                    }
                } 
                else {//defoliated so that we need to search through its column 
                    DataRow curRow = be.Row;
                    if ( curRow == null )
                        return false;
                    //going through its attribute columns
                    DataColumn curCol = PreviousColumn( curRow, col, true );
                    while ( curCol != null ) {
                        if ( curCol.Namespace == s_strReservedXmlns && !DuplicateNS( be, curCol.ColumnName ) ) {
                            MoveTo( be, curCol, false );
                            return true;
                        }
                        curCol = PreviousColumn( curRow, curCol, true );
                    }
                }
            } 
            return false;
        }
        
        //Caller( DataDocumentXPathNavigator will make sure that the node is at the right position for this call )
        internal bool MoveToFirstNamespace(XPathNamespaceScope namespaceScope) {
            //Debug.WriteLineIf( XmlTrace.traceXPathNodePointerFunctions.Enabled, "XPathNodePointer:MoveToFirstNamespace(namespaceScope)");
            RealFoliate();
            _parentOfNS = this._node as XmlBoundElement;
            //only need to check with _node, even if _column is not null and its mapping type is element, it can't have attributes
            if ( _parentOfNS == null )
                return false; 
            XmlNode node = this._node;
            XmlBoundElement be = null;
            while ( node != null ) {
                be = node as XmlBoundElement;
                if ( MoveToNextNamespace( be, null, null ) )
                    return true;
                //didn't find it
                if ( namespaceScope == XPathNamespaceScope.Local )
                    goto labelNoNS;
                //try the next element anccestor.
                do {
                    node = node.ParentNode;
                } while ( node != null && node.NodeType != XmlNodeType.Element );
            }
            if ( namespaceScope == XPathNamespaceScope.All ) {
                MoveTo( this._doc.attrXml, null, false );
                return true;
            }
labelNoNS:
            //didn't find one namespace node
            _parentOfNS = null;
            return false;
        }

        //endElem is on the path from startElem to root is enforced by the caller
        private bool DuplicateNS( XmlBoundElement endElem, string lname) {
            //Debug.WriteLineIf( XmlTrace.traceXPathNodePointerFunctions.Enabled, "XPathNodePointer:DuplicateNS(endElem, lname)");
            if ( this._parentOfNS == null || endElem == null )
                return false;
            XmlBoundElement be = this._parentOfNS; 
            XmlNode node = null;
            while ( be != null && be != endElem ) {
                if ( GetNamespace( be, lname ) != null )
                    return true;
                node = (XmlNode)be;
                do {
                    node = node.ParentNode;
                } while ( node != null && node.NodeType != XmlNodeType.Element );
                be = node as XmlBoundElement;
            }
            return false;            
        }
        
        //Caller( DataDocumentXPathNavigator will make sure that the node is at the right position for this call )
        internal bool MoveToNextNamespace(XPathNamespaceScope namespaceScope) {
            //Debug.WriteLineIf( XmlTrace.traceXPathNodePointerFunctions.Enabled, "XPathNodePointer:MoveToNextNamespace(namespaceScope)");
            RealFoliate();
            Debug.Assert( _parentOfNS != null );
            XmlNode node = this._node;
            //first check within the same boundelement
            if ( this._column != null ) {
                Debug.Assert( this._column.Namespace == s_strReservedXmlns );
                if ( namespaceScope == XPathNamespaceScope.Local && _parentOfNS != this._node ) //already outside scope
                    return false;
                XmlBoundElement be = this._node as XmlBoundElement;
                Debug.Assert( be != null );
                DataRow curRow = be.Row;
                Debug.Assert( curRow != null );
                DataColumn curCol = PreviousColumn( curRow, this._column, true );
                while ( curCol != null ) {
                    if ( curCol.Namespace == s_strReservedXmlns ) {
                        MoveTo( be, curCol, false );
                        return true;
                    }
                    curCol = PreviousColumn( curRow, curCol, true );
                }                
                //didn't find it in this loop
                if ( namespaceScope == XPathNamespaceScope.Local )
                    return false;
                //try its ancesstor
                do {
                    node = node.ParentNode;
                } while ( node != null && node.NodeType != XmlNodeType.Element );
            } 
            else  if ( this._node.NodeType == XmlNodeType.Attribute ) {
                XmlAttribute attr = (XmlAttribute)(this._node);
                Debug.Assert( attr != null );
                node = attr.OwnerElement;
                if ( node == null )
                    return false;
                if ( namespaceScope == XPathNamespaceScope.Local && _parentOfNS != node ) //already outside scope
                    return false;
                if ( MoveToNextNamespace( (XmlBoundElement)node, null, (XmlAttribute)attr ) )
                    return true;
                //didn't find it
                if ( namespaceScope == XPathNamespaceScope.Local )
                    return false;
                do {
                    node = node.ParentNode;
                } while ( node != null && node.NodeType != XmlNodeType.Element );
            }
            // till now, node should be the next accesstor (bound) element of the element parent of current namespace node (attribute or data column)
            while ( node != null ) {
                //try the namespace attributes from the same element
                XmlBoundElement be = node as XmlBoundElement;
                if ( MoveToNextNamespace( be, null, null ) )
                    return true;
                //no more namespace attribute under the same element
                do {
                    node = node.ParentNode;
                } while ( node != null && node.NodeType == XmlNodeType.Element );
            }
            //didn't find the next namespace, thus return
            if ( namespaceScope == XPathNamespaceScope.All ) {
                MoveTo( this._doc.attrXml, null, false );
                return true;
            }
            return false;
        }
        
        [System.Diagnostics.Conditional("DEBUG")]
        private void AssertValid() {
            // This pointer must be int the document list
            //RealFoliate();
            this._doc.AssertPointerPresent( this );
            if ( this._column != null ) {
                // We must be on a de-foliated region
                XmlBoundElement rowElem = this._node as XmlBoundElement;
                Debug.Assert( rowElem != null );

                DataRow row = rowElem.Row;
                Debug.Assert( row != null );

                //ElementState state = rowElem.ElementState;
                //Debug.Assert( state == ElementState.Defoliated || _bNeedFoliated, "Region is accessed using column, but it's state is FOLIATED" );

                // We cannot be on a column for which the value is DBNull
                DataRowVersion rowVersion = ( row.RowState == DataRowState.Detached ) ? DataRowVersion.Proposed : DataRowVersion.Current;
                Debug.Assert( ! Convert.IsDBNull( row[ this._column, rowVersion ] ) );

                // If we are on the Text column, we should always have _fOnValue == true
                Debug.Assert( (this._column.ColumnMapping == MappingType.SimpleContent) ? (this._fOnValue == true) : true );
            }
            if ( this._column == null ) 
                Debug.Assert( !this._fOnValue );
        }

        internal XmlDataDocument Document { get { return _doc; } }

        bool IXmlDataVirtualNode.IsInUse() {
            //Debug.WriteLineIf( XmlTrace.traceXPathNodePointerFunctions.Enabled, "XPathNodePointer:IsInUse()");
            return _owner.IsAlive;
        }
    }
}

