//------------------------------------------------------------------------------
// <copyright file="TreeIterator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------
#pragma warning disable 618 // ignore obsolete warning about XmlDataDocument
namespace System.Xml {
    using System.Diagnostics;

    // Iterates over non-attribute nodes
    internal sealed class TreeIterator : BaseTreeIterator {
        private XmlNode         nodeTop;
        private XmlNode         currentNode;

        internal TreeIterator( XmlNode nodeTop ) : base( ((XmlDataDocument)(nodeTop.OwnerDocument)).Mapper ) {
            Debug.Assert( nodeTop != null );
            this.nodeTop     = nodeTop;
            this.currentNode = nodeTop;
        }

        internal override void Reset() {
            currentNode = nodeTop;
        }

        internal override XmlNode CurrentNode {
            get {
                return currentNode;
            }
        }

        internal override bool Next() {
            XmlNode nextNode;

            // Try to move to the first child
            nextNode = currentNode.FirstChild;

            // No children, try next sibling
            if ( nextNode != null ) {
                currentNode = nextNode;
                return true;
            }
            return NextRight();
        }

        internal override bool NextRight() {
            // Make sure we do not get past the nodeTop if we call NextRight on a just initialized iterator and nodeTop has no children
            if ( currentNode == nodeTop ) {
                currentNode = null;
                return false;
            }

            XmlNode nextNode = currentNode.NextSibling;

            if ( nextNode != null ) {
                currentNode = nextNode;
                return true;
            }

            // No next sibling, try the first sibling of from the parent chain
            nextNode = currentNode;
            while ( nextNode != nodeTop && nextNode.NextSibling == null )
                nextNode = nextNode.ParentNode;

            if ( nextNode == nodeTop ) {
                currentNode = null;
                return false;
            }

            currentNode = nextNode.NextSibling;
            Debug.Assert( currentNode != null );
            return true;
        }
    }
}

