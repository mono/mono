//------------------------------------------------------------------------------
// <copyright file="RegionIterator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------
#pragma warning disable 618 // ignore obsolete warning about XmlDataDocument
namespace System.Xml {
    using System;
    using System.Diagnostics;
    using System.Text;


    internal abstract class BaseRegionIterator : BaseTreeIterator {
        internal BaseRegionIterator( DataSetMapper mapper ) : base( mapper ) {
        }

    }


    // Iterates over non-attribute nodes
    internal sealed class RegionIterator : BaseRegionIterator {
        private XmlBoundElement rowElement;
        private XmlNode         currentNode;

        internal RegionIterator( XmlBoundElement rowElement ) : base( ((XmlDataDocument)(rowElement.OwnerDocument)).Mapper ) {
            Debug.Assert( rowElement != null && rowElement.Row != null );
            this.rowElement  = rowElement;
            this.currentNode = rowElement;
        }

        internal override void Reset() {
            currentNode = rowElement;
        }

        internal override XmlNode CurrentNode {
            get {
                return currentNode;
            }
        }

        internal override bool Next() {
            XmlNode nextNode;
            ElementState oldState = rowElement.ElementState;
            // We do not want to cause any foliation w/ this iterator or use this iterator once the region was defoliated
            Debug.Assert( oldState != ElementState.None );

            // Try to move to the first child
            nextNode = currentNode.FirstChild;

            // No children, try next sibling
            if ( nextNode != null ) {
                currentNode = nextNode;
                // If we have been defoliated, we should have stayed that way
                Debug.Assert( (oldState == ElementState.Defoliated) ? (rowElement.ElementState == ElementState.Defoliated) : true );
                // Rollback foliation
                rowElement.ElementState = oldState;
                return true;
            }
            return NextRight();
        }

        internal override bool NextRight() {
            // Make sure we do not get past the rowElement if we call NextRight on a just initialized iterator and rowElement has no children
            if ( currentNode == rowElement ) {
                currentNode = null;
                return false;
            }

            ElementState oldState = rowElement.ElementState;
            // We do not want to cause any foliation w/ this iterator or use this iterator once the region was defoliated
            Debug.Assert( oldState != ElementState.None );

            XmlNode nextNode = currentNode.NextSibling;

            if ( nextNode != null ) {
                currentNode = nextNode;
                // If we have been defoliated, we should have stayed that way
                Debug.Assert( (oldState == ElementState.Defoliated) ? (rowElement.ElementState == ElementState.Defoliated) : true );
                // Rollback foliation
                rowElement.ElementState = oldState;
                return true;
            }

            // No next sibling, try the first sibling of from the parent chain
            nextNode = currentNode;
            while ( nextNode != rowElement && nextNode.NextSibling == null )
                nextNode = nextNode.ParentNode;

            if ( nextNode == rowElement ) {
                currentNode = null;
                // If we have been defoliated, we should have stayed that way
                Debug.Assert( (oldState == ElementState.Defoliated) ? (rowElement.ElementState == ElementState.Defoliated) : true );
                // Rollback foliation
                rowElement.ElementState = oldState;
                return false;
            }

            currentNode = nextNode.NextSibling;
            Debug.Assert( currentNode != null );
            // If we have been defoliated, we should have stayed that way
            Debug.Assert( (oldState == ElementState.Defoliated) ? (rowElement.ElementState == ElementState.Defoliated) : true );
            // Rollback foliation
            rowElement.ElementState = oldState;
            return true;
        }

        // Get the initial text value for the current node. You should be positioned on the node (element) for
        // which to get the initial text value, not on the text node.
        internal bool NextInitialTextLikeNodes( out String value ) {
            Debug.Assert( this.CurrentNode != null );
            Debug.Assert( this.CurrentNode.NodeType == XmlNodeType.Element );
#if DEBUG
            // It's not OK to try to read the initial text value for sub-regions, because we do not know how to revert their initial state
            if ( this.CurrentNode.NodeType == XmlNodeType.Element && mapper.GetTableSchemaForElement( (XmlElement)(this.CurrentNode) ) != null ) {
                if ( this.CurrentNode != rowElement )
                    Debug.Assert( false );
            }
#endif

            ElementState oldState = rowElement.ElementState;
            // We do not want to cause any foliation w/ this iterator or use this iterator once the region was defoliated
            Debug.Assert( oldState != ElementState.None );

            XmlNode n = this.CurrentNode.FirstChild;
            value = GetInitialTextFromNodes( ref n );
            if ( n == null ) {
                // If we have been defoliated, we should have stayed that way
                Debug.Assert( (oldState == ElementState.Defoliated) ? (rowElement.ElementState == ElementState.Defoliated) : true );
                // Rollback eventual foliation
                rowElement.ElementState = oldState;
                return NextRight();
            }
            Debug.Assert( ! XmlDataDocument.IsTextLikeNode( n ) );
            currentNode = n;
            // If we have been defoliated, we should have stayed that way
            Debug.Assert( (oldState == ElementState.Defoliated) ? (rowElement.ElementState == ElementState.Defoliated) : true );
            // Rollback eventual foliation
            rowElement.ElementState = oldState;
            return true;
        }

        private static string GetInitialTextFromNodes( ref XmlNode n ) {
            string value = null;

            if ( n != null ) {
                // don't consider whitespace
                while ( n.NodeType == XmlNodeType.Whitespace ) {
                    n = n.NextSibling;
                    if ( n == null )
                        return String.Empty;
                }

                if ( XmlDataDocument.IsTextLikeNode( n ) && (n.NextSibling == null || ! XmlDataDocument.IsTextLikeNode( n.NextSibling )) ) {
                    // don't use string builder if only one text node exists
                    value = n.Value;
                    n = n.NextSibling;
                }
                else {
                    StringBuilder sb = new StringBuilder();
                    while ( n != null && XmlDataDocument.IsTextLikeNode( n ) ) {
                        // Ignore non-significant whitespace nodes
                        if ( n.NodeType != XmlNodeType.Whitespace )
                            sb.Append( n.Value );
                        n = n.NextSibling;
                    }
                    value = sb.ToString();
                }
            }

            if ( value == null )
                value = String.Empty;

            return value;
        }


        
    }
}
