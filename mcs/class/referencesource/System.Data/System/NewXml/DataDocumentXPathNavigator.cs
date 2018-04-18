//------------------------------------------------------------------------------
// <copyright file="DataDocumentXPathNavigator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------
#pragma warning disable 618 // ignore obsolete warning about XmlDataDocument
namespace System.Xml {
    using System;
    using System.Xml.XPath;

    internal sealed class DataDocumentXPathNavigator: XPathNavigator, IHasXmlNode {
        private XPathNodePointer    _curNode;           //pointer to remember the current node position
        private XmlDataDocument     _doc;              //pointer to remember the root -- can only be XmlDataDocument for DataDocumentXPathNavigator
        private XPathNodePointer    _temp;           
         
        internal DataDocumentXPathNavigator( XmlDataDocument doc, XmlNode node ) {
            _curNode = new XPathNodePointer( this, doc, node );
            _temp = new XPathNodePointer( this, doc, node );
            _doc = doc;
        }

        private DataDocumentXPathNavigator( DataDocumentXPathNavigator other ) {
            this._curNode = other._curNode.Clone( this );
            this._temp = other._temp.Clone( this );
            this._doc = other._doc;
        }
        public override XPathNavigator Clone(){
            return new DataDocumentXPathNavigator( this );
        }

        internal XPathNodePointer CurNode { get { return _curNode; } }
        internal XmlDataDocument Document { get { return _doc; } }

        //Convert will deal with nodeType as Attribute or Namespace nodes
        public override XPathNodeType NodeType { get { return _curNode.NodeType; } }

        public override string LocalName { get { return _curNode.LocalName; } }

        public override string NamespaceURI { get { return _curNode.NamespaceURI; } }

        public override string Name { get { return _curNode.Name; } }

        public override string Prefix { get { return _curNode.Prefix; } }

        public override string Value { 
            get { 
                XPathNodeType xnt = _curNode.NodeType;
                if ( xnt == XPathNodeType.Element || xnt == XPathNodeType.Root )
                    return _curNode.InnerText; 
                return _curNode.Value;
            } 
        }

        public override String BaseURI { get { return _curNode.BaseURI; } }

        public override String XmlLang { get { return _curNode.XmlLang; } }
        
        public override bool IsEmptyElement { get { return _curNode.IsEmptyElement; } }

        public override XmlNameTable NameTable { get { return _doc.NameTable; } }

        // Attributes
        public override bool HasAttributes { get { return _curNode.AttributeCount > 0; } }

        public override string GetAttribute( string localName, string namespaceURI ) {
            if ( _curNode.NodeType != XPathNodeType.Element )
                return string.Empty; //other type of nodes can't have attributes
            _temp.MoveTo( _curNode );
            if ( _temp.MoveToAttribute( localName, namespaceURI ) )
                return _temp.Value;
            return string.Empty;
        }

//#if SupportNamespaces

        public override string GetNamespace(string name) {
            return _curNode.GetNamespace( name );
        }

        public override bool MoveToNamespace(string name) {
            if ( _curNode.NodeType != XPathNodeType.Element )
                return false;
            return _curNode.MoveToNamespace( name );
        }

        public override bool MoveToFirstNamespace(XPathNamespaceScope namespaceScope) {
            if ( _curNode.NodeType != XPathNodeType.Element )
                return false;
            return _curNode.MoveToFirstNamespace(namespaceScope);
        }

        public override bool MoveToNextNamespace(XPathNamespaceScope namespaceScope) {
            if ( _curNode.NodeType != XPathNodeType.Namespace )
                return false;
            return _curNode.MoveToNextNamespace(namespaceScope);
        }
//#endif        

        public override bool MoveToAttribute( string localName, string namespaceURI ) {
            if ( _curNode.NodeType != XPathNodeType.Element )
                return false; //other type of nodes can't have attributes
            return _curNode.MoveToAttribute( localName, namespaceURI );
        }

        public override bool MoveToFirstAttribute() {
            if ( _curNode.NodeType != XPathNodeType.Element )
                return false; //other type of nodes can't have attributes
            return _curNode.MoveToNextAttribute(true);
        }

        public override bool MoveToNextAttribute() {
            if ( _curNode.NodeType != XPathNodeType.Attribute ) 
                return false;
            return _curNode.MoveToNextAttribute(false);
        }


        // Tree
        public override bool MoveToNext() {
            if ( _curNode.NodeType == XPathNodeType.Attribute ) 
                return false;
            return _curNode.MoveToNextSibling();
        }

        public override bool MoveToPrevious() {
            if ( _curNode.NodeType == XPathNodeType.Attribute ) 
                return false;
            return _curNode.MoveToPreviousSibling();
        }

        public override bool MoveToFirst() {
            if ( _curNode.NodeType == XPathNodeType.Attribute ) 
                return false;
            return _curNode.MoveToFirst();
        }

        public override bool HasChildren { get { return _curNode.HasChildren; } }

        public override bool MoveToFirstChild() {
            return _curNode.MoveToFirstChild();
        }

        public override bool MoveToParent() {
            return _curNode.MoveToParent();
        }

        public override void MoveToRoot() {
            _curNode.MoveToRoot();
        }

        public override bool MoveTo( XPathNavigator other ) {
            if ( other == null )
                return false;
            DataDocumentXPathNavigator otherDataDocXPathNav = other as DataDocumentXPathNavigator;
            if ( otherDataDocXPathNav != null ) {
                if ( _curNode.MoveTo( otherDataDocXPathNav.CurNode ) ) {
                    _doc = _curNode.Document;
                    return true;
                } 
                else
                    return false;
            }
            return false;
        }

        //doesn't support MoveToId
        public override bool MoveToId( string id ) {
            return false;
        }

        public override bool IsSamePosition( XPathNavigator other ) {
            if ( other == null )
                return false;
            DataDocumentXPathNavigator otherDataDocXPathNav = other as DataDocumentXPathNavigator;
            if ( otherDataDocXPathNav != null ) {
                if ( this._doc == otherDataDocXPathNav.Document && this._curNode.IsSamePosition(otherDataDocXPathNav.CurNode) )
                    return true;
            }
            return false;
        }

        //the function is only called for XPathNodeList enumerate nodes and 
        // shouldn't be promoted to frequently use because it will cause foliation
        XmlNode IHasXmlNode.GetNode() { return _curNode.Node; }

        public override XmlNodeOrder ComparePosition( XPathNavigator other ) {
            if ( other == null )
                return XmlNodeOrder.Unknown; // this is what XPathDocument does. // WebData 103403
            
            DataDocumentXPathNavigator otherDataDocXPathNav = other as DataDocumentXPathNavigator;

            if ( otherDataDocXPathNav == null || otherDataDocXPathNav.Document != this._doc )
                return XmlNodeOrder.Unknown;            

            return this._curNode.ComparePosition( otherDataDocXPathNav.CurNode );
        }

    }

}
