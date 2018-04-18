//------------------------------------------------------------------------------
// <copyright file="DataPointer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------
#pragma warning disable 618 // ignore obsolete warning about XmlDataDocument
namespace System.Xml {
    using System;
    using System.Data;
    using System.Diagnostics;
    
    internal sealed class DataPointer : IXmlDataVirtualNode {
        private XmlDataDocument doc;
        private XmlNode node;
        private DataColumn column;
        private bool fOnValue;
        private bool bNeedFoliate = false;
        private bool _isInUse;

        internal DataPointer( XmlDataDocument doc, XmlNode node ) {
            this.doc = doc;
            this.node = node;
            this.column = null;
            this.fOnValue = false;
            bNeedFoliate = false;
            this._isInUse = true;
            AssertValid();
        }

        internal DataPointer( DataPointer pointer ) {
            this.doc = pointer.doc;
            this.node = pointer.node;
            this.column = pointer.column;
            this.fOnValue = pointer.fOnValue;
            this.bNeedFoliate = false;
            this._isInUse = true;
            AssertValid();
        }

        internal void AddPointer() {
            this.doc.AddPointer( (IXmlDataVirtualNode)this );
        }

        // Returns the row element of the region that the pointer points into
        private XmlBoundElement GetRowElement() {
            //AssertValid();

            XmlBoundElement rowElem;
            if ( this.column != null ) {
                rowElem = this.node as XmlBoundElement;
                Debug.Assert( rowElem != null );
                Debug.Assert( rowElem.Row != null );
                return rowElem;
            }

            doc.Mapper.GetRegion( this.node, out rowElem );
            return rowElem;
        }

        private DataRow Row {
            get { 
                //AssertValid();
                XmlBoundElement rowElem = GetRowElement();
                if ( rowElem == null )
                    return null;

                Debug.Assert( rowElem.Row != null );
                return rowElem.Row;
            }
        }

        private static bool IsFoliated( XmlNode node ) {
            if (node != null && node is XmlBoundElement)
                return((XmlBoundElement)node).IsFoliated;
            return true;
        }
        
        internal void MoveTo( DataPointer pointer ) {
            AssertValid();
            // You should not move outside of this document
            Debug.Assert( node == this.doc || node.OwnerDocument == this.doc );

            this.doc = pointer.doc;
            this.node = pointer.node;
            this.column = pointer.column;
            this.fOnValue = pointer.fOnValue;
            AssertValid();
        }
        private void MoveTo( XmlNode node ) {
            //AssertValid();
            // You should not move outside of this document
            Debug.Assert( node == this.doc || node.OwnerDocument == this.doc );

            this.node = node;
            this.column = null;
            this.fOnValue = false;
            AssertValid();
        }
        
        private void MoveTo( XmlNode node, DataColumn column, bool fOnValue ) {
            //AssertValid();
            // You should not move outside of this document
            Debug.Assert( node == this.doc || node.OwnerDocument == this.doc );

            this.node = node;
            this.column = column;
            this.fOnValue = fOnValue;
            AssertValid();
        }

        private DataColumn NextColumn( DataRow row, DataColumn col, bool fAttribute, bool fNulls ) {
            if (row.RowState == DataRowState.Deleted)
                return null;

            DataTable table = row.Table;
            DataColumnCollection columns = table.Columns;
            int iColumn = (col != null) ? col.Ordinal + 1 : 0;
            int cColumns = columns.Count;
            DataRowVersion rowVersion = ( row.RowState == DataRowState.Detached ) ? DataRowVersion.Proposed : DataRowVersion.Current;

            for (; iColumn < cColumns; iColumn++) {
                DataColumn c = columns[iColumn];
                if (!doc.IsNotMapped( c ) && (c.ColumnMapping == MappingType.Attribute) == fAttribute && (fNulls || ! Convert.IsDBNull( row[c, rowVersion] ) ) )
                    return c;
            }

            return null;
        }

        private DataColumn NthColumn( DataRow row, bool fAttribute, int iColumn, bool fNulls ) {
            DataColumn c = null;
            while ((c = NextColumn( row, c, fAttribute, fNulls )) != null) {
                if (iColumn == 0)
                    return c;

                iColumn = checked((int)iColumn-1);
            }
            return null;
        }

        private int ColumnCount( DataRow row, bool fAttribute, bool fNulls ) {
            DataColumn c = null;
            int count = 0;
            while ((c = NextColumn( row, c, fAttribute, fNulls )) != null) {
                count++;
            }
            return count;
        }

        internal bool MoveToFirstChild() {
            RealFoliate();
            AssertValid();
            if (node == null)
                return false;

            if (column != null) {
                if (fOnValue)
                    return false;

                fOnValue = true;
                return true;
            }
            else if (!IsFoliated( node )) {
                // find virtual column elements first
                DataColumn c = NextColumn( Row, null, false, false );
                if (c != null) {
                    MoveTo( node, c, doc.IsTextOnly(c) );
                    return true;
                }
            }

            // look for anything
            XmlNode n = doc.SafeFirstChild( node );
            if (n != null) {
                MoveTo(n);
                return true;
            }

            return false;
        }

        internal bool MoveToNextSibling() {
            RealFoliate();
            AssertValid();
            if (node != null) {
                if (column != null) {
                    if (fOnValue && !doc.IsTextOnly(column))
                        return false;

                    DataColumn c = NextColumn( Row, column, false, false );
                    if (c != null) {
                        MoveTo( this.node, c, false );
                        return true;
                    }

                    XmlNode n = doc.SafeFirstChild( node );
                    if (n != null) {
                        MoveTo( n );
                        return true;
                    }
                }
                else {
                    XmlNode n = doc.SafeNextSibling( node );
                    if (n != null) {
                        MoveTo(n);
                        return true;
                    }
                }
            }

            return false;
        }

        internal bool MoveToParent() {
            RealFoliate();
            AssertValid();
            if (node != null) {
                if (column != null) {
                    if (fOnValue && !doc.IsTextOnly(column)) {
                        MoveTo( node, column, false );
                        return true;
                    }

                    if (column.ColumnMapping != MappingType.Attribute) {
                        MoveTo( node, null, false );
                        return true;
                    }
                }
                else {
                    XmlNode n = node.ParentNode;
                    if (n != null) {
                        MoveTo(n);
                        return true;
                    }
                }
            }
            return false;
        }

        internal bool MoveToOwnerElement() {
            RealFoliate();
            AssertValid();
            if (node != null) {
                if (column != null) {
                    if (fOnValue || doc.IsTextOnly(column) || column.ColumnMapping != MappingType.Attribute)
                        return false;

                    MoveTo( node, null, false );
                    return true;
                }
                else if (node.NodeType == XmlNodeType.Attribute) {
                    XmlNode n = ((XmlAttribute)node).OwnerElement;
                    if (n != null) {
                        MoveTo( n, null, false );
                        return true;
                    }
                }
            }

            return false;
        }


        internal int AttributeCount {
            get {
                RealFoliate();
                AssertValid();
                if (node != null) {
                    if (column == null && node.NodeType == XmlNodeType.Element) {
                        if (!IsFoliated( node )) {
                            return ColumnCount( Row, true, false );
                        }
                        else 
                            return node.Attributes.Count;
                    }
                }
                return 0;
            }            
        }

        internal bool MoveToAttribute( int i ) {
            RealFoliate();
            AssertValid();
            if ( i < 0 ) 
                return false;
            if (node != null) {
                if ((column == null || column.ColumnMapping == MappingType.Attribute) && node.NodeType == XmlNodeType.Element) {
                    if (!IsFoliated( node )) {
                        DataColumn c = NthColumn( Row, true, i, false );
                        if (c != null) {
                            MoveTo( node, c, false );
                            return true;
                        }
                    }
                    else {
                        XmlNode n = node.Attributes.Item(i);
                        if (n != null) {
                            MoveTo( n, null, false );
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        internal XmlNodeType NodeType { 
            get {
                RealFoliate();
                AssertValid();
                if (this.node == null) {
                    return XmlNodeType.None;
                }
                else if (this.column == null) {
                    return this.node.NodeType; 
                }
                else if (this.fOnValue) {
                    return XmlNodeType.Text;
                }
                else if (this.column.ColumnMapping == MappingType.Attribute) {
                    return XmlNodeType.Attribute;
                }
                else {
                    return XmlNodeType.Element;
                }
            }
        }

        internal string LocalName { 
            get {
                RealFoliate();
                AssertValid();
                if (this.node == null) {
                    return string.Empty;
                }else if (this.column == null) {
                    String name = node.LocalName;
                    Debug.Assert( name != null );
                    if ( IsLocalNameEmpty( this.node.NodeType ) )
                        return String.Empty;
                    return name;
                }
                else if (this.fOnValue) {
                    return String.Empty;
                }
                else {
                    return doc.NameTable.Add(column.EncodedColumnName);
                }
            }
        }

        internal string NamespaceURI { 
            get {
                RealFoliate();
                AssertValid();
                if (this.node == null) {
                    return string.Empty;
                }
                else if (this.column == null) {
                    return node.NamespaceURI; 
                }
                else if (this.fOnValue) {
                    return string.Empty;
                }
                else {
                    return doc.NameTable.Add(column.Namespace);
                }
            }
        }
        
        internal string Name { 
            get {
                RealFoliate();
                AssertValid();
                if (this.node == null) {
                    return string.Empty;
                }
                else if (this.column == null) {
                    String name = node.Name;
                    //Again it could be String.Empty at null position
                    Debug.Assert( name != null );
                    if ( IsLocalNameEmpty( this.node.NodeType ) )
                        return String.Empty;
                    return name;
                }
                else {
                    string prefix = Prefix;
                    string lname = LocalName;
                    if (prefix != null && prefix.Length > 0) {
                        if (lname != null && lname.Length > 0) {
                            return doc.NameTable.Add( prefix + ":" + lname );
                        }
                        else {
                            return prefix;
                        }
                    }
                    else {
                        return lname;
                    }
                }
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

        internal string Prefix { 
            get {
                RealFoliate();
                AssertValid();
                if (this.node == null) {
                    return string.Empty;
                }
                else if (this.column == null) {
                    return node.Prefix; 
                }
                else {
                    return string.Empty;
                }
            }
        }

        internal string Value { 
            get {
                RealFoliate();
                AssertValid();
                if (this.node == null) {
                    return null;
                }
                else if (this.column == null) {
                    return this.node.Value;
                }
                else if (this.column.ColumnMapping == MappingType.Attribute || this.fOnValue) {
                    DataRow row = this.Row;
                    DataRowVersion rowVersion = ( row.RowState == DataRowState.Detached ) ? DataRowVersion.Proposed : DataRowVersion.Current;
                    object value = row[ this.column, rowVersion ];
                    if ( ! Convert.IsDBNull( value ) )
                        return this.column.ConvertObjectToXml( value );
                    return null;
                }
                else {
                    // column element has no value
                    return null;
                }
            }
        }

        bool IXmlDataVirtualNode.IsOnNode( XmlNode nodeToCheck ) {
            RealFoliate();
            return nodeToCheck == this.node;
        }
        
        bool IXmlDataVirtualNode.IsOnColumn( DataColumn col ) {
            RealFoliate();
            return col == this.column;
        }
        
        internal XmlNode GetNode() {
            return this.node;
        }
        
        internal bool IsEmptyElement { 
            get {
                RealFoliate();
                AssertValid();
                if (node != null && column == null) {
                    // 
                    if (node.NodeType == XmlNodeType.Element) {
                        return((XmlElement)node).IsEmpty;
                    }
                }
                return false;
            }
        }

        internal bool IsDefault { 
            get {
                RealFoliate();
                AssertValid();
                if (node != null && column == null && node.NodeType == XmlNodeType.Attribute) {
                    return !((XmlAttribute)node).Specified;
                }

                return false;
            }
        }

        void IXmlDataVirtualNode.OnFoliated( XmlNode foliatedNode ) {
            // update the pointer if the element node has been foliated
            if (node == foliatedNode) {
                // if already on this node, nothing to do!
                if (column == null)
                    return;
                bNeedFoliate = true;
            }
        }

        internal void RealFoliate() {
            if ( !bNeedFoliate )
                return;

            XmlNode n = null;

            if (doc.IsTextOnly( column )) {
                n = node.FirstChild;
            }
            else {
                if (column.ColumnMapping == MappingType.Attribute) {
                    n = node.Attributes.GetNamedItem( column.EncodedColumnName, column.Namespace );
                }
                else {
                    for (n = node.FirstChild; n != null; n = n.NextSibling) {
                        if (n.LocalName == column.EncodedColumnName && n.NamespaceURI == column.Namespace)
                            break;
                    }
                }

                if (n != null && fOnValue)
                    n = n.FirstChild;
            }

            if (n == null)
                throw new InvalidOperationException(Res.GetString(Res.DataDom_Foliation));

            // Cannot use MoveTo( n ); b/c the initial state for MoveTo is invalid (region is foliated but this is not)
            this.node = n;
            this.column = null;
            this.fOnValue = false;
            AssertValid();
            
            bNeedFoliate = false;
        }

        //for the 6 properties below, only when the this.column == null that the nodetype could be XmlDeclaration node
        internal String PublicId {
            get {
                XmlNodeType nt = NodeType;
                switch ( nt ) {
                    case XmlNodeType.DocumentType : {
                        Debug.Assert( this.column == null );
                        return ( ( XmlDocumentType ) (this.node)).PublicId;
                    }
                    case XmlNodeType.Entity : {
                        Debug.Assert( this.column == null );
                        return ( ( XmlEntity ) (this.node)).PublicId;
                    }
                    case XmlNodeType.Notation : {
                        Debug.Assert( this.column == null );
                        return ( ( XmlNotation ) (this.node)).PublicId;
                    }
                }
                return null;
            }
        }

        internal String SystemId {
            get {
                XmlNodeType nt = NodeType;
                switch ( nt ) {
                    case XmlNodeType.DocumentType : {
                        Debug.Assert( this.column == null );
                        return ( ( XmlDocumentType ) (this.node)).SystemId;
                    }
                    case XmlNodeType.Entity : {
                        Debug.Assert( this.column == null );
                        return ( ( XmlEntity ) (this.node)).SystemId;
                    }
                    case XmlNodeType.Notation : {
                        Debug.Assert( this.column == null );
                        return ( ( XmlNotation ) (this.node)).SystemId;
                    }
                }
                return null;
            }
        }

        internal String InternalSubset {
            get {
                if ( NodeType == XmlNodeType.DocumentType ) {
                    Debug.Assert( this.column == null );
                    return ( ( XmlDocumentType ) (this.node)).InternalSubset;
                }
                return null;
            }
        }

        internal XmlDeclaration Declaration {
            get {
                XmlNode child = doc.SafeFirstChild(doc);
                if ( child != null && child.NodeType == XmlNodeType.XmlDeclaration )
                    return (XmlDeclaration)child;
                return null;
            }
        }
        
        internal String Encoding {
            get {
                if ( NodeType == XmlNodeType.XmlDeclaration ) {
                    Debug.Assert( this.column == null );
                    return ( ( XmlDeclaration ) (this.node)).Encoding;
                } else if ( NodeType == XmlNodeType.Document ) {
                    XmlDeclaration dec = Declaration;
                    if ( dec != null )
                        return dec.Encoding;
                }
                return null;
            }
        }
        
        internal String Standalone {
            get {
                if ( NodeType == XmlNodeType.XmlDeclaration ) {
                    Debug.Assert( this.column == null );
                    return ( ( XmlDeclaration ) (this.node)).Standalone;
                } else if ( NodeType == XmlNodeType.Document ) {
                    XmlDeclaration dec = Declaration;
                    if ( dec != null )
                        return dec.Standalone;
                }
                return null;
            }
        }

        internal String Version {
            get {
                if ( NodeType == XmlNodeType.XmlDeclaration ) {
                    Debug.Assert( this.column == null );
                    return ( ( XmlDeclaration ) (this.node)).Version;
                } else if ( NodeType == XmlNodeType.Document ) {
                    XmlDeclaration dec = Declaration;
                    if ( dec != null )
                        return dec.Version;
                }
                return null;
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private void AssertValid() {
            // This pointer must be int the document list
            if ( this.column != null ) {
                // We must be on a de-foliated region
                XmlBoundElement rowElem = this.node as XmlBoundElement;
                Debug.Assert( rowElem != null );

                DataRow row = rowElem.Row;
                Debug.Assert( row != null );

                ElementState state = rowElem.ElementState;
                Debug.Assert( state == ElementState.Defoliated, "Region is accessed using column, but it's state is FOLIATED" );

                // We cannot be on a column for which the value is DBNull
                DataRowVersion rowVersion = ( row.RowState == DataRowState.Detached ) ? DataRowVersion.Proposed : DataRowVersion.Current;
                Debug.Assert( ! Convert.IsDBNull( row[ this.column, rowVersion ] ) );

                // If we are on the Text column, we should always have fOnValue == true
                Debug.Assert( (this.column.ColumnMapping == MappingType.SimpleContent) ? (this.fOnValue == true) : true );
            }
        }

        bool IXmlDataVirtualNode.IsInUse() {
            return _isInUse;
        }

        internal void SetNoLongerUse() {
            this.node = null;
            this.column = null;
            this.fOnValue = false;
            this.bNeedFoliate = false;
            this._isInUse = false;
        }
        
    }
}
