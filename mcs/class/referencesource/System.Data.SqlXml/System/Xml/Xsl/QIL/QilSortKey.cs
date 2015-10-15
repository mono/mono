//------------------------------------------------------------------------------
// <copyright file="QilSortKey.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------
using System;
using System.Diagnostics;

namespace System.Xml.Xsl.Qil {

    /// <summary>
    /// View over a Qil SortKey operator.
    /// </summary>
    /// <remarks>
    /// Don't construct QIL nodes directly; instead, use the <see cref="QilFactory">QilFactory</see>.
    /// </remarks>
    internal class QilSortKey : QilBinary {

        //-----------------------------------------------
        // Constructor
        //-----------------------------------------------

        /// <summary>
        /// Construct a new node
        /// </summary>
        public QilSortKey(QilNodeType nodeType, QilNode key, QilNode collation) : base(nodeType, key, collation) {
        }


        //-----------------------------------------------
        // QilSortKey methods
        //-----------------------------------------------

        public QilNode Key {
            get { return Left; }
            set { Left = value; }
        }

        public QilNode Collation {
            get { return Right; }
            set { Right = value; }
        }
    }
}
