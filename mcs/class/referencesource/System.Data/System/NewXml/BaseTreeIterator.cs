//------------------------------------------------------------------------------
// <copyright file="BaseTreeIterator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------
namespace System.Xml {

    // Iterates over non-attribute nodes
    internal abstract class BaseTreeIterator {
        protected DataSetMapper   mapper;

        internal BaseTreeIterator( DataSetMapper mapper ) {
            this.mapper      = mapper;
        }

        internal abstract void Reset();

        internal abstract XmlNode CurrentNode { get; }

        internal abstract bool Next();
        internal abstract bool NextRight();

        internal bool NextRowElement() {
            while ( Next() ) {
                if ( OnRowElement() )
                    return true;
            }
            return false;
        }

        internal bool NextRightRowElement() {
            if ( NextRight() ) {
                if ( OnRowElement() )
                    return true;
                return NextRowElement();
            }
            return false;
        }

        // Returns true if the current node is on a row element (head of a region)
        internal bool OnRowElement() {
            XmlBoundElement be = CurrentNode as XmlBoundElement;
            return (be != null) && (be.Row != null);
        }
    }
}

